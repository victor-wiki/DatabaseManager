namespace DatabaseManager
{
    partial class frmBackupSettingRedefine
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBackupSettingRedefine));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.lblOutputFolder = new System.Windows.Forms.Label();
            this.btnSaveFolder = new System.Windows.Forms.Button();
            this.txtSaveFolder = new System.Windows.Forms.TextBox();
            this.chkZipFile = new System.Windows.Forms.CheckBox();
            this.chkSetAsDefault = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(272, 156);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 33);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConfirm.Location = new System.Drawing.Point(150, 156);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(4);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(88, 33);
            this.btnConfirm.TabIndex = 12;
            this.btnConfirm.Text = "OK";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.Location = new System.Drawing.Point(14, 16);
            this.lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(79, 17);
            this.lblOutputFolder.TabIndex = 55;
            this.lblOutputFolder.Text = "Save Folder:";
            // 
            // btnSaveFolder
            // 
            this.btnSaveFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveFolder.Location = new System.Drawing.Point(469, 11);
            this.btnSaveFolder.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveFolder.Name = "btnSaveFolder";
            this.btnSaveFolder.Size = new System.Drawing.Size(42, 23);
            this.btnSaveFolder.TabIndex = 54;
            this.btnSaveFolder.Text = "...";
            this.btnSaveFolder.UseVisualStyleBackColor = true;
            this.btnSaveFolder.Click += new System.EventHandler(this.btnSaveFolder_Click);
            // 
            // txtSaveFolder
            // 
            this.txtSaveFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtSaveFolder.Location = new System.Drawing.Point(125, 11);
            this.txtSaveFolder.Margin = new System.Windows.Forms.Padding(4);
            this.txtSaveFolder.Name = "txtSaveFolder";
            this.txtSaveFolder.Size = new System.Drawing.Size(339, 23);
            this.txtSaveFolder.TabIndex = 53;
            // 
            // chkZipFile
            // 
            this.chkZipFile.AutoSize = true;
            this.chkZipFile.Location = new System.Drawing.Point(125, 64);
            this.chkZipFile.Margin = new System.Windows.Forms.Padding(4);
            this.chkZipFile.Name = "chkZipFile";
            this.chkZipFile.Size = new System.Drawing.Size(113, 21);
            this.chkZipFile.TabIndex = 56;
            this.chkZipFile.Text = "Zip backup file";
            this.chkZipFile.UseVisualStyleBackColor = true;
            // 
            // chkSetAsDefault
            // 
            this.chkSetAsDefault.AutoSize = true;
            this.chkSetAsDefault.Location = new System.Drawing.Point(125, 105);
            this.chkSetAsDefault.Margin = new System.Windows.Forms.Padding(4);
            this.chkSetAsDefault.Name = "chkSetAsDefault";
            this.chkSetAsDefault.Size = new System.Drawing.Size(189, 21);
            this.chkSetAsDefault.TabIndex = 57;
            this.chkSetAsDefault.Text = "Set above setting as default";
            this.chkSetAsDefault.UseVisualStyleBackColor = true;
            // 
            // frmBackupSettingRedefine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 205);
            this.Controls.Add(this.chkSetAsDefault);
            this.Controls.Add(this.chkZipFile);
            this.Controls.Add(this.lblOutputFolder);
            this.Controls.Add(this.btnSaveFolder);
            this.Controls.Add(this.txtSaveFolder);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmBackupSettingRedefine";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Backup Setting";
            this.Load += new System.EventHandler(this.frmBackupSettingRedefine_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.Button btnSaveFolder;
        private System.Windows.Forms.TextBox txtSaveFolder;
        private System.Windows.Forms.CheckBox chkZipFile;
        private System.Windows.Forms.CheckBox chkSetAsDefault;
    }
}