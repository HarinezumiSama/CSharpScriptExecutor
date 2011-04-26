namespace CSharpScriptExecutor
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.niTrayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.cmsTrayIconMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.labAbout = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiRun = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsTrayIconMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // niTrayIcon
            // 
            this.niTrayIcon.ContextMenuStrip = this.cmsTrayIconMenu;
            this.niTrayIcon.Visible = true;
            this.niTrayIcon.DoubleClick += new System.EventHandler(this.niTrayIcon_DoubleClick);
            // 
            // cmsTrayIconMenu
            // 
            this.cmsTrayIconMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiRun,
            this.toolStripSeparator2,
            this.tsmiAbout,
            this.toolStripSeparator1,
            this.tsmiExit});
            this.cmsTrayIconMenu.Name = "TrayIconMenu";
            this.cmsTrayIconMenu.Size = new System.Drawing.Size(153, 104);
            // 
            // tsmiAbout
            // 
            this.tsmiAbout.Name = "tsmiAbout";
            this.tsmiAbout.Size = new System.Drawing.Size(152, 22);
            this.tsmiAbout.Text = "&About";
            this.tsmiAbout.Click += new System.EventHandler(this.tsmiAbout_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // tsmiExit
            // 
            this.tsmiExit.Name = "tsmiExit";
            this.tsmiExit.Size = new System.Drawing.Size(152, 22);
            this.tsmiExit.Text = "E&xit";
            this.tsmiExit.Click += new System.EventHandler(this.tsmiExit_Click);
            // 
            // labAbout
            // 
            this.labAbout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labAbout.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labAbout.Location = new System.Drawing.Point(13, 13);
            this.labAbout.Name = "labAbout";
            this.labAbout.Size = new System.Drawing.Size(573, 23);
            this.labAbout.TabIndex = 1;
            this.labAbout.Text = "(About)";
            this.labAbout.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOK.Location = new System.Drawing.Point(511, 51);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // tsmiRun
            // 
            this.tsmiRun.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tsmiRun.Name = "tsmiRun";
            this.tsmiRun.Size = new System.Drawing.Size(152, 22);
            this.tsmiRun.Text = "&Run";
            this.tsmiRun.Click += new System.EventHandler(this.tsmiRun_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnOK;
            this.ClientSize = new System.Drawing.Size(598, 86);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.labAbout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.cmsTrayIconMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon niTrayIcon;
        private System.Windows.Forms.ContextMenuStrip cmsTrayIconMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiAbout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsmiExit;
        private System.Windows.Forms.Label labAbout;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ToolStripMenuItem tsmiRun;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}