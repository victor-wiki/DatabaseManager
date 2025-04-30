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
            chkScriptData.Location = new System.Drawing.Point(177, 137);
            chkScriptData.Margin = new System.Windows.Forms.Padding(4);
            chkScriptData.Name = "chkScriptData";
            chkScriptData.Size = new System.Drawing.Size(53, 21);
            chkScriptData.TabIndex = 15;
            chkScriptData.Text = "data";
            chkScriptData.UseVisualStyleBackColor = true;
            // 
            // chkScriptSchema
            // 
            chkScriptSchema.AutoSize = true;
            chkScriptSchema.Checked = true;
            chkScriptSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            chkScriptSchema.Location = new System.Drawing.Point(84, 137);
            chkScriptSchema.Margin = new System.Windows.Forms.Padding(4);
            chkScriptSchema.Name = "chkScriptSchema";
            chkScriptSchema.Size = new System.Drawing.Size(71, 21);
            chkScriptSchema.TabIndex = 14;
            chkScriptSchema.Text = "schema";
            chkScriptSchema.UseVisualStyleBackColor = true;
            // 
            // lblScriptsMode
            // 
            lblScriptsMode.AutoSize = true;
            lblScriptsMode.Location = new System.Drawing.Point(16, 138);
            lblScriptsMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblScriptsMode.Name = "lblScriptsMode";
            lblScriptsMode.Size = new System.Drawing.Size(46, 17);
            lblScriptsMode.TabIndex = 13;
            lblScriptsMode.Text = "Mode:";
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnClose.Location = new System.Drawing.Point(371, 230);
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
            btnExecute.Location = new System.Drawing.Point(258, 230);
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
            ucConnection.OnSelectedChanged += ucConnection_OnSelectedChanged;
            // 
            // chkGenerateIdentity
            // 
            chkGenerateIdentity.AutoSize = true;
            chkGenerateIdentity.Checked = true;
            chkGenerateIdentity.CheckState = System.Windows.Forms.CheckState.Checked;
            chkGenerateIdentity.Location = new System.Drawing.Point(20, 197);
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
            chkPrimaryKey.Location = new System.Drawing.Point(20, 168);
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
            chkForeignKey.Location = new System.Drawing.Point(144, 168);
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
            chkIndex.Location = new System.Drawing.Point(268, 168);
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
            chkCheckConstraint.Location = new System.Drawing.Point(353, 168);
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
            chkTrigger.Location = new System.Drawing.Point(513, 168);
            chkTrigger.Margin = new System.Windows.Forms.Padding(4);
            chkTrigger.Name = "chkTrigger";
            chkTrigger.Size = new System.Drawing.Size(70, 21);
            chkTrigger.TabIndex = 35;
            chkTrigger.Text = "Trigger";
            chkTrigger.UseVisualStyleBackColor = true;
            // 
            // frmTableCopy
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(686, 280);
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
    }
}