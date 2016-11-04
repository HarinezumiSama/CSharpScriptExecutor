using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CSharpScriptExecutor.Common;
using CSharpScriptExecutor.Properties;

namespace CSharpScriptExecutor
{
    public partial class MainForm : Form
    {
        #region Fields

        private static readonly string LastScriptFilePath = Path.Combine(
            Program.ProgramDataPath,
            "~LastScript" + ScriptExecutor.ScriptFileExtension);

        private bool _isScriptFormActive;
        private ScriptForm _scriptForm;
        private Size _lastScriptFormSize;
        private string _lastScriptFormScript;

        #endregion

        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            Text = Program.ProgramName;
            labAbout.Text = Program.FullProgramName;
            Icon = Resources.MainIcon;

            niTrayIcon.Icon = Icon;
            niTrayIcon.Text = Program.ProgramName;
            niTrayIcon.Visible = true;
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

        #region Private Methods: General

        [DebuggerStepThrough]
        private static void DoAttachDebuggerNow()
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }

        private void DoShow()
        {
            Visible = true;
            ShowInTaskbar = true;
            tsmiAbout.Visible = false;
        }

        private void DoHide()
        {
            ShowInTaskbar = false;
            Visible = false;
            tsmiAbout.Visible = true;
        }

        private void ShowScriptForm()
        {
            if (_isScriptFormActive)
            {
                if (_scriptForm != null && _scriptForm.Enabled)
                {
                    _scriptForm.Activate();
                }

                return;
            }

            bool exit;
            var workingArea = Screen.FromControl(this).WorkingArea;

            var oldCursor = Cursor.Current;
            var oldRunEnabled = tsmiRun.Enabled;
            var oldAboutEnabled = tsmiAbout.Enabled;
            try
            {
                _isScriptFormActive = true;
                Cursor.Current = Cursors.AppStarting;
                tsmiRun.Enabled = false;
                tsmiAbout.Enabled = false;

                using (_scriptForm = new ScriptForm())
                {
                    _scriptForm.Icon = Icon;

                    _scriptForm.Size = _lastScriptFormSize.IsEmpty
                        ? new Size(workingArea.Width * 3 / 5, workingArea.Height * 3 / 5)
                        : _lastScriptFormSize;
                    _scriptForm.Location = new Point(
                        workingArea.Width - _scriptForm.Size.Width,
                        workingArea.Height - _scriptForm.Size.Height);
                    _scriptForm.Script = _lastScriptFormScript;

                    exit = _scriptForm.ShowDialog(this) == DialogResult.Abort;
                    Cursor.Current = Cursors.AppStarting;

                    _lastScriptFormSize = _scriptForm.Size;
                    _lastScriptFormScript = _scriptForm.Script;
                }
            }
            finally
            {
                tsmiRun.Enabled = oldRunEnabled;
                tsmiAbout.Enabled = oldAboutEnabled;
                Cursor.Current = oldCursor;

                _scriptForm = null;
                _isScriptFormActive = false;
            }

            if (exit)
            {
                Close();
            }
        }

        private void TryLoadLastScript() => _lastScriptFormScript = LocalHelper.TryLoadScript(LastScriptFilePath);

        private void TrySaveLastScript()
        {
            var lastScript = _isScriptFormActive && _scriptForm != null
                ? _scriptForm.Script
                : _lastScriptFormScript;
            LocalHelper.TrySaveScript(LastScriptFilePath, lastScript);
        }

        #endregion

        #region Private Methods: Event Handlers

        private void ExitMenuItem_Click(object sender, EventArgs e) => Close();

        private void AboutMenuItem_Click(object sender, EventArgs e) => DoShow();

        private void OKButton_Click(object sender, EventArgs e) => DoHide();

        private void TrayIcon_DoubleClick(object sender, EventArgs e) => ShowScriptForm();

        private void RunMenuItem_Click(object sender, EventArgs e) => ShowScriptForm();

        [DebuggerStepThrough]
        private void AttachDebuggerNowMenuItem_Click(object sender, EventArgs e) => DoAttachDebuggerNow();

        private void TrayIconMenu_Opening(object sender, CancelEventArgs e)
            => tsmiAttachDebuggerNow.Enabled = !Debugger.IsAttached;

        #endregion
    }
}