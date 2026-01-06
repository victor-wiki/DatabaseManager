using System.Drawing;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    partial class frmJsonViewer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer1 = new SplitContainer();
            tvViwer = new TreeView();
            txtJson = new TextBox();
            contextMenuStrip2 = new ContextMenuStrip(components);
            tsmiLoadJsonTree = new ToolStripMenuItem();
            tsmiFormat = new ToolStripMenuItem();
            contextMenuStrip1 = new ContextMenuStrip(components);
            tsmiExpandAll = new ToolStripMenuItem();
            tsmiCollapseAll = new ToolStripMenuItem();
            tsmiExpandChildren = new ToolStripMenuItem();
            tsmiCollapseToChildren = new ToolStripMenuItem();
            tsmiFind = new ToolStripMenuItem();
            tsmiFindChild = new ToolStripMenuItem();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            tsmiOpenFile = new ToolStripMenuItem();
            tsmiExit = new ToolStripMenuItem();
            openFileDialog1 = new OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            contextMenuStrip2.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(0, 28);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tvViwer);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(txtJson);
            splitContainer1.Size = new Size(800, 422);
            splitContainer1.SplitterDistance = 400;
            splitContainer1.TabIndex = 0;
            // 
            // tvViwer
            // 
            tvViwer.Dock = DockStyle.Fill;
            tvViwer.DrawMode = TreeViewDrawMode.OwnerDrawText;
            tvViwer.FullRowSelect = true;
            tvViwer.HideSelection = false;
            tvViwer.Location = new Point(0, 0);
            tvViwer.Name = "tvViwer";
            tvViwer.Size = new Size(400, 422);
            tvViwer.TabIndex = 0;
            tvViwer.AfterExpand += tvViwer_AfterExpand;
            tvViwer.DrawNode += tvViwer_DrawNode;
            tvViwer.NodeMouseClick += tvViwer_NodeMouseClick;
            tvViwer.KeyDown += tvViwer_KeyDown;
            // 
            // txtJson
            // 
            txtJson.ContextMenuStrip = contextMenuStrip2;
            txtJson.Dock = DockStyle.Fill;
            txtJson.Location = new Point(0, 0);
            txtJson.Multiline = true;
            txtJson.Name = "txtJson";
            txtJson.PlaceholderText = "Input or paste JSON text";
            txtJson.ScrollBars = ScrollBars.Vertical;
            txtJson.Size = new Size(396, 422);
            txtJson.TabIndex = 0;
            txtJson.TextChanged += txtJson_TextChanged;
            txtJson.KeyDown += txtJson_KeyDown;
            // 
            // contextMenuStrip2
            // 
            contextMenuStrip2.Items.AddRange(new ToolStripItem[] { tsmiLoadJsonTree, tsmiFormat });
            contextMenuStrip2.Name = "contextMenuStrip2";
            contextMenuStrip2.Size = new Size(161, 48);
            // 
            // tsmiLoadJsonTree
            // 
            tsmiLoadJsonTree.Name = "tsmiLoadJsonTree";
            tsmiLoadJsonTree.Size = new Size(160, 22);
            tsmiLoadJsonTree.Text = "Load json tree";
            tsmiLoadJsonTree.Click += tsmiLoadJsonTree_Click;
            // 
            // tsmiFormat
            // 
            tsmiFormat.Name = "tsmiFormat";
            tsmiFormat.Size = new Size(160, 22);
            tsmiFormat.Text = "Format json";
            tsmiFormat.Click += tsmiFormat_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { tsmiExpandAll, tsmiCollapseAll, tsmiExpandChildren, tsmiCollapseToChildren, tsmiFind, tsmiFindChild });
            contextMenuStrip1.Name = "tsmiExpandAll";
            contextMenuStrip1.Size = new Size(177, 136);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // tsmiExpandAll
            // 
            tsmiExpandAll.Name = "tsmiExpandAll";
            tsmiExpandAll.Size = new Size(176, 22);
            tsmiExpandAll.Text = "Expand all";
            tsmiExpandAll.Click += tsmiExpandAll_Click;
            // 
            // tsmiCollapseAll
            // 
            tsmiCollapseAll.Name = "tsmiCollapseAll";
            tsmiCollapseAll.Size = new Size(176, 22);
            tsmiCollapseAll.Text = "Collapse all";
            tsmiCollapseAll.Click += tsmiCollapseAll_Click;
            // 
            // tsmiExpandChildren
            // 
            tsmiExpandChildren.Name = "tsmiExpandChildren";
            tsmiExpandChildren.Size = new Size(176, 22);
            tsmiExpandChildren.Text = "Expand children";
            tsmiExpandChildren.Click += tsmiExpandChildren_Click;
            // 
            // tsmiCollapseToChildren
            // 
            tsmiCollapseToChildren.Name = "tsmiCollapseToChildren";
            tsmiCollapseToChildren.Size = new Size(176, 22);
            tsmiCollapseToChildren.Text = "Collapse children";
            tsmiCollapseToChildren.Click += tsmiCollapseToChildren_Click;
            // 
            // tsmiFind
            // 
            tsmiFind.Name = "tsmiFind";
            tsmiFind.Size = new Size(176, 22);
            tsmiFind.Text = "Find";
            tsmiFind.Click += tsmiFind_Click;
            // 
            // tsmiFindChild
            // 
            tsmiFindChild.Name = "tsmiFindChild";
            tsmiFindChild.Size = new Size(176, 22);
            tsmiFindChild.Text = "Find child";
            tsmiFindChild.Click += tsmiFindChild_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 25);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmiOpenFile, tsmiExit });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(39, 21);
            fileToolStripMenuItem.Text = "File";
            // 
            // tsmiOpenFile
            // 
            tsmiOpenFile.Name = "tsmiOpenFile";
            tsmiOpenFile.ShortcutKeys = Keys.Control | Keys.O;
            tsmiOpenFile.Size = new Size(155, 22);
            tsmiOpenFile.Text = "Open";
            tsmiOpenFile.Click += tsmiOpenFile_Click;
            // 
            // tsmiExit
            // 
            tsmiExit.Name = "tsmiExit";
            tsmiExit.Size = new Size(155, 22);
            tsmiExit.Text = "Exit";
            tsmiExit.Click += tsmiExit_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog1.Title = "Select a JSON file";
            // 
            // frmJsonViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(menuStrip1);
            Controls.Add(splitContainer1);
            MainMenuStrip = menuStrip1;
            Name = "frmJsonViewer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "JSON Viewer";
            Load += frmJsonViewer_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            contextMenuStrip2.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer1;
        private TreeView tvViwer;
        private TextBox txtJson;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem tsmiExpandAll;
        private ToolStripMenuItem tsmiCollapseAll;
        private ToolStripMenuItem tsmiExpandChildren;
        private ToolStripMenuItem tsmiCollapseToChildren;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem tsmiOpenFile;
        private OpenFileDialog openFileDialog1;
        private ToolStripMenuItem tsmiExit;
        private ContextMenuStrip contextMenuStrip2;
        private ToolStripMenuItem tsmiLoadJsonTree;
        private ToolStripMenuItem tsmiFormat;
        private ToolStripMenuItem tsmiFind;
        private ToolStripMenuItem tsmiFindChild;
    }
}
