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

            #region Private Methods

            //private static System.Windows.Media.Pen CreateErrorPen()
            //{
            //    var geometry = new StreamGeometry();
            //    using (var context = geometry.Open())
            //    {
            //        context.BeginFigure(new System.Windows.Point(0.0, 0.0), false, false);
            //        context.PolyLineTo(
            //            new[]
            //            {
            //                new System.Windows.Point(0.75, 0.75),
            //                new System.Windows.Point(1.5, 0.0),
            //                new System.Windows.Point(2.25, 0.75),
            //                new System.Windows.Point(3.0, 0.0)
            //            },
            //            true,
            //            true);
            //    }

            //    var brushPattern = new GeometryDrawing
            //    {
            //        Pen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, 0.5d),
            //        Geometry = geometry
            //    };

            //    var brush = new DrawingBrush(brushPattern)
            //    {
            //        TileMode = TileMode.Tile,
            //        Viewport = new Rect(0.0, 1.5, 9.0, 3.0),
            //        ViewportUnits = BrushMappingMode.Absolute
            //    };

            //    var result = new System.Windows.Media.Pen(brush, 3.0);
            //    result.Freeze();

            //    return result;
            //}

            #endregion

            #region Protected Methods

            protected override void ColorizeLine(DocumentLine line)
            {
                if (line == null)
                {
                    return;
                }

                var errorBrush = new SolidColorBrush(Colors.Red);

                if (m_compilerErrors.Any(item => item.Line == line.LineNumber + m_lineOffset))
                {
                    ChangeLinePart(
                        line.Offset,
                        line.EndOffset,
                        item =>
                        {
                            item.TextRunProperties.SetBackgroundBrush(errorBrush);

                            //item.TextRunProperties.SetTextDecorations(
                            //    new TextDecorationCollection(
                            //        new[]
                            //        {
                            //            new TextDecoration()
                            //            {
                            //                Pen = s_errorPen,
                            //                PenThicknessUnit = TextDecorationUnit.FontRecommended
                            //            }
                            //        }));
                        });
                }
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
            tewSourceCode.InnerEditor.IsReadOnly = true;
            tewGeneratedCode.InnerEditor.IsReadOnly = true;

            tewSourceCode.InnerEditor.MouseHover += CodeEditor_MouseHover;
            tewSourceCode.InnerEditor.MouseHoverStopped += CodeEditor_MouseHoverStopped;

            tewGeneratedCode.InnerEditor.MouseHover += CodeEditor_MouseHover;
            tewGeneratedCode.InnerEditor.MouseHoverStopped += CodeEditor_MouseHoverStopped;
        }

        #endregion

        #region Private Methods

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

            tbMessage.Text = m_executionResult.Message;
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

            using (var form = new ExecutionResultViewerForm()
            {
                ExecutionResult = executionResult,
                Icon = owner.Icon
            })
            {
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

            var lineOffset = 0;
            if (editor == tewSourceCode.InnerEditor)
            {
                if (!m_executionResult.SourceCodeLineOffset.HasValue)
                {
                    return;
                }
                lineOffset = m_executionResult.SourceCodeLineOffset.Value;
            }

            var position = editor.GetPositionFromPoint(e.GetPosition(editor));
            if (!position.HasValue)
            {
                return;
            }

            var errors = m_executionResult
                .CompilerErrors
                .Where(item => item.Line == position.Value.Line + lineOffset)
                .ToList();
            if (!errors.Any())
            {
                return;
            }

            var errorTooltip = string.Join(
                Environment.NewLine,
                errors.Select(item => string.Format("Error {0}: {1}", item.ErrorNumber, item.ErrorText)));

            m_errorToolTip.PlacementTarget = editor;
            m_errorToolTip.Content = errorTooltip;
            m_errorToolTip.IsOpen = true;
            e.Handled = true;
        }

        private void CodeEditor_MouseHoverStopped(object sender, System.Windows.Input.MouseEventArgs e)
        {
            m_errorToolTip.IsOpen = false;
        }

        #endregion
    }
}