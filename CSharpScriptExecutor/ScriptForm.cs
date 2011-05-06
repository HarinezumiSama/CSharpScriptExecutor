using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSharpScriptExecutor.Common;
using CSharpScriptExecutor.Properties;
using ICSharpCode.AvalonEdit;

namespace CSharpScriptExecutor
{
    // TODO: Make possible to enter arguments from GUI

    // TODO: `Auto-run if successful compilation` check box

    // TODO: Script history

    public partial class ScriptForm : Form
    {
        #region Constructors

        public ScriptForm()
        {
            InitializeComponent();

            this.Text = string.Format("Script — {0}", Program.ProgramName);
            scPanels.Panel2Collapsed = true;
        }

        #endregion

        #region Private Methods

        private void CloseForm()
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ShowError(string errorMessage)
        {
            MessageBox.Show(this, errorMessage, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

            ScriptExecutionResult executionResult = null;

            Cursor oldCursor = Cursor.Current;
            var oldResultImage = pbResult.Image;
            var oldResultTag = pbResult.Tag;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                pbResult.Image = Resources.Wait;
                pbResult.Tag = null;
                Application.DoEvents();

                var parameters = new ScriptExecutorParameters(script, Enumerable.Empty<string>(), enableDebugging);

                try
                {
                    using (var executor = ScriptExecutor.Create(parameters))
                    {
                        executionResult = executor.Execute();
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

                rtbConsoleOut.Text = executionResult.ConsoleOut;
                rtbConsoleError.Text = executionResult.ConsoleError;

                if (!string.IsNullOrEmpty(executionResult.ConsoleOut)
                    || !string.IsNullOrEmpty(executionResult.ConsoleError))
                {
                    ShowConsole(true);
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
                pbResult.Tag = executionResult ?? oldResultTag;

                Cursor.Current = oldCursor;
            }

            if (executionResult != null && executionResult.IsSuccess)
            {
                return;
            }

            ShowExecutionResult(executionResult);
        }

        private void SetControlStates()
        {
            var canRun = !string.IsNullOrWhiteSpace(this.Script);
            btnExecute.Enabled = canRun;
            btnDebug.Enabled = canRun;
            tsmiExecute.Enabled = canRun;
            tsmiDebug.Enabled = canRun;
        }

        private void ShowConsole(bool autoSelectTab = false)
        {
            scPanels.Panel2Collapsed = false;

            if (autoSelectTab)
            {
                if (!string.IsNullOrEmpty(rtbConsoleOut.Text))
                {
                    tcConsole.SelectedTab = tpConsoleOut;
                }
                else if (!string.IsNullOrEmpty(rtbConsoleError.Text))
                {
                    tcConsole.SelectedTab = tpConsoleError;
                }
            }
        }

        private void HideConsole()
        {
            scPanels.Panel2Collapsed = true;
            tewTextEditor.InnerEditor.Focus();
        }

        private void ToggleConsole()
        {
            if (scPanels.Panel2Collapsed)
            {
                ShowConsole();
            }
            else
            {
                HideConsole();
            }
        }

        #endregion

        #region Protected Methods

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

            // If the caller sets non-default cursor, resetting it
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

        private void tsmiScriptConsole_Click(object sender, EventArgs e)
        {
            ToggleConsole();
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
            ShowExecutionResult(pbResult.Tag as ScriptExecutionResult);
        }

        #endregion
    }
}