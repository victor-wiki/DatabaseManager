namespace DatabaseManager
{
    partial class frmTableDependency
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
            this.btnLocate = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.tvDbObjects = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiExpandAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiExpandChildren = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiClearStyles = new System.Windows.Forms.ToolStripMenuItem();
            this.chkShowNotReferenced = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLocate
            // 
            this.btnLocate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLocate.Location = new System.Drawing.Point(388, 9);
            this.btnLocate.Name = "btnLocate";
            this.btnLocate.Size = new System.Drawing.Size(66, 25);
            this.btnLocate.TabIndex = 0;
            this.btnLocate.Text = "Locate";
            this.btnLocate.UseVisualStyleBackColor = true;
            this.btnLocate.Click += new System.EventHandler(this.btnLocate_Click);
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(8, 10);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(374, 23);
            this.txtName.TabIndex = 1;
            this.txtName.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtName_KeyUp);
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvDbObjects.ContextMenuStrip = this.contextMenuStrip1;
            this.tvDbObjects.Location = new System.Drawing.Point(8, 40);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(446, 390);
            this.tvDbObjects.TabIndex = 2;
            this.tvDbObjects.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvDbObjects_NodeMouseClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiExpandAll,
            this.tsmiExpandChildren,
            this.tsmiCollapseAll,
            this.tsmiClearStyles});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(172, 92);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // tsmiExpandAll
            // 
            this.tsmiExpandAll.Name = "tsmiExpandAll";
            this.tsmiExpandAll.Size = new System.Drawing.Size(171, 22);
            this.tsmiExpandAll.Text = "Expand All";
            this.tsmiExpandAll.Click += new System.EventHandler(this.tsmiExpandAll_Click);
            // 
            // tsmiExpandChildren
            // 
            this.tsmiExpandChildren.Name = "tsmiExpandChildren";
            this.tsmiExpandChildren.Size = new System.Drawing.Size(171, 22);
            this.tsmiExpandChildren.Text = "Expand Children";
            this.tsmiExpandChildren.Click += new System.EventHandler(this.tsmiExpandChildren_Click);
            // 
            // tsmiCollapseAll
            // 
            this.tsmiCollapseAll.Name = "tsmiCollapseAll";
            this.tsmiCollapseAll.Size = new System.Drawing.Size(171, 22);
            this.tsmiCollapseAll.Text = "Collapse All";
            this.tsmiCollapseAll.Click += new System.EventHandler(this.tsmiCollapseAll_Click);
            // 
            // tsmiClearStyles
            // 
            this.tsmiClearStyles.Name = "tsmiClearStyles";
            this.tsmiClearStyles.Size = new System.Drawing.Size(171, 22);
            this.tsmiClearStyles.Text = "Clear Styles";
            this.tsmiClearStyles.Click += new System.EventHandler(this.tsmiClearStyles_Click);
            // 
            // chkShowNotReferenced
            // 
            this.chkShowNotReferenced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShowNotReferenced.AutoSize = true;
            this.chkShowNotReferenced.Location = new System.Drawing.Point(10, 434);
            this.chkShowNotReferenced.Name = "chkShowNotReferenced";
            this.chkShowNotReferenced.Size = new System.Drawing.Size(187, 21);
            this.chkShowNotReferenced.TabIndex = 3;
            this.chkShowNotReferenced.Text = "Show not referenced tables";
            this.chkShowNotReferenced.UseVisualStyleBackColor = true;
            this.chkShowNotReferenced.CheckedChanged += new System.EventHandler(this.chkShowNotReferenced_CheckedChanged);
            // 
            // frmTableDependency
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 455);
            this.Controls.Add(this.chkShowNotReferenced);
            this.Controls.Add(this.tvDbObjects);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.btnLocate);
            this.Name = "frmTableDependency";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Table Dependencies";
            this.Load += new System.EventHandler(this.frmDependency_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLocate;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TreeView tvDbObjects;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiExpandAll;
        private System.Windows.Forms.ToolStripMenuItem tsmiCollapseAll;
        private System.Windows.Forms.CheckBox chkShowNotReferenced;
        private System.Windows.Forms.ToolStripMenuItem tsmiExpandChildren;
        private System.Windows.Forms.ToolStripMenuItem tsmiClearStyles;
    }
}