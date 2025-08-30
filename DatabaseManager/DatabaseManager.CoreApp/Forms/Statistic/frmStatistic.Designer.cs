namespace DatabaseManager.Forms
{
    partial class frmStatistic
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
            rbTableRowCount = new System.Windows.Forms.RadioButton();
            rbColumnContentLength = new System.Windows.Forms.RadioButton();
            btnClose = new System.Windows.Forms.Button();
            btnStart = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // rbTableRowCount
            // 
            rbTableRowCount.AutoSize = true;
            rbTableRowCount.Location = new System.Drawing.Point(13, 13);
            rbTableRowCount.Margin = new System.Windows.Forms.Padding(4);
            rbTableRowCount.Name = "rbTableRowCount";
            rbTableRowCount.Size = new System.Drawing.Size(117, 21);
            rbTableRowCount.TabIndex = 7;
            rbTableRowCount.Text = "table row count";
            rbTableRowCount.UseVisualStyleBackColor = true;
            // 
            // rbColumnContentLength
            // 
            rbColumnContentLength.AutoSize = true;
            rbColumnContentLength.Location = new System.Drawing.Point(13, 41);
            rbColumnContentLength.Margin = new System.Windows.Forms.Padding(4);
            rbColumnContentLength.Name = "rbColumnContentLength";
            rbColumnContentLength.Size = new System.Drawing.Size(183, 21);
            rbColumnContentLength.TabIndex = 6;
            rbColumnContentLength.Text = "column content max length";
            rbColumnContentLength.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.Location = new System.Drawing.Point(192, 113);
            btnClose.Margin = new System.Windows.Forms.Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(88, 33);
            btnClose.TabIndex = 9;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnStart
            // 
            btnStart.Location = new System.Drawing.Point(85, 113);
            btnStart.Margin = new System.Windows.Forms.Padding(4);
            btnStart.Name = "btnStart";
            btnStart.Size = new System.Drawing.Size(88, 33);
            btnStart.TabIndex = 8;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // frmStatistic
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(375, 167);
            Controls.Add(btnClose);
            Controls.Add(btnStart);
            Controls.Add(rbTableRowCount);
            Controls.Add(rbColumnContentLength);
            MaximizeBox = false;
            Name = "frmStatistic";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Statistic";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RadioButton rbTableRowCount;
        private System.Windows.Forms.RadioButton rbColumnContentLength;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnStart;
    }
}