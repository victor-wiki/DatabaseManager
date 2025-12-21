using DatabaseInterpreter.Model;

namespace DatabaseManager.Forms
{
    partial class frmSchemaCompare
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSchemaCompare));
            btnCompare = new System.Windows.Forms.Button();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tlvDifferences = new BrightIdeasSoftware.TreeListView();
            colType = new BrightIdeasSoftware.OLVColumn();
            colSourceName = new BrightIdeasSoftware.OLVColumn();
            colTargetName = new BrightIdeasSoftware.OLVColumn();
            colChangeType = new BrightIdeasSoftware.OLVColumn();
            imageList1 = new System.Windows.Forms.ImageList(components);
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            txtSource = new SqlCodeEditor.TextEditorControlEx();
            txtTarget = new SqlCodeEditor.TextEditorControlEx();
            btnSync = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            btnGenerate = new System.Windows.Forms.Button();
            imageList2 = new System.Windows.Forms.ImageList(components);
            txtMessage = new System.Windows.Forms.TextBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiExpandAll = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
            tsmiGenerateChangedScripts = new System.Windows.Forms.ToolStripMenuItem();
            tsmiFindText = new System.Windows.Forms.ToolStripMenuItem();
            targetDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            sourceDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)tlvDifferences).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnCompare
            // 
            btnCompare.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnCompare.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F);
            btnCompare.Location = new System.Drawing.Point(739, 7);
            btnCompare.Margin = new System.Windows.Forms.Padding(4);
            btnCompare.Name = "btnCompare";
            btnCompare.Size = new System.Drawing.Size(83, 55);
            btnCompare.TabIndex = 38;
            btnCompare.Text = "Compare";
            btnCompare.UseVisualStyleBackColor = true;
            btnCompare.Click += btnCompare_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(5, 70);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tlvDifferences);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new System.Drawing.Size(853, 490);
            splitContainer1.SplitterDistance = 302;
            splitContainer1.SplitterWidth = 3;
            splitContainer1.TabIndex = 41;
            // 
            // tlvDifferences
            // 
            tlvDifferences.AllColumns.Add(colType);
            tlvDifferences.AllColumns.Add(colSourceName);
            tlvDifferences.AllColumns.Add(colTargetName);
            tlvDifferences.AllColumns.Add(colChangeType);
            tlvDifferences.BackColor = System.Drawing.SystemColors.Window;
            tlvDifferences.CellEditUseWholeCell = false;
            tlvDifferences.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colType, colSourceName, colTargetName, colChangeType });
            tlvDifferences.Dock = System.Windows.Forms.DockStyle.Fill;
            tlvDifferences.FullRowSelect = true;
            tlvDifferences.GridLines = true;
            tlvDifferences.Location = new System.Drawing.Point(0, 0);
            tlvDifferences.Margin = new System.Windows.Forms.Padding(4);
            tlvDifferences.MultiSelect = false;
            tlvDifferences.Name = "tlvDifferences";
            tlvDifferences.SelectColumnsOnRightClick = false;
            tlvDifferences.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.None;
            tlvDifferences.SelectedBackColor = System.Drawing.SystemColors.ButtonFace;
            tlvDifferences.SelectedForeColor = System.Drawing.Color.Black;
            tlvDifferences.ShowFilterMenuOnRightClick = false;
            tlvDifferences.ShowGroups = false;
            tlvDifferences.ShowImagesOnSubItems = true;
            tlvDifferences.Size = new System.Drawing.Size(853, 302);
            tlvDifferences.SmallImageList = imageList1;
            tlvDifferences.TabIndex = 1;
            tlvDifferences.UseCompatibleStateImageBehavior = false;
            tlvDifferences.View = System.Windows.Forms.View.Details;
            tlvDifferences.VirtualMode = true;
            tlvDifferences.SelectedIndexChanged += tlvDifferences_SelectedIndexChanged;
            tlvDifferences.KeyDown += tlvDifferences_KeyDown;
            tlvDifferences.MouseDown += tlvDifferences_MouseDown;
            // 
            // colType
            // 
            colType.AspectName = "Type";
            colType.Sortable = false;
            colType.Text = "Type";
            colType.Width = 180;
            // 
            // colSourceName
            // 
            colSourceName.AspectName = "SourceName";
            colSourceName.Sortable = false;
            colSourceName.Text = "Source Name";
            colSourceName.Width = 280;
            // 
            // colTargetName
            // 
            colTargetName.AspectName = "TargetName";
            colTargetName.Sortable = false;
            colTargetName.Text = "Target Name";
            colTargetName.Width = 280;
            // 
            // colChangeType
            // 
            colChangeType.AspectName = "DifferenceType";
            colChangeType.Sortable = false;
            colChangeType.Text = "Change Type";
            colChangeType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            colChangeType.Width = 90;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "tree_Fake.png");
            imageList1.Images.SetKeyName(1, "tree_Folder.png");
            imageList1.Images.SetKeyName(2, "tree_TableForeignKey.png");
            imageList1.Images.SetKeyName(3, "tree_Procedure.png");
            imageList1.Images.SetKeyName(4, "tree_View.png");
            imageList1.Images.SetKeyName(5, "tree_TableIndex.png");
            imageList1.Images.SetKeyName(6, "tree_TablePrimaryKey.png");
            imageList1.Images.SetKeyName(7, "tree_Table.png");
            imageList1.Images.SetKeyName(8, "tree_TableConstraint.png");
            imageList1.Images.SetKeyName(9, "tree_TableTrigger.png");
            imageList1.Images.SetKeyName(10, "tree_Function.png");
            imageList1.Images.SetKeyName(11, "tree_TableColumn.png");
            imageList1.Images.SetKeyName(12, "tree_UserDefinedType.png");
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(4);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(txtSource);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(txtTarget);
            splitContainer2.Size = new System.Drawing.Size(853, 185);
            splitContainer2.SplitterDistance = 407;
            splitContainer2.SplitterWidth = 10;
            splitContainer2.TabIndex = 0;
            // 
            // txtSource
            // 
            txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSource.EnableFolding = false;
            txtSource.FoldingStrategy = null;
            txtSource.Font = new System.Drawing.Font("Courier New", 10F);
            txtSource.Location = new System.Drawing.Point(0, 0);
            txtSource.Name = "txtSource";
            txtSource.ShowVRuler = false;
            txtSource.Size = new System.Drawing.Size(407, 185);
            txtSource.SyntaxHighlighting = "\"\"";
            txtSource.TabIndex = 0;
            // 
            // txtTarget
            // 
            txtTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            txtTarget.EnableFolding = false;
            txtTarget.FoldingStrategy = null;
            txtTarget.Font = new System.Drawing.Font("Courier New", 10F);
            txtTarget.Location = new System.Drawing.Point(0, 0);
            txtTarget.Name = "txtTarget";
            txtTarget.ShowVRuler = false;
            txtTarget.Size = new System.Drawing.Size(436, 185);
            txtTarget.SyntaxHighlighting = "\"\"";
            txtTarget.TabIndex = 0;
            // 
            // btnSync
            // 
            btnSync.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSync.Location = new System.Drawing.Point(674, 566);
            btnSync.Margin = new System.Windows.Forms.Padding(4);
            btnSync.Name = "btnSync";
            btnSync.Size = new System.Drawing.Size(88, 33);
            btnSync.TabIndex = 42;
            btnSync.Text = "Synchroize";
            btnSync.UseVisualStyleBackColor = true;
            btnSync.Click += btnSync_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnClose.Location = new System.Drawing.Point(770, 566);
            btnClose.Margin = new System.Windows.Forms.Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(88, 33);
            btnClose.TabIndex = 43;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnGenerate
            // 
            btnGenerate.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnGenerate.Location = new System.Drawing.Point(568, 566);
            btnGenerate.Margin = new System.Windows.Forms.Padding(4);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new System.Drawing.Size(98, 33);
            btnGenerate.TabIndex = 44;
            btnGenerate.Text = "Generate";
            toolTip1.SetToolTip(btnGenerate, "Generate Changed Scripts");
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // imageList2
            // 
            imageList2.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageList2.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList2.ImageStream");
            imageList2.TransparentColor = System.Drawing.Color.Transparent;
            imageList2.Images.SetKeyName(0, "Add.png");
            imageList2.Images.SetKeyName(1, "Remove.png");
            imageList2.Images.SetKeyName(2, "Edit.png");
            // 
            // txtMessage
            // 
            txtMessage.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            txtMessage.BackColor = System.Drawing.SystemColors.Control;
            txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtMessage.Location = new System.Drawing.Point(5, 574);
            txtMessage.Margin = new System.Windows.Forms.Padding(4);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new System.Drawing.Size(400, 16);
            txtMessage.TabIndex = 45;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiExpandAll, tsmiCollapseAll, tsmiGenerateChangedScripts, tsmiFindText });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(229, 114);
            // 
            // tsmiExpandAll
            // 
            tsmiExpandAll.Name = "tsmiExpandAll";
            tsmiExpandAll.Size = new System.Drawing.Size(228, 22);
            tsmiExpandAll.Text = "Expand All";
            tsmiExpandAll.Visible = false;
            tsmiExpandAll.Click += tsmiExpandAll_Click;
            // 
            // tsmiCollapseAll
            // 
            tsmiCollapseAll.Name = "tsmiCollapseAll";
            tsmiCollapseAll.Size = new System.Drawing.Size(228, 22);
            tsmiCollapseAll.Text = "Collapse All";
            tsmiCollapseAll.Visible = false;
            tsmiCollapseAll.Click += tsmiCollapseAll_Click;
            // 
            // tsmiGenerateChangedScripts
            // 
            tsmiGenerateChangedScripts.Name = "tsmiGenerateChangedScripts";
            tsmiGenerateChangedScripts.Size = new System.Drawing.Size(228, 22);
            tsmiGenerateChangedScripts.Text = "Generate Changed Scripts";
            tsmiGenerateChangedScripts.Visible = false;
            tsmiGenerateChangedScripts.Click += tsmiGenerateChangedScripts_Click;
            // 
            // tsmiFindText
            // 
            tsmiFindText.Name = "tsmiFindText";
            tsmiFindText.Size = new System.Drawing.Size(228, 22);
            tsmiFindText.Text = "Find Text";
            tsmiFindText.Click += tsmiFindText_Click;
            // 
            // targetDbProfile
            // 
            targetDbProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            targetDbProfile.DatabaseType = DatabaseType.Unknown;
            targetDbProfile.EnableDatabaseType = true;
            targetDbProfile.Location = new System.Drawing.Point(16, 36);
            targetDbProfile.Margin = new System.Windows.Forms.Padding(0);
            targetDbProfile.Name = "targetDbProfile";
            targetDbProfile.Size = new System.Drawing.Size(717, 26);
            targetDbProfile.TabIndex = 40;
            targetDbProfile.Title = "Target:";
            targetDbProfile.ProfileSelectedChanged += targetDbProfile_OnSelectedChanged;
            targetDbProfile.DatabaseTypeSelectedChanged += targetDbProfile_DatabaseTypeSelectedChanged;
            // 
            // sourceDbProfile
            // 
            sourceDbProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            sourceDbProfile.DatabaseType = DatabaseType.Unknown;
            sourceDbProfile.EnableDatabaseType = true;
            sourceDbProfile.Location = new System.Drawing.Point(16, 5);
            sourceDbProfile.Margin = new System.Windows.Forms.Padding(0);
            sourceDbProfile.Name = "sourceDbProfile";
            sourceDbProfile.Size = new System.Drawing.Size(717, 29);
            sourceDbProfile.TabIndex = 39;
            sourceDbProfile.Title = "Source:";
            sourceDbProfile.ProfileSelectedChanged += sourceDbProfile_OnSelectedChanged;
            sourceDbProfile.DatabaseTypeSelectedChanged += targetDbProfile_DatabaseTypeSelectedChanged;
            // 
            // frmSchemaCompare
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(864, 603);
            Controls.Add(txtMessage);
            Controls.Add(btnGenerate);
            Controls.Add(btnClose);
            Controls.Add(btnSync);
            Controls.Add(splitContainer1);
            Controls.Add(targetDbProfile);
            Controls.Add(sourceDbProfile);
            Controls.Add(btnCompare);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmSchemaCompare";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Compare";
            Load += frmCompare_Load;
            SizeChanged += frmCompare_SizeChanged;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)tlvDifferences).EndInit();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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
        private SqlCodeEditor.TextEditorControlEx txtSource;
        private SqlCodeEditor.TextEditorControlEx txtTarget;
    }
}