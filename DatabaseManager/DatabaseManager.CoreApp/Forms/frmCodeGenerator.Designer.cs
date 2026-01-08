namespace DatabaseManager
{
    partial class frmCodeGenerator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCodeGenerator));
            dbConnectionProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            btnConnect = new System.Windows.Forms.Button();
            tvDbObjects = new DatabaseManager.Controls.UC_DbObjectsSimpleTree();
            lblOutputFolder = new System.Windows.Forms.Label();
            btnOutputFolder = new System.Windows.Forms.Button();
            txtOutputFolder = new System.Windows.Forms.TextBox();
            btnCancel = new System.Windows.Forms.Button();
            btnGenerate = new System.Windows.Forms.Button();
            cboLanguage = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            txtMessage = new System.Windows.Forms.TextBox();
            dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            lblNamespace = new System.Windows.Forms.Label();
            txtNamespance = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // dbConnectionProfile
            // 
            dbConnectionProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dbConnectionProfile.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            dbConnectionProfile.EnableDatabaseType = true;
            dbConnectionProfile.Location = new System.Drawing.Point(-1, 3);
            dbConnectionProfile.Margin = new System.Windows.Forms.Padding(0);
            dbConnectionProfile.Name = "dbConnectionProfile";
            dbConnectionProfile.Size = new System.Drawing.Size(663, 27);
            dbConnectionProfile.TabIndex = 43;
            dbConnectionProfile.Title = "Database:";
            dbConnectionProfile.ProfileSelectedChanged += dbConnectionProfile_ProfileSelectedChanged;
            // 
            // btnConnect
            // 
            btnConnect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnConnect.Image = (System.Drawing.Image)resources.GetObject("btnConnect.Image");
            btnConnect.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btnConnect.Location = new System.Drawing.Point(666, 4);
            btnConnect.Margin = new System.Windows.Forms.Padding(4);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(83, 26);
            btnConnect.TabIndex = 45;
            btnConnect.Text = "Connect";
            btnConnect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // tvDbObjects
            // 
            tvDbObjects.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tvDbObjects.Location = new System.Drawing.Point(1, 36);
            tvDbObjects.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tvDbObjects.Name = "tvDbObjects";
            tvDbObjects.ShowCheckBox = true;
            tvDbObjects.Size = new System.Drawing.Size(750, 298);
            tvDbObjects.TabIndex = 46;
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Location = new System.Drawing.Point(7, 393);
            lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new System.Drawing.Size(92, 17);
            lblOutputFolder.TabIndex = 57;
            lblOutputFolder.Text = "Output Folder:";
            // 
            // btnOutputFolder
            // 
            btnOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnOutputFolder.Location = new System.Drawing.Point(694, 388);
            btnOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            btnOutputFolder.Name = "btnOutputFolder";
            btnOutputFolder.Size = new System.Drawing.Size(42, 25);
            btnOutputFolder.TabIndex = 56;
            btnOutputFolder.Text = "...";
            btnOutputFolder.UseVisualStyleBackColor = true;
            btnOutputFolder.Click += btnOutputFolder_Click;
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            txtOutputFolder.Location = new System.Drawing.Point(117, 389);
            txtOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.Size = new System.Drawing.Size(571, 23);
            txtOutputFolder.TabIndex = 55;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnCancel.Enabled = false;
            btnCancel.Location = new System.Drawing.Point(399, 425);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 33);
            btnCancel.TabIndex = 54;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnGenerate.Location = new System.Drawing.Point(286, 425);
            btnGenerate.Margin = new System.Windows.Forms.Padding(4);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new System.Drawing.Size(88, 33);
            btnGenerate.TabIndex = 53;
            btnGenerate.Text = "Generate";
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // cboLanguage
            // 
            cboLanguage.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            cboLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboLanguage.FormattingEnabled = true;
            cboLanguage.Location = new System.Drawing.Point(117, 348);
            cboLanguage.Name = "cboLanguage";
            cboLanguage.Size = new System.Drawing.Size(121, 25);
            cboLanguage.TabIndex = 58;
            cboLanguage.SelectedIndexChanged += cboLanguage_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 351);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(68, 17);
            label1.TabIndex = 59;
            label1.Text = "Language:";
            // 
            // txtMessage
            // 
            txtMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            txtMessage.Location = new System.Drawing.Point(0, 465);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new System.Drawing.Size(754, 46);
            txtMessage.TabIndex = 60;
            // 
            // lblNamespace
            // 
            lblNamespace.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            lblNamespace.AutoSize = true;
            lblNamespace.Location = new System.Drawing.Point(341, 351);
            lblNamespace.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblNamespace.Name = "lblNamespace";
            lblNamespace.Size = new System.Drawing.Size(80, 17);
            lblNamespace.TabIndex = 61;
            lblNamespace.Text = "Namespace:";
            // 
            // txtNamespance
            // 
            txtNamespance.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            txtNamespance.Location = new System.Drawing.Point(428, 348);
            txtNamespance.Name = "txtNamespance";
            txtNamespance.Size = new System.Drawing.Size(260, 23);
            txtNamespance.TabIndex = 62;
            // 
            // frmCodeGenerator
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(754, 511);
            Controls.Add(txtNamespance);
            Controls.Add(lblNamespace);
            Controls.Add(txtMessage);
            Controls.Add(label1);
            Controls.Add(cboLanguage);
            Controls.Add(lblOutputFolder);
            Controls.Add(btnOutputFolder);
            Controls.Add(txtOutputFolder);
            Controls.Add(btnCancel);
            Controls.Add(btnGenerate);
            Controls.Add(tvDbObjects);
            Controls.Add(btnConnect);
            Controls.Add(dbConnectionProfile);
            Name = "frmCodeGenerator";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Code Generator";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Controls.UC_DbConnectionProfile dbConnectionProfile;
        private System.Windows.Forms.Button btnConnect;
        private Controls.UC_DbObjectsSimpleTree tvDbObjects;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.Button btnOutputFolder;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ComboBox cboLanguage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
        private System.Windows.Forms.Label lblNamespace;
        private System.Windows.Forms.TextBox txtNamespance;
    }
}