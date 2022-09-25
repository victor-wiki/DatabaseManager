namespace DatabaseManager
{
    partial class frmDiagnose
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDiagnose));
            this.rbNotNullWithEmpty = new System.Windows.Forms.RadioButton();
            this.rbSelfReferenceSame = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.rbWithLeadingOrTrailingWhitespace = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // rbNotNullWithEmpty
            // 
            this.rbNotNullWithEmpty.AutoSize = true;
            this.rbNotNullWithEmpty.Checked = true;
            this.rbNotNullWithEmpty.Location = new System.Drawing.Point(78, 17);
            this.rbNotNullWithEmpty.Margin = new System.Windows.Forms.Padding(4);
            this.rbNotNullWithEmpty.Name = "rbNotNullWithEmpty";
            this.rbNotNullWithEmpty.Size = new System.Drawing.Size(199, 21);
            this.rbNotNullWithEmpty.TabIndex = 0;
            this.rbNotNullWithEmpty.TabStop = true;
            this.rbNotNullWithEmpty.Text = "not null field with empty value";
            this.rbNotNullWithEmpty.UseVisualStyleBackColor = true;
            // 
            // rbSelfReferenceSame
            // 
            this.rbSelfReferenceSame.AutoSize = true;
            this.rbSelfReferenceSame.Location = new System.Drawing.Point(78, 74);
            this.rbSelfReferenceSame.Margin = new System.Windows.Forms.Padding(4);
            this.rbSelfReferenceSame.Name = "rbSelfReferenceSame";
            this.rbSelfReferenceSame.Size = new System.Drawing.Size(196, 21);
            this.rbSelfReferenceSame.TabIndex = 1;
            this.rbSelfReferenceSame.Text = "table self refer to same value";
            this.rbSelfReferenceSame.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Type:";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(110, 125);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(88, 33);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(217, 125);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 33);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // rbWithLeadingOrTrailingWhitespace
            // 
            this.rbWithLeadingOrTrailingWhitespace.AutoSize = true;
            this.rbWithLeadingOrTrailingWhitespace.Location = new System.Drawing.Point(78, 46);
            this.rbWithLeadingOrTrailingWhitespace.Margin = new System.Windows.Forms.Padding(4);
            this.rbWithLeadingOrTrailingWhitespace.Name = "rbWithLeadingOrTrailingWhitespace";
            this.rbWithLeadingOrTrailingWhitespace.Size = new System.Drawing.Size(225, 21);
            this.rbWithLeadingOrTrailingWhitespace.TabIndex = 5;
            this.rbWithLeadingOrTrailingWhitespace.Text = "with leading or trailing whitespace";
            this.rbWithLeadingOrTrailingWhitespace.UseVisualStyleBackColor = true;
            // 
            // frmDiagnose
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 171);
            this.Controls.Add(this.rbWithLeadingOrTrailingWhitespace);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rbSelfReferenceSame);
            this.Controls.Add(this.rbNotNullWithEmpty);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmDiagnose";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Diagnose";
            this.Load += new System.EventHandler(this.frmDiagnose_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbNotNullWithEmpty;
        private System.Windows.Forms.RadioButton rbSelfReferenceSame;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.RadioButton rbWithLeadingOrTrailingWhitespace;
    }
}