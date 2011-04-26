using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

        private void DoRun()
        {
            string errorMessage;

            try
            {
                var script = rtbScript.Text;
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

                var parameters = new ScriptExecutorParameters(script, new string[0], false);

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

            MessageBox.Show(this, errorMessage, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        #endregion

        #region Event Handlers

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            DoRun();
        }

        private void ScriptForm_Shown(object sender, EventArgs e)
        {
            rtbScript.Clear();
            rtbScript.Focus();
        }

        private void ScriptForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Control | Keys.Enter))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                DoRun();
                return;
            }
        }

        #endregion
    }
}