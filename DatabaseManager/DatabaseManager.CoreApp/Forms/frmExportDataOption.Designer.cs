namespace DatabaseManager.Forms
{
    partial class frmExportDataOption
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
            label1 = new System.Windows.Forms.Label();
            cboFileType = new System.Windows.Forms.ComboBox();
            chkShowColumnName = new System.Windows.Forms.CheckBox();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            label2 = new System.Windows.Forms.Label();
            txtFilePath = new System.Windows.Forms.TextBox();
            btnSelectPath = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(62, 17);
            label1.TabIndex = 0;
            label1.Text = "File Type:";
            // 
            // cboFileType
            // 
            cboFileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboFileType.FormattingEnabled = true;
            cboFileType.Items.AddRange(new object[] { "CSV", "EXCEL" });
            cboFileType.Location = new System.Drawing.Point(78, 6);
            cboFileType.Name = "cboFileType";
            cboFileType.Size = new System.Drawing.Size(216, 25);
            cboFileType.TabIndex = 1;
            // 
            // chkShowColumnName
            // 
            chkShowColumnName.AutoSize = true;
            chkShowColumnName.Location = new System.Drawing.Point(12, 47);
            chkShowColumnName.Name = "chkShowColumnName";
            chkShowColumnName.Size = new System.Drawing.Size(145, 21);
            chkShowColumnName.TabIndex = 5;
            chkShowColumnName.Text = "Show Column Name";
            chkShowColumnName.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(71, 144);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 6;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(182, 144);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 92);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(67, 17);
            label2.TabIndex = 8;
            label2.Text = "Save Path:";
            // 
            // txtFilePath
            // 
            txtFilePath.Location = new System.Drawing.Point(81, 89);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new System.Drawing.Size(214, 23);
            txtFilePath.TabIndex = 9;
            // 
            // btnSelectPath
            // 
            btnSelectPath.Location = new System.Drawing.Point(301, 89);
            btnSelectPath.Name = "btnSelectPath";
            btnSelectPath.Size = new System.Drawing.Size(40, 23);
            btnSelectPath.TabIndex = 10;
            btnSelectPath.Text = "...";
            btnSelectPath.UseVisualStyleBackColor = true;
            btnSelectPath.Click += btnSelectPath_Click;
            // 
            // frmExportData
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(345, 190);
            Controls.Add(btnSelectPath);
            Controls.Add(txtFilePath);
            Controls.Add(label2);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(chkShowColumnName);
            Controls.Add(cboFileType);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmExportData";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Export Data";
            Load += frmExportData_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboFileType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkShowColumnName;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnSelectPath;
    }
}