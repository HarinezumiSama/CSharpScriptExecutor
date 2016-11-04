using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using CSharpScriptExecutor.Common;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace CSharpScriptExecutor
{
    public partial class ExecutionResultViewerForm : Form
    {
        #region Constants and Fields

        private readonly System.Windows.Controls.ToolTip _errorToolTip = new System.Windows.Controls.ToolTip();

        private ScriptExecutionResult _executionResult;

        #endregion

        #region Constructors

        private ExecutionResultViewerForm()
        {
            InitializeComponent();

            Text = $@"Execution Result — {Program.ProgramName}";

            MakeEditorReadOnly(tewSourceCode);
            MakeEditorReadOnly(tewGeneratedCode);

            tewSourceCode.InnerEditor.MouseHover += CodeEditor_MouseHover;
            tewSourceCode.InnerEditor.MouseHoverStopped += CodeEditor_MouseHoverStopped;

            tewGeneratedCode.InnerEditor.MouseHover += CodeEditor_MouseHover;
            tewGeneratedCode.InnerEditor.MouseHoverStopped += CodeEditor_MouseHoverStopped;
        }

        #endregion

        #region Public Properties

        public sealed override string Text
        {
            [DebuggerNonUserCode]
            get
            {
                return base.Text;
            }

            [DebuggerNonUserCode]
            set
            {
                base.Text = value;
            }
        }

        public ScriptExecutionResult ExecutionResult
        {
            [DebuggerStepThrough]
            get
            {
                return _executionResult;
            }

            private set
            {
                _executionResult = value;
                ParseExecutionResult();
            }
        }

        #endregion

        #region Public Methods

        public static void Show(Form owner, ScriptExecutionResult executionResult)
        {
            #region Argument Check

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (executionResult == null)
            {
                throw new ArgumentNullException(nameof(executionResult));
            }

            #endregion

            Func<int, int> safeReduce = x => Math.Max(x * 9 / 10, 100);

            using (var form = new ExecutionResultViewerForm())
            {
                form.ExecutionResult = executionResult;
                form.Icon = owner.Icon;
                form.Size = new System.Drawing.Size(safeReduce(owner.Width), safeReduce(owner.Height));

                form.ShowDialog(owner);
            }
        }

        #endregion

        #region Private Methods: General

        private static void MakeEditorReadOnly(TextEditorWrapper editor)
        {
            editor.InnerEditor.IsReadOnly = true;
            editor.InnerEditor.TextArea.TextView.BackgroundRenderers.Add(ReadOnlyBackgroundRenderer.Instance);
        }

        private static string FormatCompilerError(
            CompilerError compilerError,
            bool indent,
            int? lineCount = null,
            int lineOffset = 0)
        {
            var line = compilerError.Line - lineOffset;
            if (lineCount.HasValue && line > lineCount.Value)
            {
                line = lineCount.Value;
            }

            return $@"{(indent ? "    " : string.Empty)}Script({line},{compilerError.Column}): error {compilerError
                .ErrorNumber}: {compilerError.ErrorText}";
        }

        private void SetTabPageVisibility(TabPage page, bool visible)
        {
            #region Argument Check

            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            #endregion

            var contains = tcResults.TabPages.Contains(page);

            if (visible)
            {
                if (!contains)
                {
                    tcResults.TabPages.Add(page); // TODO: Insert tab page at right position automatically
                }
            }
            else
            {
                if (contains)
                {
                    tcResults.TabPages.Remove(page);
                }
            }
        }

        private void SetActiveControl(Control control)
        {
            #region Argument Check

            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            #endregion

            ActiveControl = control;

            var textBox = control as TextBoxBase;
            if (textBox != null)
            {
                textBox.SelectionStart = 0;
                textBox.SelectionLength = 0;
            }
        }

        private void ParseExecutionResult()
        {
            if (_executionResult == null)
            {
                gbResult.Text = @"[Unknown Result]";
                tbMessage.Text = string.Empty;
                scDetails.Visible = false;
                return;
            }

            scDetails.Visible = true;

            switch (_executionResult.Type)
            {
                case ScriptExecutionResultType.InternalError:
                    gbResult.Text = @"Internal Error";
                    break;

                case ScriptExecutionResultType.CompilationError:
                    gbResult.Text = @"Compilation Error";
                    tewGeneratedCode.InnerEditor.TextArea.TextView.LineTransformers.Add(
                        new ErrorColorizer(_executionResult.CompilerErrors, 0));
                    if (_executionResult.SourceCodeLineOffset.HasValue)
                    {
                        tewSourceCode.InnerEditor.TextArea.TextView.LineTransformers.Add(
                            new ErrorColorizer(
                                _executionResult.CompilerErrors,
                                _executionResult.SourceCodeLineOffset.Value));
                    }

                    break;

                case ScriptExecutionResultType.ExecutionError:
                    gbResult.Text = @"Execution Error";
                    break;

                case ScriptExecutionResultType.Success:
                    gbResult.Text = @"Success";
                    break;

                default:
                    throw new NotImplementedException();
            }

            ////TODO: Recognize no-return-value case
            var hasReturnValue = _executionResult.IsSuccess; // && !_executionResult.ReturnValue.IsNull();

            SetTabPageVisibility(tpReturnValue, hasReturnValue);
            pgReturnValue.PropertySort = PropertySort.NoSort;
            pgReturnValue.SelectedObject = hasReturnValue ? _executionResult.ReturnValue : null;

            rtbConsoleOut.Text = _executionResult.ConsoleOut;
            var hasConsoleOut = !string.IsNullOrEmpty(_executionResult.ConsoleOut);
            SetTabPageVisibility(tpConsoleOut, hasConsoleOut);

            rtbConsoleError.Text = _executionResult.ConsoleError;
            var hasConsoleError = !string.IsNullOrEmpty(_executionResult.ConsoleError);
            SetTabPageVisibility(tpConsoleError, hasConsoleError);

            tbMessage.Text = string.Format(
                "{0}{1}{2}",
                _executionResult.Message,
                Environment.NewLine,
                string.Join(
                    Environment.NewLine,
                    _executionResult.CompilerErrors.Select(item => FormatCompilerError(item, true))));
            scDetails.Panel1Collapsed = _executionResult.IsSuccess;
            tewSourceCode.InnerEditor.Text = _executionResult.SourceCode;
            tewGeneratedCode.InnerEditor.Text = _executionResult.GeneratedCode;

            if (!_executionResult.IsSuccess)
            {
                SetActiveControl(tbMessage);
            }
            else if (hasReturnValue)
            {
                SetActiveControl(pgReturnValue);
            }
            else if (hasConsoleOut)
            {
                SetActiveControl(rtbConsoleOut);
            }
            else if (hasConsoleError)
            {
                SetActiveControl(rtbConsoleError);
            }
        }

        #endregion

        #region Private Methods: Event Handlers

        private void CloseButton_Click(object sender, EventArgs e) => Close();

        private void CodeEditor_MouseHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_executionResult.Type != ScriptExecutionResultType.CompilationError)
            {
                return;
            }

            var editor = sender as TextEditor;
            if (editor == null)
            {
                return;
            }

            e.Handled = true;

            var lineOffset = 0;
            var isSourceCodeEditor = false;
            if (ReferenceEquals(editor, tewSourceCode.InnerEditor))
            {
                if (!_executionResult.SourceCodeLineOffset.HasValue)
                {
                    return;
                }

                lineOffset = _executionResult.SourceCodeLineOffset.Value;
                isSourceCodeEditor = true;
            }

            var position = editor.GetPositionFromPoint(e.GetPosition(editor));
            if (!position.HasValue)
            {
                return;
            }

            var compiledLineNumber = position.Value.Line + lineOffset;
            var totalLineCount = editor.LineCount;

            var errors = _executionResult
                .CompilerErrors
                .Where(item => item.Line == compiledLineNumber || item.Line > totalLineCount)
                .OrderBy(item => item.Line)
                .ThenBy(item => item.Column)
                .ToList();
            if (!errors.Any())
            {
                return;
            }

            var errorTooltip = string.Join(
                Environment.NewLine,
                errors.Select(
                    item => FormatCompilerError(
                        item,
                        false,
                        isSourceCodeEditor ? totalLineCount : (int?)null,
                        lineOffset)));

            _errorToolTip.PlacementTarget = editor;
            _errorToolTip.Content = errorTooltip;
            _errorToolTip.IsOpen = true;
        }

        private void CodeEditor_MouseHoverStopped(object sender, System.Windows.Input.MouseEventArgs e)
            => _errorToolTip.IsOpen = false;

        #endregion

        #region ErrorColorizer Class

        private sealed class ErrorColorizer : DocumentColorizingTransformer
        {
            private static readonly SolidColorBrush ErrorBrush = new SolidColorBrush(Colors.LightPink);

            private readonly IList<CompilerError> _compilerErrors;
            private readonly int _lineOffset;

            public ErrorColorizer(ICollection<CompilerError> compilerErrors, int lineOffset)
            {
                if (compilerErrors == null)
                {
                    throw new ArgumentNullException(nameof(compilerErrors));
                }

                if (compilerErrors.Any(item => item == null))
                {
                    throw new ArgumentException(@"The collection contains a null element.", nameof(compilerErrors));
                }

                if (lineOffset < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(lineOffset),
                        lineOffset,
                        @"The value must be non-negative.");
                }

                _compilerErrors = compilerErrors.ToList().AsReadOnly();
                _lineOffset = lineOffset;
            }

            protected override void ColorizeLine(DocumentLine line)
            {
                if (line == null)
                {
                    return;
                }

                var compiledLineNumber = line.LineNumber + _lineOffset;
                if (_compilerErrors.Any(item => item.Line == compiledLineNumber)
                    || (line.NextLine == null && _compilerErrors.Any(item => item.Line > compiledLineNumber)))
                {
                    ChangeLinePart(
                        line.Offset,
                        line.EndOffset,
                        item => item.TextRunProperties.SetBackgroundBrush(ErrorBrush));
                }
            }
        }

        #endregion

        #region ReadOnlyBackgroundRenderer Class

        private sealed class ReadOnlyBackgroundRenderer : IBackgroundRenderer
        {
            public static readonly ReadOnlyBackgroundRenderer Instance = new ReadOnlyBackgroundRenderer();

            private static readonly SolidColorBrush BackgroundBrush = new SolidColorBrush(Colors.WhiteSmoke);

            private ReadOnlyBackgroundRenderer()
            {
                // Nothing to do
            }

            public void Draw(TextView textView, DrawingContext drawingContext)
                => drawingContext.DrawRectangle(
                    BackgroundBrush,
                    null,
                    new Rect(new Point(), new Size(textView.ActualWidth, textView.ActualHeight)));

            public KnownLayer Layer => KnownLayer.Background;
        }

        #endregion
    }
}