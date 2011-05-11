﻿using System;
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

        private string FormatCompilerError(
            CompilerError compilerError,
            bool indent,
            TextEditor sourceCodeEditor = null,
            int lineOffset = 0)
        {
            return string.Format(
                "{0}Script({1},{2}): error {3}: {4}",
                indent ? "    " : string.Empty,
                sourceCodeEditor != null
                    ? Math.Min(compilerError.Line - lineOffset, sourceCodeEditor.LineCount)
                    : compilerError.Line,
                compilerError.Column,
                compilerError.ErrorNumber,
                compilerError.ErrorText);
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

            var errors = m_executionResult
                .CompilerErrors
                .Where(item => item.Line == compiledLineNumber || item.Line > editor.LineCount)
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
                item => FormatCompilerError(item, false, isSourceCodeEditor ? editor : null, lineOffset)));

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