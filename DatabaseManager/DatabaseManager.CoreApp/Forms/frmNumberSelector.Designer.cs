namespace DatabaseManager.Forms
{
    partial class frmNumberSelector
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
            lblTitle = new System.Windows.Forms.Label();
            nudValue = new System.Windows.Forms.NumericUpDown();
            btnOK = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)nudValue).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new System.Drawing.Point(9, 7);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(0, 17);
            lblTitle.TabIndex = 0;
            // 
            // nudValue
            // 
            nudValue.Location = new System.Drawing.Point(9, 29);
            nudValue.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudValue.Name = "nudValue";
            nudValue.Size = new System.Drawing.Size(191, 23);
            nudValue.TabIndex = 1;
            nudValue.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(19, 65);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(104, 65);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(75, 23);
            button2.TabIndex = 3;
            button2.Text = "Cancel";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // frmNumberSelector
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(208, 91);
            Controls.Add(button2);
            Controls.Add(btnOK);
            Controls.Add(nudValue);
            Controls.Add(lblTitle);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmNumberSelector";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Load += frmNumberSelector_Load;
            ((System.ComponentModel.ISupportInitialize)nudValue).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.NumericUpDown nudValue;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button button2;
    }
}