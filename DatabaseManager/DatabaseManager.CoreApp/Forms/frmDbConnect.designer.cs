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
            this.btnConfirm = new System.Windows.Forms.Button();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.cboDatabase = new System.Windows.Forms.ComboBox();
            this.lblProfileName = new System.Windows.Forms.Label();
            this.txtProfileName = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.rbInput = new System.Windows.Forms.RadioButton();
            this.rbChoose = new System.Windows.Forms.RadioButton();
            this.ucDbAccountInfo = new DatabaseManager.Controls.UC_DbAccountInfo();
            this.SuspendLayout();
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(144, 387);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(4);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(88, 33);
            this.btnConfirm.TabIndex = 9;
            this.btnConfirm.Text = "OK";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(16, 295);
            this.lblDatabase.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(66, 17);
            this.lblDatabase.TabIndex = 11;
            this.lblDatabase.Text = "Database:";
            // 
            // cboDatabase
            // 
            this.cboDatabase.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboDatabase.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboDatabase.DropDownHeight = 200;
            this.cboDatabase.FormattingEnabled = true;
            this.cboDatabase.IntegralHeight = false;
            this.cboDatabase.Location = new System.Drawing.Point(144, 290);
            this.cboDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.cboDatabase.Name = "cboDatabase";
            this.cboDatabase.Size = new System.Drawing.Size(193, 25);
            this.cboDatabase.TabIndex = 7;
            this.cboDatabase.SelectedIndexChanged += new System.EventHandler(this.cboDatabase_SelectedIndexChanged);
            this.cboDatabase.MouseClick += new System.Windows.Forms.MouseEventHandler(this.cboDatabase_MouseClick);
            // 
            // lblProfileName
            // 
            this.lblProfileName.AutoSize = true;
            this.lblProfileName.Location = new System.Drawing.Point(16, 337);
            this.lblProfileName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProfileName.Name = "lblProfileName";
            this.lblProfileName.Size = new System.Drawing.Size(84, 17);
            this.lblProfileName.TabIndex = 16;
            this.lblProfileName.Text = "Profile name:";
            // 
            // txtProfileName
            // 
            this.txtProfileName.Location = new System.Drawing.Point(143, 333);
            this.txtProfileName.Margin = new System.Windows.Forms.Padding(4);
            this.txtProfileName.Name = "txtProfileName";
            this.txtProfileName.Size = new System.Drawing.Size(193, 23);
            this.txtProfileName.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(250, 387);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 33);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 23);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 17);
            this.label2.TabIndex = 19;
            this.label2.Text = "Mode:";
            // 
            // rbInput
            // 
            this.rbInput.AutoSize = true;
            this.rbInput.Checked = true;
            this.rbInput.Location = new System.Drawing.Point(142, 21);
            this.rbInput.Margin = new System.Windows.Forms.Padding(4);
            this.rbInput.Name = "rbInput";
            this.rbInput.Size = new System.Drawing.Size(56, 21);
            this.rbInput.TabIndex = 20;
            this.rbInput.TabStop = true;
            this.rbInput.Text = "Input";
            this.rbInput.UseVisualStyleBackColor = true;
            // 
            // rbChoose
            // 
            this.rbChoose.AutoSize = true;
            this.rbChoose.Location = new System.Drawing.Point(265, 21);
            this.rbChoose.Margin = new System.Windows.Forms.Padding(4);
            this.rbChoose.Name = "rbChoose";
            this.rbChoose.Size = new System.Drawing.Size(70, 21);
            this.rbChoose.TabIndex = 21;
            this.rbChoose.Text = "Choose";
            this.rbChoose.UseVisualStyleBackColor = true;
            this.rbChoose.CheckedChanged += new System.EventHandler(this.rbChoose_CheckedChanged);
            // 
            // ucDbAccountInfo
            // 
            this.ucDbAccountInfo.DatabaseType = DatabaseInterpreter.Model.DatabaseType.SqlServer;
            this.ucDbAccountInfo.Location = new System.Drawing.Point(6, 52);
            this.ucDbAccountInfo.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.ucDbAccountInfo.Name = "ucDbAccountInfo";
            this.ucDbAccountInfo.Size = new System.Drawing.Size(435, 230);
            this.ucDbAccountInfo.TabIndex = 1;
            // 
            // frmDbConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 435);
            this.Controls.Add(this.rbChoose);
            this.Controls.Add(this.rbInput);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.ucDbAccountInfo);
            this.Controls.Add(this.txtProfileName);
            this.Controls.Add(this.lblProfileName);
            this.Controls.Add(this.cboDatabase);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.btnConfirm);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDbConnect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Connection";
            this.Activated += new System.EventHandler(this.frmDbConnect_Activated);
            this.Load += new System.EventHandler(this.frmDbConnect_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.ComboBox cboDatabase;
        private System.Windows.Forms.Label lblProfileName;
        private System.Windows.Forms.TextBox txtProfileName;
        private Controls.UC_DbAccountInfo ucDbAccountInfo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rbInput;
        private System.Windows.Forms.RadioButton rbChoose;
    }
}