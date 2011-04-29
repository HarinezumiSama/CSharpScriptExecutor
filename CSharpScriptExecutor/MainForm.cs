using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSharpScriptExecutor.Properties;

namespace CSharpScriptExecutor
{
    public partial class MainForm : Form
    {
        #region Fields

        private Size m_lastScriptFormSize;
        private string m_lastScriptFormScript;

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            this.Text = Program.ProgramName;
            this.Icon = Resources.MainIcon;

            niTrayIcon.Icon = this.Icon;
            niTrayIcon.Text = Program.ProgramName;
            niTrayIcon.Visible = true;
        }

        #endregion

        #region Private Methods

        private void DoShow()
        {
            this.Visible = true;
            this.ShowInTaskbar = true;
            tsmiAbout.Visible = false;
        }

        private void DoHide()
        {
            this.ShowInTaskbar = false;
            this.Visible = false;
            tsmiAbout.Visible = true;
        }

        private void DoRun()
        {
            var workingArea = Screen.FromControl(this).WorkingArea;
            using (var scriptForm = new ScriptForm())
            {
                scriptForm.Icon = this.Icon;

                scriptForm.Size = m_lastScriptFormSize.IsEmpty
                    ? new Size(workingArea.Width / 2, workingArea.Height / 2)
                    : m_lastScriptFormSize;
                scriptForm.Location = new Point(
                    workingArea.Width - scriptForm.Size.Width,
                    workingArea.Height - scriptForm.Size.Height);
                scriptForm.Script = m_lastScriptFormScript;

                var oldRunEnabled = tsmiRun.Enabled;
                var oldAboutEnabled = tsmiAbout.Enabled;
                try
                {
                    tsmiRun.Enabled = false;
                    tsmiAbout.Enabled = false;

                    scriptForm.ShowDialog(this);
                }
                finally
                {
                    tsmiRun.Enabled = oldRunEnabled;
                    tsmiAbout.Enabled = oldAboutEnabled;
                }

                m_lastScriptFormSize = scriptForm.Size;
                m_lastScriptFormScript = scriptForm.Script;
            }
        }

        [DebuggerStepThrough]
        private void DoAttachDebuggerNow()
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            labAbout.Text = Program.FullProgramName;
            DoHide();
        }

        #endregion

        #region Event Handlers

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            DoShow();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DoHide();
        }

        private void niTrayIcon_DoubleClick(object sender, EventArgs e)
        {
            DoRun();
        }

        private void tsmiRun_Click(object sender, EventArgs e)
        {
            DoRun();
        }

        [DebuggerStepThrough]
        private void tsmiAttachDebuggerNow_Click(object sender, EventArgs e)
        {
            DoAttachDebuggerNow();
        }

        private void cmsTrayIconMenu_Opening(object sender, CancelEventArgs e)
        {
            tsmiAttachDebuggerNow.Enabled = !Debugger.IsAttached;
        }

        #endregion
    }
}