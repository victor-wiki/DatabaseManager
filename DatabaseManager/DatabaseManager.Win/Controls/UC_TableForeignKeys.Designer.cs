namespace DatabaseManager.Controls
{
    partial class UC_TableForeignKeys
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvForeignKeys = new System.Windows.Forms.DataGridView();
            this.colReferenceTable = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colKeyName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colColumns = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colUpdateCascade = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDeleteCascade = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiDeleteForeignKey = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateChangeScripts = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvForeignKeys)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvForeignKeys
            // 
            this.dgvForeignKeys.AllowDrop = true;
            this.dgvForeignKeys.BackgroundColor = System.Drawing.Color.White;
            this.dgvForeignKeys.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvForeignKeys.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgvForeignKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvForeignKeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colReferenceTable,
            this.colKeyName,
            this.colColumns,
            this.colUpdateCascade,
            this.colDeleteCascade,
            this.colComment});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvForeignKeys.DefaultCellStyle = dataGridViewCellStyle6;
            this.dgvForeignKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvForeignKeys.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvForeignKeys.Location = new System.Drawing.Point(0, 0);
            this.dgvForeignKeys.MultiSelect = false;
            this.dgvForeignKeys.Name = "dgvForeignKeys";
            this.dgvForeignKeys.RowHeadersWidth = 25;
            this.dgvForeignKeys.RowTemplate.Height = 23;
            this.dgvForeignKeys.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvForeignKeys.Size = new System.Drawing.Size(825, 261);
            this.dgvForeignKeys.TabIndex = 9;
            this.dgvForeignKeys.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvForeignKeys_CellContentClick);
            this.dgvForeignKeys.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvForeignKeys_CellValueChanged);
            this.dgvForeignKeys.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvForeignKeys_DataError);
            this.dgvForeignKeys.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvForeignKeys_RowHeaderMouseClick);
            this.dgvForeignKeys.SizeChanged += new System.EventHandler(this.dgvForeignKeys_SizeChanged);
            this.dgvForeignKeys.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvForeignKeys_KeyDown);
            this.dgvForeignKeys.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgvForeignKeys_MouseUp);
            // 
            // colReferenceTable
            // 
            this.colReferenceTable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colReferenceTable.HeaderText = "Reference Table";
            this.colReferenceTable.Name = "colReferenceTable";
            this.colReferenceTable.Width = 150;
            // 
            // colKeyName
            // 
            this.colKeyName.DataPropertyName = "Name";
            this.colKeyName.HeaderText = "Name";
            this.colKeyName.Name = "colKeyName";
            this.colKeyName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colKeyName.Width = 150;
            // 
            // colColumns
            // 
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.colColumns.DefaultCellStyle = dataGridViewCellStyle5;
            this.colColumns.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colColumns.HeaderText = "Column Mapping";
            this.colColumns.Name = "colColumns";
            this.colColumns.ReadOnly = true;
            this.colColumns.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colColumns.Width = 200;
            // 
            // colUpdateCascade
            // 
            this.colUpdateCascade.HeaderText = "Update Cascade";
            this.colUpdateCascade.Name = "colUpdateCascade";
            // 
            // colDeleteCascade
            // 
            this.colDeleteCascade.HeaderText = "Delete Cascade";
            this.colDeleteCascade.Name = "colDeleteCascade";
            // 
            // colComment
            // 
            this.colComment.DataPropertyName = "Comment";
            this.colComment.HeaderText = "Comment";
            this.colComment.Name = "colComment";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiDeleteForeignKey,
            this.tsmiGenerateChangeScripts});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(229, 70);
            // 
            // tsmiDeleteForeignKey
            // 
            this.tsmiDeleteForeignKey.Name = "tsmiDeleteForeignKey";
            this.tsmiDeleteForeignKey.Size = new System.Drawing.Size(220, 22);
            this.tsmiDeleteForeignKey.Text = "Delete Foreign Key";
            this.tsmiDeleteForeignKey.Click += new System.EventHandler(this.tsmiDeleteForeignKey_Click);
            // 
            // tsmiGenerateChangeScripts
            // 
            this.tsmiGenerateChangeScripts.Name = "tsmiGenerateChangeScripts";
            this.tsmiGenerateChangeScripts.Size = new System.Drawing.Size(228, 22);
            this.tsmiGenerateChangeScripts.Text = "Generate Changed Scripts";
            this.tsmiGenerateChangeScripts.Click += new System.EventHandler(this.tsmiGenerateChangeScripts_Click);
            // 
            // UC_TableForeignKeys
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvForeignKeys);
            this.Name = "UC_TableForeignKeys";
            this.Size = new System.Drawing.Size(825, 261);
            this.Load += new System.EventHandler(this.UC_TableForeignKeys_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvForeignKeys)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvForeignKeys;  
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiDeleteForeignKey;
        private System.Windows.Forms.DataGridViewComboBoxColumn colReferenceTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKeyName;
        private System.Windows.Forms.DataGridViewButtonColumn colColumns;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colUpdateCascade;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colDeleteCascade;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
        private System.Windows.Forms.ToolStripMenuItem tsmiGenerateChangeScripts;
    }
}
