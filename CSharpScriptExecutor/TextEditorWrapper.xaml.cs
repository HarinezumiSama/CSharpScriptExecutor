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

        public TextEditor InnerEditor => InnerEditorValue;

        private void InnerEditorValue_KeyDown(object sender, KeyEventArgs e) => OnKeyDown(e);
    }
}