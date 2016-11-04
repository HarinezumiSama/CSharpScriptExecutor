using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using CSharpScriptExecutor.Common;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace CSharpScriptExecutor
{
    public partial class TextEditorWrapper
    {
        #region Constructors

        public TextEditorWrapper()
        {
            InitializeComponent();

            InnerEditorValue.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(
                ScriptExecutor.SourceFileExtension);

            InnerEditorValue.Options.ConvertTabsToSpaces = true;
            InnerEditorValue.Options.CutCopyWholeLine = true;
            InnerEditorValue.Options.IndentationSize = 4;
            InnerEditorValue.ShowLineNumbers = true;
        }

        #endregion

        #region Public Properties

        public TextEditor InnerEditor
        {
            [DebuggerStepThrough]
            get
            {
                return InnerEditorValue;
            }
        }

        #endregion

        #region Event Handlers

        private void InnerEditorValue_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        #endregion
    }
}