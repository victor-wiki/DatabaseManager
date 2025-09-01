namespace DatabaseManager
{
    partial class frmTableCopy
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTableCopy));
            rbSameDatabase = new System.Windows.Forms.RadioButton();
            rbAnotherDatabase = new System.Windows.Forms.RadioButton();
            chkScriptData = new System.Windows.Forms.CheckBox();
            chkScriptSchema = new System.Windows.Forms.CheckBox();
            lblScriptsMode = new System.Windows.Forms.Label();
            btnClose = new System.Windows.Forms.Button();
            btnExecute = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            ucConnection = new Controls.UC_DbConnectionProfile();
            chkGenerateIdentity = new System.Windows.Forms.CheckBox();
            lblSchema = new System.Windows.Forms.Label();
            cboSchema = new System.Windows.Forms.ComboBox();
            chkPrimaryKey = new System.Windows.Forms.CheckBox();
            chkForeignKey = new System.Windows.Forms.CheckBox();
            chkIndex = new System.Windows.Forms.CheckBox();
            chkCheckConstraint = new System.Windows.Forms.CheckBox();
            chkTrigger = new System.Windows.Forms.CheckBox();
            lblDataTypeMappingFileType = new System.Windows.Forms.LinkLabel();
            cboDataTypeMappingFile = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            chkNeedPreview = new System.Windows.Forms.CheckBox();
            picNeedPreviewTip = new System.Windows.Forms.PictureBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)picNeedPreviewTip).BeginInit();
            SuspendLayout();
            // 
            // rbSameDatabase
            // 
            rbSameDatabase.AutoSize = true;
            rbSameDatabase.Checked = true;
            rbSameDatabase.Location = new System.Drawing.Point(16, 8);
            rbSameDatabase.Margin = new System.Windows.Forms.Padding(4);
            rbSameDatabase.Name = "rbSameDatabase";
            rbSameDatabase.Size = new System.Drawing.Size(115, 21);
            rbSameDatabase.TabIndex = 1;
            rbSameDatabase.TabStop = true;
            rbSameDatabase.Text = "same database";
            rbSameDatabase.UseVisualStyleBackColor = true;
            rbSameDatabase.CheckedChanged += rbSameDatabase_CheckedChanged;
            // 
            // rbAnotherDatabase
            // 
            rbAnotherDatabase.AutoSize = true;
            rbAnotherDatabase.Location = new System.Drawing.Point(144, 8);
            rbAnotherDatabase.Margin = new System.Windows.Forms.Padding(4);
            rbAnotherDatabase.Name = "rbAnotherDatabase";
            rbAnotherDatabase.Size = new System.Drawing.Size(129, 21);
            rbAnotherDatabase.TabIndex = 2;
            rbAnotherDatabase.Text = "another database";
            rbAnotherDatabase.UseVisualStyleBackColor = true;
            // 
            // chkScriptData
            // 
            chkScriptData.AutoSize = true;
            chkScriptData.Checked = true;
            chkScriptData.CheckState = System.Windows.Forms.CheckState.Checked;
            chkScriptData.Location = new System.Drawing.Point(176, 180);
            chkScriptData.Margin = new System.Windows.Forms.Padding(4);
            chkScriptData.Name = "chkScriptData";
            chkScriptData.Size = new System.Drawing.Size(53, 21);
            chkScriptData.TabIndex = 15;
            chkScriptData.Text = "data";
            chkScriptData.UseVisualStyleBackColor = true;
            chkScriptData.CheckedChanged += chkScriptData_CheckedChanged;
            // 
            // chkScriptSchema
            // 
            chkScriptSchema.AutoSize = true;
            chkScriptSchema.Checked = true;
            chkScriptSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            chkScriptSchema.Location = new System.Drawing.Point(83, 180);
            chkScriptSchema.Margin = new System.Windows.Forms.Padding(4);
            chkScriptSchema.Name = "chkScriptSchema";
            chkScriptSchema.Size = new System.Drawing.Size(71, 21);
            chkScriptSchema.TabIndex = 14;
            chkScriptSchema.Text = "schema";
            chkScriptSchema.UseVisualStyleBackColor = true;
            chkScriptSchema.CheckedChanged += chkScriptSchema_CheckedChanged;
            // 
            // lblScriptsMode
            // 
            lblScriptsMode.AutoSize = true;
            lblScriptsMode.Location = new System.Drawing.Point(15, 181);
            lblScriptsMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblScriptsMode.Name = "lblScriptsMode";
            lblScriptsMode.Size = new System.Drawing.Size(46, 17);
            lblScriptsMode.TabIndex = 13;
            lblScriptsMode.Text = "Mode:";
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnClose.Location = new System.Drawing.Point(371, 328);
            btnClose.Margin = new System.Windows.Forms.Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(88, 33);
            btnClose.TabIndex = 24;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnCancel_Click;
            // 
            // btnExecute
            // 
            btnExecute.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnExecute.Location = new System.Drawing.Point(258, 328);
            btnExecute.Margin = new System.Windows.Forms.Padding(4);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new System.Drawing.Size(88, 33);
            btnExecute.TabIndex = 23;
            btnExecute.Text = "Execute";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(16, 93);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(46, 17);
            label1.TabIndex = 25;
            label1.Text = "Name:";
            // 
            // txtName
            // 
            txtName.Location = new System.Drawing.Point(83, 89);
            txtName.Margin = new System.Windows.Forms.Padding(4);
            txtName.Name = "txtName";
            txtName.Size = new System.Drawing.Size(302, 23);
            txtName.TabIndex = 26;
            // 
            // ucConnection
            // 
            ucConnection.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ucConnection.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            ucConnection.Enabled = false;
            ucConnection.EnableDatabaseType = true;
            ucConnection.Location = new System.Drawing.Point(12, 42);
            ucConnection.Margin = new System.Windows.Forms.Padding(0);
            ucConnection.Name = "ucConnection";
            ucConnection.Size = new System.Drawing.Size(666, 33);
            ucConnection.TabIndex = 0;
            ucConnection.Title = "Target:";
            ucConnection.ProfileSelectedChanged += ucConnection_ProfileSelectedChanged;
            ucConnection.DatabaseTypeSelectedChanged += ucConnection_DatabaseTypeSelectedChanged;
            // 
            // chkGenerateIdentity
            // 
            chkGenerateIdentity.AutoSize = true;
            chkGenerateIdentity.Checked = true;
            chkGenerateIdentity.CheckState = System.Windows.Forms.CheckState.Checked;
            chkGenerateIdentity.Location = new System.Drawing.Point(19, 240);
            chkGenerateIdentity.Margin = new System.Windows.Forms.Padding(4);
            chkGenerateIdentity.Name = "chkGenerateIdentity";
            chkGenerateIdentity.Size = new System.Drawing.Size(126, 21);
            chkGenerateIdentity.TabIndex = 27;
            chkGenerateIdentity.Text = "Generate identity";
            chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // lblSchema
            // 
            lblSchema.AutoSize = true;
            lblSchema.Location = new System.Drawing.Point(449, 92);
            lblSchema.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblSchema.Name = "lblSchema";
            lblSchema.Size = new System.Drawing.Size(56, 17);
            lblSchema.TabIndex = 29;
            lblSchema.Text = "Schema:";
            // 
            // cboSchema
            // 
            cboSchema.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboSchema.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cboSchema.FormattingEnabled = true;
            cboSchema.Location = new System.Drawing.Point(517, 87);
            cboSchema.Name = "cboSchema";
            cboSchema.Size = new System.Drawing.Size(121, 25);
            cboSchema.TabIndex = 30;
            // 
            // chkPrimaryKey
            // 
            chkPrimaryKey.AutoSize = true;
            chkPrimaryKey.Location = new System.Drawing.Point(19, 211);
            chkPrimaryKey.Margin = new System.Windows.Forms.Padding(4);
            chkPrimaryKey.Name = "chkPrimaryKey";
            chkPrimaryKey.Size = new System.Drawing.Size(96, 21);
            chkPrimaryKey.TabIndex = 31;
            chkPrimaryKey.Text = "Primary Key";
            chkPrimaryKey.UseVisualStyleBackColor = true;
            // 
            // chkForeignKey
            // 
            chkForeignKey.AutoSize = true;
            chkForeignKey.Location = new System.Drawing.Point(143, 211);
            chkForeignKey.Margin = new System.Windows.Forms.Padding(4);
            chkForeignKey.Name = "chkForeignKey";
            chkForeignKey.Size = new System.Drawing.Size(96, 21);
            chkForeignKey.TabIndex = 32;
            chkForeignKey.Text = "Foreign Key";
            chkForeignKey.UseVisualStyleBackColor = true;
            // 
            // chkIndex
            // 
            chkIndex.AutoSize = true;
            chkIndex.Location = new System.Drawing.Point(267, 211);
            chkIndex.Margin = new System.Windows.Forms.Padding(4);
            chkIndex.Name = "chkIndex";
            chkIndex.Size = new System.Drawing.Size(59, 21);
            chkIndex.TabIndex = 33;
            chkIndex.Text = "Index";
            chkIndex.UseVisualStyleBackColor = true;
            // 
            // chkCheckConstraint
            // 
            chkCheckConstraint.AutoSize = true;
            chkCheckConstraint.Location = new System.Drawing.Point(352, 211);
            chkCheckConstraint.Margin = new System.Windows.Forms.Padding(4);
            chkCheckConstraint.Name = "chkCheckConstraint";
            chkCheckConstraint.Size = new System.Drawing.Size(125, 21);
            chkCheckConstraint.TabIndex = 34;
            chkCheckConstraint.Text = "Check Constraint";
            chkCheckConstraint.UseVisualStyleBackColor = true;
            // 
            // chkTrigger
            // 
            chkTrigger.AutoSize = true;
            chkTrigger.Location = new System.Drawing.Point(512, 211);
            chkTrigger.Margin = new System.Windows.Forms.Padding(4);
            chkTrigger.Name = "chkTrigger";
            chkTrigger.Size = new System.Drawing.Size(70, 21);
            chkTrigger.TabIndex = 35;
            chkTrigger.Text = "Trigger";
            chkTrigger.UseVisualStyleBackColor = true;
            // 
            // lblDataTypeMappingFileType
            // 
            lblDataTypeMappingFileType.AutoSize = true;
            lblDataTypeMappingFileType.Location = new System.Drawing.Point(331, 142);
            lblDataTypeMappingFileType.Name = "lblDataTypeMappingFileType";
            lblDataTypeMappingFileType.Size = new System.Drawing.Size(0, 17);
            lblDataTypeMappingFileType.TabIndex = 71;
            lblDataTypeMappingFileType.Click += lblDataTypeMappingFileType_Click;
            // 
            // cboDataTypeMappingFile
            // 
            cboDataTypeMappingFile.DisplayMember = "Name";
            cboDataTypeMappingFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDataTypeMappingFile.FormattingEnabled = true;
            cboDataTypeMappingFile.Location = new System.Drawing.Point(163, 137);
            cboDataTypeMappingFile.Name = "cboDataTypeMappingFile";
            cboDataTypeMappingFile.Size = new System.Drawing.Size(162, 25);
            cboDataTypeMappingFile.TabIndex = 70;
            cboDataTypeMappingFile.SelectedIndexChanged += cboDataTypeMappingFile_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(15, 140);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(144, 17);
            label2.TabIndex = 69;
            label2.Text = "Data type mapping file:";
            // 
            // chkNeedPreview
            // 
            chkNeedPreview.AutoSize = true;
            chkNeedPreview.Enabled = false;
            chkNeedPreview.Location = new System.Drawing.Point(19, 269);
            chkNeedPreview.Margin = new System.Windows.Forms.Padding(4);
            chkNeedPreview.Name = "chkNeedPreview";
            chkNeedPreview.Size = new System.Drawing.Size(108, 21);
            chkNeedPreview.TabIndex = 72;
            chkNeedPreview.Tag = "Schema";
            chkNeedPreview.Text = "Need preview";
            chkNeedPreview.UseVisualStyleBackColor = true;
            // 
            // picNeedPreviewTip
            // 
            picNeedPreviewTip.Location = new System.Drawing.Point(133, 269);
            picNeedPreviewTip.Name = "picNeedPreviewTip";
            picNeedPreviewTip.Size = new System.Drawing.Size(17, 19);
            picNeedPreviewTip.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            picNeedPreviewTip.TabIndex = 73;
            picNeedPreviewTip.TabStop = false;
            toolTip1.SetToolTip(picNeedPreviewTip, "You can edit the target data type in the previewer.");
            // 
            // frmTableCopy
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(686, 378);
            Controls.Add(picNeedPreviewTip);
            Controls.Add(chkNeedPreview);
            Controls.Add(lblDataTypeMappingFileType);
            Controls.Add(cboDataTypeMappingFile);
            Controls.Add(label2);
            Controls.Add(chkTrigger);
            Controls.Add(chkCheckConstraint);
            Controls.Add(chkIndex);
            Controls.Add(chkForeignKey);
            Controls.Add(chkPrimaryKey);
            Controls.Add(cboSchema);
            Controls.Add(lblSchema);
            Controls.Add(chkGenerateIdentity);
            Controls.Add(txtName);
            Controls.Add(label1);
            Controls.Add(btnClose);
            Controls.Add(btnExecute);
            Controls.Add(chkScriptData);
            Controls.Add(chkScriptSchema);
            Controls.Add(lblScriptsMode);
            Controls.Add(rbAnotherDatabase);
            Controls.Add(rbSameDatabase);
            Controls.Add(ucConnection);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            Name = "frmTableCopy";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Copy Table";
            Load += frmDbObjectCopy_Load;
            ((System.ComponentModel.ISupportInitialize)picNeedPreviewTip).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Controls.UC_DbConnectionProfile ucConnection;
        private System.Windows.Forms.RadioButton rbSameDatabase;
        private System.Windows.Forms.RadioButton rbAnotherDatabase;
        private System.Windows.Forms.CheckBox chkScriptData;
        private System.Windows.Forms.CheckBox chkScriptSchema;
        private System.Windows.Forms.Label lblScriptsMode;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.CheckBox chkGenerateIdentity;
        private System.Windows.Forms.Label lblSchema;
        private System.Windows.Forms.ComboBox cboSchema;
        private System.Windows.Forms.CheckBox chkPrimaryKey;
        private System.Windows.Forms.CheckBox chkForeignKey;
        private System.Windows.Forms.CheckBox chkIndex;
        private System.Windows.Forms.CheckBox chkCheckConstraint;
        private System.Windows.Forms.CheckBox chkTrigger;
        private System.Windows.Forms.LinkLabel lblDataTypeMappingFileType;
        private System.Windows.Forms.ComboBox cboDataTypeMappingFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkNeedPreview;
        private System.Windows.Forms.PictureBox picNeedPreviewTip;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}