using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using CSharpScriptExecutor.Common;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace CSharpScriptExecutor
{
    public partial class ExecutionResultViewerForm : Form
    {
        #region Nested Types

        #region ErrorColorizer Class

        private sealed class ErrorColorizer : DocumentColorizingTransformer
        {
            #region Fields

            //private static readonly System.Windows.Media.Pen s_errorPen = CreateErrorPen();

            private readonly IList<CompilerError> m_compilerErrors;
            private readonly int m_lineOffset;

            #endregion

            #region Constructors

            public ErrorColorizer(IEnumerable<CompilerError> compilerErrors, int lineOffset)
            {
                #region Argument Check

                if (compilerErrors == null)
                {
                    throw new ArgumentNullException("compilerErrors");
                }
                if (compilerErrors.Contains(null))
                {
                    throw new ArgumentException("The collection contains a null element.", "compilerErrors");
                }
                if (lineOffset < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "lineOffset",
                        lineOffset,
                        "The value must be non-negative.");
                }

                #endregion

                m_compilerErrors = compilerErrors.ToList().AsReadOnly();
                m_lineOffset = lineOffset;
            }

            #endregion

            #region Protected Methods

            protected override void ColorizeLine(DocumentLine line)
            {
                if (line == null)
                {
                    return;
                }

                var errorBrush = new SolidColorBrush(Colors.LightPink);

                var compiledLineNumber = line.LineNumber + m_lineOffset;
                if (m_compilerErrors.Any(item => item.Line == compiledLineNumber)
                    || (line.NextLine == null && m_compilerErrors.Any(item => item.Line > compiledLineNumber)))
                {
                    ChangeLinePart(
                        line.Offset,
                        line.EndOffset,
                        item => item.TextRunProperties.SetBackgroundBrush(errorBrush));
                }
            }

            #endregion
        }

        #endregion

        #region ReadOnlyBackgroundRenderer Class

        private sealed class ReadOnlyBackgroundRenderer : IBackgroundRenderer
        {
            #region Fields

            public static readonly ReadOnlyBackgroundRenderer Instance = new ReadOnlyBackgroundRenderer();

            #endregion

            #region Constructors

            private ReadOnlyBackgroundRenderer()
            {
                // Nothing to do
            }

            #endregion

            #region IBackgroundRenderer Members

            public void Draw(TextView textView, DrawingContext drawingContext)
            {
                drawingContext.DrawRectangle(
                    new SolidColorBrush(Colors.WhiteSmoke),
                    null,
                    new Rect(
                        new System.Windows.Point(),
                        new System.Windows.Size(textView.ActualWidth, textView.ActualHeight)));
            }

            public KnownLayer Layer
            {
                get { return KnownLayer.Background; }
            }

            #endregion
        }

        #endregion

        #endregion

        #region Fields

        private ScriptExecutionResult m_executionResult;
        private System.Windows.Controls.ToolTip m_errorToolTip = new System.Windows.Controls.ToolTip();

        #endregion

        #region Constructors

        private ExecutionResultViewerForm()
        {
            InitializeComponent();

            this.Text = string.Format("Execution Result — {0}", Program.ProgramName);

            MakeEditorReadOnly(tewSourceCode);
            MakeEditorReadOnly(tewGeneratedCode);

            tewSourceCode.InnerEditor.MouseHover += CodeEditor_MouseHover;
            tewSourceCode.InnerEditor.MouseHoverStopped += CodeEditor_MouseHoverStopped;

            tewGeneratedCode.InnerEditor.MouseHover += CodeEditor_MouseHover;
            tewGeneratedCode.InnerEditor.MouseHoverStopped += CodeEditor_MouseHoverStopped;
        }

        #endregion

        #region Private Methods

        private static void MakeEditorReadOnly(TextEditorWrapper editor)
        {
            editor.InnerEditor.IsReadOnly = true;
            editor.InnerEditor.TextArea.TextView.BackgroundRenderers.Add(ReadOnlyBackgroundRenderer.Instance);
        }

        private void SetTabPageVisibility(TabPage page, bool visible)
        {
            #region Argument Check

            if (page == null)
            {
                throw new ArgumentNullException("page");
            }

            #endregion

            bool contains = tcResults.TabPages.Contains(page);

            if (visible)
            {
                if (!contains)
                {
                    tcResults.TabPages.Add(page);  // TODO: Insert tab page at right position automatically
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

        private string FormatCompilerError(
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
            return string.Format(
                "{0}Script({1},{2}): error {3}: {4}",
                indent ? "    " : string.Empty,
                line,
                compilerError.Column,
                compilerError.ErrorNumber,
                compilerError.ErrorText);
        }

        private void ParseExecutionResult()
        {
            if (m_executionResult == null)
            {
                gbResult.Text = "[Unknown Result]";
                tbMessage.Text = string.Empty;
                scDetails.Visible = false;
                return;
            }

            scDetails.Visible = true;

            switch (m_executionResult.Type)
            {
                case ScriptExecutionResultType.InternalError:
                    gbResult.Text = "Internal Error";
                    break;
                case ScriptExecutionResultType.CompilationError:
                    {
                        gbResult.Text = "Compilation Error";
                        tewGeneratedCode.InnerEditor.TextArea.TextView.LineTransformers.Add(
                            new ErrorColorizer(m_executionResult.CompilerErrors, 0));
                        if (m_executionResult.SourceCodeLineOffset.HasValue)
                        {
                            tewSourceCode.InnerEditor.TextArea.TextView.LineTransformers.Add(
                                new ErrorColorizer(
                                    m_executionResult.CompilerErrors,
                                    m_executionResult.SourceCodeLineOffset.Value));
                        }
                    }
                    break;
                case ScriptExecutionResultType.ExecutionError:
                    gbResult.Text = "Execution Error";
                    break;
                case ScriptExecutionResultType.Success:
                    gbResult.Text = "Success";
                    break;
                default:
                    throw new NotImplementedException();
            }

            var hasReturnValue = m_executionResult.IsSuccess; // && !m_executionResult.ReturnValue.IsNull();

            SetTabPageVisibility(tpReturnValue, hasReturnValue);
            pgReturnValue.PropertySort = PropertySort.NoSort;
            pgReturnValue.SelectedObject = hasReturnValue ? m_executionResult.ReturnValue : null;

            rtbConsoleOut.Text = m_executionResult.ConsoleOut;
            SetTabPageVisibility(tpConsoleOut, !string.IsNullOrEmpty(m_executionResult.ConsoleOut));

            rtbConsoleError.Text = m_executionResult.ConsoleError;
            SetTabPageVisibility(tpConsoleError, !string.IsNullOrEmpty(m_executionResult.ConsoleError));

            tbMessage.Text = string.Format(
                "{0}{1}{2}",
                m_executionResult.Message,
                Environment.NewLine,
                string.Join(
                    Environment.NewLine,
                    m_executionResult.CompilerErrors.Select(item => FormatCompilerError(item, true))));
            scDetails.Panel1Collapsed = m_executionResult.IsSuccess;
            tewSourceCode.InnerEditor.Text = m_executionResult.SourceCode;
            tewGeneratedCode.InnerEditor.Text = m_executionResult.GeneratedCode;
        }

        #endregion

        #region Public Properties

        public ScriptExecutionResult ExecutionResult
        {
            [DebuggerStepThrough]
            get { return m_executionResult; }
            private set
            {
                m_executionResult = value;
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
                throw new ArgumentNullException("owner");
            }
            if (executionResult == null)
            {
                throw new ArgumentNullException("executionResult");
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

        #region Event Handlers

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CodeEditor_MouseHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (m_executionResult.Type != ScriptExecutionResultType.CompilationError)
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
            if (editor == tewSourceCode.InnerEditor)
            {
                if (!m_executionResult.SourceCodeLineOffset.HasValue)
                {
                    return;
                }
                lineOffset = m_executionResult.SourceCodeLineOffset.Value;
                isSourceCodeEditor = true;
            }

            var position = editor.GetPositionFromPoint(e.GetPosition(editor));
            if (!position.HasValue)
            {
                return;
            }

            var compiledLineNumber = position.Value.Line + lineOffset;
            var totalLineCount = editor.LineCount;

            var errors = m_executionResult
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

            m_errorToolTip.PlacementTarget = editor;
            m_errorToolTip.Content = errorTooltip;
            m_errorToolTip.IsOpen = true;
        }

        private void CodeEditor_MouseHoverStopped(object sender, System.Windows.Input.MouseEventArgs e)
        {
            m_errorToolTip.IsOpen = false;
        }

        #endregion
    }
}