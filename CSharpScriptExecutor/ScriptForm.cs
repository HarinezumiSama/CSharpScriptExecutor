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

namespace CSharpScriptExecutor
{
    // TODO: Use freeware open source C# code editor instead of RichTextBox

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

        private static string GetErrorMessage(ScriptExecutionResult executionResult)
        {
            #region Argument Check

            if (executionResult == null)
            {
                throw new ArgumentNullException("executionResult");
            }

            #endregion

            switch (executionResult.Type)
            {
                case ScriptExecutionResultType.InternalError:
                    return "Internal error: " + executionResult.Message;
                case ScriptExecutionResultType.CompileError:
                    return "Compilation error: " + executionResult.Message;
                case ScriptExecutionResultType.ExecutionError:
                    return "Execution error: " + executionResult.Message;
                case ScriptExecutionResultType.Success:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        private void ExecuteScript(bool enableDebugging)
        {
            string errorMessage;

            bool? success = null;
            Cursor oldCursor = Cursor.Current;
            var oldResult = pbResult.Image;
            var resultTag = pbResult.Tag;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                pbResult.Image = Resources.Wait;
                pbResult.Tag = null;
                Application.DoEvents();

                var script = this.Script;
                if (string.IsNullOrWhiteSpace(script))
                {
                    //ShowError("No script entered.");
                    return;
                }

                var parameters = new ScriptExecutorParameters(script, new string[0], enableDebugging);

                var executor = ScriptExecutorProxy.Create(parameters);
                var executionResult = executor.Execute();

                rtbConsoleOut.Text = executionResult.ConsoleOut;
                rtbConsoleError.Text = executionResult.ConsoleError;

                if (!string.IsNullOrEmpty(executionResult.ConsoleOut)
                    || !string.IsNullOrEmpty(executionResult.ConsoleError))
                {
                    ShowConsole(true);
                }

                errorMessage = GetErrorMessage(executionResult);

                success = errorMessage == null;
                resultTag = executionResult;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                success = false;
            }
            finally
            {
                pbResult.Image = success.HasValue
                    ? (success.Value ? Resources.OKShield_32x32 : Resources.ErrorCircle_32x32)
                    : oldResult;
                pbResult.Tag = resultTag;
                Cursor.Current = oldCursor;
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                return;
            }

            // TODO: use more convenient dialog box (with vertical scrolling and maybe formatting)
            ShowError(errorMessage);
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
            rtbScript.Select();
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
        }

        #endregion

        #region Public Properties

        public string Script
        {
            [DebuggerNonUserCode]
            get { return rtbScript.Text; }
            [DebuggerNonUserCode]
            set { rtbScript.Text = value; }
        }

        #endregion

        #region Event Handlers

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseForm();
        }

        private void ScriptForm_Shown(object sender, EventArgs e)
        {
            rtbScript.SelectAll();
            rtbScript.Focus();

            SetControlStates();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            ExecuteScript(false);
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            ExecuteScript(true);
        }

        private void rtbScript_TextChanged(object sender, EventArgs e)
        {
            var selectionStart = rtbScript.SelectionStart;
            var selectionLength = rtbScript.SelectionLength;

            try
            {
                rtbScript.SelectAll();
                rtbScript.SelectionBackColor = rtbScript.BackColor;
                rtbScript.SelectionColor = rtbScript.ForeColor;
                rtbScript.Font = rtbScript.Font;
            }
            finally
            {
                rtbScript.SelectionStart = selectionStart;
                rtbScript.SelectionLength = selectionLength;
            }

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
            var executionResult = pbResult.Tag as ScriptExecutionResult;
            if (executionResult == null)
            {
                return;
            }

            var errorMessage = GetErrorMessage(executionResult);
            if (string.IsNullOrEmpty(errorMessage))
            {
                return;
            }

            ShowError(errorMessage);
        }

        #endregion
    }
}