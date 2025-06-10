namespace DatabaseManager.Forms
{
    partial class frmMessage
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
            txtMessage = new System.Windows.Forms.RichTextBox();
            SuspendLayout();
            // 
            // txtMessage
            // 
            txtMessage.BackColor = System.Drawing.Color.White;
            txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            txtMessage.Location = new System.Drawing.Point(0, 0);
            txtMessage.Name = "txtMessage";
            txtMessage.ReadOnly = true;
            txtMessage.Size = new System.Drawing.Size(800, 138);
            txtMessage.TabIndex = 0;
            txtMessage.Text = "";
            // 
            // frmMessage
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 138);
            Controls.Add(txtMessage);
            Name = "frmMessage";
            Text = "Message";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RichTextBox txtMessage;
    }
}