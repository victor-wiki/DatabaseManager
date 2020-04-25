namespace DatabaseManager.Controls
{
    partial class UC_DbObjectContent
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.scriptContentMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiClose = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCloseOthers = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSaveScript = new System.Windows.Forms.ToolStripMenuItem();
            this.dlgSave = new System.Windows.Forms.SaveFileDialog();
            this.scriptContentMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(0, 0);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.ShowToolTips = true;
            this.tabControl1.Size = new System.Drawing.Size(496, 461);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tabControl1_DrawItem);
            this.tabControl1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tabControl1_MouseClick);
            this.tabControl1.MouseHover += new System.EventHandler(this.tabControl1_MouseHover);
            this.tabControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tabControl1_MouseUp);
            // 
            // scriptContentMenu
            // 
            this.scriptContentMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiClose,
            this.tsmiCloseOthers,
            this.tsmiCloseAll,
            this.tsmiSaveScript});
            this.scriptContentMenu.Name = "scriptContentMenu";
            this.scriptContentMenu.Size = new System.Drawing.Size(150, 92);
            // 
            // tsmiClose
            // 
            this.tsmiClose.Name = "tsmiClose";
            this.tsmiClose.Size = new System.Drawing.Size(149, 22);
            this.tsmiClose.Text = "Close";
            this.tsmiClose.Click += new System.EventHandler(this.tsmiClose_Click);
            // 
            // tsmiCloseOthers
            // 
            this.tsmiCloseOthers.Name = "tsmiCloseOthers";
            this.tsmiCloseOthers.Size = new System.Drawing.Size(149, 22);
            this.tsmiCloseOthers.Text = "Close others";
            this.tsmiCloseOthers.Click += new System.EventHandler(this.tsmiCloseOthers_Click);
            // 
            // tsmiCloseAll
            // 
            this.tsmiCloseAll.Name = "tsmiCloseAll";
            this.tsmiCloseAll.Size = new System.Drawing.Size(149, 22);
            this.tsmiCloseAll.Text = "Close all";
            this.tsmiCloseAll.Click += new System.EventHandler(this.tsmiCloseAll_Click);
            // 
            // tsmiSaveScript
            // 
            this.tsmiSaveScript.Name = "tsmiSaveScript";
            this.tsmiSaveScript.Size = new System.Drawing.Size(149, 22);
            this.tsmiSaveScript.Text = "Save";
            this.tsmiSaveScript.Click += new System.EventHandler(this.tsmiSave_Click);
            // 
            // dlgSave
            // 
            this.dlgSave.Filter = "sql file|*.sql|txt file|*.txt";
            // 
            // UC_DbObjectContent
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tabControl1);
            this.Name = "UC_DbObjectContent";
            this.Size = new System.Drawing.Size(496, 461);
            this.scriptContentMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ContextMenuStrip scriptContentMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiClose;
        private System.Windows.Forms.ToolStripMenuItem tsmiCloseOthers;
        private System.Windows.Forms.ToolStripMenuItem tsmiCloseAll;
        private System.Windows.Forms.ToolStripMenuItem tsmiSaveScript;
        private System.Windows.Forms.SaveFileDialog dlgSave;
    }
}
