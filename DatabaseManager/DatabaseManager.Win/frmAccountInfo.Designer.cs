namespace DatabaseManager
{
    partial class frmAccountInfo
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
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.ucAccountInfo = new DatabaseManager.Controls.UC_DbAccountInfo();
            this.SuspendLayout();
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(210, 197);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 11;
            this.btnConfirm.Text = "OK";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(113, 197);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 10;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // ucAccountInfo
            // 
            this.ucAccountInfo.DatabaseType = DatabaseInterpreter.Model.DatabaseType.SqlServer;
            this.ucAccountInfo.Location = new System.Drawing.Point(3, 12);
            this.ucAccountInfo.Name = "ucAccountInfo";
            this.ucAccountInfo.Size = new System.Drawing.Size(381, 178);
            this.ucAccountInfo.TabIndex = 0;
            // 
            // frmAccountInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(386, 232);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.ucAccountInfo);
            this.MaximizeBox = false;
            this.Name = "frmAccountInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database account";
            this.Activated += new System.EventHandler(this.frmAccountInfo_Activated);
            this.Load += new System.EventHandler(this.frmAccountInfo_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.UC_DbAccountInfo ucAccountInfo;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnTest;
    }
}