namespace DatabaseManager.Controls
{
    partial class UC_DbObjectsSimpleTree
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tvDbObjects = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiShowSortedNames = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.CheckBoxes = true;
            this.tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDbObjects.Location = new System.Drawing.Point(0, 0);
            this.tvDbObjects.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(290, 449);
            this.tvDbObjects.TabIndex = 20;
            this.tvDbObjects.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvDbObjects_AfterCheck);
            this.tvDbObjects.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvDbObjects_NodeMouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiShowSortedNames});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(196, 26);
            // 
            // tsmiShowSortedNames
            // 
            this.tsmiShowSortedNames.Name = "tsmiShowSortedNames";
            this.tsmiShowSortedNames.Size = new System.Drawing.Size(195, 22);
            this.tsmiShowSortedNames.Text = "Show Sorted Names";
            this.tsmiShowSortedNames.Click += new System.EventHandler(this.tsmiShowSortedNames_Click);
            // 
            // UC_DbObjectsSimpleTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvDbObjects);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "UC_DbObjectsSimpleTree";
            this.Size = new System.Drawing.Size(290, 449);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvDbObjects;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowSortedNames;
    }
}
