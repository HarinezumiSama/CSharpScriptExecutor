namespace CSharpScriptExecutor
{
    partial class ExecutionResultViewerForm
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
            this.btnClose = new System.Windows.Forms.Button();
            this.gbResult = new System.Windows.Forms.GroupBox();
            this.scDetails = new System.Windows.Forms.SplitContainer();
            this.lblMessage = new System.Windows.Forms.Label();
            this.tbMessage = new System.Windows.Forms.TextBox();
            this.tcResults = new System.Windows.Forms.TabControl();
            this.tpReturnValue = new System.Windows.Forms.TabPage();
            this.pgReturnValue = new System.Windows.Forms.PropertyGrid();
            this.tpConsoleOut = new System.Windows.Forms.TabPage();
            this.rtbConsoleOut = new System.Windows.Forms.RichTextBox();
            this.tpConsoleError = new System.Windows.Forms.TabPage();
            this.rtbConsoleError = new System.Windows.Forms.RichTextBox();
            this.tpSourceCode = new System.Windows.Forms.TabPage();
            this.ehSourceCode = new System.Windows.Forms.Integration.ElementHost();
            this.tewSourceCode = new CSharpScriptExecutor.TextEditorWrapper();
            this.tpGeneratedCode = new System.Windows.Forms.TabPage();
            this.ehGeneratedCode = new System.Windows.Forms.Integration.ElementHost();
            this.tewGeneratedCode = new CSharpScriptExecutor.TextEditorWrapper();
            this.gbResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scDetails)).BeginInit();
            this.scDetails.Panel1.SuspendLayout();
            this.scDetails.Panel2.SuspendLayout();
            this.scDetails.SuspendLayout();
            this.tcResults.SuspendLayout();
            this.tpReturnValue.SuspendLayout();
            this.tpConsoleOut.SuspendLayout();
            this.tpConsoleError.SuspendLayout();
            this.tpSourceCode.SuspendLayout();
            this.tpGeneratedCode.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(497, 327);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // gbResult
            // 
            this.gbResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbResult.Controls.Add(this.scDetails);
            this.gbResult.Location = new System.Drawing.Point(12, 12);
            this.gbResult.Name = "gbResult";
            this.gbResult.Size = new System.Drawing.Size(560, 309);
            this.gbResult.TabIndex = 2;
            this.gbResult.TabStop = false;
            // 
            // scDetails
            // 
            this.scDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scDetails.Location = new System.Drawing.Point(12, 20);
            this.scDetails.MinimumSize = new System.Drawing.Size(100, 210);
            this.scDetails.Name = "scDetails";
            this.scDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scDetails.Panel1
            // 
            this.scDetails.Panel1.Controls.Add(this.lblMessage);
            this.scDetails.Panel1.Controls.Add(this.tbMessage);
            this.scDetails.Panel1MinSize = 100;
            // 
            // scDetails.Panel2
            // 
            this.scDetails.Panel2.Controls.Add(this.tcResults);
            this.scDetails.Panel2MinSize = 100;
            this.scDetails.Size = new System.Drawing.Size(542, 283);
            this.scDetails.SplitterDistance = 141;
            this.scDetails.TabIndex = 1;
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(3, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(78, 13);
            this.lblMessage.TabIndex = 2;
            this.lblMessage.Text = "Error &Message:";
            // 
            // tbMessage
            // 
            this.tbMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbMessage.HideSelection = false;
            this.tbMessage.Location = new System.Drawing.Point(3, 16);
            this.tbMessage.MaxLength = 0;
            this.tbMessage.MinimumSize = new System.Drawing.Size(100, 50);
            this.tbMessage.Multiline = true;
            this.tbMessage.Name = "tbMessage";
            this.tbMessage.ReadOnly = true;
            this.tbMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbMessage.Size = new System.Drawing.Size(536, 113);
            this.tbMessage.TabIndex = 1;
            this.tbMessage.WordWrap = false;
            // 
            // tcResults
            // 
            this.tcResults.Controls.Add(this.tpReturnValue);
            this.tcResults.Controls.Add(this.tpConsoleOut);
            this.tcResults.Controls.Add(this.tpConsoleError);
            this.tcResults.Controls.Add(this.tpSourceCode);
            this.tcResults.Controls.Add(this.tpGeneratedCode);
            this.tcResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcResults.Location = new System.Drawing.Point(0, 0);
            this.tcResults.Name = "tcResults";
            this.tcResults.SelectedIndex = 0;
            this.tcResults.Size = new System.Drawing.Size(542, 138);
            this.tcResults.TabIndex = 0;
            this.tcResults.TabStop = false;
            // 
            // tpReturnValue
            // 
            this.tpReturnValue.Controls.Add(this.pgReturnValue);
            this.tpReturnValue.Location = new System.Drawing.Point(4, 22);
            this.tpReturnValue.Name = "tpReturnValue";
            this.tpReturnValue.Padding = new System.Windows.Forms.Padding(3);
            this.tpReturnValue.Size = new System.Drawing.Size(534, 112);
            this.tpReturnValue.TabIndex = 4;
            this.tpReturnValue.Text = "Return Value";
            this.tpReturnValue.UseVisualStyleBackColor = true;
            // 
            // pgReturnValue
            // 
            this.pgReturnValue.CausesValidation = false;
            this.pgReturnValue.CommandsVisibleIfAvailable = false;
            this.pgReturnValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgReturnValue.HelpVisible = false;
            this.pgReturnValue.Location = new System.Drawing.Point(3, 3);
            this.pgReturnValue.Name = "pgReturnValue";
            this.pgReturnValue.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.pgReturnValue.Size = new System.Drawing.Size(528, 106);
            this.pgReturnValue.TabIndex = 0;
            this.pgReturnValue.ToolbarVisible = false;
            // 
            // tpConsoleOut
            // 
            this.tpConsoleOut.Controls.Add(this.rtbConsoleOut);
            this.tpConsoleOut.Location = new System.Drawing.Point(4, 22);
            this.tpConsoleOut.Name = "tpConsoleOut";
            this.tpConsoleOut.Padding = new System.Windows.Forms.Padding(3);
            this.tpConsoleOut.Size = new System.Drawing.Size(534, 112);
            this.tpConsoleOut.TabIndex = 2;
            this.tpConsoleOut.Text = "Console.Out";
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
            this.rtbConsoleOut.Size = new System.Drawing.Size(528, 106);
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
            this.tpConsoleError.Size = new System.Drawing.Size(534, 112);
            this.tpConsoleError.TabIndex = 3;
            this.tpConsoleError.Text = "Console.Error";
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
            this.rtbConsoleError.Size = new System.Drawing.Size(528, 106);
            this.rtbConsoleError.TabIndex = 1;
            this.rtbConsoleError.Text = "";
            this.rtbConsoleError.WordWrap = false;
            // 
            // tpSourceCode
            // 
            this.tpSourceCode.Controls.Add(this.ehSourceCode);
            this.tpSourceCode.Location = new System.Drawing.Point(4, 22);
            this.tpSourceCode.Name = "tpSourceCode";
            this.tpSourceCode.Padding = new System.Windows.Forms.Padding(3);
            this.tpSourceCode.Size = new System.Drawing.Size(534, 112);
            this.tpSourceCode.TabIndex = 0;
            this.tpSourceCode.Text = "Source Code";
            this.tpSourceCode.UseVisualStyleBackColor = true;
            // 
            // ehSourceCode
            // 
            this.ehSourceCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ehSourceCode.Location = new System.Drawing.Point(3, 3);
            this.ehSourceCode.Name = "ehSourceCode";
            this.ehSourceCode.Size = new System.Drawing.Size(528, 106);
            this.ehSourceCode.TabIndex = 0;
            this.ehSourceCode.Child = this.tewSourceCode;
            // 
            // tpGeneratedCode
            // 
            this.tpGeneratedCode.Controls.Add(this.ehGeneratedCode);
            this.tpGeneratedCode.Location = new System.Drawing.Point(4, 22);
            this.tpGeneratedCode.Name = "tpGeneratedCode";
            this.tpGeneratedCode.Padding = new System.Windows.Forms.Padding(3);
            this.tpGeneratedCode.Size = new System.Drawing.Size(534, 112);
            this.tpGeneratedCode.TabIndex = 1;
            this.tpGeneratedCode.Text = "Generated Code";
            this.tpGeneratedCode.UseVisualStyleBackColor = true;
            // 
            // ehGeneratedCode
            // 
            this.ehGeneratedCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ehGeneratedCode.Location = new System.Drawing.Point(3, 3);
            this.ehGeneratedCode.Name = "ehGeneratedCode";
            this.ehGeneratedCode.Size = new System.Drawing.Size(528, 106);
            this.ehGeneratedCode.TabIndex = 0;
            this.ehGeneratedCode.Child = this.tewGeneratedCode;
            // 
            // ExecutionResultViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(584, 362);
            this.Controls.Add(this.gbResult);
            this.Controls.Add(this.btnClose);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "ExecutionResultViewerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ExecutionResultViewerForm";
            this.gbResult.ResumeLayout(false);
            this.scDetails.Panel1.ResumeLayout(false);
            this.scDetails.Panel1.PerformLayout();
            this.scDetails.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scDetails)).EndInit();
            this.scDetails.ResumeLayout(false);
            this.tcResults.ResumeLayout(false);
            this.tpReturnValue.ResumeLayout(false);
            this.tpConsoleOut.ResumeLayout(false);
            this.tpConsoleError.ResumeLayout(false);
            this.tpSourceCode.ResumeLayout(false);
            this.tpGeneratedCode.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.GroupBox gbResult;
        private System.Windows.Forms.SplitContainer scDetails;
        private System.Windows.Forms.TextBox tbMessage;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.TabControl tcResults;
        private System.Windows.Forms.TabPage tpSourceCode;
        private System.Windows.Forms.TabPage tpGeneratedCode;
        private System.Windows.Forms.Integration.ElementHost ehSourceCode;
        private TextEditorWrapper tewSourceCode;
        private System.Windows.Forms.Integration.ElementHost ehGeneratedCode;
        private TextEditorWrapper tewGeneratedCode;
        private System.Windows.Forms.TabPage tpConsoleOut;
        private System.Windows.Forms.RichTextBox rtbConsoleOut;
        private System.Windows.Forms.TabPage tpConsoleError;
        private System.Windows.Forms.RichTextBox rtbConsoleError;
        private System.Windows.Forms.TabPage tpReturnValue;
        private System.Windows.Forms.PropertyGrid pgReturnValue;
    }
}