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
            btnConfirm = new System.Windows.Forms.Button();
            lblDatabase = new System.Windows.Forms.Label();
            cboDatabase = new System.Windows.Forms.ComboBox();
            lblProfileName = new System.Windows.Forms.Label();
            txtProfileName = new System.Windows.Forms.TextBox();
            btnCancel = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            rbInput = new System.Windows.Forms.RadioButton();
            rbChoose = new System.Windows.Forms.RadioButton();
            ucDbAccountInfo = new Controls.UC_DbAccountInfo();
            panelMode = new System.Windows.Forms.Panel();
            panelContent = new System.Windows.Forms.Panel();
            panelButton = new System.Windows.Forms.Panel();
            panelMode.SuspendLayout();
            panelContent.SuspendLayout();
            panelButton.SuspendLayout();
            SuspendLayout();
            // 
            // btnConfirm
            // 
            btnConfirm.Location = new System.Drawing.Point(141, 7);
            btnConfirm.Margin = new System.Windows.Forms.Padding(4);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new System.Drawing.Size(88, 33);
            btnConfirm.TabIndex = 9;
            btnConfirm.Text = "OK";
            btnConfirm.UseVisualStyleBackColor = true;
            btnConfirm.Click += btnConfirm_Click;
            // 
            // lblDatabase
            // 
            lblDatabase.AutoSize = true;
            lblDatabase.Location = new System.Drawing.Point(15, 249);
            lblDatabase.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new System.Drawing.Size(66, 17);
            lblDatabase.TabIndex = 11;
            lblDatabase.Text = "Database:";
            // 
            // cboDatabase
            // 
            cboDatabase.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            cboDatabase.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            cboDatabase.DropDownHeight = 200;
            cboDatabase.FormattingEnabled = true;
            cboDatabase.IntegralHeight = false;
            cboDatabase.Location = new System.Drawing.Point(143, 244);
            cboDatabase.Margin = new System.Windows.Forms.Padding(4);
            cboDatabase.Name = "cboDatabase";
            cboDatabase.Size = new System.Drawing.Size(193, 25);
            cboDatabase.TabIndex = 7;
            cboDatabase.SelectedIndexChanged += cboDatabase_SelectedIndexChanged;
            cboDatabase.MouseClick += cboDatabase_MouseClick;
            // 
            // lblProfileName
            // 
            lblProfileName.AutoSize = true;
            lblProfileName.Location = new System.Drawing.Point(15, 291);
            lblProfileName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblProfileName.Name = "lblProfileName";
            lblProfileName.Size = new System.Drawing.Size(84, 17);
            lblProfileName.TabIndex = 16;
            lblProfileName.Text = "Profile name:";
            // 
            // txtProfileName
            // 
            txtProfileName.Location = new System.Drawing.Point(142, 287);
            txtProfileName.Margin = new System.Windows.Forms.Padding(4);
            txtProfileName.Name = "txtProfileName";
            txtProfileName.Size = new System.Drawing.Size(193, 23);
            txtProfileName.TabIndex = 0;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(247, 7);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 33);
            btnCancel.TabIndex = 17;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(8, 11);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(46, 17);
            label2.TabIndex = 19;
            label2.Text = "Mode:";
            // 
            // rbInput
            // 
            rbInput.AutoSize = true;
            rbInput.Checked = true;
            rbInput.Location = new System.Drawing.Point(131, 9);
            rbInput.Margin = new System.Windows.Forms.Padding(4);
            rbInput.Name = "rbInput";
            rbInput.Size = new System.Drawing.Size(56, 21);
            rbInput.TabIndex = 20;
            rbInput.TabStop = true;
            rbInput.Text = "Input";
            rbInput.UseVisualStyleBackColor = true;
            // 
            // rbChoose
            // 
            rbChoose.AutoSize = true;
            rbChoose.Location = new System.Drawing.Point(254, 9);
            rbChoose.Margin = new System.Windows.Forms.Padding(4);
            rbChoose.Name = "rbChoose";
            rbChoose.Size = new System.Drawing.Size(70, 21);
            rbChoose.TabIndex = 21;
            rbChoose.Text = "Choose";
            rbChoose.UseVisualStyleBackColor = true;
            rbChoose.CheckedChanged += rbChoose_CheckedChanged;
            // 
            // ucDbAccountInfo
            // 
            ucDbAccountInfo.DatabaseType = DatabaseInterpreter.Model.DatabaseType.SqlServer;
            ucDbAccountInfo.Location = new System.Drawing.Point(5, 6);
            ucDbAccountInfo.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            ucDbAccountInfo.Name = "ucDbAccountInfo";
            ucDbAccountInfo.Size = new System.Drawing.Size(435, 230);
            ucDbAccountInfo.TabIndex = 1;
            // 
            // panelMode
            // 
            panelMode.Controls.Add(rbChoose);
            panelMode.Controls.Add(label2);
            panelMode.Controls.Add(rbInput);
            panelMode.Location = new System.Drawing.Point(12, 3);
            panelMode.Name = "panelMode";
            panelMode.Size = new System.Drawing.Size(392, 39);
            panelMode.TabIndex = 22;
            // 
            // panelContent
            // 
            panelContent.Controls.Add(ucDbAccountInfo);
            panelContent.Controls.Add(lblDatabase);
            panelContent.Controls.Add(cboDatabase);
            panelContent.Controls.Add(lblProfileName);
            panelContent.Controls.Add(txtProfileName);
            panelContent.Location = new System.Drawing.Point(12, 48);
            panelContent.Name = "panelContent";
            panelContent.Size = new System.Drawing.Size(449, 320);
            panelContent.TabIndex = 23;
            // 
            // panelButton
            // 
            panelButton.Controls.Add(btnCancel);
            panelButton.Controls.Add(btnConfirm);
            panelButton.Location = new System.Drawing.Point(12, 379);
            panelButton.Name = "panelButton";
            panelButton.Size = new System.Drawing.Size(449, 44);
            panelButton.TabIndex = 24;
            // 
            // frmDbConnect
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(466, 435);
            Controls.Add(panelButton);
            Controls.Add(panelContent);
            Controls.Add(panelMode);
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmDbConnect";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Connection";
            Activated += frmDbConnect_Activated;
            Load += frmDbConnect_Load;
            panelMode.ResumeLayout(false);
            panelMode.PerformLayout();
            panelContent.ResumeLayout(false);
            panelContent.PerformLayout();
            panelButton.ResumeLayout(false);
            ResumeLayout(false);
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
        private System.Windows.Forms.Panel panelMode;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Panel panelButton;
    }
}