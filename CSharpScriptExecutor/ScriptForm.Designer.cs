namespace CSharpScriptExecutor
{
    partial class ScriptForm
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
            this.labScript = new System.Windows.Forms.Label();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDebug = new System.Windows.Forms.Button();
            this.msMainMenu = new System.Windows.Forms.MenuStrip();
            this.tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiExecute = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiClose = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiScriptConsole = new System.Windows.Forms.ToolStripMenuItem();
            this.scPanels = new System.Windows.Forms.SplitContainer();
            this.tcConsole = new System.Windows.Forms.TabControl();
            this.tpConsoleOut = new System.Windows.Forms.TabPage();
            this.rtbConsoleOut = new System.Windows.Forms.RichTextBox();
            this.tpConsoleError = new System.Windows.Forms.TabPage();
            this.rtbConsoleError = new System.Windows.Forms.RichTextBox();
            this.pbResult = new System.Windows.Forms.PictureBox();
            this.ehTextEditor = new System.Windows.Forms.Integration.ElementHost();
            this.tewTextEditor = new CSharpScriptExecutor.TextEditorWrapper();
            this.msMainMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scPanels)).BeginInit();
            this.scPanels.Panel1.SuspendLayout();
            this.scPanels.Panel2.SuspendLayout();
            this.scPanels.SuspendLayout();
            this.tcConsole.SuspendLayout();
            this.tpConsoleOut.SuspendLayout();
            this.tpConsoleError.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbResult)).BeginInit();
            this.SuspendLayout();
            // 
            // labScript
            // 
            this.labScript.AutoSize = true;
            this.labScript.Location = new System.Drawing.Point(3, 0);
            this.labScript.Name = "labScript";
            this.labScript.Size = new System.Drawing.Size(37, 13);
            this.labScript.TabIndex = 0;
            this.labScript.Text = "&Script:";
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExecute.Location = new System.Drawing.Point(235, 327);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 2;
            this.btnExecute.Text = "E&xecute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(397, 327);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDebug
            // 
            this.btnDebug.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDebug.Location = new System.Drawing.Point(316, 327);
            this.btnDebug.Name = "btnDebug";
            this.btnDebug.Size = new System.Drawing.Size(75, 23);
            this.btnDebug.TabIndex = 3;
            this.btnDebug.Text = "&Debug";
            this.btnDebug.UseVisualStyleBackColor = true;
            this.btnDebug.Click += new System.EventHandler(this.btnDebug_Click);
            // 
            // msMainMenu
            // 
            this.msMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiFile,
            this.tsmiView});
            this.msMainMenu.Location = new System.Drawing.Point(0, 0);
            this.msMainMenu.Name = "msMainMenu";
            this.msMainMenu.Size = new System.Drawing.Size(484, 24);
            this.msMainMenu.TabIndex = 5;
            this.msMainMenu.Text = "MainMenu";
            // 
            // tsmiFile
            // 
            this.tsmiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiExecute,
            this.tsmiDebug,
            this.tssSeparator1,
            this.tsmiClose});
            this.tsmiFile.Name = "tsmiFile";
            this.tsmiFile.Size = new System.Drawing.Size(37, 20);
            this.tsmiFile.Text = "&File";
            // 
            // tsmiExecute
            // 
            this.tsmiExecute.Name = "tsmiExecute";
            this.tsmiExecute.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.tsmiExecute.Size = new System.Drawing.Size(148, 22);
            this.tsmiExecute.Text = "E&xecute";
            this.tsmiExecute.Click += new System.EventHandler(this.tsmiExecute_Click);
            // 
            // tsmiDebug
            // 
            this.tsmiDebug.Name = "tsmiDebug";
            this.tsmiDebug.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.tsmiDebug.Size = new System.Drawing.Size(148, 22);
            this.tsmiDebug.Text = "&Debug";
            this.tsmiDebug.Click += new System.EventHandler(this.tsmiDebug_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(145, 6);
            // 
            // tsmiClose
            // 
            this.tsmiClose.Name = "tsmiClose";
            this.tsmiClose.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.tsmiClose.Size = new System.Drawing.Size(148, 22);
            this.tsmiClose.Text = "&Close";
            this.tsmiClose.Click += new System.EventHandler(this.tsmiClose_Click);
            // 
            // tsmiView
            // 
            this.tsmiView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiScriptConsole});
            this.tsmiView.Name = "tsmiView";
            this.tsmiView.Size = new System.Drawing.Size(44, 20);
            this.tsmiView.Text = "&View";
            // 
            // tsmiScriptConsole
            // 
            this.tsmiScriptConsole.Name = "tsmiScriptConsole";
            this.tsmiScriptConsole.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.K)));
            this.tsmiScriptConsole.Size = new System.Drawing.Size(231, 22);
            this.tsmiScriptConsole.Text = "Toggle Script &Console";
            this.tsmiScriptConsole.Click += new System.EventHandler(this.tsmiScriptConsole_Click);
            // 
            // scPanels
            // 
            this.scPanels.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scPanels.Location = new System.Drawing.Point(16, 27);
            this.scPanels.Name = "scPanels";
            this.scPanels.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scPanels.Panel1
            // 
            this.scPanels.Panel1.Controls.Add(this.ehTextEditor);
            this.scPanels.Panel1.Controls.Add(this.labScript);
            this.scPanels.Panel1MinSize = 50;
            // 
            // scPanels.Panel2
            // 
            this.scPanels.Panel2.Controls.Add(this.tcConsole);
            this.scPanels.Panel2MinSize = 50;
            this.scPanels.Size = new System.Drawing.Size(456, 283);
            this.scPanels.SplitterDistance = 149;
            this.scPanels.TabIndex = 1;
            // 
            // tcConsole
            // 
            this.tcConsole.Controls.Add(this.tpConsoleOut);
            this.tcConsole.Controls.Add(this.tpConsoleError);
            this.tcConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcConsole.Location = new System.Drawing.Point(0, 0);
            this.tcConsole.Name = "tcConsole";
            this.tcConsole.SelectedIndex = 0;
            this.tcConsole.Size = new System.Drawing.Size(456, 130);
            this.tcConsole.TabIndex = 0;
            // 
            // tpConsoleOut
            // 
            this.tpConsoleOut.Controls.Add(this.rtbConsoleOut);
            this.tpConsoleOut.Location = new System.Drawing.Point(4, 22);
            this.tpConsoleOut.Name = "tpConsoleOut";
            this.tpConsoleOut.Padding = new System.Windows.Forms.Padding(3);
            this.tpConsoleOut.Size = new System.Drawing.Size(448, 104);
            this.tpConsoleOut.TabIndex = 0;
            this.tpConsoleOut.Text = "Out";
            this.tpConsoleOut.UseVisualStyleBackColor = true;
            // 
            // rtbConsoleOut
            // 
            this.rtbConsoleOut.DetectUrls = false;
            this.rtbConsoleOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbConsoleOut.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rtbConsoleOut.HideSelection = false;
            this.rtbConsoleOut.Location = new System.Drawing.Point(3, 3);
            this.rtbConsoleOut.Name = "rtbConsoleOut";
            this.rtbConsoleOut.ReadOnly = true;
            this.rtbConsoleOut.Size = new System.Drawing.Size(442, 98);
            this.rtbConsoleOut.TabIndex = 0;
            this.rtbConsoleOut.Text = "";
            this.rtbConsoleOut.WordWrap = false;
            // 
            // tpConsoleError
            // 
            this.tpConsoleError.Controls.Add(this.rtbConsoleError);
            this.tpConsoleError.Location = new System.Drawing.Point(4, 22);
            this.tpConsoleError.Name = "tpConsoleError";
            this.tpConsoleError.Padding = new System.Windows.Forms.Padding(3);
            this.tpConsoleError.Size = new System.Drawing.Size(448, 104);
            this.tpConsoleError.TabIndex = 1;
            this.tpConsoleError.Text = "Error";
            this.tpConsoleError.UseVisualStyleBackColor = true;
            // 
            // rtbConsoleError
            // 
            this.rtbConsoleError.DetectUrls = false;
            this.rtbConsoleError.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbConsoleError.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rtbConsoleError.HideSelection = false;
            this.rtbConsoleError.Location = new System.Drawing.Point(3, 3);
            this.rtbConsoleError.Name = "rtbConsoleError";
            this.rtbConsoleError.ReadOnly = true;
            this.rtbConsoleError.Size = new System.Drawing.Size(442, 98);
            this.rtbConsoleError.TabIndex = 1;
            this.rtbConsoleError.Text = "";
            this.rtbConsoleError.WordWrap = false;
            // 
            // pbResult
            // 
            this.pbResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pbResult.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pbResult.Location = new System.Drawing.Point(16, 317);
            this.pbResult.Name = "pbResult";
            this.pbResult.Size = new System.Drawing.Size(34, 34);
            this.pbResult.TabIndex = 6;
            this.pbResult.TabStop = false;
            this.pbResult.Click += new System.EventHandler(this.pbResult_Click);
            // 
            // ehTextEditor
            // 
            this.ehTextEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ehTextEditor.Location = new System.Drawing.Point(0, 0);
            this.ehTextEditor.Name = "ehTextEditor";
            this.ehTextEditor.Size = new System.Drawing.Size(456, 149);
            this.ehTextEditor.TabIndex = 1;
            this.ehTextEditor.Child = this.tewTextEditor;
            // 
            // ScriptForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 362);
            this.Controls.Add(this.pbResult);
            this.Controls.Add(this.scPanels);
            this.Controls.Add(this.btnDebug);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.msMainMenu);
            this.KeyPreview = true;
            this.MainMenuStrip = this.msMainMenu;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "ScriptForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ScriptForm";
            this.Shown += new System.EventHandler(this.ScriptForm_Shown);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScriptForm_KeyDown);
            this.msMainMenu.ResumeLayout(false);
            this.msMainMenu.PerformLayout();
            this.scPanels.Panel1.ResumeLayout(false);
            this.scPanels.Panel1.PerformLayout();
            this.scPanels.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scPanels)).EndInit();
            this.scPanels.ResumeLayout(false);
            this.tcConsole.ResumeLayout(false);
            this.tpConsoleOut.ResumeLayout(false);
            this.tpConsoleError.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labScript;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnDebug;
        private System.Windows.Forms.MenuStrip msMainMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiView;
        private System.Windows.Forms.ToolStripMenuItem tsmiFile;
        private System.Windows.Forms.ToolStripMenuItem tsmiClose;
        private System.Windows.Forms.ToolStripMenuItem tsmiScriptConsole;
        private System.Windows.Forms.SplitContainer scPanels;
        private System.Windows.Forms.TabControl tcConsole;
        private System.Windows.Forms.TabPage tpConsoleOut;
        private System.Windows.Forms.TabPage tpConsoleError;
        private System.Windows.Forms.RichTextBox rtbConsoleOut;
        private System.Windows.Forms.RichTextBox rtbConsoleError;
        private System.Windows.Forms.PictureBox pbResult;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsmiExecute;
        private System.Windows.Forms.ToolStripMenuItem tsmiDebug;
        private System.Windows.Forms.Integration.ElementHost ehTextEditor;
        private TextEditorWrapper tewTextEditor;
    }
}