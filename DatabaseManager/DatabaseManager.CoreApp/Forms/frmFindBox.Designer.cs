namespace DatabaseManager.Forms
{
    partial class frmFindBox
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFindBox));
            this.txtWord = new System.Windows.Forms.TextBox();
            this.btnFind = new System.Windows.Forms.Button();
            this.chkMatchCase = new System.Windows.Forms.CheckBox();
            this.chkMatchWholeWord = new System.Windows.Forms.CheckBox();
            this.optionsPanel = new System.Windows.Forms.Panel();
            this.optionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtWord
            // 
            this.txtWord.Location = new System.Drawing.Point(12, 10);
            this.txtWord.Name = "txtWord";
            this.txtWord.Size = new System.Drawing.Size(191, 23);
            this.txtWord.TabIndex = 0;
            this.txtWord.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtWord_KeyUp);
            // 
            // btnFind
            // 
            this.btnFind.Location = new System.Drawing.Point(210, 9);
            this.btnFind.Name = "btnFind";
            this.btnFind.Size = new System.Drawing.Size(75, 25);
            this.btnFind.TabIndex = 1;
            this.btnFind.Text = "Find";
            this.btnFind.UseVisualStyleBackColor = true;
            this.btnFind.Click += new System.EventHandler(this.btnFind_Click);
            // 
            // chkMatchCase
            // 
            this.chkMatchCase.AutoSize = true;
            this.chkMatchCase.Location = new System.Drawing.Point(4, 3);
            this.chkMatchCase.Name = "chkMatchCase";
            this.chkMatchCase.Size = new System.Drawing.Size(93, 21);
            this.chkMatchCase.TabIndex = 2;
            this.chkMatchCase.Text = "Match case";
            this.chkMatchCase.UseVisualStyleBackColor = true;
            // 
            // chkMatchWholeWord
            // 
            this.chkMatchWholeWord.AutoSize = true;
            this.chkMatchWholeWord.Location = new System.Drawing.Point(125, 3);
            this.chkMatchWholeWord.Name = "chkMatchWholeWord";
            this.chkMatchWholeWord.Size = new System.Drawing.Size(135, 21);
            this.chkMatchWholeWord.TabIndex = 3;
            this.chkMatchWholeWord.Text = "Match whole word";
            this.chkMatchWholeWord.UseVisualStyleBackColor = true;
            // 
            // optionsPanel
            // 
            this.optionsPanel.Controls.Add(this.chkMatchWholeWord);
            this.optionsPanel.Controls.Add(this.chkMatchCase);
            this.optionsPanel.Location = new System.Drawing.Point(12, 38);
            this.optionsPanel.Name = "optionsPanel";
            this.optionsPanel.Size = new System.Drawing.Size(273, 23);
            this.optionsPanel.TabIndex = 4;
            // 
            // frmFindBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 62);
            this.Controls.Add(this.optionsPanel);
            this.Controls.Add(this.btnFind);
            this.Controls.Add(this.txtWord);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmFindBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Find Text";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmFindBox_FormClosed);
            this.Load += new System.EventHandler(this.frmFindBox_Load);
            this.optionsPanel.ResumeLayout(false);
            this.optionsPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtWord;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.CheckBox chkMatchCase;
        private System.Windows.Forms.CheckBox chkMatchWholeWord;
        private System.Windows.Forms.Panel optionsPanel;
    }
}