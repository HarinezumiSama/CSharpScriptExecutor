using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using CSharpScriptExecutor.Common;
using CSharpScriptExecutor.Properties;
using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace CSharpScriptExecutor
{
    //// TODO: Insert new directive(s) along with existing ones or at the very beginning of document

    //// TODO: Add/delete assembly reference via GUI (auto-change directives in the code)

    //// TODO: Highlighting error 'as you type'

    //// TODO: Fix drag and drop (*.cs and *.cssx) under Windows 7 (and probably under Vista as well)

    public partial class ScriptForm : Form
    {
        #region Constants and Fields

        private const int MaxHistoryItemGuiLength = 100;

        private static readonly Regex NewLineRegex = new Regex(
            @"(\r\n)+ | (\r)+ | (\n)+",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        private static readonly HashSet<string> AllowedDropExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ScriptExecutor.ScriptFileExtension,
                ScriptExecutor.SourceFileExtension
            };

        private static readonly string InvalidDroppedFileExtensionFormat =
            "Invalid file has been dragged-and-dropped: \"{0}\".\n" + "\n"
                + $"The following files are only supported: {string.Join(", ", AllowedDropExtensions.Select(item => "*" + item))}.";

        private static readonly string LastStartedScriptFilePath = Path.Combine(
            Program.ProgramDataPath,
            "~LastStartedScript" + ScriptExecutor.ScriptFileExtension);

        private readonly List<ToolStripMenuItem> _historyMenuItems = new List<ToolStripMenuItem>();
        private IScriptExecutor _scriptExecutor;
        private ScriptExecutionResult _executionResult;

        #endregion

        #region Constructors

        public ScriptForm()
        {
            InitializeComponent();

            ////if (Environment.OSVersion.Version >= new Version(6, 1))
            ////{
            ////    WinApi
            ////        .ChangeWindowMessageFilterEx(
            ////            this.Handle,
            ////            WinApi.Messages.WM_DROPFILES,
            ////            WinApi.ChangeWindowMessageFilterExAction.Allow)
            ////        .AssertWinApiResult();
            ////    WinApi
            ////        .ChangeWindowMessageFilterEx(
            ////            this.Handle,
            ////            WinApi.Messages.WM_COPYDATA,
            ////            WinApi.ChangeWindowMessageFilterExAction.Allow)
            ////        .AssertWinApiResult();
            ////    WinApi
            ////        .ChangeWindowMessageFilterEx(
            ////            this.Handle,
            ////            WinApi.Messages.WM_DropFilesInternalRelated,
            ////            WinApi.ChangeWindowMessageFilterExAction.Allow)
            ////        .AssertWinApiResult();
            ////}

            Text = $@"Script — {Program.ProgramName}{(IsAdministrator() ? " [Administrator]" : string.Empty)}";

            TextEditorWrapper.KeyDown += TextEditorWrapper_KeyDown;
            TextEditorWrapper.AllowDrop = true;
            TextEditorWrapper.DragEnter += TextEditorWrapper_DragEnter;
            TextEditorWrapper.Drop += TextEditorWrapper_Drop;

            ResultPictureBox.Visible = false;

            OpenScriptDialog.CustomPlaces.Add(Program.ProgramDataPath);
            SaveScriptDialog.CustomPlaces.Add(Program.ProgramDataPath);
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

        public string Script
        {
            [DebuggerNonUserCode]
            get
            {
                return TextEditorWrapper.InnerEditor.Text;
            }

            [DebuggerNonUserCode]
            set
            {
                TextEditorWrapper.InnerEditor.Text = value;
            }
        }

        #endregion

        #region Public Methods

        public static bool IsAdministrator()
            => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        #endregion

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();

                if (_scriptExecutor != null)
                {
                    _scriptExecutor.Dispose();
                    _scriptExecutor = null;
                }

                ClearHistory(false);
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DialogResult = DialogResult.None;
            TextEditorWrapper.InnerEditor.TextChanged += TextEditorWrapper_InnerEditor_TextChanged;
        }

        #endregion

        #region Private Methods: General

        private string GetActualCaption(string caption) => string.IsNullOrWhiteSpace(caption) ? Text : caption;

        private void CloseForm(bool exit)
        {
            DialogResult = exit ? DialogResult.Abort : DialogResult.Cancel;
            Close();
        }

        private void ShowError(string errorMessage, string caption = null)
        {
            var actualCaption = GetActualCaption(caption);
            MessageBox.Show(this, errorMessage, actualCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private DialogResult AskQuestion(string message, string caption = null)
        {
            var actualCaption = GetActualCaption(caption);
            return MessageBox.Show(this, message, actualCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private void ShowExecutionResult(ScriptExecutionResult executionResult)
        {
            if (executionResult == null)
            {
                return;
            }

            ExecutionResultViewerForm.Show(this, executionResult);
        }

        private void ExecuteScript(bool enableDebugging)
        {
            var script = Script;
            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            ScriptExecutionResult executionResult = null;

            var oldCursor = Cursor.Current;
            var oldResultImage = ResultPictureBox.Image;
            var oldExecutionResult = _executionResult;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ResultPictureBox.Image = Resources.Wait;
                _executionResult = null;
                Application.DoEvents();

                AddToHistory(script);
                LocalHelper.TrySaveScript(LastStartedScriptFilePath, script);

                var commandLine = CommandLineTextBox.Text;
                var commandLineParameters = InternalHelper.ParseCommandLineParameters(commandLine);

                var parameters = new ScriptExecutorParameters(script, commandLineParameters, enableDebugging);

                try
                {
                    if (_scriptExecutor != null)
                    {
                        _scriptExecutor.Dispose();
                        _scriptExecutor = null;
                    }

                    _scriptExecutor = ScriptExecutor.Create(parameters);
                    executionResult = _scriptExecutor.Execute();
                }
                catch (Exception ex)
                {
                    executionResult = ScriptExecutionResult.CreateInternalError(ex, script);
                }
            }
            catch (Exception ex)
            {
                executionResult = ScriptExecutionResult.CreateInternalError(ex, script);
            }
            finally
            {
                ResultPictureBox.Image = executionResult == null
                    ? oldResultImage
                    : executionResult.IsSuccess ? Resources.OKShield_32x32 : Resources.ErrorCircle_32x32;
                _executionResult = executionResult ?? oldExecutionResult;
                SetControlStates();
                ResultPictureBox.Show();

                Cursor.Current = oldCursor;
            }

            if (executionResult == null)
            {
                return;
            }

            if (!executionResult.IsSuccess || !executionResult.ReturnValue.IsNull()
                || !string.IsNullOrEmpty(executionResult.ConsoleOut)
                || !string.IsNullOrEmpty(executionResult.ConsoleError))
            {
                ShowExecutionResult(executionResult);
            }
        }

        private void SetHistoryRelatedControlStates()
        {
            var hasHistory = _historyMenuItems.Any();
            ClearHistoryMenuItem.Enabled = hasHistory;
            ClearHistoryMenuItemSeparator.Visible = hasHistory;
        }

        private void ReassignHistoryItemShortcuts()
        {
            // Firstly, clearing all shortcuts in history items in order to avoid conflicts
            foreach (var historyMenuItem in _historyMenuItems)
            {
                historyMenuItem.ShortcutKeys = Keys.None;
            }

            for (int index = _historyMenuItems.Count - 1, keyIndex = 1;
                index >= 0 && keyIndex <= 10;
                index--, keyIndex++)
            {
                _historyMenuItems[index].ShortcutKeys = Keys.Control | (Keys.D0 + keyIndex % 10);
            }
        }

        private void AddToHistory(string script)
        {
            var guiScript = NewLineRegex.Replace(script, " ");
            if (guiScript.Length > MaxHistoryItemGuiLength)
            {
                guiScript = guiScript.Substring(0, MaxHistoryItemGuiLength);
            }

            var historyMenuItem = new ToolStripMenuItem(guiScript) { Tag = script, ToolTipText = script };
            historyMenuItem.Click += HistoryMenuItem_Click;

            _historyMenuItems.Add(historyMenuItem);
            HistoryMenuItem.DropDownItems.Insert(0, historyMenuItem);

            ReassignHistoryItemShortcuts();
            SetHistoryRelatedControlStates();
        }

        private void ClearHistory(bool ask)
        {
            if (!_historyMenuItems.Any())
            {
                return;
            }

            if (ask)
            {
                var answer = AskQuestion("Are you sure you wish to clear the script history?");
                if (answer != DialogResult.Yes)
                {
                    return;
                }
            }

            foreach (var historyMenuItem in _historyMenuItems)
            {
                HistoryMenuItem.DropDownItems.Remove(historyMenuItem);
                historyMenuItem.Dispose();
            }

            _historyMenuItems.Clear();

            SetHistoryRelatedControlStates();
        }

        private void SetTextFromHistory(string script)
        {
            if (string.IsNullOrEmpty(script))
            {
                return;
            }

            InsertTextInEditor(script, true);
        }

        private void SetControlStates()
        {
            var canRun = !string.IsNullOrWhiteSpace(Script);
            ExecuteButton.Enabled = canRun;
            DebugButton.Enabled = canRun;
            ExecuteMenuItem.Enabled = canRun;
            DebugMenuItem.Enabled = canRun;

            ShowResultMenuItem.Enabled = _executionResult != null;
        }

        private void CollapseSelection()
        {
            var editor = TextEditorWrapper.InnerEditor;
            editor.Select(editor.SelectionStart + editor.SelectionLength, 0);
        }

        private void InsertTextInEditor(string value, bool replaceAll = false)
        {
            var editor = TextEditorWrapper.InnerEditor;

            editor.BeginChange();
            try
            {
                if (replaceAll)
                {
                    editor.Document.Text = string.Empty;
                }

                editor.SelectedText = value;
                CollapseSelection();
            }
            finally
            {
                editor.EndChange();
            }
        }

        private bool PerformDragDrop(object droppedObjectData, bool isDrop)
        {
            var paths = droppedObjectData as IEnumerable<string>;
            if (paths == null)
            {
                return false;
            }

            var filePaths = paths.ToArray();
            if (filePaths.Length != 1)
            {
                if (isDrop)
                {
                    ShowError(
                        $@"Only a single file can be dragged-and-dropped while {filePaths.Length} files have been done.");
                }

                return false;
            }

            var filePath = filePaths.Single();
            var extension = Path.GetExtension(filePath);
            if (!AllowedDropExtensions.Contains(extension))
            {
                if (isDrop)
                {
                    ShowError(string.Format(InvalidDroppedFileExtensionFormat, filePath));
                }

                return false;
            }

            if (isDrop)
            {
                var script = File.ReadAllText(filePath);
                InsertTextInEditor(script, true);
            }

            return true;
        }

        private void QueryWindowDragDrop(DragEventArgs e, bool isDrop)
        {
            e.Effect = DragDropEffects.None;
            if ((e.AllowedEffect & DragDropEffects.Copy) == 0)
            {
                return;
            }

            var present = e.Data.GetDataPresent(DataFormats.FileDrop, true);
            if (!present)
            {
                return;
            }

            var data = e.Data.GetData(DataFormats.FileDrop, true);
            if (PerformDragDrop(data, isDrop))
            {
                return;
            }

            e.Effect = DragDropEffects.Copy;
        }

        private void QueryEditorDragDrop(System.Windows.DragEventArgs e, bool isDrop)
        {
            e.Effects = System.Windows.DragDropEffects.None;
            e.Handled = true;
            if ((e.AllowedEffects & System.Windows.DragDropEffects.Copy) == 0)
            {
                return;
            }

            var present = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true);
            if (!present)
            {
                return;
            }

            var data = e.Data.GetData(System.Windows.DataFormats.FileDrop, true);
            if (PerformDragDrop(data, isDrop))
            {
                return;
            }

            e.Effects = System.Windows.DragDropEffects.Copy;
        }

        #endregion

        #region Private Methods: Event Handlers

        private void CloseButton_Click(object sender, EventArgs e) => CloseForm(false);

        private void ScriptForm_Shown(object sender, EventArgs e)
        {
            TextEditorWrapper.InnerEditor.SelectAll();
            TextEditorWrapper.InnerEditor.Focus();

            SetControlStates();
            SetHistoryRelatedControlStates();
            Activate();

            // If the caller did set non-default cursor, resetting it
            Cursor.Current = Cursors.Default;
        }

        private void ExecuteButton_Click(object sender, EventArgs e) => ExecuteScript(false);

        private void DebugButton_Click(object sender, EventArgs e) => ExecuteScript(true);

        private void TextEditorWrapper_InnerEditor_TextChanged(object sender, EventArgs e) => SetControlStates();

        private void CloseMenuItem_Click(object sender, EventArgs e) => CloseForm(false);

        private void ExitMenuItem_Click(object sender, EventArgs e) => CloseForm(true);

        private void ExecuteMenuItem_Click(object sender, EventArgs e) => ExecuteScript(false);

        private void DebugMenuItem_Click(object sender, EventArgs e) => ExecuteScript(true);

        private void ResultPictureBox_Click(object sender, EventArgs e) => ShowExecutionResult(_executionResult);

        private void ScriptForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Escape | Keys.Shift))
            {
                e.Handled = true;
                CloseForm(false);
            }
        }

        private void TextEditorWrapper_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape && e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Shift)
            {
                e.Handled = true;
                CloseForm(false);
            }
        }

        private void ShowResultMenuItem_Click(object sender, EventArgs e) => ShowExecutionResult(_executionResult);

        private void InsertReturnMenuItem_Click(object sender, EventArgs e) => InsertTextInEditor("return ");

        private void InsertConsoleWriteLineMenuItem_Click(object sender, EventArgs e)
            => InsertTextInEditor("Console.WriteLine");

        private void InsertReferenceDirectiveMenuItem_Click(object sender, EventArgs e)
            => InsertTextInEditor("//##REF ");

        private void InsertUsingDirectiveMenuItem_Click(object sender, EventArgs e)
            => InsertTextInEditor("//##USING ");

        private void ClearHistoryMenuItem_Click(object sender, EventArgs e) => ClearHistory(true);

        private void HistoryMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;

            var script = item?.Tag as string;
            if (string.IsNullOrEmpty(script))
            {
                return;
            }

            SetTextFromHistory(script);
        }

        private void TextEditorWrapper_DragEnter(object sender, System.Windows.DragEventArgs e)
            => QueryEditorDragDrop(e, false);

        private void TextEditorWrapper_Drop(object sender, System.Windows.DragEventArgs e)
            => QueryEditorDragDrop(e, true);

        private void ScriptForm_DragEnter(object sender, DragEventArgs e) => QueryWindowDragDrop(e, false);

        private void ScriptForm_DragDrop(object sender, DragEventArgs e) => QueryWindowDragDrop(e, true);

        private void SaveAsMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveScriptDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var filePath = SaveScriptDialog.FileName;
            try
            {
                File.WriteAllText(filePath, Script, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ShowError($"Cannot save script to the file \"{filePath}\":\n" + $"{ex.Message}");
            }
        }

        private void OpenScriptMenuItem_Click(object sender, EventArgs e)
        {
            if (OpenScriptDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var filePath = OpenScriptDialog.FileName;
            try
            {
                var script = File.ReadAllText(filePath);
                Script = script;
            }
            catch (Exception ex)
            {
                ShowError($"Cannot open a script from \"{filePath}\":\n" + $"{ex.Message}");
            }
        }

        #endregion
    }
}