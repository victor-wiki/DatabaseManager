namespace DatabaseManager.Forms
{
    partial class frmInput
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
            txtContent = new System.Windows.Forms.TextBox();
            btnOK = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // txtContent
            // 
            txtContent.Location = new System.Drawing.Point(12, 10);
            txtContent.Name = "txtContent";
            txtContent.Size = new System.Drawing.Size(429, 23);
            txtContent.TabIndex = 0;
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(447, 8);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 25);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // frmInput
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(533, 42);
            Controls.Add(btnOK);
            Controls.Add(txtContent);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmInput";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Load += frmInput_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.Button btnOK;
    }
}