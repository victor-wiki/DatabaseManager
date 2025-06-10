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
            txtWord = new System.Windows.Forms.TextBox();
            btnFind = new System.Windows.Forms.Button();
            chkMatchCase = new System.Windows.Forms.CheckBox();
            chkMatchWholeWord = new System.Windows.Forms.CheckBox();
            optionsPanel = new System.Windows.Forms.Panel();
            optionsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // txtWord
            // 
            txtWord.Location = new System.Drawing.Point(12, 10);
            txtWord.Name = "txtWord";
            txtWord.Size = new System.Drawing.Size(191, 23);
            txtWord.TabIndex = 0;
            txtWord.KeyUp += txtWord_KeyUp;
            // 
            // btnFind
            // 
            btnFind.Location = new System.Drawing.Point(210, 9);
            btnFind.Name = "btnFind";
            btnFind.Size = new System.Drawing.Size(75, 25);
            btnFind.TabIndex = 1;
            btnFind.Text = "Find";
            btnFind.UseVisualStyleBackColor = true;
            btnFind.Click += btnFind_Click;
            // 
            // chkMatchCase
            // 
            chkMatchCase.AutoSize = true;
            chkMatchCase.Location = new System.Drawing.Point(4, 3);
            chkMatchCase.Name = "chkMatchCase";
            chkMatchCase.Size = new System.Drawing.Size(93, 21);
            chkMatchCase.TabIndex = 2;
            chkMatchCase.Text = "Match case";
            chkMatchCase.UseVisualStyleBackColor = true;
            // 
            // chkMatchWholeWord
            // 
            chkMatchWholeWord.AutoSize = true;
            chkMatchWholeWord.Location = new System.Drawing.Point(125, 3);
            chkMatchWholeWord.Name = "chkMatchWholeWord";
            chkMatchWholeWord.Size = new System.Drawing.Size(135, 21);
            chkMatchWholeWord.TabIndex = 3;
            chkMatchWholeWord.Text = "Match whole word";
            chkMatchWholeWord.UseVisualStyleBackColor = true;
            // 
            // optionsPanel
            // 
            optionsPanel.Controls.Add(chkMatchWholeWord);
            optionsPanel.Controls.Add(chkMatchCase);
            optionsPanel.Location = new System.Drawing.Point(12, 38);
            optionsPanel.Name = "optionsPanel";
            optionsPanel.Size = new System.Drawing.Size(273, 23);
            optionsPanel.TabIndex = 4;
            // 
            // frmFindBox
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(297, 62);
            Controls.Add(optionsPanel);
            Controls.Add(btnFind);
            Controls.Add(txtWord);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmFindBox";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Find Text";
            FormClosed += frmFindBox_FormClosed;
            Load += frmFindBox_Load;
            KeyDown += frmFindBox_KeyDown;
            optionsPanel.ResumeLayout(false);
            optionsPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtWord;
        private System.Windows.Forms.Button btnFind;
        private System.Windows.Forms.CheckBox chkMatchCase;
        private System.Windows.Forms.CheckBox chkMatchWholeWord;
        private System.Windows.Forms.Panel optionsPanel;
    }
}