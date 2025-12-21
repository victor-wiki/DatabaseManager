namespace DatabaseManager.Forms
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            tsmiSettings = new System.Windows.Forms.ToolStripMenuItem();
            tsmiSetting = new System.Windows.Forms.ToolStripMenuItem();
            tsmiDbConnection = new System.Windows.Forms.ToolStripMenuItem();
            tsmiBackupSetting = new System.Windows.Forms.ToolStripMenuItem();
            tsmiLock = new System.Windows.Forms.ToolStripMenuItem();
            tsmiViews = new System.Windows.Forms.ToolStripMenuItem();
            tsmiObjectsExplorer = new System.Windows.Forms.ToolStripMenuItem();
            tsmiMessage = new System.Windows.Forms.ToolStripMenuItem();
            tsmiTools = new System.Windows.Forms.ToolStripMenuItem();
            tsmiWktView = new System.Windows.Forms.ToolStripMenuItem();
            tsmiImageViewer = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsBtnAddQuery = new System.Windows.Forms.ToolStripButton();
            tsBtnOpenFile = new System.Windows.Forms.ToolStripButton();
            tsBtnSave = new System.Windows.Forms.ToolStripButton();
            tsBtnGenerateScripts = new System.Windows.Forms.ToolStripButton();
            tsBtnCompare = new System.Windows.Forms.ToolStripButton();
            tsBtnDataCompare = new System.Windows.Forms.ToolStripButton();
            tsBtnConvert = new System.Windows.Forms.ToolStripButton();
            tsBtnTranslateScript = new System.Windows.Forms.ToolStripButton();
            tsBtnRun = new System.Windows.Forms.ToolStripButton();
            dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            dockPanelMain = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = System.Drawing.Color.White;
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiSettings, tsmiViews, tsmiTools });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            menuStrip1.Size = new System.Drawing.Size(917, 27);
            menuStrip1.TabIndex = 5;
            menuStrip1.Text = "menuStrip1";
            // 
            // tsmiSettings
            // 
            tsmiSettings.BackColor = System.Drawing.Color.White;
            tsmiSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            tsmiSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiSetting, tsmiDbConnection, tsmiBackupSetting, tsmiLock });
            tsmiSettings.Name = "tsmiSettings";
            tsmiSettings.Size = new System.Drawing.Size(66, 21);
            tsmiSettings.Text = "Settings";
            // 
            // tsmiSetting
            // 
            tsmiSetting.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsmiSetting.Name = "tsmiSetting";
            tsmiSetting.Size = new System.Drawing.Size(163, 22);
            tsmiSetting.Text = "Setting";
            tsmiSetting.Click += tsmiSetting_Click;
            // 
            // tsmiDbConnection
            // 
            tsmiDbConnection.Image = Resources.DbConnect16;
            tsmiDbConnection.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsmiDbConnection.Name = "tsmiDbConnection";
            tsmiDbConnection.Size = new System.Drawing.Size(163, 22);
            tsmiDbConnection.Text = "Connection";
            tsmiDbConnection.Click += tsmiDbConnection_Click;
            // 
            // tsmiBackupSetting
            // 
            tsmiBackupSetting.Image = Resources.DbBackup;
            tsmiBackupSetting.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsmiBackupSetting.Name = "tsmiBackupSetting";
            tsmiBackupSetting.Size = new System.Drawing.Size(163, 22);
            tsmiBackupSetting.Text = "Backup Setting";
            tsmiBackupSetting.Click += tsmiBackupSetting_Click;
            // 
            // tsmiLock
            // 
            tsmiLock.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsmiLock.Name = "tsmiLock";
            tsmiLock.Size = new System.Drawing.Size(163, 22);
            tsmiLock.Text = "Lock";
            tsmiLock.Click += tsmiLock_Click;
            // 
            // tsmiViews
            // 
            tsmiViews.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiObjectsExplorer, tsmiMessage });
            tsmiViews.Name = "tsmiViews";
            tsmiViews.Size = new System.Drawing.Size(53, 21);
            tsmiViews.Text = "Views";
            // 
            // tsmiObjectsExplorer
            // 
            tsmiObjectsExplorer.Name = "tsmiObjectsExplorer";
            tsmiObjectsExplorer.Size = new System.Drawing.Size(173, 22);
            tsmiObjectsExplorer.Text = "Objects Explorer";
            tsmiObjectsExplorer.Click += tsmiObjectsExplorer_Click;
            // 
            // tsmiMessage
            // 
            tsmiMessage.Name = "tsmiMessage";
            tsmiMessage.Size = new System.Drawing.Size(173, 22);
            tsmiMessage.Text = "Message";
            tsmiMessage.Click += tsmiMessage_Click;
            // 
            // tsmiTools
            // 
            tsmiTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiWktView, tsmiImageViewer });
            tsmiTools.Name = "tsmiTools";
            tsmiTools.Size = new System.Drawing.Size(52, 21);
            tsmiTools.Text = "Tools";
            // 
            // tsmiWktView
            // 
            tsmiWktView.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsmiWktView.Name = "tsmiWktView";
            tsmiWktView.Size = new System.Drawing.Size(156, 22);
            tsmiWktView.Text = "WKT Viewer";
            tsmiWktView.Click += tsmiWktView_Click;
            // 
            // tsmiImageViewer
            // 
            tsmiImageViewer.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsmiImageViewer.Name = "tsmiImageViewer";
            tsmiImageViewer.Size = new System.Drawing.Size(156, 22);
            tsmiImageViewer.Text = "Image Viewer";
            tsmiImageViewer.Click += tsmiImageViewer_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.AutoSize = false;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsBtnAddQuery, tsBtnOpenFile, tsBtnSave, tsBtnGenerateScripts, tsBtnCompare, tsBtnDataCompare, tsBtnConvert, tsBtnTranslateScript, tsBtnRun });
            toolStrip1.Location = new System.Drawing.Point(0, 27);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            toolStrip1.Size = new System.Drawing.Size(917, 40);
            toolStrip1.TabIndex = 10;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsBtnAddQuery
            // 
            tsBtnAddQuery.AutoSize = false;
            tsBtnAddQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnAddQuery.Image = (System.Drawing.Image)resources.GetObject("tsBtnAddQuery.Image");
            tsBtnAddQuery.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnAddQuery.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnAddQuery.Name = "tsBtnAddQuery";
            tsBtnAddQuery.Size = new System.Drawing.Size(35, 35);
            tsBtnAddQuery.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            tsBtnAddQuery.ToolTipText = "New Query";
            tsBtnAddQuery.Click += tsBtnAddQuery_Click;
            // 
            // tsBtnOpenFile
            // 
            tsBtnOpenFile.AutoSize = false;
            tsBtnOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnOpenFile.Image = (System.Drawing.Image)resources.GetObject("tsBtnOpenFile.Image");
            tsBtnOpenFile.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnOpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnOpenFile.Name = "tsBtnOpenFile";
            tsBtnOpenFile.Size = new System.Drawing.Size(35, 35);
            tsBtnOpenFile.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            tsBtnOpenFile.ToolTipText = "Open File";
            tsBtnOpenFile.Click += tsBtnOpenFile_Click;
            // 
            // tsBtnSave
            // 
            tsBtnSave.AutoSize = false;
            tsBtnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnSave.Image = (System.Drawing.Image)resources.GetObject("tsBtnSave.Image");
            tsBtnSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnSave.Name = "tsBtnSave";
            tsBtnSave.Size = new System.Drawing.Size(35, 35);
            tsBtnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            tsBtnSave.ToolTipText = "Save(Ctrl+S)";
            tsBtnSave.Click += tsBtnSave_Click;
            // 
            // tsBtnGenerateScripts
            // 
            tsBtnGenerateScripts.AutoSize = false;
            tsBtnGenerateScripts.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnGenerateScripts.Image = (System.Drawing.Image)resources.GetObject("tsBtnGenerateScripts.Image");
            tsBtnGenerateScripts.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnGenerateScripts.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnGenerateScripts.Name = "tsBtnGenerateScripts";
            tsBtnGenerateScripts.Size = new System.Drawing.Size(35, 35);
            tsBtnGenerateScripts.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            tsBtnGenerateScripts.ToolTipText = "Generate Scripts";
            tsBtnGenerateScripts.Click += tsBtnGenerateScripts_Click;
            // 
            // tsBtnCompare
            // 
            tsBtnCompare.AutoSize = false;
            tsBtnCompare.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnCompare.Image = (System.Drawing.Image)resources.GetObject("tsBtnCompare.Image");
            tsBtnCompare.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnCompare.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnCompare.Name = "tsBtnCompare";
            tsBtnCompare.Size = new System.Drawing.Size(35, 35);
            tsBtnCompare.ToolTipText = "Schema Compare";
            tsBtnCompare.Click += tsBtnCompare_Click;
            // 
            // tsBtnDataCompare
            // 
            tsBtnDataCompare.AutoSize = false;
            tsBtnDataCompare.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnDataCompare.Image = Resources.DataCompare32;
            tsBtnDataCompare.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnDataCompare.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnDataCompare.Name = "tsBtnDataCompare";
            tsBtnDataCompare.Size = new System.Drawing.Size(35, 35);
            tsBtnDataCompare.Text = "Data Compare";
            tsBtnDataCompare.Click += tsBtnDataCompare_Click;
            // 
            // tsBtnConvert
            // 
            tsBtnConvert.AutoSize = false;
            tsBtnConvert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnConvert.Image = (System.Drawing.Image)resources.GetObject("tsBtnConvert.Image");
            tsBtnConvert.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnConvert.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnConvert.Name = "tsBtnConvert";
            tsBtnConvert.Size = new System.Drawing.Size(35, 35);
            tsBtnConvert.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            tsBtnConvert.ToolTipText = "Convert Database";
            tsBtnConvert.Click += tsBtnConvert_Click;
            // 
            // tsBtnTranslateScript
            // 
            tsBtnTranslateScript.AutoSize = false;
            tsBtnTranslateScript.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnTranslateScript.Image = Resources.Translate;
            tsBtnTranslateScript.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnTranslateScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnTranslateScript.Name = "tsBtnTranslateScript";
            tsBtnTranslateScript.Size = new System.Drawing.Size(35, 35);
            tsBtnTranslateScript.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            tsBtnTranslateScript.ToolTipText = "Translate Script";
            tsBtnTranslateScript.Click += tsBtnTranslateScript_Click;
            // 
            // tsBtnRun
            // 
            tsBtnRun.AutoSize = false;
            tsBtnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnRun.Image = (System.Drawing.Image)resources.GetObject("tsBtnRun.Image");
            tsBtnRun.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnRun.Name = "tsBtnRun";
            tsBtnRun.Size = new System.Drawing.Size(35, 35);
            tsBtnRun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            tsBtnRun.ToolTipText = "Run(F5)";
            tsBtnRun.Click += tsBtnRun_Click;
            // 
            // dlgOpenFile
            // 
            dlgOpenFile.Filter = "sql file|*.sql|all files|*.*";
            // 
            // dockPanelMain
            // 
            dockPanelMain.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dockPanelMain.Location = new System.Drawing.Point(0, 65);
            dockPanelMain.Name = "dockPanelMain";
            dockPanelMain.Size = new System.Drawing.Size(917, 511);
            dockPanelMain.TabIndex = 12;
            // 
            // frmMain
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(917, 579);
            Controls.Add(dockPanelMain);
            Controls.Add(toolStrip1);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmMain";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Manager";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            FormClosing += frmMain_FormClosing;
            Load += frmMain_Load;
            DragDrop += frmMain_DragDrop;
            DragOver += frmMain_DragOver;
            KeyDown += frmMain_KeyDown;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiSettings;
        private System.Windows.Forms.ToolStripMenuItem tsmiSetting;
        private System.Windows.Forms.ToolStripMenuItem tsmiDbConnection;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsBtnGenerateScripts;
        private System.Windows.Forms.ToolStripButton tsBtnConvert;
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
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanelMain;
        private System.Windows.Forms.ToolStripMenuItem tsmiViews;
        private System.Windows.Forms.ToolStripMenuItem tsmiObjectsExplorer;
        private System.Windows.Forms.ToolStripMenuItem tsmiMessage;
        private System.Windows.Forms.ToolStripButton tsBtnDataCompare;
    }
}

