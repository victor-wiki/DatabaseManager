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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            tsmiSettings = new System.Windows.Forms.ToolStripMenuItem();
            tsmiSetting = new System.Windows.Forms.ToolStripMenuItem();
            tsmiDbConnection = new System.Windows.Forms.ToolStripMenuItem();
            tsmiBackupSetting = new System.Windows.Forms.ToolStripMenuItem();
            tsmiLock = new System.Windows.Forms.ToolStripMenuItem();
            tsmiTools = new System.Windows.Forms.ToolStripMenuItem();
            tsmiWktView = new System.Windows.Forms.ToolStripMenuItem();
            tsmiImageViewer = new System.Windows.Forms.ToolStripMenuItem();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            navigator = new Controls.UC_DbObjectsNavigator();
            panelContent = new System.Windows.Forms.Panel();
            ucContent = new Controls.UC_DbObjectContent();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsBtnAddQuery = new System.Windows.Forms.ToolStripButton();
            tsBtnOpenFile = new System.Windows.Forms.ToolStripButton();
            tsBtnSave = new System.Windows.Forms.ToolStripButton();
            tsBtnGenerateScripts = new System.Windows.Forms.ToolStripButton();
            tsBtnCompare = new System.Windows.Forms.ToolStripButton();
            tsBtnConvert = new System.Windows.Forms.ToolStripButton();
            tsBtnTranslateScript = new System.Windows.Forms.ToolStripButton();
            tsBtnRun = new System.Windows.Forms.ToolStripButton();
            txtMessage = new System.Windows.Forms.TextBox();
            dlgOpenFile = new System.Windows.Forms.OpenFileDialog();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panelContent.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = System.Drawing.Color.White;
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiSettings, tsmiTools });
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
            tsmiSettings.Image = Resources.Settings;
            tsmiSettings.Name = "tsmiSettings";
            tsmiSettings.Size = new System.Drawing.Size(82, 21);
            tsmiSettings.Text = "Settings";
            // 
            // tsmiSetting
            // 
            tsmiSetting.Image = Resources.Config;
            tsmiSetting.Name = "tsmiSetting";
            tsmiSetting.Size = new System.Drawing.Size(163, 22);
            tsmiSetting.Text = "Setting";
            tsmiSetting.Click += tsmiSetting_Click;
            // 
            // tsmiDbConnection
            // 
            tsmiDbConnection.Image = (System.Drawing.Image)resources.GetObject("tsmiDbConnection.Image");
            tsmiDbConnection.Name = "tsmiDbConnection";
            tsmiDbConnection.Size = new System.Drawing.Size(163, 22);
            tsmiDbConnection.Text = "Connection";
            tsmiDbConnection.Click += tsmiDbConnection_Click;
            // 
            // tsmiBackupSetting
            // 
            tsmiBackupSetting.Image = (System.Drawing.Image)resources.GetObject("tsmiBackupSetting.Image");
            tsmiBackupSetting.Name = "tsmiBackupSetting";
            tsmiBackupSetting.Size = new System.Drawing.Size(163, 22);
            tsmiBackupSetting.Text = "Backup Setting";
            tsmiBackupSetting.Click += tsmiBackupSetting_Click;
            // 
            // tsmiLock
            // 
            tsmiLock.Image = Resources.Lock;
            tsmiLock.Name = "tsmiLock";
            tsmiLock.Size = new System.Drawing.Size(163, 22);
            tsmiLock.Text = "Lock";
            tsmiLock.Click += tsmiLock_Click;
            // 
            // tsmiTools
            // 
            tsmiTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiWktView, tsmiImageViewer });
            tsmiTools.Image = Resources.Tool16;
            tsmiTools.Name = "tsmiTools";
            tsmiTools.Size = new System.Drawing.Size(68, 21);
            tsmiTools.Text = "Tools";
            // 
            // tsmiWktView
            // 
            tsmiWktView.Image = Resources.Polygon;
            tsmiWktView.Name = "tsmiWktView";
            tsmiWktView.Size = new System.Drawing.Size(156, 22);
            tsmiWktView.Text = "WKT Viewer";
            tsmiWktView.Click += tsmiWktView_Click;
            // 
            // tsmiImageViewer
            // 
            tsmiImageViewer.Image = Resources.Image;
            tsmiImageViewer.Name = "tsmiImageViewer";
            tsmiImageViewer.Size = new System.Drawing.Size(156, 22);
            tsmiImageViewer.Text = "Image Viewer";
            tsmiImageViewer.Click += tsmiImageViewer_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(0, 65);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(navigator);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = System.Drawing.Color.White;
            splitContainer1.Panel2.Controls.Add(panelContent);
            splitContainer1.Size = new System.Drawing.Size(917, 495);
            splitContainer1.SplitterDistance = 175;
            splitContainer1.TabIndex = 8;
            // 
            // navigator
            // 
            navigator.Dock = System.Windows.Forms.DockStyle.Fill;
            navigator.Location = new System.Drawing.Point(0, 0);
            navigator.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            navigator.Name = "navigator";
            navigator.Size = new System.Drawing.Size(175, 495);
            navigator.TabIndex = 0;
            // 
            // panelContent
            // 
            panelContent.Controls.Add(ucContent);
            panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            panelContent.Location = new System.Drawing.Point(0, 0);
            panelContent.Margin = new System.Windows.Forms.Padding(4);
            panelContent.Name = "panelContent";
            panelContent.Size = new System.Drawing.Size(738, 495);
            panelContent.TabIndex = 0;
            // 
            // ucContent
            // 
            ucContent.BackColor = System.Drawing.SystemColors.Control;
            ucContent.Dock = System.Windows.Forms.DockStyle.Fill;
            ucContent.Location = new System.Drawing.Point(0, 0);
            ucContent.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            ucContent.Name = "ucContent";
            ucContent.Size = new System.Drawing.Size(738, 495);
            ucContent.TabIndex = 0;
            ucContent.Visible = false;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsBtnAddQuery, tsBtnOpenFile, tsBtnSave, tsBtnGenerateScripts, tsBtnCompare, tsBtnConvert, tsBtnTranslateScript, tsBtnRun });
            toolStrip1.Location = new System.Drawing.Point(0, 27);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 4, 0);
            toolStrip1.Size = new System.Drawing.Size(917, 43);
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
            tsBtnAddQuery.Size = new System.Drawing.Size(40, 40);
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
            tsBtnOpenFile.Size = new System.Drawing.Size(40, 40);
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
            tsBtnSave.Size = new System.Drawing.Size(40, 40);
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
            tsBtnGenerateScripts.Size = new System.Drawing.Size(40, 40);
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
            tsBtnCompare.Size = new System.Drawing.Size(40, 40);
            tsBtnCompare.ToolTipText = "Compare Database";
            tsBtnCompare.Click += tsBtnCompare_Click;
            // 
            // tsBtnConvert
            // 
            tsBtnConvert.AutoSize = false;
            tsBtnConvert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsBtnConvert.Image = (System.Drawing.Image)resources.GetObject("tsBtnConvert.Image");
            tsBtnConvert.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsBtnConvert.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsBtnConvert.Name = "tsBtnConvert";
            tsBtnConvert.Size = new System.Drawing.Size(40, 40);
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
            tsBtnTranslateScript.Size = new System.Drawing.Size(40, 40);
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
            tsBtnRun.Size = new System.Drawing.Size(40, 40);
            tsBtnRun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            tsBtnRun.ToolTipText = "Run(F5)";
            tsBtnRun.Click += tsBtnRun_Click;
            // 
            // txtMessage
            // 
            txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            txtMessage.ForeColor = System.Drawing.Color.Black;
            txtMessage.Location = new System.Drawing.Point(0, 563);
            txtMessage.Margin = new System.Windows.Forms.Padding(4);
            txtMessage.Name = "txtMessage";
            txtMessage.ReadOnly = true;
            txtMessage.Size = new System.Drawing.Size(917, 16);
            txtMessage.TabIndex = 11;
            txtMessage.MouseHover += txtMessage_MouseHover;
            // 
            // dlgOpenFile
            // 
            dlgOpenFile.Filter = "sql file|*.sql|all files|*.*";
            // 
            // frmMain
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(917, 579);
            Controls.Add(txtMessage);
            Controls.Add(toolStrip1);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmMain";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Manager2";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Load += frmMain_Load;
            DragDrop += frmMain_DragDrop;
            DragOver += frmMain_DragOver;
            KeyDown += frmMain_KeyDown;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panelContent.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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

