using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CSharpScriptExecutor.Common;
using CSharpScriptExecutor.Properties;

namespace CSharpScriptExecutor
{
    public partial class MainForm : Form
    {
        #region Fields

        private static readonly string s_lastScriptFilePath = Path.Combine(
            Program.ProgramDataPath,
            "LastScript" + ScriptExecutor.ScriptFileExtension);

        private bool m_isScriptFormActive;
        private ScriptForm m_scriptForm;
        private Size m_lastScriptFormSize;
        private string m_lastScriptFormScript;

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            this.Text = Program.ProgramName;
            labAbout.Text = Program.FullProgramName;
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

        private void ShowScriptForm()
        {
            if (m_isScriptFormActive)
            {
                if (m_scriptForm != null && m_scriptForm.Enabled)
                {
                    m_scriptForm.Activate();
                }
                return;
            }

            bool exit;
            var workingArea = Screen.FromControl(this).WorkingArea;

            Cursor oldCursor = Cursor.Current;
            var oldRunEnabled = tsmiRun.Enabled;
            var oldAboutEnabled = tsmiAbout.Enabled;
            try
            {
                m_isScriptFormActive = true;
                Cursor.Current = Cursors.AppStarting;
                tsmiRun.Enabled = false;
                tsmiAbout.Enabled = false;

                using (m_scriptForm = new ScriptForm())
                {
                    m_scriptForm.Icon = this.Icon;

                    m_scriptForm.Size = m_lastScriptFormSize.IsEmpty
                        ? new Size(workingArea.Width * 3 / 5, workingArea.Height * 3 / 5)
                        : m_lastScriptFormSize;
                    m_scriptForm.Location = new Point(
                        workingArea.Width - m_scriptForm.Size.Width,
                        workingArea.Height - m_scriptForm.Size.Height);
                    m_scriptForm.Script = m_lastScriptFormScript;

                    exit = m_scriptForm.ShowDialog(this) == DialogResult.Abort;
                    Cursor.Current = Cursors.AppStarting;

                    m_lastScriptFormSize = m_scriptForm.Size;
                    m_lastScriptFormScript = m_scriptForm.Script;
                }
            }
            finally
            {
                tsmiRun.Enabled = oldRunEnabled;
                tsmiAbout.Enabled = oldAboutEnabled;
                Cursor.Current = oldCursor;

                m_scriptForm = null;
                m_isScriptFormActive = false;
            }

            if (exit)
            {
                Close();
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

        private void TryLoadLastScript()
        {
            try
            {
                if (!File.Exists(s_lastScriptFilePath))
                {
                    return;
                }

                m_lastScriptFormScript = File.ReadAllText(s_lastScriptFilePath);
            }
            catch (Exception)
            {
                // Nothing to do
            }
        }

        private void TrySaveLastScript()
        {
            try
            {
                var directory = Path.GetDirectoryName(s_lastScriptFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var lastScriptFile = new FileInfo(s_lastScriptFilePath);
                if (lastScriptFile.Exists)
                {
                    lastScriptFile.Attributes = FileAttributes.Normal;
                }

                var lastScript = m_isScriptFormActive && m_scriptForm != null
                    ? m_scriptForm.Script
                    : m_lastScriptFormScript;

                if (string.IsNullOrWhiteSpace(lastScript))
                {
                    lastScriptFile.Refresh();
                    if (lastScriptFile.Exists)
                    {
                        lastScriptFile.Delete();
                        lastScriptFile = null;
                    }
                }
                else
                {
                    File.WriteAllText(s_lastScriptFilePath, lastScript, Encoding.UTF8);
                }
            }
            catch (Exception)
            {
                // Nothing to do
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DoHide();
            TryLoadLastScript();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            TrySaveLastScript();

            base.OnFormClosing(e);
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
            ShowScriptForm();
        }

        private void tsmiRun_Click(object sender, EventArgs e)
        {
            ShowScriptForm();
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