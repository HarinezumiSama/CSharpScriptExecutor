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
            this.tcSource = new System.Windows.Forms.TabControl();
            this.tpSourceCode = new System.Windows.Forms.TabPage();
            this.tpGeneratedCode = new System.Windows.Forms.TabPage();
            this.ehSourceCode = new System.Windows.Forms.Integration.ElementHost();
            this.tewSourceCode = new CSharpScriptExecutor.TextEditorWrapper();
            this.ehGeneratedCode = new System.Windows.Forms.Integration.ElementHost();
            this.tewGeneratedCode = new CSharpScriptExecutor.TextEditorWrapper();
            this.gbResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scDetails)).BeginInit();
            this.scDetails.Panel1.SuspendLayout();
            this.scDetails.Panel2.SuspendLayout();
            this.scDetails.SuspendLayout();
            this.tcSource.SuspendLayout();
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
            this.scDetails.Panel2.Controls.Add(this.tcSource);
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
            this.tbMessage.Multiline = true;
            this.tbMessage.Name = "tbMessage";
            this.tbMessage.ReadOnly = true;
            this.tbMessage.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbMessage.Size = new System.Drawing.Size(536, 113);
            this.tbMessage.TabIndex = 1;
            this.tbMessage.WordWrap = false;
            // 
            // tcSource
            // 
            this.tcSource.Controls.Add(this.tpSourceCode);
            this.tcSource.Controls.Add(this.tpGeneratedCode);
            this.tcSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcSource.Location = new System.Drawing.Point(0, 0);
            this.tcSource.Name = "tcSource";
            this.tcSource.SelectedIndex = 0;
            this.tcSource.Size = new System.Drawing.Size(542, 138);
            this.tcSource.TabIndex = 0;
            this.tcSource.TabStop = false;
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
            // ehSourceCode
            // 
            this.ehSourceCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ehSourceCode.Location = new System.Drawing.Point(3, 3);
            this.ehSourceCode.Name = "ehSourceCode";
            this.ehSourceCode.Size = new System.Drawing.Size(528, 106);
            this.ehSourceCode.TabIndex = 0;
            this.ehSourceCode.Child = this.tewSourceCode;
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
            this.Name = "ExecutionResultViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ExecutionResultViewerForm";
            this.gbResult.ResumeLayout(false);
            this.scDetails.Panel1.ResumeLayout(false);
            this.scDetails.Panel1.PerformLayout();
            this.scDetails.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scDetails)).EndInit();
            this.scDetails.ResumeLayout(false);
            this.tcSource.ResumeLayout(false);
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
        private System.Windows.Forms.TabControl tcSource;
        private System.Windows.Forms.TabPage tpSourceCode;
        private System.Windows.Forms.TabPage tpGeneratedCode;
        private System.Windows.Forms.Integration.ElementHost ehSourceCode;
        private TextEditorWrapper tewSourceCode;
        private System.Windows.Forms.Integration.ElementHost ehGeneratedCode;
        private TextEditorWrapper tewGeneratedCode;
    }
}