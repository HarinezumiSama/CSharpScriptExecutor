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

namespace CSharpScriptExecutor
{
    public partial class ScriptForm : Form
    {
        #region Constructors

        public ScriptForm()
        {
            InitializeComponent();

            this.Text = string.Format("Script — {0}", Program.ProgramName);
        }

        #endregion

        #region Private Methods

        private void DoCancel()
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void DoRun(bool enableDebugging)
        {
            string errorMessage;

            try
            {
                var script = this.Script;
                if (string.IsNullOrWhiteSpace(script))
                {
                    MessageBox.Show(
                        this,
                        "No script entered.",
                        this.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                var parameters = new ScriptExecutorParameters(script, new string[0], enableDebugging);

                var executor = ScriptExecutorProxy.Create(parameters);
                var executionResult = executor.Execute();

                switch (executionResult.Type)
                {
                    case ScriptExecutionResultType.InternalError:
                        errorMessage = "Internal error: " + executionResult.Message;
                        break;
                    case ScriptExecutionResultType.CompileError:
                        errorMessage = "Compilation error: " + executionResult.Message;
                        break;
                    case ScriptExecutionResultType.ExecutionError:
                        errorMessage = "Execution error: " + executionResult.Message;
                        break;
                    case ScriptExecutionResultType.Success:
                        errorMessage = null;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                this.DialogResult = DialogResult.OK;
                return;
            }

            // TODO: use more convenient dialog box (with vertical scrolling)
            MessageBox.Show(this, errorMessage, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void SetControlStates()
        {
            var canRun = !string.IsNullOrWhiteSpace(this.Script);
            btnExecute.Enabled = canRun;
            btnDebug.Enabled = canRun;
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DoCancel();
        }

        private void ScriptForm_Shown(object sender, EventArgs e)
        {
            rtbScript.SelectAll();
            rtbScript.Focus();

            SetControlStates();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            DoRun(false);
        }

        private void btnDebug_Click(object sender, EventArgs e)
        {
            DoRun(true);
        }

        private void ScriptForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.F5)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                DoRun(false);
                return;
            }

            if (e.KeyData == Keys.F8)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                DoRun(true);
                return;
            }

            //if (e.KeyData == Keys.Escape)
            //{
            //    e.Handled = true;
            //    e.SuppressKeyPress = true;
            //    DoCancel();
            //    return;
            //}
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

        #endregion
    }
}