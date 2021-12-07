namespace DatabaseManager.Controls
{
    partial class UC_QueryEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_QueryEditor));
            this.txtEditor = new System.Windows.Forms.RichTextBox();
            this.editorContexMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDisableIntellisense = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUpdateIntellisense = new System.Windows.Forms.ToolStripMenuItem();
            this.lvWords = new System.Windows.Forms.ListView();
            this.Icon = new System.Windows.Forms.ColumnHeader();
            this.Word = new System.Windows.Forms.ColumnHeader();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.txtToolTip = new System.Windows.Forms.TextBox();
            this.panelWords = new System.Windows.Forms.Panel();
            this.editorContexMenu.SuspendLayout();
            this.panelWords.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtEditor
            // 
            this.txtEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEditor.EnableAutoDragDrop = true;
            this.txtEditor.Font = new System.Drawing.Font("宋体", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtEditor.HideSelection = false;
            this.txtEditor.Location = new System.Drawing.Point(0, 0);
            this.txtEditor.Margin = new System.Windows.Forms.Padding(4);
            this.txtEditor.Name = "txtEditor";
            this.txtEditor.Size = new System.Drawing.Size(384, 482);
            this.txtEditor.TabIndex = 0;
            this.txtEditor.Text = "";
            this.txtEditor.SelectionChanged += new System.EventHandler(this.txtEditor_SelectionChanged);
            this.txtEditor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtEditor_MouseClick);
            this.txtEditor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtEditor_KeyDown);
            this.txtEditor.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtEditor_KeyUp);
            this.txtEditor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtEditor_MouseDown);
            this.txtEditor.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtEditor_MouseUp);
            // 
            // editorContexMenu
            // 
            this.editorContexMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCopy,
            this.tsmiPaste,
            this.tsmiDisableIntellisense,
            this.tsmiUpdateIntellisense});
            this.editorContexMenu.Name = "contextMenuStrip1";
            this.editorContexMenu.Size = new System.Drawing.Size(188, 92);
            // 
            // tsmiCopy
            // 
            this.tsmiCopy.Name = "tsmiCopy";
            this.tsmiCopy.Size = new System.Drawing.Size(187, 22);
            this.tsmiCopy.Text = "Copy";
            this.tsmiCopy.Click += new System.EventHandler(this.tsmiCopy_Click);
            // 
            // tsmiPaste
            // 
            this.tsmiPaste.Name = "tsmiPaste";
            this.tsmiPaste.Size = new System.Drawing.Size(187, 22);
            this.tsmiPaste.Text = "Paste";
            this.tsmiPaste.Click += new System.EventHandler(this.tsmiPaste_Click);
            // 
            // tsmiDisableIntellisense
            // 
            this.tsmiDisableIntellisense.Name = "tsmiDisableIntellisense";
            this.tsmiDisableIntellisense.Size = new System.Drawing.Size(187, 22);
            this.tsmiDisableIntellisense.Text = "Disable Intellisense";
            this.tsmiDisableIntellisense.Click += new System.EventHandler(this.tsmiDisableIntellisense_Click);
            // 
            // tsmiUpdateIntellisense
            // 
            this.tsmiUpdateIntellisense.Name = "tsmiUpdateIntellisense";
            this.tsmiUpdateIntellisense.Size = new System.Drawing.Size(187, 22);
            this.tsmiUpdateIntellisense.Text = "Update Intellisense";
            this.tsmiUpdateIntellisense.Click += new System.EventHandler(this.tsmiUpdateIntellisense_Click);
            // 
            // lvWords
            // 
            this.lvWords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvWords.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Icon,
            this.Word});
            this.lvWords.FullRowSelect = true;
            this.lvWords.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvWords.HideSelection = false;
            this.lvWords.Location = new System.Drawing.Point(0, 0);
            this.lvWords.Margin = new System.Windows.Forms.Padding(4);
            this.lvWords.MultiSelect = false;
            this.lvWords.Name = "lvWords";
            this.lvWords.ShowItemToolTips = true;
            this.lvWords.Size = new System.Drawing.Size(338, 167);
            this.lvWords.SmallImageList = this.imageList1;
            this.lvWords.TabIndex = 2;
            this.lvWords.UseCompatibleStateImageBehavior = false;
            this.lvWords.View = System.Windows.Forms.View.Details;
            this.lvWords.SelectedIndexChanged += new System.EventHandler(this.lvWords_SelectedIndexChanged);
            this.lvWords.DoubleClick += new System.EventHandler(this.lvWords_DoubleClick);
            this.lvWords.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvWords_KeyDown);
            // 
            // Icon
            // 
            this.Icon.Text = "Icon";
            this.Icon.Width = 20;
            // 
            // Word
            // 
            this.Word.Text = "Word";
            this.Word.Width = 200;
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Keyword.png");
            this.imageList1.Images.SetKeyName(1, "tree_Function.png");
            this.imageList1.Images.SetKeyName(2, "tree_Table.png");
            this.imageList1.Images.SetKeyName(3, "tree_View.png");
            this.imageList1.Images.SetKeyName(4, "tree_TableColumn.png");
            this.imageList1.Images.SetKeyName(5, "Owner.png");
            // 
            // txtToolTip
            // 
            this.txtToolTip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(225)))));
            this.txtToolTip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtToolTip.Location = new System.Drawing.Point(172, 411);
            this.txtToolTip.Margin = new System.Windows.Forms.Padding(4);
            this.txtToolTip.Name = "txtToolTip";
            this.txtToolTip.ReadOnly = true;
            this.txtToolTip.Size = new System.Drawing.Size(186, 23);
            this.txtToolTip.TabIndex = 6;
            this.txtToolTip.Visible = false;
            // 
            // panelWords
            // 
            this.panelWords.AutoScroll = true;
            this.panelWords.Controls.Add(this.lvWords);
            this.panelWords.Location = new System.Drawing.Point(20, 309);
            this.panelWords.Margin = new System.Windows.Forms.Padding(4);
            this.panelWords.Name = "panelWords";
            this.panelWords.Size = new System.Drawing.Size(338, 169);
            this.panelWords.TabIndex = 7;
            this.panelWords.Visible = false;
            // 
            // UC_QueryEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelWords);
            this.Controls.Add(this.txtToolTip);
            this.Controls.Add(this.txtEditor);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UC_QueryEditor";
            this.Size = new System.Drawing.Size(384, 482);
            this.editorContexMenu.ResumeLayout(false);
            this.panelWords.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtEditor;
        private System.Windows.Forms.ContextMenuStrip editorContexMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.ToolStripMenuItem tsmiPaste;
        private System.Windows.Forms.ListView lvWords;
        private System.Windows.Forms.ColumnHeader Icon;
        private System.Windows.Forms.ColumnHeader Word;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem tsmiDisableIntellisense;
        private System.Windows.Forms.TextBox txtToolTip;
        private System.Windows.Forms.Panel panelWords;
        private System.Windows.Forms.ToolStripMenuItem tsmiUpdateIntellisense;
    }
}
