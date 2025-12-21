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
            components = new System.ComponentModel.Container();
            tvDbObjects = new System.Windows.Forms.TreeView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiShowSortedNames = new System.Windows.Forms.ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tvDbObjects
            // 
            tvDbObjects.CheckBoxes = true;
            tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            tvDbObjects.Location = new System.Drawing.Point(0, 0);
            tvDbObjects.Margin = new System.Windows.Forms.Padding(4);
            tvDbObjects.Name = "tvDbObjects";
            tvDbObjects.Size = new System.Drawing.Size(290, 449);
            tvDbObjects.TabIndex = 20;      
            tvDbObjects.AfterCheck += tvDbObjects_AfterCheck;
            tvDbObjects.AfterSelect += tvDbObjects_AfterSelect;
            tvDbObjects.NodeMouseClick += tvDbObjects_NodeMouseClick;
            tvDbObjects.KeyDown += tvDbObjects_KeyDown;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiShowSortedNames });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(196, 26);
            // 
            // tsmiShowSortedNames
            // 
            tsmiShowSortedNames.Name = "tsmiShowSortedNames";
            tsmiShowSortedNames.Size = new System.Drawing.Size(195, 22);
            tsmiShowSortedNames.Text = "Show Sorted Names";
            tsmiShowSortedNames.Click += tsmiShowSortedNames_Click;
            // 
            // UC_DbObjectsSimpleTree
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tvDbObjects);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "UC_DbObjectsSimpleTree";
            Size = new System.Drawing.Size(290, 449);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TreeView tvDbObjects;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowSortedNames;
    }
}
