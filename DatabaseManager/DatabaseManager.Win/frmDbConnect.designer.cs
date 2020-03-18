namespace DatabaseManager
{
    partial class frmDbConnect
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
            this.btnTest = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.cboDatabase = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtProfileName = new System.Windows.Forms.TextBox();
            this.ucDbAccountInfo = new DatabaseManager.Controls.UC_DbAccountInfo();
            this.SuspendLayout();
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(118, 251);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 8;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(215, 251);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 9;
            this.btnConfirm.Text = "OK";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(14, 208);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(59, 12);
            this.lblDatabase.TabIndex = 11;
            this.lblDatabase.Text = "Database:";
            // 
            // cboDatabase
            // 
            this.cboDatabase.FormattingEnabled = true;
            this.cboDatabase.Location = new System.Drawing.Point(123, 205);
            this.cboDatabase.Name = "cboDatabase";
            this.cboDatabase.Size = new System.Drawing.Size(166, 20);
            this.cboDatabase.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 16;
            this.label1.Text = "Profile name:";
            // 
            // txtProfileName
            // 
            this.txtProfileName.Location = new System.Drawing.Point(123, 10);
            this.txtProfileName.Name = "txtProfileName";
            this.txtProfileName.Size = new System.Drawing.Size(166, 21);
            this.txtProfileName.TabIndex = 0;
            // 
            // ucDbAccountInfo
            // 
            this.ucDbAccountInfo.DatabaseType = DatabaseInterpreter.Model.DatabaseType.SqlServer;
            this.ucDbAccountInfo.Location = new System.Drawing.Point(5, 37);
            this.ucDbAccountInfo.Name = "ucDbAccountInfo";
            this.ucDbAccountInfo.Size = new System.Drawing.Size(373, 162);
            this.ucDbAccountInfo.TabIndex = 1;
            // 
            // frmDbConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(390, 286);
            this.Controls.Add(this.ucDbAccountInfo);
            this.Controls.Add(this.txtProfileName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboDatabase);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.btnTest);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDbConnect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database connection";
            this.Activated += new System.EventHandler(this.frmDbConnect_Activated);
            this.Load += new System.EventHandler(this.frmDbConnect_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.ComboBox cboDatabase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtProfileName;
        private Controls.UC_DbAccountInfo ucDbAccountInfo;
    }
}