using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSharpScriptExecutor.Common;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.CSharp;

namespace CSharpScriptExecutor
{
    public partial class TextEditorWrapper : UserControl
    {
        #region Constructors

        public TextEditorWrapper()
        {
            InitializeComponent();

            innerEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(
                ScriptExecutor.SourceFileExtension);
        }

        #endregion

        #region Public Properties

        public TextEditor InnerEditor
        {
            [DebuggerStepThrough]
            get { return innerEditor; }
        }

        #endregion
    }
}
