namespace DatabaseManager.Controls
{
    partial class UC_TableForeignKeys
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvForeignKeys = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiDeleteForeignKey = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateChangeScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.colReferenceTable = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colKeyName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colColumns = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colUpdateCascade = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDeleteCascade = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvForeignKeys)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvForeignKeys
            // 
            this.dgvForeignKeys.AllowDrop = true;
            this.dgvForeignKeys.BackgroundColor = System.Drawing.Color.White;
            this.dgvForeignKeys.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvForeignKeys.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvForeignKeys.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvForeignKeys.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colReferenceTable,
            this.colKeyName,
            this.colColumns,
            this.colUpdateCascade,
            this.colDeleteCascade,
            this.colComment});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvForeignKeys.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvForeignKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvForeignKeys.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvForeignKeys.Location = new System.Drawing.Point(0, 0);
            this.dgvForeignKeys.Margin = new System.Windows.Forms.Padding(4);
            this.dgvForeignKeys.MultiSelect = false;
            this.dgvForeignKeys.Name = "dgvForeignKeys";
            this.dgvForeignKeys.RowHeadersWidth = 25;
            this.dgvForeignKeys.Size = new System.Drawing.Size(962, 370);
            this.dgvForeignKeys.TabIndex = 9;
            this.dgvForeignKeys.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvForeignKeys_CellContentClick);
            this.dgvForeignKeys.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvForeignKeys_CellValueChanged);
            this.dgvForeignKeys.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvForeignKeys_DataError);
            this.dgvForeignKeys.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvForeignKeys_RowHeaderMouseClick);
            this.dgvForeignKeys.SizeChanged += new System.EventHandler(this.dgvForeignKeys_SizeChanged);
            this.dgvForeignKeys.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvForeignKeys_KeyDown);
            this.dgvForeignKeys.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgvForeignKeys_MouseUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiDeleteForeignKey,
            this.tsmiGenerateChangeScripts});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(229, 48);
            // 
            // tsmiDeleteForeignKey
            // 
            this.tsmiDeleteForeignKey.Name = "tsmiDeleteForeignKey";
            this.tsmiDeleteForeignKey.Size = new System.Drawing.Size(228, 22);
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
            // colReferenceTable
            // 
            this.colReferenceTable.DataPropertyName = "Table.ReferencedTableName";
            this.colReferenceTable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colReferenceTable.HeaderText = "Reference Table";
            this.colReferenceTable.Name = "colReferenceTable";
            this.colReferenceTable.Width = 200;
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
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.colColumns.DefaultCellStyle = dataGridViewCellStyle2;
            this.colColumns.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colColumns.HeaderText = "Column Mapping";
            this.colColumns.Name = "colColumns";
            this.colColumns.ReadOnly = true;
            this.colColumns.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colColumns.Width = 200;
            // 
            // colUpdateCascade
            // 
            this.colUpdateCascade.DataPropertyName = "UpdateCascade";
            this.colUpdateCascade.HeaderText = "Update Cascade";
            this.colUpdateCascade.Name = "colUpdateCascade";
            this.colUpdateCascade.Width = 110;
            // 
            // colDeleteCascade
            // 
            this.colDeleteCascade.DataPropertyName = "DeleteCascade";
            this.colDeleteCascade.HeaderText = "Delete Cascade";
            this.colDeleteCascade.Name = "colDeleteCascade";
            // 
            // colComment
            // 
            this.colComment.DataPropertyName = "Comment";
            this.colComment.HeaderText = "Comment";
            this.colComment.Name = "colComment";
            // 
            // UC_TableForeignKeys
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvForeignKeys);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UC_TableForeignKeys";
            this.Size = new System.Drawing.Size(962, 370);
            this.Load += new System.EventHandler(this.UC_TableForeignKeys_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvForeignKeys)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvForeignKeys;  
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiDeleteForeignKey;
        private System.Windows.Forms.ToolStripMenuItem tsmiGenerateChangeScripts;
        private System.Windows.Forms.DataGridViewComboBoxColumn colReferenceTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKeyName;
        private System.Windows.Forms.DataGridViewButtonColumn colColumns;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colUpdateCascade;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colDeleteCascade;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
    }
}
