namespace DatabaseManager.Forms
{
    partial class frmContent
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
            ucContent = new Controls.UC_DbObjectContent();
            SuspendLayout();
            // 
            // ucContent
            // 
            ucContent.BackColor = System.Drawing.Color.White;
            ucContent.Dock = System.Windows.Forms.DockStyle.Fill;
            ucContent.Location = new System.Drawing.Point(0, 0);
            ucContent.Margin = new System.Windows.Forms.Padding(4);
            ucContent.Name = "ucContent";
            ucContent.Size = new System.Drawing.Size(800, 450);
            ucContent.TabIndex = 0;
            // 
            // frmContent
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(ucContent);
            Name = "frmContent";
            Text = "Content";
            FormClosing += frmContent_FormClosing;
            ResumeLayout(false);
        }

        #endregion

        private Controls.UC_DbObjectContent ucContent;
    }
}