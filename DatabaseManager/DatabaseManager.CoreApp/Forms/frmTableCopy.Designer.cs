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
            this.rbSameDatabase = new System.Windows.Forms.RadioButton();
            this.rbAnotherDatabase = new System.Windows.Forms.RadioButton();
            this.chkScriptData = new System.Windows.Forms.CheckBox();
            this.chkScriptSchema = new System.Windows.Forms.CheckBox();
            this.lblScriptsMode = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.ucConnection = new DatabaseManager.Controls.UC_DbConnectionProfile();
            this.chkGenerateIdentity = new System.Windows.Forms.CheckBox();
            this.chkOnlyCopyTable = new System.Windows.Forms.CheckBox();
            this.lblSchema = new System.Windows.Forms.Label();
            this.cboSchema = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // rbSameDatabase
            // 
            this.rbSameDatabase.AutoSize = true;
            this.rbSameDatabase.Checked = true;
            this.rbSameDatabase.Location = new System.Drawing.Point(16, 8);
            this.rbSameDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.rbSameDatabase.Name = "rbSameDatabase";
            this.rbSameDatabase.Size = new System.Drawing.Size(115, 21);
            this.rbSameDatabase.TabIndex = 1;
            this.rbSameDatabase.TabStop = true;
            this.rbSameDatabase.Text = "same database";
            this.rbSameDatabase.UseVisualStyleBackColor = true;
            this.rbSameDatabase.CheckedChanged += new System.EventHandler(this.rbSameDatabase_CheckedChanged);
            // 
            // rbAnotherDatabase
            // 
            this.rbAnotherDatabase.AutoSize = true;
            this.rbAnotherDatabase.Location = new System.Drawing.Point(144, 8);
            this.rbAnotherDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.rbAnotherDatabase.Name = "rbAnotherDatabase";
            this.rbAnotherDatabase.Size = new System.Drawing.Size(129, 21);
            this.rbAnotherDatabase.TabIndex = 2;
            this.rbAnotherDatabase.Text = "another database";
            this.rbAnotherDatabase.UseVisualStyleBackColor = true;
            // 
            // chkScriptData
            // 
            this.chkScriptData.AutoSize = true;
            this.chkScriptData.Checked = true;
            this.chkScriptData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptData.Location = new System.Drawing.Point(177, 137);
            this.chkScriptData.Margin = new System.Windows.Forms.Padding(4);
            this.chkScriptData.Name = "chkScriptData";
            this.chkScriptData.Size = new System.Drawing.Size(53, 21);
            this.chkScriptData.TabIndex = 15;
            this.chkScriptData.Text = "data";
            this.chkScriptData.UseVisualStyleBackColor = true;
            // 
            // chkScriptSchema
            // 
            this.chkScriptSchema.AutoSize = true;
            this.chkScriptSchema.Checked = true;
            this.chkScriptSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptSchema.Location = new System.Drawing.Point(84, 137);
            this.chkScriptSchema.Margin = new System.Windows.Forms.Padding(4);
            this.chkScriptSchema.Name = "chkScriptSchema";
            this.chkScriptSchema.Size = new System.Drawing.Size(71, 21);
            this.chkScriptSchema.TabIndex = 14;
            this.chkScriptSchema.Text = "schema";
            this.chkScriptSchema.UseVisualStyleBackColor = true;
            // 
            // lblScriptsMode
            // 
            this.lblScriptsMode.AutoSize = true;
            this.lblScriptsMode.Location = new System.Drawing.Point(16, 138);
            this.lblScriptsMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScriptsMode.Name = "lblScriptsMode";
            this.lblScriptsMode.Size = new System.Drawing.Size(46, 17);
            this.lblScriptsMode.TabIndex = 13;
            this.lblScriptsMode.Text = "Mode:";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClose.Location = new System.Drawing.Point(371, 230);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 33);
            this.btnClose.TabIndex = 24;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnExecute.Location = new System.Drawing.Point(258, 230);
            this.btnExecute.Margin = new System.Windows.Forms.Padding(4);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(88, 33);
            this.btnExecute.TabIndex = 23;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 93);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 17);
            this.label1.TabIndex = 25;
            this.label1.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(83, 89);
            this.txtName.Margin = new System.Windows.Forms.Padding(4);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(302, 23);
            this.txtName.TabIndex = 26;
            // 
            // ucConnection
            // 
            this.ucConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ucConnection.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            this.ucConnection.Enabled = false;
            this.ucConnection.EnableDatabaseType = true;
            this.ucConnection.Location = new System.Drawing.Point(12, 42);
            this.ucConnection.Margin = new System.Windows.Forms.Padding(0);
            this.ucConnection.Name = "ucConnection";
            this.ucConnection.Size = new System.Drawing.Size(666, 33);
            this.ucConnection.TabIndex = 0;
            this.ucConnection.Title = "Target:";
            this.ucConnection.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.ucConnection_OnSelectedChanged);
            // 
            // chkGenerateIdentity
            // 
            this.chkGenerateIdentity.AutoSize = true;
            this.chkGenerateIdentity.Checked = true;
            this.chkGenerateIdentity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateIdentity.Location = new System.Drawing.Point(20, 197);
            this.chkGenerateIdentity.Margin = new System.Windows.Forms.Padding(4);
            this.chkGenerateIdentity.Name = "chkGenerateIdentity";
            this.chkGenerateIdentity.Size = new System.Drawing.Size(126, 21);
            this.chkGenerateIdentity.TabIndex = 27;
            this.chkGenerateIdentity.Text = "Generate identity";
            this.chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // chkOnlyCopyTable
            // 
            this.chkOnlyCopyTable.AutoSize = true;
            this.chkOnlyCopyTable.Checked = true;
            this.chkOnlyCopyTable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOnlyCopyTable.Location = new System.Drawing.Point(20, 168);
            this.chkOnlyCopyTable.Margin = new System.Windows.Forms.Padding(4);
            this.chkOnlyCopyTable.Name = "chkOnlyCopyTable";
            this.chkOnlyCopyTable.Size = new System.Drawing.Size(324, 21);
            this.chkOnlyCopyTable.TabIndex = 28;
            this.chkOnlyCopyTable.Text = "Only copy table(no primary/foreign key and so on)";
            this.chkOnlyCopyTable.UseVisualStyleBackColor = true;
            // 
            // lblSchema
            // 
            this.lblSchema.AutoSize = true;
            this.lblSchema.Location = new System.Drawing.Point(449, 92);
            this.lblSchema.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSchema.Name = "lblSchema";
            this.lblSchema.Size = new System.Drawing.Size(56, 17);
            this.lblSchema.TabIndex = 29;
            this.lblSchema.Text = "Schema:";
            // 
            // cboSchema
            // 
            this.cboSchema.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSchema.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboSchema.FormattingEnabled = true;
            this.cboSchema.Location = new System.Drawing.Point(517, 87);
            this.cboSchema.Name = "cboSchema";
            this.cboSchema.Size = new System.Drawing.Size(121, 25);
            this.cboSchema.TabIndex = 30;
            // 
            // frmTableCopy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(686, 280);
            this.Controls.Add(this.cboSchema);
            this.Controls.Add(this.lblSchema);
            this.Controls.Add(this.chkOnlyCopyTable);
            this.Controls.Add(this.chkGenerateIdentity);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.chkScriptData);
            this.Controls.Add(this.chkScriptSchema);
            this.Controls.Add(this.lblScriptsMode);
            this.Controls.Add(this.rbAnotherDatabase);
            this.Controls.Add(this.rbSameDatabase);
            this.Controls.Add(this.ucConnection);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmTableCopy";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Copy Table";
            this.Load += new System.EventHandler(this.frmDbObjectCopy_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.CheckBox chkOnlyCopyTable;
        private System.Windows.Forms.Label lblSchema;
        private System.Windows.Forms.ComboBox cboSchema;
    }
}