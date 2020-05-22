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
            this.SuspendLayout();
            // 
            // rbSameDatabase
            // 
            this.rbSameDatabase.AutoSize = true;
            this.rbSameDatabase.Checked = true;
            this.rbSameDatabase.Location = new System.Drawing.Point(14, 6);
            this.rbSameDatabase.Name = "rbSameDatabase";
            this.rbSameDatabase.Size = new System.Drawing.Size(101, 16);
            this.rbSameDatabase.TabIndex = 1;
            this.rbSameDatabase.TabStop = true;
            this.rbSameDatabase.Text = "same database";
            this.rbSameDatabase.UseVisualStyleBackColor = true;
            this.rbSameDatabase.CheckedChanged += new System.EventHandler(this.rbSameDatabase_CheckedChanged);
            // 
            // rbAnotherDatabase
            // 
            this.rbAnotherDatabase.AutoSize = true;
            this.rbAnotherDatabase.Location = new System.Drawing.Point(123, 6);
            this.rbAnotherDatabase.Name = "rbAnotherDatabase";
            this.rbAnotherDatabase.Size = new System.Drawing.Size(119, 16);
            this.rbAnotherDatabase.TabIndex = 2;
            this.rbAnotherDatabase.Text = "another database";
            this.rbAnotherDatabase.UseVisualStyleBackColor = true;
            this.rbAnotherDatabase.CheckedChanged += new System.EventHandler(this.rbAnotherDatabase_CheckedChanged);
            // 
            // chkScriptData
            // 
            this.chkScriptData.AutoSize = true;
            this.chkScriptData.Checked = true;
            this.chkScriptData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptData.Location = new System.Drawing.Point(152, 106);
            this.chkScriptData.Name = "chkScriptData";
            this.chkScriptData.Size = new System.Drawing.Size(48, 16);
            this.chkScriptData.TabIndex = 15;
            this.chkScriptData.Text = "data";
            this.chkScriptData.UseVisualStyleBackColor = true;
            // 
            // chkScriptSchema
            // 
            this.chkScriptSchema.AutoSize = true;
            this.chkScriptSchema.Checked = true;
            this.chkScriptSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptSchema.Location = new System.Drawing.Point(72, 106);
            this.chkScriptSchema.Name = "chkScriptSchema";
            this.chkScriptSchema.Size = new System.Drawing.Size(60, 16);
            this.chkScriptSchema.TabIndex = 14;
            this.chkScriptSchema.Text = "schema";
            this.chkScriptSchema.UseVisualStyleBackColor = true;
            // 
            // lblScriptsMode
            // 
            this.lblScriptsMode.AutoSize = true;
            this.lblScriptsMode.Location = new System.Drawing.Point(15, 107);
            this.lblScriptsMode.Name = "lblScriptsMode";
            this.lblScriptsMode.Size = new System.Drawing.Size(35, 12);
            this.lblScriptsMode.TabIndex = 13;
            this.lblScriptsMode.Text = "Mode:";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClose.Location = new System.Drawing.Point(288, 176);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 24;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnExecute.Location = new System.Drawing.Point(191, 176);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 23;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 25;
            this.label1.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(71, 69);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(259, 21);
            this.txtName.TabIndex = 26;
            // 
            // ucConnection
            // 
            this.ucConnection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ucConnection.Enabled = false;
            this.ucConnection.Location = new System.Drawing.Point(10, 30);
            this.ucConnection.Margin = new System.Windows.Forms.Padding(0);
            this.ucConnection.Name = "ucConnection";
            this.ucConnection.Size = new System.Drawing.Size(514, 23);
            this.ucConnection.TabIndex = 0;
            this.ucConnection.Title = "Target:";
            this.ucConnection.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.ucConnection_OnSelectedChanged);
            // 
            // chkGenerateIdentity
            // 
            this.chkGenerateIdentity.AutoSize = true;
            this.chkGenerateIdentity.Checked = true;
            this.chkGenerateIdentity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateIdentity.Location = new System.Drawing.Point(17, 137);
            this.chkGenerateIdentity.Name = "chkGenerateIdentity";
            this.chkGenerateIdentity.Size = new System.Drawing.Size(126, 16);
            this.chkGenerateIdentity.TabIndex = 27;
            this.chkGenerateIdentity.Text = "Generate identity";
            this.chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // frmTableCopy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 211);
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
    }
}