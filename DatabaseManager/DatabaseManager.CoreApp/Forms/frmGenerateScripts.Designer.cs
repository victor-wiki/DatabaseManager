using DatabaseInterpreter.Model;

namespace DatabaseManager
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGenerateScripts));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.chkTreatBytesAsNull = new System.Windows.Forms.CheckBox();
            this.panelConnector = new System.Windows.Forms.Panel();
            this.dbConnectionProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            this.btnConnect = new System.Windows.Forms.Button();
            this.tvDbObjects = new DatabaseManager.Controls.UC_DbObjectsSimpleTree();
            this.lblOutputFolder = new System.Windows.Forms.Label();
            this.btnOutputFolder = new System.Windows.Forms.Button();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.chkScriptData = new System.Windows.Forms.CheckBox();
            this.chkScriptSchema = new System.Windows.Forms.CheckBox();
            this.lblScriptsMode = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.RichTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.chkGenerateIdentity = new System.Windows.Forms.CheckBox();
            this.chkGenerateComment = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelConnector.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.chkGenerateComment);
            this.splitContainer1.Panel1.Controls.Add(this.chkGenerateIdentity);
            this.splitContainer1.Panel1.Controls.Add(this.chkTreatBytesAsNull);
            this.splitContainer1.Panel1.Controls.Add(this.panelConnector);
            this.splitContainer1.Panel1.Controls.Add(this.tvDbObjects);
            this.splitContainer1.Panel1.Controls.Add(this.lblOutputFolder);
            this.splitContainer1.Panel1.Controls.Add(this.btnOutputFolder);
            this.splitContainer1.Panel1.Controls.Add(this.txtOutputFolder);
            this.splitContainer1.Panel1.Controls.Add(this.chkScriptData);
            this.splitContainer1.Panel1.Controls.Add(this.chkScriptSchema);
            this.splitContainer1.Panel1.Controls.Add(this.lblScriptsMode);
            this.splitContainer1.Panel1.Controls.Add(this.btnClose);
            this.splitContainer1.Panel1.Controls.Add(this.btnGenerate);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtMessage);
            this.splitContainer1.Size = new System.Drawing.Size(764, 623);
            this.splitContainer1.SplitterDistance = 515;
            this.splitContainer1.TabIndex = 0;
            // 
            // chkTreatBytesAsNull
            // 
            this.chkTreatBytesAsNull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkTreatBytesAsNull.AutoSize = true;
            this.chkTreatBytesAsNull.Checked = true;
            this.chkTreatBytesAsNull.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTreatBytesAsNull.Location = new System.Drawing.Point(17, 391);
            this.chkTreatBytesAsNull.Margin = new System.Windows.Forms.Padding(4);
            this.chkTreatBytesAsNull.Name = "chkTreatBytesAsNull";
            this.chkTreatBytesAsNull.Size = new System.Drawing.Size(133, 21);
            this.chkTreatBytesAsNull.TabIndex = 54;
            this.chkTreatBytesAsNull.Text = "Treat bytes as null";
            this.chkTreatBytesAsNull.UseVisualStyleBackColor = true;
            // 
            // panelConnector
            // 
            this.panelConnector.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelConnector.Controls.Add(this.dbConnectionProfile);
            this.panelConnector.Controls.Add(this.btnConnect);
            this.panelConnector.Location = new System.Drawing.Point(4, 4);
            this.panelConnector.Margin = new System.Windows.Forms.Padding(4);
            this.panelConnector.Name = "panelConnector";
            this.panelConnector.Size = new System.Drawing.Size(757, 38);
            this.panelConnector.TabIndex = 53;
            // 
            // dbConnectionProfile
            // 
            this.dbConnectionProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dbConnectionProfile.DatabaseType = DatabaseType.Unknown;
            this.dbConnectionProfile.EnableDatabaseType = true;
            this.dbConnectionProfile.Location = new System.Drawing.Point(7, 5);
            this.dbConnectionProfile.Margin = new System.Windows.Forms.Padding(0);
            this.dbConnectionProfile.Name = "dbConnectionProfile";
            this.dbConnectionProfile.Size = new System.Drawing.Size(655, 31);
            this.dbConnectionProfile.TabIndex = 42;
            this.dbConnectionProfile.Title = "Database:";
            this.dbConnectionProfile.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.dbConnectionProfile_OnSelectedChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Image = ((System.Drawing.Image)(resources.GetObject("btnConnect.Image")));
            this.btnConnect.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConnect.Location = new System.Drawing.Point(664, 6);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(83, 26);
            this.btnConnect.TabIndex = 44;
            this.btnConnect.Text = "Connect";
            this.btnConnect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.btnConnect, "Connect");
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvDbObjects.Location = new System.Drawing.Point(4, 46);
            this.tvDbObjects.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(757, 292);
            this.tvDbObjects.TabIndex = 43;
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.Location = new System.Drawing.Point(14, 433);
            this.lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(92, 17);
            this.lblOutputFolder.TabIndex = 52;
            this.lblOutputFolder.Text = "Output Folder:";
            // 
            // btnOutputFolder
            // 
            this.btnOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOutputFolder.Location = new System.Drawing.Point(701, 428);
            this.btnOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            this.btnOutputFolder.Name = "btnOutputFolder";
            this.btnOutputFolder.Size = new System.Drawing.Size(42, 25);
            this.btnOutputFolder.TabIndex = 51;
            this.btnOutputFolder.Text = "...";
            this.btnOutputFolder.UseVisualStyleBackColor = true;
            this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtOutputFolder.Location = new System.Drawing.Point(124, 429);
            this.txtOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(571, 23);
            this.txtOutputFolder.TabIndex = 50;
            // 
            // chkScriptData
            // 
            this.chkScriptData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkScriptData.AutoSize = true;
            this.chkScriptData.Checked = true;
            this.chkScriptData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptData.Location = new System.Drawing.Point(157, 353);
            this.chkScriptData.Margin = new System.Windows.Forms.Padding(4);
            this.chkScriptData.Name = "chkScriptData";
            this.chkScriptData.Size = new System.Drawing.Size(53, 21);
            this.chkScriptData.TabIndex = 49;
            this.chkScriptData.Text = "data";
            this.chkScriptData.UseVisualStyleBackColor = true;
            // 
            // chkScriptSchema
            // 
            this.chkScriptSchema.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkScriptSchema.AutoSize = true;
            this.chkScriptSchema.Checked = true;
            this.chkScriptSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptSchema.Location = new System.Drawing.Point(71, 353);
            this.chkScriptSchema.Margin = new System.Windows.Forms.Padding(4);
            this.chkScriptSchema.Name = "chkScriptSchema";
            this.chkScriptSchema.Size = new System.Drawing.Size(71, 21);
            this.chkScriptSchema.TabIndex = 48;
            this.chkScriptSchema.Text = "schema";
            this.chkScriptSchema.UseVisualStyleBackColor = true;
            // 
            // lblScriptsMode
            // 
            this.lblScriptsMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblScriptsMode.AutoSize = true;
            this.lblScriptsMode.Location = new System.Drawing.Point(13, 353);
            this.lblScriptsMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScriptsMode.Name = "lblScriptsMode";
            this.lblScriptsMode.Size = new System.Drawing.Size(46, 17);
            this.lblScriptsMode.TabIndex = 47;
            this.lblScriptsMode.Text = "Mode:";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClose.Location = new System.Drawing.Point(400, 471);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 33);
            this.btnClose.TabIndex = 46;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnGenerate.Location = new System.Drawing.Point(287, 471);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(4);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(88, 33);
            this.btnGenerate.TabIndex = 45;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.Color.White;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Location = new System.Drawing.Point(0, 0);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(4);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(764, 104);
            this.txtMessage.TabIndex = 1;
            this.txtMessage.Text = "";
            // 
            // chkGenerateIdentity
            // 
            this.chkGenerateIdentity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkGenerateIdentity.AutoSize = true;
            this.chkGenerateIdentity.Checked = true;
            this.chkGenerateIdentity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateIdentity.Location = new System.Drawing.Point(176, 391);
            this.chkGenerateIdentity.Margin = new System.Windows.Forms.Padding(4);
            this.chkGenerateIdentity.Name = "chkGenerateIdentity";
            this.chkGenerateIdentity.Size = new System.Drawing.Size(127, 21);
            this.chkGenerateIdentity.TabIndex = 55;
            this.chkGenerateIdentity.Text = "Generate Identity";
            this.chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // chkGenerateComment
            // 
            this.chkGenerateComment.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkGenerateComment.AutoSize = true;
            this.chkGenerateComment.Checked = true;
            this.chkGenerateComment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateComment.Location = new System.Drawing.Point(324, 391);
            this.chkGenerateComment.Margin = new System.Windows.Forms.Padding(4);
            this.chkGenerateComment.Name = "chkGenerateComment";
            this.chkGenerateComment.Size = new System.Drawing.Size(140, 21);
            this.chkGenerateComment.TabIndex = 56;
            this.chkGenerateComment.Text = "Generate Comment";
            this.chkGenerateComment.UseVisualStyleBackColor = true;
            // 
            // frmGenerateScripts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 623);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmGenerateScripts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Generate Scripts";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmGenerateScripts_FormClosing);
            this.Load += new System.EventHandler(this.frmGenerateScripts_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelConnector.ResumeLayout(false);
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.RichTextBox txtMessage;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
        private System.Windows.Forms.Panel panelConnector;
        private System.Windows.Forms.CheckBox chkTreatBytesAsNull;
        private System.Windows.Forms.CheckBox chkGenerateComment;
        private System.Windows.Forms.CheckBox chkGenerateIdentity;
    }
}