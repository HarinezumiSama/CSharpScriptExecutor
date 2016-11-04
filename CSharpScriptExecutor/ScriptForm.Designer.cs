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
            this.ExecuteButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.DebugButton = new System.Windows.Forms.Button();
            this.MainMenuMenuStrip = new System.Windows.Forms.MenuStrip();
            this.FileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenScriptMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExecuteMenuItemSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.ExecuteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CloseMenuItemSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.CloseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenuItemSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HistoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ClearHistoryMenuItemSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.ClearHistoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ShowResultMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ResultPictureBox = new System.Windows.Forms.PictureBox();
            this.EditorContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.InsertSnippetMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InsertReturnMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InsertConsoleWriteLineMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextMenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.InsertReferenceDirectiveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InsertUsingDirectiveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveScriptDialog = new System.Windows.Forms.SaveFileDialog();
            this.TextEditorElementHost = new System.Windows.Forms.Integration.ElementHost();
            this.TextEditorWrapper = new CSharpScriptExecutor.TextEditorWrapper();
            this.OpenScriptDialog = new System.Windows.Forms.OpenFileDialog();
            this.CommandLineGroupBox = new System.Windows.Forms.GroupBox();
            this.CommandLineTextBox = new System.Windows.Forms.TextBox();
            this.MainMenuMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ResultPictureBox)).BeginInit();
            this.EditorContextMenu.SuspendLayout();
            this.CommandLineGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExecuteButton
            // 
            this.ExecuteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ExecuteButton.Location = new System.Drawing.Point(235, 327);
            this.ExecuteButton.Name = "ExecuteButton";
            this.ExecuteButton.Size = new System.Drawing.Size(75, 23);
            this.ExecuteButton.TabIndex = 2;
            this.ExecuteButton.Text = "E&xecute";
            this.ExecuteButton.UseVisualStyleBackColor = true;
            this.ExecuteButton.Click += new System.EventHandler(this.ExecuteButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = new System.Drawing.Point(397, 327);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 4;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // DebugButton
            // 
            this.DebugButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.DebugButton.Location = new System.Drawing.Point(316, 327);
            this.DebugButton.Name = "DebugButton";
            this.DebugButton.Size = new System.Drawing.Size(75, 23);
            this.DebugButton.TabIndex = 3;
            this.DebugButton.Text = "&Debug";
            this.DebugButton.UseVisualStyleBackColor = true;
            this.DebugButton.Click += new System.EventHandler(this.DebugButton_Click);
            // 
            // MainMenuMenuStrip
            // 
            this.MainMenuMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenuItem,
            this.HistoryMenuItem,
            this.ViewMenuItem});
            this.MainMenuMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.MainMenuMenuStrip.Name = "msMainMenu";
            this.MainMenuMenuStrip.Size = new System.Drawing.Size(484, 24);
            this.MainMenuMenuStrip.TabIndex = 5;
            this.MainMenuMenuStrip.Text = "MainMenu";
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenScriptMenuItem,
            this.SaveAsMenuItem,
            this.ExecuteMenuItemSeparator,
            this.ExecuteMenuItem,
            this.DebugMenuItem,
            this.CloseMenuItemSeparator,
            this.CloseMenuItem,
            this.ExitMenuItemSeparator,
            this.ExitMenuItem});
            this.FileMenuItem.Name = "FileMenuItem";
            this.FileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.FileMenuItem.Text = "&File";
            // 
            // OpenScriptMenuItem
            // 
            this.OpenScriptMenuItem.Name = "OpenScriptMenuItem";
            this.OpenScriptMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.OpenScriptMenuItem.Size = new System.Drawing.Size(186, 22);
            this.OpenScriptMenuItem.Text = "&Open...";
            this.OpenScriptMenuItem.Click += new System.EventHandler(this.OpenScriptMenuItem_Click);
            // 
            // SaveAsMenuItem
            // 
            this.SaveAsMenuItem.Name = "SaveAsMenuItem";
            this.SaveAsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.S)));
            this.SaveAsMenuItem.Size = new System.Drawing.Size(186, 22);
            this.SaveAsMenuItem.Text = "Save &As...";
            this.SaveAsMenuItem.Click += new System.EventHandler(this.SaveAsMenuItem_Click);
            // 
            // ExecuteMenuItemSeparator
            // 
            this.ExecuteMenuItemSeparator.Name = "ExecuteMenuItemSeparator";
            this.ExecuteMenuItemSeparator.Size = new System.Drawing.Size(183, 6);
            // 
            // ExecuteMenuItem
            // 
            this.ExecuteMenuItem.Name = "ExecuteMenuItem";
            this.ExecuteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.ExecuteMenuItem.Size = new System.Drawing.Size(186, 22);
            this.ExecuteMenuItem.Text = "E&xecute";
            this.ExecuteMenuItem.Click += new System.EventHandler(this.ExecuteMenuItem_Click);
            // 
            // DebugMenuItem
            // 
            this.DebugMenuItem.Name = "DebugMenuItem";
            this.DebugMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F8;
            this.DebugMenuItem.Size = new System.Drawing.Size(186, 22);
            this.DebugMenuItem.Text = "&Debug";
            this.DebugMenuItem.Click += new System.EventHandler(this.DebugMenuItem_Click);
            // 
            // CloseMenuItemSeparator
            // 
            this.CloseMenuItemSeparator.Name = "CloseMenuItemSeparator";
            this.CloseMenuItemSeparator.Size = new System.Drawing.Size(183, 6);
            // 
            // CloseMenuItem
            // 
            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.ShortcutKeyDisplayString = "";
            this.CloseMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.CloseMenuItem.Size = new System.Drawing.Size(186, 22);
            this.CloseMenuItem.Text = "&Close";
            this.CloseMenuItem.Click += new System.EventHandler(this.CloseMenuItem_Click);
            // 
            // ExitMenuItemSeparator
            // 
            this.ExitMenuItemSeparator.Name = "ExitMenuItemSeparator";
            this.ExitMenuItemSeparator.Size = new System.Drawing.Size(183, 6);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.ExitMenuItem.Size = new System.Drawing.Size(186, 22);
            this.ExitMenuItem.Text = "E&xit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // HistoryMenuItem
            // 
            this.HistoryMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClearHistoryMenuItemSeparator,
            this.ClearHistoryMenuItem});
            this.HistoryMenuItem.Name = "HistoryMenuItem";
            this.HistoryMenuItem.Size = new System.Drawing.Size(57, 20);
            this.HistoryMenuItem.Text = "&History";
            // 
            // ClearHistoryMenuItemSeparator
            // 
            this.ClearHistoryMenuItemSeparator.Name = "ClearHistoryMenuItemSeparator";
            this.ClearHistoryMenuItemSeparator.Size = new System.Drawing.Size(223, 6);
            // 
            // ClearHistoryMenuItem
            // 
            this.ClearHistoryMenuItem.Name = "ClearHistoryMenuItem";
            this.ClearHistoryMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.H)));
            this.ClearHistoryMenuItem.Size = new System.Drawing.Size(226, 22);
            this.ClearHistoryMenuItem.Text = "Clear &History...";
            this.ClearHistoryMenuItem.Click += new System.EventHandler(this.ClearHistoryMenuItem_Click);
            // 
            // ViewMenuItem
            // 
            this.ViewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowResultMenuItem});
            this.ViewMenuItem.Name = "ViewMenuItem";
            this.ViewMenuItem.Size = new System.Drawing.Size(44, 20);
            this.ViewMenuItem.Text = "&View";
            // 
            // ShowResultMenuItem
            // 
            this.ShowResultMenuItem.Name = "ShowResultMenuItem";
            this.ShowResultMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt) 
            | System.Windows.Forms.Keys.R)));
            this.ShowResultMenuItem.Size = new System.Drawing.Size(211, 22);
            this.ShowResultMenuItem.Text = "Show &Result...";
            this.ShowResultMenuItem.Click += new System.EventHandler(this.ShowResultMenuItem_Click);
            // 
            // ResultPictureBox
            // 
            this.ResultPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ResultPictureBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ResultPictureBox.Location = new System.Drawing.Point(16, 317);
            this.ResultPictureBox.Name = "ResultPictureBox";
            this.ResultPictureBox.Size = new System.Drawing.Size(34, 34);
            this.ResultPictureBox.TabIndex = 6;
            this.ResultPictureBox.TabStop = false;
            this.ResultPictureBox.Click += new System.EventHandler(this.ResultPictureBox_Click);
            // 
            // EditorContextMenu
            // 
            this.EditorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertSnippetMenuItem});
            this.EditorContextMenu.Name = "EditorContextMenu";
            this.EditorContextMenu.Size = new System.Drawing.Size(153, 48);
            // 
            // InsertSnippetMenuItem
            // 
            this.InsertSnippetMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertReturnMenuItem,
            this.InsertConsoleWriteLineMenuItem,
            this.ContextMenuSeparator1,
            this.InsertReferenceDirectiveMenuItem,
            this.InsertUsingDirectiveMenuItem});
            this.InsertSnippetMenuItem.Name = "InsertSnippetMenuItem";
            this.InsertSnippetMenuItem.Size = new System.Drawing.Size(152, 22);
            this.InsertSnippetMenuItem.Text = "&Insert Snippet";
            // 
            // InsertReturnMenuItem
            // 
            this.InsertReturnMenuItem.Name = "InsertReturnMenuItem";
            this.InsertReturnMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.R)));
            this.InsertReturnMenuItem.Size = new System.Drawing.Size(211, 22);
            this.InsertReturnMenuItem.Text = "&return";
            this.InsertReturnMenuItem.Click += new System.EventHandler(this.InsertReturnMenuItem_Click);
            // 
            // InsertConsoleWriteLineMenuItem
            // 
            this.InsertConsoleWriteLineMenuItem.Name = "InsertConsoleWriteLineMenuItem";
            this.InsertConsoleWriteLineMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.W)));
            this.InsertConsoleWriteLineMenuItem.Size = new System.Drawing.Size(211, 22);
            this.InsertConsoleWriteLineMenuItem.Text = "Console.&WriteLine";
            this.InsertConsoleWriteLineMenuItem.Click += new System.EventHandler(this.InsertConsoleWriteLineMenuItem_Click);
            // 
            // ContextMenuSeparator1
            // 
            this.ContextMenuSeparator1.Name = "ContextMenuSeparator1";
            this.ContextMenuSeparator1.Size = new System.Drawing.Size(208, 6);
            // 
            // InsertReferenceDirectiveMenuItem
            // 
            this.InsertReferenceDirectiveMenuItem.Name = "InsertReferenceDirectiveMenuItem";
            this.InsertReferenceDirectiveMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.InsertReferenceDirectiveMenuItem.Size = new System.Drawing.Size(211, 22);
            this.InsertReferenceDirectiveMenuItem.Text = "//##RE&F";
            this.InsertReferenceDirectiveMenuItem.Click += new System.EventHandler(this.InsertReferenceDirectiveMenuItem_Click);
            // 
            // InsertUsingDirectiveMenuItem
            // 
            this.InsertUsingDirectiveMenuItem.Name = "InsertUsingDirectiveMenuItem";
            this.InsertUsingDirectiveMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.InsertUsingDirectiveMenuItem.Size = new System.Drawing.Size(211, 22);
            this.InsertUsingDirectiveMenuItem.Text = "//##USING";
            this.InsertUsingDirectiveMenuItem.Click += new System.EventHandler(this.InsertUsingDirectiveMenuItem_Click);
            // 
            // SaveScriptDialog
            // 
            this.SaveScriptDialog.DefaultExt = "cssx";
            this.SaveScriptDialog.Filter = "C# scripts|*.cssx";
            this.SaveScriptDialog.SupportMultiDottedExtensions = true;
            this.SaveScriptDialog.Title = "Save Script";
            // 
            // TextEditorElementHost
            // 
            this.TextEditorElementHost.AllowDrop = true;
            this.TextEditorElementHost.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextEditorElementHost.ContextMenuStrip = this.EditorContextMenu;
            this.TextEditorElementHost.Location = new System.Drawing.Point(12, 27);
            this.TextEditorElementHost.Name = "TextEditorElementHost";
            this.TextEditorElementHost.Size = new System.Drawing.Size(460, 225);
            this.TextEditorElementHost.TabIndex = 0;
            this.TextEditorElementHost.Child = this.TextEditorWrapper;
            // 
            // OpenScriptDialog
            // 
            this.OpenScriptDialog.AddExtension = false;
            this.OpenScriptDialog.Filter = "C# scripts|*.cssx|C# code files|*.cs|All files|*.*";
            this.OpenScriptDialog.SupportMultiDottedExtensions = true;
            this.OpenScriptDialog.Title = "Open Script";
            // 
            // CommandLineGroupBox
            // 
            this.CommandLineGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandLineGroupBox.Controls.Add(this.CommandLineTextBox);
            this.CommandLineGroupBox.Location = new System.Drawing.Point(12, 258);
            this.CommandLineGroupBox.Name = "CommandLineGroupBox";
            this.CommandLineGroupBox.Size = new System.Drawing.Size(460, 53);
            this.CommandLineGroupBox.TabIndex = 1;
            this.CommandLineGroupBox.TabStop = false;
            this.CommandLineGroupBox.Text = "Command-line parameters (optional)";
            // 
            // CommandLineTextBox
            // 
            this.CommandLineTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandLineTextBox.Location = new System.Drawing.Point(7, 20);
            this.CommandLineTextBox.Name = "CommandLineTextBox";
            this.CommandLineTextBox.Size = new System.Drawing.Size(447, 20);
            this.CommandLineTextBox.TabIndex = 0;
            // 
            // ScriptForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 362);
            this.Controls.Add(this.CommandLineGroupBox);
            this.Controls.Add(this.TextEditorElementHost);
            this.Controls.Add(this.ResultPictureBox);
            this.Controls.Add(this.DebugButton);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.ExecuteButton);
            this.Controls.Add(this.MainMenuMenuStrip);
            this.KeyPreview = true;
            this.MainMenuStrip = this.MainMenuMenuStrip;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "ScriptForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "ScriptForm";
            this.Shown += new System.EventHandler(this.ScriptForm_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ScriptForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ScriptForm_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScriptForm_KeyDown);
            this.MainMenuMenuStrip.ResumeLayout(false);
            this.MainMenuMenuStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ResultPictureBox)).EndInit();
            this.EditorContextMenu.ResumeLayout(false);
            this.CommandLineGroupBox.ResumeLayout(false);
            this.CommandLineGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ExecuteButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Button DebugButton;
        private System.Windows.Forms.MenuStrip MainMenuMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CloseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ShowResultMenuItem;
        private System.Windows.Forms.PictureBox ResultPictureBox;
        private System.Windows.Forms.ToolStripSeparator CloseMenuItemSeparator;
        private System.Windows.Forms.ToolStripMenuItem ExecuteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DebugMenuItem;
        private System.Windows.Forms.Integration.ElementHost TextEditorElementHost;
        private TextEditorWrapper TextEditorWrapper;
        private System.Windows.Forms.ContextMenuStrip EditorContextMenu;
        private System.Windows.Forms.ToolStripMenuItem InsertSnippetMenuItem;
        private System.Windows.Forms.ToolStripMenuItem InsertReturnMenuItem;
        private System.Windows.Forms.ToolStripMenuItem InsertConsoleWriteLineMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HistoryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ClearHistoryMenuItem;
        private System.Windows.Forms.ToolStripSeparator ClearHistoryMenuItemSeparator;
        private System.Windows.Forms.SaveFileDialog SaveScriptDialog;
        private System.Windows.Forms.ToolStripMenuItem SaveAsMenuItem;
        private System.Windows.Forms.ToolStripSeparator ExecuteMenuItemSeparator;
        private System.Windows.Forms.OpenFileDialog OpenScriptDialog;
        private System.Windows.Forms.ToolStripMenuItem OpenScriptMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
        private System.Windows.Forms.ToolStripSeparator ExitMenuItemSeparator;
        private System.Windows.Forms.ToolStripMenuItem InsertReferenceDirectiveMenuItem;
        private System.Windows.Forms.ToolStripSeparator ContextMenuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem InsertUsingDirectiveMenuItem;
        private System.Windows.Forms.GroupBox CommandLineGroupBox;
        private System.Windows.Forms.TextBox CommandLineTextBox;
    }
}