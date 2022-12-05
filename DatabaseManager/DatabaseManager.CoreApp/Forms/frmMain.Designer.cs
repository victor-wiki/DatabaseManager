namespace DatabaseManager
{
    partial class frmMain
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

        #region Windows Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tsmiSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSetting = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDbConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiBackupSetting = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiLock = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiTools = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiWktView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiImageViewer = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.navigator = new DatabaseManager.Controls.UC_DbObjectsNavigator();
            this.panelContent = new System.Windows.Forms.Panel();
            this.ucContent = new DatabaseManager.Controls.UC_DbObjectContent();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsBtnAddQuery = new System.Windows.Forms.ToolStripButton();
            this.tsBtnOpenFile = new System.Windows.Forms.ToolStripButton();
            this.tsBtnSave = new System.Windows.Forms.ToolStripButton();
            this.tsBtnGenerateScripts = new System.Windows.Forms.ToolStripButton();
            this.tsBtnCompare = new System.Windows.Forms.ToolStripButton();
            this.tsBtnConvert = new System.Windows.Forms.ToolStripButton();
            this.tsBtnTranslateScript = new System.Windows.Forms.ToolStripButton();
            this.tsBtnRun = new System.Windows.Forms.ToolStripButton();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelContent.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.White;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiSettings,
            this.tsmiTools});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(917, 27);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tsmiSettings
            // 
            this.tsmiSettings.BackColor = System.Drawing.Color.White;
            this.tsmiSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.tsmiSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiSetting,
            this.tsmiDbConnection,
            this.tsmiBackupSetting,
            this.tsmiLock});
            this.tsmiSettings.Image = global::DatabaseManager.Resources.Settings;
            this.tsmiSettings.Name = "tsmiSettings";
            this.tsmiSettings.Size = new System.Drawing.Size(82, 21);
            this.tsmiSettings.Text = "Settings";
            // 
            // tsmiSetting
            // 
            this.tsmiSetting.Image = global::DatabaseManager.Resources.Config;
            this.tsmiSetting.Name = "tsmiSetting";
            this.tsmiSetting.Size = new System.Drawing.Size(163, 22);
            this.tsmiSetting.Text = "Setting";
            this.tsmiSetting.Click += new System.EventHandler(this.tsmiSetting_Click);
            // 
            // tsmiDbConnection
            // 
            this.tsmiDbConnection.Image = ((System.Drawing.Image)(resources.GetObject("tsmiDbConnection.Image")));
            this.tsmiDbConnection.Name = "tsmiDbConnection";
            this.tsmiDbConnection.Size = new System.Drawing.Size(163, 22);
            this.tsmiDbConnection.Text = "Connection";
            this.tsmiDbConnection.Click += new System.EventHandler(this.tsmiDbConnection_Click);
            // 
            // tsmiBackupSetting
            // 
            this.tsmiBackupSetting.Image = ((System.Drawing.Image)(resources.GetObject("tsmiBackupSetting.Image")));
            this.tsmiBackupSetting.Name = "tsmiBackupSetting";
            this.tsmiBackupSetting.Size = new System.Drawing.Size(163, 22);
            this.tsmiBackupSetting.Text = "Backup Setting";
            this.tsmiBackupSetting.Click += new System.EventHandler(this.tsmiBackupSetting_Click);
            // 
            // tsmiLock
            // 
            this.tsmiLock.Image = global::DatabaseManager.Resources.Lock;
            this.tsmiLock.Name = "tsmiLock";
            this.tsmiLock.Size = new System.Drawing.Size(163, 22);
            this.tsmiLock.Text = "Lock";
            this.tsmiLock.Click += new System.EventHandler(this.tsmiLock_Click);
            // 
            // tsmiTools
            // 
            this.tsmiTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiWktView,
            this.tsmiImageViewer});
            this.tsmiTools.Image = global::DatabaseManager.Resources.Tool16;
            this.tsmiTools.Name = "tsmiTools";
            this.tsmiTools.Size = new System.Drawing.Size(68, 21);
            this.tsmiTools.Text = "Tools";
            // 
            // tsmiWktView
            // 
            this.tsmiWktView.Image = global::DatabaseManager.Resources.Polygon;
            this.tsmiWktView.Name = "tsmiWktView";
            this.tsmiWktView.Size = new System.Drawing.Size(180, 22);
            this.tsmiWktView.Text = "WKT Viewer";
            this.tsmiWktView.Click += new System.EventHandler(this.tsmiWktView_Click);
            // 
            // tsmiImageViewer
            // 
            this.tsmiImageViewer.Image = global::DatabaseManager.Resources.Image;
            this.tsmiImageViewer.Name = "tsmiImageViewer";
            this.tsmiImageViewer.Size = new System.Drawing.Size(180, 22);
            this.tsmiImageViewer.Text = "Image Viewer";
            this.tsmiImageViewer.Click += new System.EventHandler(this.tsmiImageViewer_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 65);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.navigator);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.White;
            this.splitContainer1.Panel2.Controls.Add(this.panelContent);
            this.splitContainer1.Size = new System.Drawing.Size(917, 495);
            this.splitContainer1.SplitterDistance = 175;
            this.splitContainer1.TabIndex = 8;
            // 
            // navigator
            // 
            this.navigator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.navigator.Location = new System.Drawing.Point(0, 0);
            this.navigator.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.navigator.Name = "navigator";
            this.navigator.Size = new System.Drawing.Size(175, 495);
            this.navigator.TabIndex = 0;
            // 
            // panelContent
            // 
            this.panelContent.Controls.Add(this.ucContent);
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Margin = new System.Windows.Forms.Padding(4);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(738, 495);
            this.panelContent.TabIndex = 0;
            // 
            // ucContent
            // 
            this.ucContent.BackColor = System.Drawing.SystemColors.Control;
            this.ucContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucContent.Location = new System.Drawing.Point(0, 0);
            this.ucContent.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.ucContent.Name = "ucContent";
            this.ucContent.Size = new System.Drawing.Size(738, 495);
            this.ucContent.TabIndex = 0;
            this.ucContent.Visible = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBtnAddQuery,
            this.tsBtnOpenFile,
            this.tsBtnSave,
            this.tsBtnGenerateScripts,
            this.tsBtnCompare,
            this.tsBtnConvert,
            this.tsBtnTranslateScript,
            this.tsBtnRun});
            this.toolStrip1.Location = new System.Drawing.Point(0, 27);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.toolStrip1.Size = new System.Drawing.Size(917, 43);
            this.toolStrip1.TabIndex = 10;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsBtnAddQuery
            // 
            this.tsBtnAddQuery.AutoSize = false;
            this.tsBtnAddQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnAddQuery.Image = ((System.Drawing.Image)(resources.GetObject("tsBtnAddQuery.Image")));
            this.tsBtnAddQuery.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsBtnAddQuery.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnAddQuery.Name = "tsBtnAddQuery";
            this.tsBtnAddQuery.Size = new System.Drawing.Size(40, 40);
            this.tsBtnAddQuery.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsBtnAddQuery.ToolTipText = "New Query";
            this.tsBtnAddQuery.Click += new System.EventHandler(this.tsBtnAddQuery_Click);
            // 
            // tsBtnOpenFile
            // 
            this.tsBtnOpenFile.AutoSize = false;
            this.tsBtnOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnOpenFile.Image = ((System.Drawing.Image)(resources.GetObject("tsBtnOpenFile.Image")));
            this.tsBtnOpenFile.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsBtnOpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnOpenFile.Name = "tsBtnOpenFile";
            this.tsBtnOpenFile.Size = new System.Drawing.Size(40, 40);
            this.tsBtnOpenFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsBtnOpenFile.ToolTipText = "Open File";
            this.tsBtnOpenFile.Click += new System.EventHandler(this.tsBtnOpenFile_Click);
            // 
            // tsBtnSave
            // 
            this.tsBtnSave.AutoSize = false;
            this.tsBtnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnSave.Image = ((System.Drawing.Image)(resources.GetObject("tsBtnSave.Image")));
            this.tsBtnSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsBtnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnSave.Name = "tsBtnSave";
            this.tsBtnSave.Size = new System.Drawing.Size(40, 40);
            this.tsBtnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsBtnSave.ToolTipText = "Save(Ctrl+S)";
            this.tsBtnSave.Click += new System.EventHandler(this.tsBtnSave_Click);
            // 
            // tsBtnGenerateScripts
            // 
            this.tsBtnGenerateScripts.AutoSize = false;
            this.tsBtnGenerateScripts.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnGenerateScripts.Image = ((System.Drawing.Image)(resources.GetObject("tsBtnGenerateScripts.Image")));
            this.tsBtnGenerateScripts.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsBtnGenerateScripts.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnGenerateScripts.Name = "tsBtnGenerateScripts";
            this.tsBtnGenerateScripts.Size = new System.Drawing.Size(40, 40);
            this.tsBtnGenerateScripts.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsBtnGenerateScripts.ToolTipText = "Generate Scripts";
            this.tsBtnGenerateScripts.Click += new System.EventHandler(this.tsBtnGenerateScripts_Click);
            // 
            // tsBtnCompare
            // 
            this.tsBtnCompare.AutoSize = false;
            this.tsBtnCompare.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnCompare.Image = ((System.Drawing.Image)(resources.GetObject("tsBtnCompare.Image")));
            this.tsBtnCompare.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsBtnCompare.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnCompare.Name = "tsBtnCompare";
            this.tsBtnCompare.Size = new System.Drawing.Size(40, 40);
            this.tsBtnCompare.ToolTipText = "Compare Database";
            this.tsBtnCompare.Click += new System.EventHandler(this.tsBtnCompare_Click);
            // 
            // tsBtnConvert
            // 
            this.tsBtnConvert.AutoSize = false;
            this.tsBtnConvert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnConvert.Image = ((System.Drawing.Image)(resources.GetObject("tsBtnConvert.Image")));
            this.tsBtnConvert.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsBtnConvert.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnConvert.Name = "tsBtnConvert";
            this.tsBtnConvert.Size = new System.Drawing.Size(40, 40);
            this.tsBtnConvert.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsBtnConvert.ToolTipText = "Convert Database";
            this.tsBtnConvert.Click += new System.EventHandler(this.tsBtnConvert_Click);
            // 
            // tsBtnTranslateScript
            // 
            this.tsBtnTranslateScript.AutoSize = false;
            this.tsBtnTranslateScript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnTranslateScript.Image = global::DatabaseManager.Resources.Translate;
            this.tsBtnTranslateScript.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsBtnTranslateScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnTranslateScript.Name = "tsBtnTranslateScript";
            this.tsBtnTranslateScript.Size = new System.Drawing.Size(40, 40);
            this.tsBtnTranslateScript.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsBtnTranslateScript.ToolTipText = "Translate Script";
            this.tsBtnTranslateScript.Click += new System.EventHandler(this.tsBtnTranslateScript_Click);
            // 
            // tsBtnRun
            // 
            this.tsBtnRun.AutoSize = false;
            this.tsBtnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsBtnRun.Image = ((System.Drawing.Image)(resources.GetObject("tsBtnRun.Image")));
            this.tsBtnRun.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.tsBtnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnRun.Name = "tsBtnRun";
            this.tsBtnRun.Size = new System.Drawing.Size(40, 40);
            this.tsBtnRun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsBtnRun.ToolTipText = "Run(F5)";
            this.tsBtnRun.Click += new System.EventHandler(this.tsBtnRun_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtMessage.ForeColor = System.Drawing.Color.Black;
            this.txtMessage.Location = new System.Drawing.Point(0, 563);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(4);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.Size = new System.Drawing.Size(917, 16);
            this.txtMessage.TabIndex = 11;
            this.txtMessage.MouseHover += new System.EventHandler(this.txtMessage_MouseHover);
            // 
            // dlgOpenFile
            // 
            this.dlgOpenFile.Filter = "sql file|*.sql|all files|*.*";
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(917, 579);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Manager";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmMain_DragDrop);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.frmMain_DragOver);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelContent.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiSettings;
        private System.Windows.Forms.ToolStripMenuItem tsmiSetting;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private Controls.UC_DbObjectsNavigator navigator;
        private System.Windows.Forms.Panel panelContent;
        private Controls.UC_DbObjectContent ucContent;
        private System.Windows.Forms.ToolStripMenuItem tsmiDbConnection;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsBtnGenerateScripts;
        private System.Windows.Forms.ToolStripButton tsBtnConvert;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.ToolStripButton tsBtnAddQuery;
        private System.Windows.Forms.ToolStripButton tsBtnRun;
        private System.Windows.Forms.ToolStripButton tsBtnSave;
        private System.Windows.Forms.ToolStripButton tsBtnOpenFile;
        private System.Windows.Forms.OpenFileDialog dlgOpenFile;
        private System.Windows.Forms.ToolStripMenuItem tsmiBackupSetting;
        private System.Windows.Forms.ToolStripButton tsBtnCompare;
        private System.Windows.Forms.ToolStripMenuItem tsmiLock;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripButton tsBtnTranslateScript;
        private System.Windows.Forms.ToolStripMenuItem tsmiTools;
        private System.Windows.Forms.ToolStripMenuItem tsmiWktView;
        private System.Windows.Forms.ToolStripMenuItem tsmiImageViewer;
    }
}

