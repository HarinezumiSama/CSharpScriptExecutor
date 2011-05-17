using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
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

    // TODO: `Auto-run if successful compilation` check box
    // TODO: Highlighting error 'as you type'

    // TODO: Script history
    // TODO: File | Open and File | Save As...

    // TODO: Shortcuts for often used texts such as Console.WriteLine and so on

    // TODO: Allow script to return a value, and then parse this value properties and/or fields and show them in UI

    public partial class ScriptForm : Form
    {
        #region Fields

        private ScriptExecutionResult m_executionResult;

        #endregion

        #region Constructors

        public ScriptForm()
        {
            InitializeComponent();

            this.Text = string.Format("Script — {0}", Program.ProgramName);
            //tewTextEditor.KeyDown += this.tewTextEditor_KeyDown;
            pbResult.Visible = false;
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

        private void SetControlStates()
        {
            var canRun = !string.IsNullOrWhiteSpace(this.Script);
            btnExecute.Enabled = canRun;
            btnDebug.Enabled = canRun;
            tsmiExecute.Enabled = canRun;
            tsmiDebug.Enabled = canRun;
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

        #endregion

        private void tsmiShowResult_Click(object sender, EventArgs e)
        {

        }
    }
}