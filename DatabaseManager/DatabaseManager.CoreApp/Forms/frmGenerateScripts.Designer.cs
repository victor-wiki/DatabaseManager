using DatabaseInterpreter.Model;

namespace DatabaseManager.Forms
{
    partial class frmGenerateScripts
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGenerateScripts));
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            chkGenerateComment = new System.Windows.Forms.CheckBox();
            chkGenerateIdentity = new System.Windows.Forms.CheckBox();
            chkTreatBytesAsNull = new System.Windows.Forms.CheckBox();
            panelConnector = new System.Windows.Forms.Panel();
            dbConnectionProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            btnConnect = new System.Windows.Forms.Button();
            tvDbObjects = new DatabaseManager.Controls.UC_DbObjectsSimpleTree();
            lblOutputFolder = new System.Windows.Forms.Label();
            btnOutputFolder = new System.Windows.Forms.Button();
            txtOutputFolder = new System.Windows.Forms.TextBox();
            chkScriptData = new System.Windows.Forms.CheckBox();
            chkScriptSchema = new System.Windows.Forms.CheckBox();
            lblScriptsMode = new System.Windows.Forms.Label();
            btnCancel = new System.Windows.Forms.Button();
            btnGenerate = new System.Windows.Forms.Button();
            txtMessage = new System.Windows.Forms.RichTextBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panelConnector.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(chkGenerateComment);
            splitContainer1.Panel1.Controls.Add(chkGenerateIdentity);
            splitContainer1.Panel1.Controls.Add(chkTreatBytesAsNull);
            splitContainer1.Panel1.Controls.Add(panelConnector);
            splitContainer1.Panel1.Controls.Add(tvDbObjects);
            splitContainer1.Panel1.Controls.Add(lblOutputFolder);
            splitContainer1.Panel1.Controls.Add(btnOutputFolder);
            splitContainer1.Panel1.Controls.Add(txtOutputFolder);
            splitContainer1.Panel1.Controls.Add(chkScriptData);
            splitContainer1.Panel1.Controls.Add(chkScriptSchema);
            splitContainer1.Panel1.Controls.Add(lblScriptsMode);
            splitContainer1.Panel1.Controls.Add(btnCancel);
            splitContainer1.Panel1.Controls.Add(btnGenerate);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(txtMessage);
            splitContainer1.Size = new System.Drawing.Size(764, 623);
            splitContainer1.SplitterDistance = 515;
            splitContainer1.TabIndex = 0;
            // 
            // chkGenerateComment
            // 
            chkGenerateComment.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkGenerateComment.AutoSize = true;
            chkGenerateComment.Checked = true;
            chkGenerateComment.CheckState = System.Windows.Forms.CheckState.Checked;
            chkGenerateComment.Location = new System.Drawing.Point(324, 391);
            chkGenerateComment.Margin = new System.Windows.Forms.Padding(4);
            chkGenerateComment.Name = "chkGenerateComment";
            chkGenerateComment.Size = new System.Drawing.Size(140, 21);
            chkGenerateComment.TabIndex = 56;
            chkGenerateComment.Text = "Generate Comment";
            chkGenerateComment.UseVisualStyleBackColor = true;
            // 
            // chkGenerateIdentity
            // 
            chkGenerateIdentity.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkGenerateIdentity.AutoSize = true;
            chkGenerateIdentity.Checked = true;
            chkGenerateIdentity.CheckState = System.Windows.Forms.CheckState.Checked;
            chkGenerateIdentity.Location = new System.Drawing.Point(176, 391);
            chkGenerateIdentity.Margin = new System.Windows.Forms.Padding(4);
            chkGenerateIdentity.Name = "chkGenerateIdentity";
            chkGenerateIdentity.Size = new System.Drawing.Size(127, 21);
            chkGenerateIdentity.TabIndex = 55;
            chkGenerateIdentity.Text = "Generate Identity";
            chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // chkTreatBytesAsNull
            // 
            chkTreatBytesAsNull.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkTreatBytesAsNull.AutoSize = true;
            chkTreatBytesAsNull.Checked = true;
            chkTreatBytesAsNull.CheckState = System.Windows.Forms.CheckState.Checked;
            chkTreatBytesAsNull.Location = new System.Drawing.Point(17, 391);
            chkTreatBytesAsNull.Margin = new System.Windows.Forms.Padding(4);
            chkTreatBytesAsNull.Name = "chkTreatBytesAsNull";
            chkTreatBytesAsNull.Size = new System.Drawing.Size(133, 21);
            chkTreatBytesAsNull.TabIndex = 54;
            chkTreatBytesAsNull.Text = "Treat bytes as null";
            chkTreatBytesAsNull.UseVisualStyleBackColor = true;
            // 
            // panelConnector
            // 
            panelConnector.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panelConnector.Controls.Add(dbConnectionProfile);
            panelConnector.Controls.Add(btnConnect);
            panelConnector.Location = new System.Drawing.Point(4, 4);
            panelConnector.Margin = new System.Windows.Forms.Padding(4);
            panelConnector.Name = "panelConnector";
            panelConnector.Size = new System.Drawing.Size(757, 38);
            panelConnector.TabIndex = 53;
            // 
            // dbConnectionProfile
            // 
            dbConnectionProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dbConnectionProfile.DatabaseType = DatabaseType.Unknown;
            dbConnectionProfile.EnableDatabaseType = true;
            dbConnectionProfile.Location = new System.Drawing.Point(7, 5);
            dbConnectionProfile.Margin = new System.Windows.Forms.Padding(0);
            dbConnectionProfile.Name = "dbConnectionProfile";
            dbConnectionProfile.Size = new System.Drawing.Size(655, 31);
            dbConnectionProfile.TabIndex = 42;
            dbConnectionProfile.Title = "Database:";
            dbConnectionProfile.ProfileSelectedChanged += dbConnectionProfile_OnSelectedChanged;
            // 
            // btnConnect
            // 
            btnConnect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnConnect.Image = (System.Drawing.Image)resources.GetObject("btnConnect.Image");
            btnConnect.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            btnConnect.Location = new System.Drawing.Point(664, 6);
            btnConnect.Margin = new System.Windows.Forms.Padding(4);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(83, 26);
            btnConnect.TabIndex = 44;
            btnConnect.Text = "Connect";
            btnConnect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            toolTip1.SetToolTip(btnConnect, "Connect");
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // tvDbObjects
            // 
            tvDbObjects.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tvDbObjects.Location = new System.Drawing.Point(4, 46);
            tvDbObjects.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tvDbObjects.Name = "tvDbObjects";
            tvDbObjects.ShowCheckBox = true;
            tvDbObjects.Size = new System.Drawing.Size(757, 292);
            tvDbObjects.TabIndex = 43;
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Location = new System.Drawing.Point(14, 433);
            lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new System.Drawing.Size(92, 17);
            lblOutputFolder.TabIndex = 52;
            lblOutputFolder.Text = "Output Folder:";
            // 
            // btnOutputFolder
            // 
            btnOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnOutputFolder.Location = new System.Drawing.Point(701, 428);
            btnOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            btnOutputFolder.Name = "btnOutputFolder";
            btnOutputFolder.Size = new System.Drawing.Size(42, 25);
            btnOutputFolder.TabIndex = 51;
            btnOutputFolder.Text = "...";
            btnOutputFolder.UseVisualStyleBackColor = true;
            btnOutputFolder.Click += btnOutputFolder_Click;
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            txtOutputFolder.Location = new System.Drawing.Point(124, 429);
            txtOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.Size = new System.Drawing.Size(571, 23);
            txtOutputFolder.TabIndex = 50;
            // 
            // chkScriptData
            // 
            chkScriptData.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkScriptData.AutoSize = true;
            chkScriptData.Checked = true;
            chkScriptData.CheckState = System.Windows.Forms.CheckState.Checked;
            chkScriptData.Location = new System.Drawing.Point(157, 353);
            chkScriptData.Margin = new System.Windows.Forms.Padding(4);
            chkScriptData.Name = "chkScriptData";
            chkScriptData.Size = new System.Drawing.Size(53, 21);
            chkScriptData.TabIndex = 49;
            chkScriptData.Text = "data";
            chkScriptData.UseVisualStyleBackColor = true;
            // 
            // chkScriptSchema
            // 
            chkScriptSchema.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            chkScriptSchema.AutoSize = true;
            chkScriptSchema.Checked = true;
            chkScriptSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            chkScriptSchema.Location = new System.Drawing.Point(71, 353);
            chkScriptSchema.Margin = new System.Windows.Forms.Padding(4);
            chkScriptSchema.Name = "chkScriptSchema";
            chkScriptSchema.Size = new System.Drawing.Size(71, 21);
            chkScriptSchema.TabIndex = 48;
            chkScriptSchema.Text = "schema";
            chkScriptSchema.UseVisualStyleBackColor = true;
            // 
            // lblScriptsMode
            // 
            lblScriptsMode.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lblScriptsMode.AutoSize = true;
            lblScriptsMode.Location = new System.Drawing.Point(13, 353);
            lblScriptsMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblScriptsMode.Name = "lblScriptsMode";
            lblScriptsMode.Size = new System.Drawing.Size(46, 17);
            lblScriptsMode.TabIndex = 47;
            lblScriptsMode.Text = "Mode:";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnCancel.Enabled = false;
            btnCancel.Location = new System.Drawing.Point(400, 471);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 33);
            btnCancel.TabIndex = 46;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnGenerate.Location = new System.Drawing.Point(287, 471);
            btnGenerate.Margin = new System.Windows.Forms.Padding(4);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new System.Drawing.Size(88, 33);
            btnGenerate.TabIndex = 45;
            btnGenerate.Text = "Generate";
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // txtMessage
            // 
            txtMessage.BackColor = System.Drawing.Color.White;
            txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            txtMessage.Location = new System.Drawing.Point(0, 0);
            txtMessage.Margin = new System.Windows.Forms.Padding(4);
            txtMessage.Name = "txtMessage";
            txtMessage.ReadOnly = true;
            txtMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            txtMessage.Size = new System.Drawing.Size(764, 104);
            txtMessage.TabIndex = 1;
            txtMessage.Text = "";
            // 
            // frmGenerateScripts
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(764, 623);
            Controls.Add(splitContainer1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmGenerateScripts";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Generate Scripts";
            FormClosing += frmGenerateScripts_FormClosing;
            Load += frmGenerateScripts_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panelConnector.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.ToolTip toolTip1;
        private Controls.UC_DbObjectsSimpleTree tvDbObjects;
        private Controls.UC_DbConnectionProfile dbConnectionProfile;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.Button btnOutputFolder;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.CheckBox chkScriptData;
        private System.Windows.Forms.CheckBox chkScriptSchema;
        private System.Windows.Forms.Label lblScriptsMode;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.RichTextBox txtMessage;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
        private System.Windows.Forms.Panel panelConnector;
        private System.Windows.Forms.CheckBox chkTreatBytesAsNull;
        private System.Windows.Forms.CheckBox chkGenerateComment;
        private System.Windows.Forms.CheckBox chkGenerateIdentity;
    }
}