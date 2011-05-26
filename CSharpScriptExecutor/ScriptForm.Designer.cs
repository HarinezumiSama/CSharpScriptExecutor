namespace CSharpScriptExecutor
{
    partial class ScriptForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnDebug = new System.Windows.Forms.Button();
            this.msMainMenu = new System.Windows.Forms.MenuStrip();
            this.tsmiFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiExecute = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiClose = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.tssClearHistorySeparator = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiClearHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiShowResult = new System.Windows.Forms.ToolStripMenuItem();
            this.pbResult = new System.Windows.Forms.PictureBox();
            this.cmsEditorContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiInsertSnippet = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiInsertReturn = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiInsertConsoleWriteLine = new System.Windows.Forms.ToolStripMenuItem();
            this.ehTextEditor = new System.Windows.Forms.Integration.ElementHost();
            this.tewTextEditor = new CSharpScriptExecutor.TextEditorWrapper();
            this.msMainMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbResult)).BeginInit();
            this.cmsEditorContextMenu.SuspendLayout();
            this.SuspendLayout();
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
            this.tsmiHistory,
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
            // tsmiHistory
            // 
            this.tsmiHistory.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssClearHistorySeparator,
            this.tsmiClearHistory});
            this.tsmiHistory.Name = "tsmiHistory";
            this.tsmiHistory.Size = new System.Drawing.Size(57, 20);
            this.tsmiHistory.Text = "&History";
            // 
            // tssClearHistorySeparator
            // 
            this.tssClearHistorySeparator.Name = "tssClearHistorySeparator";
            this.tssClearHistorySeparator.Size = new System.Drawing.Size(223, 6);
            // 
            // tsmiClearHistory
            // 
            this.tsmiClearHistory.Name = "tsmiClearHistory";
            this.tsmiClearHistory.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.H)));
            this.tsmiClearHistory.Size = new System.Drawing.Size(226, 22);
            this.tsmiClearHistory.Text = "Clear &History...";
            this.tsmiClearHistory.Click += new System.EventHandler(this.tsmiClearHistory_Click);
            // 
            // tsmiView
            // 
            this.tsmiView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiShowResult});
            this.tsmiView.Name = "tsmiView";
            this.tsmiView.Size = new System.Drawing.Size(44, 20);
            this.tsmiView.Text = "&View";
            // 
            // tsmiShowResult
            // 
            this.tsmiShowResult.Name = "tsmiShowResult";
            this.tsmiShowResult.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.tsmiShowResult.Size = new System.Drawing.Size(188, 22);
            this.tsmiShowResult.Text = "Show &Result...";
            this.tsmiShowResult.Click += new System.EventHandler(this.tsmiShowResult_Click);
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
            // cmsEditorContextMenu
            // 
            this.cmsEditorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiInsertSnippet});
            this.cmsEditorContextMenu.Name = "cmsEditorContextMenu";
            this.cmsEditorContextMenu.Size = new System.Drawing.Size(153, 48);
            // 
            // tsmiInsertSnippet
            // 
            this.tsmiInsertSnippet.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiInsertReturn,
            this.tsmiInsertConsoleWriteLine});
            this.tsmiInsertSnippet.Name = "tsmiInsertSnippet";
            this.tsmiInsertSnippet.Size = new System.Drawing.Size(152, 22);
            this.tsmiInsertSnippet.Text = "&Insert Snippet";
            // 
            // tsmiInsertReturn
            // 
            this.tsmiInsertReturn.Name = "tsmiInsertReturn";
            this.tsmiInsertReturn.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.R)));
            this.tsmiInsertReturn.Size = new System.Drawing.Size(211, 22);
            this.tsmiInsertReturn.Text = "&return";
            this.tsmiInsertReturn.Click += new System.EventHandler(this.tsmiInsertReturn_Click);
            // 
            // tsmiInsertConsoleWriteLine
            // 
            this.tsmiInsertConsoleWriteLine.Name = "tsmiInsertConsoleWriteLine";
            this.tsmiInsertConsoleWriteLine.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.W)));
            this.tsmiInsertConsoleWriteLine.Size = new System.Drawing.Size(211, 22);
            this.tsmiInsertConsoleWriteLine.Text = "Console.WriteLine";
            this.tsmiInsertConsoleWriteLine.Click += new System.EventHandler(this.tsmiInsertConsoleWriteLine_Click);
            // 
            // ehTextEditor
            // 
            this.ehTextEditor.AllowDrop = true;
            this.ehTextEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ehTextEditor.ContextMenuStrip = this.cmsEditorContextMenu;
            this.ehTextEditor.Location = new System.Drawing.Point(12, 27);
            this.ehTextEditor.Name = "ehTextEditor";
            this.ehTextEditor.Size = new System.Drawing.Size(460, 284);
            this.ehTextEditor.TabIndex = 7;
            this.ehTextEditor.Child = this.tewTextEditor;
            // 
            // ScriptForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 362);
            this.Controls.Add(this.ehTextEditor);
            this.Controls.Add(this.pbResult);
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
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ScriptForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ScriptForm_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScriptForm_KeyDown);
            this.msMainMenu.ResumeLayout(false);
            this.msMainMenu.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbResult)).EndInit();
            this.cmsEditorContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnDebug;
        private System.Windows.Forms.MenuStrip msMainMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiView;
        private System.Windows.Forms.ToolStripMenuItem tsmiFile;
        private System.Windows.Forms.ToolStripMenuItem tsmiClose;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowResult;
        private System.Windows.Forms.PictureBox pbResult;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsmiExecute;
        private System.Windows.Forms.ToolStripMenuItem tsmiDebug;
        private System.Windows.Forms.Integration.ElementHost ehTextEditor;
        private TextEditorWrapper tewTextEditor;
        private System.Windows.Forms.ContextMenuStrip cmsEditorContextMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiInsertSnippet;
        private System.Windows.Forms.ToolStripMenuItem tsmiInsertReturn;
        private System.Windows.Forms.ToolStripMenuItem tsmiInsertConsoleWriteLine;
        private System.Windows.Forms.ToolStripMenuItem tsmiHistory;
        private System.Windows.Forms.ToolStripMenuItem tsmiClearHistory;
        private System.Windows.Forms.ToolStripSeparator tssClearHistorySeparator;
    }
}