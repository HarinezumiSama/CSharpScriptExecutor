using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using CSharpScriptExecutor.Common;
using CSharpScriptExecutor.Properties;
using ICSharpCode.AvalonEdit;
using Cursor = System.Windows.Forms.Cursor;
using Cursors = System.Windows.Forms.Cursors;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;

namespace CSharpScriptExecutor
{
    // TODO: Make possible to enter arguments from GUI

    // TODO: Highlighting error 'as you type'

    // TODO: File | Open and File | Save As...
    // TODO: Fix drag and drop (*.cs and *.cssx) under Windows 7 (and probably under Vista as well)

    public partial class ScriptForm : Form
    {
        #region Constants

        private const int c_maxHistoryItemGuiLength = 100;

        #endregion

        #region Fields

        private static readonly Regex s_newLineRegex = new Regex(
            @"(\r\n)+ | (\r)+ | (\n)+",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

        private static readonly HashSet<string> s_allowedDropExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ScriptExecutor.ScriptFileExtension,
                ScriptExecutor.SourceFileExtension
            };

        private static readonly string s_invalidDroppedFileExtensionFormat = string.Format(
            "Invalid file has been dragged-and-dropped: \"{{0}}\".\n"
                + "\n"
                + "The following files are only supported: {0}.",
            string.Join(", ", s_allowedDropExtensions.Select(item => "*" + item)));

        private IScriptExecutor m_scriptExecutor;
        private ScriptExecutionResult m_executionResult;
        private readonly List<ToolStripMenuItem> m_historyMenuItems = new List<ToolStripMenuItem>();

        #endregion

        #region Constructors

        public ScriptForm()
        {
            InitializeComponent();

            //if (Environment.OSVersion.Version >= new Version(6, 1))
            //{
            //    WinApi
            //        .ChangeWindowMessageFilterEx(
            //            this.Handle,
            //            WinApi.Messages.WM_DROPFILES,
            //            WinApi.ChangeWindowMessageFilterExAction.Allow)
            //        .AssertWinApiResult();
            //    WinApi
            //        .ChangeWindowMessageFilterEx(
            //            this.Handle,
            //            WinApi.Messages.WM_COPYDATA,
            //            WinApi.ChangeWindowMessageFilterExAction.Allow)
            //        .AssertWinApiResult();
            //    WinApi
            //        .ChangeWindowMessageFilterEx(
            //            this.Handle,
            //            WinApi.Messages.WM_DropFilesInternalRelated,
            //            WinApi.ChangeWindowMessageFilterExAction.Allow)
            //        .AssertWinApiResult();
            //}

            this.Text = string.Format("Script — {0}", Program.ProgramName);

            tewTextEditor.KeyDown += this.tewTextEditor_KeyDown;
            tewTextEditor.AllowDrop = true;
            tewTextEditor.DragEnter += this.tewTextEditor_DragEnter;
            tewTextEditor.Drop += this.tewTextEditor_Drop;

            pbResult.Visible = false;
        }

        #endregion

        #region Private Methods

        private string GetActualCaption(string caption)
        {
            return string.IsNullOrWhiteSpace(caption) ? this.Text : caption;
        }

        private void CloseForm()
        {
            this.DialogResult = DialogResult.Cancel;
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
            var script = this.Script;
            if (string.IsNullOrWhiteSpace(script))
            {
                return;
            }

            AddToHistory(script);

            ScriptExecutionResult executionResult = null;

            Cursor oldCursor = Cursor.Current;
            var oldResultImage = pbResult.Image;
            var oldExecutionResult = m_executionResult;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                pbResult.Image = Resources.Wait;
                m_executionResult = null;
                Application.DoEvents();

                var parameters = new ScriptExecutorParameters(script, Enumerable.Empty<string>(), enableDebugging);

                try
                {
                    if (m_scriptExecutor != null)
                    {
                        m_scriptExecutor.Dispose();
                        m_scriptExecutor = null;
                    }

                    m_scriptExecutor = ScriptExecutor.Create(parameters);
                    executionResult = m_scriptExecutor.Execute();
                }
                catch (Exception ex)
                {
                    executionResult = ScriptExecutionResult.CreateInternalError(
                        ex,
                        string.Empty,
                        string.Empty,
                        script,
                        null);
                }
            }
            catch (Exception ex)
            {
                executionResult = ScriptExecutionResult.CreateInternalError(
                    ex,
                    string.Empty,
                    string.Empty,
                    script,
                    null);
            }
            finally
            {
                pbResult.Image = executionResult == null
                    ? oldResultImage
                    : executionResult.IsSuccess ? Resources.OKShield_32x32 : Resources.ErrorCircle_32x32;
                m_executionResult = executionResult ?? oldExecutionResult;
                SetControlStates();
                pbResult.Show();

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
            var hasHistory = m_historyMenuItems.Any();
            tsmiClearHistory.Enabled = hasHistory;
            tssClearHistorySeparator.Visible = hasHistory;
        }

        private void ReassignHistoryItemShortcuts()
        {
            // Firstly, clearing all shortcuts in history items in order to avoid conflicts
            foreach (var historyMenuItem in m_historyMenuItems)
            {
                historyMenuItem.ShortcutKeys = Keys.None;
            }

            for (int index = m_historyMenuItems.Count - 1,
                keyIndex = 1; index >= 0 && keyIndex <= 10; index--,
                keyIndex++)
            {
                m_historyMenuItems[index].ShortcutKeys = Keys.Control | (Keys.D0 + (keyIndex % 10));
            }
        }

