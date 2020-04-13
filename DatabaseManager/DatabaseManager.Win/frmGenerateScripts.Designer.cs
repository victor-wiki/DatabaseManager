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
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
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
            this.splitContainer1.Size = new System.Drawing.Size(655, 512);
            this.splitContainer1.SplitterDistance = 441;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // chkTreatBytesAsNull
            // 
            this.chkTreatBytesAsNull.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkTreatBytesAsNull.AutoSize = true;
            this.chkTreatBytesAsNull.Checked = true;
            this.chkTreatBytesAsNull.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTreatBytesAsNull.Location = new System.Drawing.Point(18, 357);
            this.chkTreatBytesAsNull.Name = "chkTreatBytesAsNull";
            this.chkTreatBytesAsNull.Size = new System.Drawing.Size(138, 16);
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
            this.panelConnector.Location = new System.Drawing.Point(3, 3);
            this.panelConnector.Name = "panelConnector";
            this.panelConnector.Size = new System.Drawing.Size(649, 33);
            this.panelConnector.TabIndex = 53;
            // 
            // dbConnectionProfile
            // 
            this.dbConnectionProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dbConnectionProfile.Location = new System.Drawing.Point(6, 6);
            this.dbConnectionProfile.Margin = new System.Windows.Forms.Padding(0);
            this.dbConnectionProfile.Name = "dbConnectionProfile";
            this.dbConnectionProfile.Size = new System.Drawing.Size(560, 23);
            this.dbConnectionProfile.TabIndex = 42;
            this.dbConnectionProfile.Title = "Database:";
            this.dbConnectionProfile.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.dbConnectionProfile_OnSelectedChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Image = global::DatabaseManager.Properties.Resources.DbConnect16;
            this.btnConnect.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConnect.Location = new System.Drawing.Point(569, 6);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(71, 23);
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
            this.tvDbObjects.Location = new System.Drawing.Point(3, 38);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(649, 278);
            this.tvDbObjects.TabIndex = 43;
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.Location = new System.Drawing.Point(15, 384);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(89, 12);
            this.lblOutputFolder.TabIndex = 52;
            this.lblOutputFolder.Text = "Output folder:";
            // 
            // btnOutputFolder
            // 
            this.btnOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOutputFolder.Location = new System.Drawing.Point(606, 380);
            this.btnOutputFolder.Name = "btnOutputFolder";
            this.btnOutputFolder.Size = new System.Drawing.Size(36, 23);
            this.btnOutputFolder.TabIndex = 51;
            this.btnOutputFolder.Text = "...";
            this.btnOutputFolder.UseVisualStyleBackColor = true;
            this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtOutputFolder.Location = new System.Drawing.Point(110, 381);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(490, 21);
            this.txtOutputFolder.TabIndex = 50;
            // 
            // chkScriptData
            // 
            this.chkScriptData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkScriptData.AutoSize = true;
            this.chkScriptData.Checked = true;
            this.chkScriptData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptData.Location = new System.Drawing.Point(138, 327);
            this.chkScriptData.Name = "chkScriptData";
            this.chkScriptData.Size = new System.Drawing.Size(48, 16);
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
            this.chkScriptSchema.Location = new System.Drawing.Point(58, 327);
            this.chkScriptSchema.Name = "chkScriptSchema";
            this.chkScriptSchema.Size = new System.Drawing.Size(60, 16);
            this.chkScriptSchema.TabIndex = 48;
            this.chkScriptSchema.Text = "schema";
            this.chkScriptSchema.UseVisualStyleBackColor = true;
            // 
            // lblScriptsMode
            // 
            this.lblScriptsMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblScriptsMode.AutoSize = true;
            this.lblScriptsMode.Location = new System.Drawing.Point(16, 328);
            this.lblScriptsMode.Name = "lblScriptsMode";
            this.lblScriptsMode.Size = new System.Drawing.Size(35, 12);
            this.lblScriptsMode.TabIndex = 47;
            this.lblScriptsMode.Text = "Mode:";
            // 
            // btnClose
            // 
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnClose.Location = new System.Drawing.Point(343, 412);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 46;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnGenerate.Location = new System.Drawing.Point(246, 412);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
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
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(655, 68);
            this.txtMessage.TabIndex = 1;
            this.txtMessage.Text = "";
            // 
            // frmGenerateScripts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 512);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
    }
}