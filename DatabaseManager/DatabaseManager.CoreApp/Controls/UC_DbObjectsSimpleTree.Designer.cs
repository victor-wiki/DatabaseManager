namespace DatabaseManager.Controls
{
    partial class UC_DbObjectsSimpleTree
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
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
            this.tvDbObjects.Margin = new System.Windows.Forms.Padding(4);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(290, 449);
            this.tvDbObjects.TabIndex = 20;
            this.tvDbObjects.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvDbObjects_AfterCheck);
            this.tvDbObjects.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvDbObjects_NodeMouseClick);
            this.tvDbObjects.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvDbObjects_KeyDown);
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
            this.Margin = new System.Windows.Forms.Padding(4);
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
