namespace DatabaseManager.Forms
{
    partial class frmImportData
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
            btnOpenFile = new System.Windows.Forms.Button();
            txtFilePath = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            chkFirstRowIsColumnName = new System.Windows.Forms.CheckBox();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            btnClose = new System.Windows.Forms.Button();
            btnOK = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // btnOpenFile
            // 
            btnOpenFile.Location = new System.Drawing.Point(377, 6);
            btnOpenFile.Name = "btnOpenFile";
            btnOpenFile.Size = new System.Drawing.Size(40, 23);
            btnOpenFile.TabIndex = 14;
            btnOpenFile.Text = "...";
            btnOpenFile.UseVisualStyleBackColor = true;
            btnOpenFile.Click += btnOpenFile_Click;
            // 
            // txtFilePath
            // 
            txtFilePath.Location = new System.Drawing.Point(81, 6);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new System.Drawing.Size(290, 23);
            txtFilePath.TabIndex = 13;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(10, 9);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(59, 17);
            label2.TabIndex = 12;
            label2.Text = "File Path:";
            // 
            // chkFirstRowIsColumnName
            // 
            chkFirstRowIsColumnName.AutoSize = true;
            chkFirstRowIsColumnName.Location = new System.Drawing.Point(12, 44);
            chkFirstRowIsColumnName.Name = "chkFirstRowIsColumnName";
            chkFirstRowIsColumnName.Size = new System.Drawing.Size(172, 21);
            chkFirstRowIsColumnName.TabIndex = 11;
            chkFirstRowIsColumnName.Text = "First row is column name";
            chkFirstRowIsColumnName.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.Filter = "csv file|*.csv|excel file|*.xlsx|excel file|*.xls";
            // 
            // btnClose
            // 
            btnClose.Location = new System.Drawing.Point(246, 99);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(75, 23);
            btnClose.TabIndex = 16;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(135, 99);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 15;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // frmImportData
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(437, 143);
            Controls.Add(btnClose);
            Controls.Add(btnOK);
            Controls.Add(btnOpenFile);
            Controls.Add(txtFilePath);
            Controls.Add(label2);
            Controls.Add(chkFirstRowIsColumnName);
            Name = "frmImportData";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Import Data";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkFirstRowIsColumnName;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOK;
    }
}