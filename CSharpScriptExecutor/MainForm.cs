using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CSharpScriptExecutor
{
    public partial class MainForm : Form
    {
        #region Constructors

        public MainForm()
        {
            InitializeComponent();

            this.Text = Program.ProgramName;

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
            using (var form = new ScriptForm())
            {
                form.Size = new Size(workingArea.Width / 4, workingArea.Height / 5);
                form.Location = new Point(workingArea.Width - form.Size.Width, workingArea.Height - form.Size.Height);

                form.ShowDialog(this);
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

        #endregion
    }
}