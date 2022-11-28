using DatabaseInterpreter.Model;

namespace DatabaseManager
{
    partial class frmCompare
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCompare));
            this.btnCompare = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tlvDifferences = new BrightIdeasSoftware.TreeListView();
            this.colType = new BrightIdeasSoftware.OLVColumn();
            this.colSourceName = new BrightIdeasSoftware.OLVColumn();
            this.colTargetName = new BrightIdeasSoftware.OLVColumn();
            this.colChangeType = new BrightIdeasSoftware.OLVColumn();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.txtSource = new System.Windows.Forms.RichTextBox();
            this.txtTarget = new System.Windows.Forms.RichTextBox();
            this.btnSync = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiExpandAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateChangedScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiFindText = new System.Windows.Forms.ToolStripMenuItem();
            this.targetDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            this.sourceDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tlvDifferences)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCompare
            // 
            this.btnCompare.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompare.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnCompare.Location = new System.Drawing.Point(739, 7);
            this.btnCompare.Margin = new System.Windows.Forms.Padding(4);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(83, 55);
            this.btnCompare.TabIndex = 38;
            this.btnCompare.Text = "Compare";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(5, 70);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tlvDifferences);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(853, 490);
            this.splitContainer1.SplitterDistance = 302;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 41;
            // 
            // tlvDifferences
            // 
            this.tlvDifferences.AllColumns.Add(this.colType);
            this.tlvDifferences.AllColumns.Add(this.colSourceName);
            this.tlvDifferences.AllColumns.Add(this.colTargetName);
            this.tlvDifferences.AllColumns.Add(this.colChangeType);
            this.tlvDifferences.BackColor = System.Drawing.SystemColors.Window;
            this.tlvDifferences.CellEditUseWholeCell = false;
            this.tlvDifferences.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colType,
            this.colSourceName,
            this.colTargetName,
            this.colChangeType});
            this.tlvDifferences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlvDifferences.FullRowSelect = true;
            this.tlvDifferences.GridLines = true;
            this.tlvDifferences.HideSelection = false;
            this.tlvDifferences.Location = new System.Drawing.Point(0, 0);
            this.tlvDifferences.Margin = new System.Windows.Forms.Padding(4);
            this.tlvDifferences.MultiSelect = false;
            this.tlvDifferences.Name = "tlvDifferences";
            this.tlvDifferences.SelectColumnsOnRightClick = false;
            this.tlvDifferences.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.None;
            this.tlvDifferences.SelectedBackColor = System.Drawing.SystemColors.ButtonFace;
            this.tlvDifferences.SelectedForeColor = System.Drawing.Color.Black;
            this.tlvDifferences.ShowFilterMenuOnRightClick = false;
            this.tlvDifferences.ShowGroups = false;
            this.tlvDifferences.Size = new System.Drawing.Size(853, 302);
            this.tlvDifferences.SmallImageList = this.imageList1;
            this.tlvDifferences.TabIndex = 1;
            this.tlvDifferences.UseCompatibleStateImageBehavior = false;
            this.tlvDifferences.View = System.Windows.Forms.View.Details;
            this.tlvDifferences.VirtualMode = true;
            this.tlvDifferences.SelectedIndexChanged += new System.EventHandler(this.tlvDifferences_SelectedIndexChanged);
            this.tlvDifferences.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tlvDifferences_KeyDown);
            this.tlvDifferences.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tlvDifferences_MouseDown);
            // 
            // colType
            // 
            this.colType.AspectName = "Type";
            this.colType.Sortable = false;
            this.colType.Text = "Type";
            this.colType.Width = 180;
            // 
            // colSourceName
            // 
            this.colSourceName.AspectName = "SourceName";
            this.colSourceName.Sortable = false;
            this.colSourceName.Text = "Source Name";
            this.colSourceName.Width = 280;
            // 
            // colTargetName
            // 
            this.colTargetName.AspectName = "TargetName";
            this.colTargetName.Sortable = false;
            this.colTargetName.Text = "Target Name";
            this.colTargetName.Width = 280;
            // 
            // colChangeType
            // 
            this.colChangeType.AspectName = "DifferenceType";
            this.colChangeType.Sortable = false;
            this.colChangeType.Text = "Change Type";
            this.colChangeType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colChangeType.Width = 90;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "tree_Fake.png");
            this.imageList1.Images.SetKeyName(1, "tree_Folder.png");
            this.imageList1.Images.SetKeyName(2, "tree_TableForeignKey.png");
            this.imageList1.Images.SetKeyName(3, "tree_Procedure.png");
            this.imageList1.Images.SetKeyName(4, "tree_View.png");
            this.imageList1.Images.SetKeyName(5, "tree_TableIndex.png");
            this.imageList1.Images.SetKeyName(6, "tree_TablePrimaryKey.png");
            this.imageList1.Images.SetKeyName(7, "tree_Table.png");
            this.imageList1.Images.SetKeyName(8, "tree_TableConstraint.png");
            this.imageList1.Images.SetKeyName(9, "tree_TableTrigger.png");
            this.imageList1.Images.SetKeyName(10, "tree_Function.png");
            this.imageList1.Images.SetKeyName(11, "tree_TableColumn.png");
            this.imageList1.Images.SetKeyName(12, "tree_UserDefinedType.png");
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.txtSource);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.txtTarget);
            this.splitContainer2.Size = new System.Drawing.Size(853, 185);
            this.splitContainer2.SplitterDistance = 407;
            this.splitContainer2.SplitterWidth = 1;
            this.splitContainer2.TabIndex = 0;
            // 
            // txtSource
            // 
            this.txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSource.Location = new System.Drawing.Point(0, 0);
            this.txtSource.Margin = new System.Windows.Forms.Padding(4);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(407, 185);
            this.txtSource.TabIndex = 0;
            this.txtSource.Text = "";
            this.txtSource.WordWrap = false;
            this.txtSource.VScroll += new System.EventHandler(this.txtSource_VScroll);
            // 
            // txtTarget
            // 
            this.txtTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTarget.Location = new System.Drawing.Point(0, 0);
            this.txtTarget.Margin = new System.Windows.Forms.Padding(4);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.Size = new System.Drawing.Size(445, 185);
            this.txtTarget.TabIndex = 1;
            this.txtTarget.Text = "";
            this.txtTarget.WordWrap = false;
            this.txtTarget.VScroll += new System.EventHandler(this.txtTarget_VScroll);
            // 
            // btnSync
            // 
            this.btnSync.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSync.Location = new System.Drawing.Point(674, 566);
            this.btnSync.Margin = new System.Windows.Forms.Padding(4);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(88, 33);
            this.btnSync.TabIndex = 42;
            this.btnSync.Text = "Sync";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(770, 566);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 33);
            this.btnClose.TabIndex = 43;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(568, 566);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(4);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(98, 33);
            this.btnGenerate.TabIndex = 44;
            this.btnGenerate.Text = "Generate";
            this.toolTip1.SetToolTip(this.btnGenerate, "Generate Changed Scripts");
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // imageList2
            // 
            this.imageList2.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "Add.png");
            this.imageList2.Images.SetKeyName(1, "Remove.png");
            this.imageList2.Images.SetKeyName(2, "Edit.png");
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtMessage.BackColor = System.Drawing.SystemColors.Control;
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Location = new System.Drawing.Point(5, 574);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(4);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(400, 16);
            this.txtMessage.TabIndex = 45;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiExpandAll,
            this.tsmiCollapseAll,
            this.tsmiGenerateChangedScripts,
            this.tsmiFindText});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(229, 92);
            // 
            // tsmiExpandAll
            // 
            this.tsmiExpandAll.Name = "tsmiExpandAll";
            this.tsmiExpandAll.Size = new System.Drawing.Size(228, 22);
            this.tsmiExpandAll.Text = "Expand All";
            this.tsmiExpandAll.Visible = false;
            this.tsmiExpandAll.Click += new System.EventHandler(this.tsmiExpandAll_Click);
            // 
            // tsmiCollapseAll
            // 
            this.tsmiCollapseAll.Name = "tsmiCollapseAll";
            this.tsmiCollapseAll.Size = new System.Drawing.Size(228, 22);
            this.tsmiCollapseAll.Text = "Collapse All";
            this.tsmiCollapseAll.Visible = false;
            this.tsmiCollapseAll.Click += new System.EventHandler(this.tsmiCollapseAll_Click);
            // 
            // tsmiGenerateChangedScripts
            // 
            this.tsmiGenerateChangedScripts.Name = "tsmiGenerateChangedScripts";
            this.tsmiGenerateChangedScripts.Size = new System.Drawing.Size(228, 22);
            this.tsmiGenerateChangedScripts.Text = "Generate Changed Scripts";
            this.tsmiGenerateChangedScripts.Visible = false;
            this.tsmiGenerateChangedScripts.Click += new System.EventHandler(this.tsmiGenerateChangedScripts_Click);
            // 
            // tsmiFindText
            // 
            this.tsmiFindText.Name = "tsmiFindText";
            this.tsmiFindText.Size = new System.Drawing.Size(228, 22);
            this.tsmiFindText.Text = "Find Text";
            this.tsmiFindText.Click += new System.EventHandler(this.tsmiFindText_Click);
            // 
            // targetDbProfile
            // 
            this.targetDbProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetDbProfile.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            this.targetDbProfile.EnableDatabaseType = true;
            this.targetDbProfile.Location = new System.Drawing.Point(16, 36);
            this.targetDbProfile.Margin = new System.Windows.Forms.Padding(0);
            this.targetDbProfile.Name = "targetDbProfile";
            this.targetDbProfile.Size = new System.Drawing.Size(717, 26);
            this.targetDbProfile.TabIndex = 40;
            this.targetDbProfile.Title = "Target:";
            this.targetDbProfile.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.targetDbProfile_OnSelectedChanged);
            // 
            // sourceDbProfile
            // 
            this.sourceDbProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceDbProfile.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            this.sourceDbProfile.EnableDatabaseType = true;
            this.sourceDbProfile.Location = new System.Drawing.Point(16, 5);
            this.sourceDbProfile.Margin = new System.Windows.Forms.Padding(0);
            this.sourceDbProfile.Name = "sourceDbProfile";
            this.sourceDbProfile.Size = new System.Drawing.Size(717, 29);
            this.sourceDbProfile.TabIndex = 39;
            this.sourceDbProfile.Title = "Source:";
            this.sourceDbProfile.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.sourceDbProfile_OnSelectedChanged);
            // 
            // frmCompare
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 603);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSync);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.targetDbProfile);
            this.Controls.Add(this.sourceDbProfile);
            this.Controls.Add(this.btnCompare);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmCompare";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Compare";
            this.Load += new System.EventHandler(this.frmCompare_Load);
            this.SizeChanged += new System.EventHandler(this.frmCompare_SizeChanged);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tlvDifferences)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.UC_DbConnectionProfile targetDbProfile;
        private Controls.UC_DbConnectionProfile sourceDbProfile;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private BrightIdeasSoftware.TreeListView tlvDifferences;
        private BrightIdeasSoftware.OLVColumn colType;
        private BrightIdeasSoftware.OLVColumn colSourceName;
        private BrightIdeasSoftware.OLVColumn colTargetName;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox txtSource;
        private System.Windows.Forms.RichTextBox txtTarget;
        private BrightIdeasSoftware.OLVColumn colChangeType;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiExpandAll;
        private System.Windows.Forms.ToolStripMenuItem tsmiCollapseAll;
        private System.Windows.Forms.ToolStripMenuItem tsmiGenerateChangedScripts;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiFindText;
    }
}