        private void AddToHistory(string script)
        {
            var guiScript = s_newLineRegex.Replace(script, " ");
            if (guiScript.Length > c_maxHistoryItemGuiLength)
            {
                guiScript = guiScript.Substring(0, c_maxHistoryItemGuiLength);
            }

            var historyMenuItem = new ToolStripMenuItem(guiScript) { Tag = script };
            historyMenuItem.Click += this.HistoryMenuItem_Click;

            m_historyMenuItems.Add(historyMenuItem);
            tsmiHistory.DropDownItems.Insert(0, historyMenuItem);

            ReassignHistoryItemShortcuts();
            SetHistoryRelatedControlStates();
        }

        private void ClearHistory(bool ask)
        {
            if (!m_historyMenuItems.Any())
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

            foreach (var historyMenuItem in m_historyMenuItems)
            {
                tsmiHistory.DropDownItems.Remove(historyMenuItem);
                historyMenuItem.Dispose();
            }
            m_historyMenuItems.Clear();

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
            var canRun = !string.IsNullOrWhiteSpace(this.Script);
            btnExecute.Enabled = canRun;
            btnDebug.Enabled = canRun;
            tsmiExecute.Enabled = canRun;
            tsmiDebug.Enabled = canRun;

            tsmiShowResult.Enabled = m_executionResult != null;
        }

        private void CollapseSelection()
        {
            var editor = tewTextEditor.InnerEditor;
            editor.Select(editor.SelectionStart + editor.SelectionLength, 0);
        }

        private void InsertTextInEditor(string value, bool replaceAll = false)
        {
            var editor = tewTextEditor.InnerEditor;

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
                        string.Format(
                            "Only a single file can be dragged-and-dropped while {0} files have been done.",
                            filePaths.Length));
                }
                return false;
            }

            var filePath = filePaths.Single();
            var extension = Path.GetExtension(filePath);
            if (!s_allowedDropExtensions.Contains(extension))
            {
                if (isDrop)
                {
                    ShowError(string.Format(s_invalidDroppedFileExtensionFormat, filePath));
                }
                return false;
            }

            if (isDrop)
            {
                string script = File.ReadAllText(filePath);
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

        #region Protected Methods

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (m_scriptExecutor != null)
                {
                    m_scriptExecutor.Dispose();
                    m_scriptExecutor = null;
                }

                ClearHistory(false);
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            tewTextEditor.InnerEditor.TextChanged += this.tewTextEditor_InnerEditor_TextChanged;
        }

        #endregion

        #region Public Properties

        public string Script
        {
            [DebuggerNonUserCode]
            get { return tewTextEditor.InnerEditor.Text; }
            [DebuggerNonUserCode]
            set { tewTextEditor.InnerEditor.Text = value; }
        }

        #endregion

        #region Event Handlers

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void ScriptForm_Shown(object sender, EventArgs e)
        {
            tewTextEditor.InnerEditor.SelectAll();
            tewTextEditor.InnerEditor.Focus();

            SetControlStates();
            SetHistoryRelatedControlStates();
            this.Activate();

            // If the caller did set non-default cursor, resetting it
            Cursor.Current = Cursors.Default;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            ExecuteScript(false);
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            ExecuteScript(true);
        }

        private void tewTextEditor_InnerEditor_TextChanged(object sender, EventArgs e)
        {
            SetControlStates();
        }

        private void tsmiClose_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void tsmiExecute_Click(object sender, EventArgs e)
        {
            ExecuteScript(false);
        }

        private void tsmiDebug_Click(object sender, EventArgs e)
        {
            ExecuteScript(true);
        }

        private void pbResult_Click(object sender, EventArgs e)
        {
            ShowExecutionResult(m_executionResult);
        }

        private void ScriptForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Escape | Keys.Shift))
            {
                e.Handled = true;
                CloseForm();
            }
        }

        private void tewTextEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape && e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Shift)
            {
                e.Handled = true;
                CloseForm();
            }
        }

        private void tsmiShowResult_Click(object sender, EventArgs e)
        {
            ShowExecutionResult(m_executionResult);
        }

        private void tsmiInsertReturn_Click(object sender, EventArgs e)
        {
            InsertTextInEditor("return ");
        }

        private void tsmiInsertConsoleWriteLine_Click(object sender, EventArgs e)
        {
            InsertTextInEditor("Console.WriteLine");
        }

        private void tsmiClearHistory_Click(object sender, EventArgs e)
        {
            ClearHistory(true);
        }

        private void HistoryMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null)
            {
                return;
            }

            var script = item.Tag as string;
            if (string.IsNullOrEmpty(script))
            {
                return;
            }

            SetTextFromHistory(script);
        }

        private void tewTextEditor_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            QueryEditorDragDrop(e, false);
        }

        private void tewTextEditor_Drop(object sender, System.Windows.DragEventArgs e)
        {
            QueryEditorDragDrop(e, true);
        }

        private void ScriptForm_DragEnter(object sender, DragEventArgs e)
        {
            QueryWindowDragDrop(e, false);
        }

        private void ScriptForm_DragDrop(object sender, DragEventArgs e)
        {
            QueryWindowDragDrop(e, true);
        }

        #endregion
    }
}