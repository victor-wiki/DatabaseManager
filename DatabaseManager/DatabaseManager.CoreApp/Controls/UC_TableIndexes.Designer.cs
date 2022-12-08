namespace DatabaseManager.Controls
{
    partial class UC_TableIndexes
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
            this.dgvIndexes = new System.Windows.Forms.DataGridView();
            this.colType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colIndexName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colColumns = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiDeleteIndex = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateChangeScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.indexPropertites = new DatabaseManager.Controls.FilteredPropertyGrid();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndexes)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvIndexes
            // 
            this.dgvIndexes.AllowDrop = true;
            this.dgvIndexes.BackgroundColor = System.Drawing.Color.White;
            this.dgvIndexes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvIndexes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvIndexes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIndexes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colType,
            this.colIndexName,
            this.colColumns,
            this.colComment});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvIndexes.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvIndexes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvIndexes.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvIndexes.Location = new System.Drawing.Point(0, 0);
            this.dgvIndexes.Margin = new System.Windows.Forms.Padding(4);
            this.dgvIndexes.MultiSelect = false;
            this.dgvIndexes.Name = "dgvIndexes";
            this.dgvIndexes.RowHeadersWidth = 25;
            this.dgvIndexes.Size = new System.Drawing.Size(884, 396);
            this.dgvIndexes.TabIndex = 8;
            this.dgvIndexes.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvIndexes_CellContentClick);
            this.dgvIndexes.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvIndexes_CellLeave);
            this.dgvIndexes.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvIndexes_CellValueChanged);
            this.dgvIndexes.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvIndexes_DataError);
            this.dgvIndexes.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvIndexes_RowHeaderMouseClick);
            this.dgvIndexes.SelectionChanged += new System.EventHandler(this.dgvIndexes_SelectionChanged);
            this.dgvIndexes.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dgvIndexes_UserAddedRow);
            this.dgvIndexes.SizeChanged += new System.EventHandler(this.dgvIndexes_SizeChanged);
            this.dgvIndexes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvIndexes_KeyDown);
            this.dgvIndexes.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgvIndexes_MouseUp);
            // 
            // colType
            // 
            this.colType.DataPropertyName = "Type";
            this.colType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colType.HeaderText = "Type";
            this.colType.Name = "colType";
            this.colType.Width = 180;
            // 
            // colIndexName
            // 
            this.colIndexName.DataPropertyName = "Name";
            this.colIndexName.HeaderText = "Name";
            this.colIndexName.Name = "colIndexName";
            this.colIndexName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colIndexName.Width = 200;
            // 
            // colColumns
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.colColumns.DefaultCellStyle = dataGridViewCellStyle2;
            this.colColumns.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colColumns.HeaderText = "Columns";
            this.colColumns.Name = "colColumns";
            this.colColumns.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colColumns.Width = 200;
            // 
            // colComment
            // 
            this.colComment.DataPropertyName = "Comment";
            this.colComment.HeaderText = "Comment";
            this.colComment.Name = "colComment";
            this.colComment.Width = 150;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiDeleteIndex,
            this.tsmiGenerateChangeScripts});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(229, 48);
            // 
            // tsmiDeleteIndex
            // 
            this.tsmiDeleteIndex.Name = "tsmiDeleteIndex";
            this.tsmiDeleteIndex.Size = new System.Drawing.Size(228, 22);
            this.tsmiDeleteIndex.Text = "Delete Index";
            this.tsmiDeleteIndex.Click += new System.EventHandler(this.tsmiDeleteIndex_Click);
            // 
            // tsmiGenerateChangeScripts
            // 
            this.tsmiGenerateChangeScripts.Name = "tsmiGenerateChangeScripts";
            this.tsmiGenerateChangeScripts.Size = new System.Drawing.Size(228, 22);
            this.tsmiGenerateChangeScripts.Text = "Generate Changed Scripts";
            this.tsmiGenerateChangeScripts.Click += new System.EventHandler(this.tsmiGenerateChangeScripts_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgvIndexes);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.indexPropertites);
            this.splitContainer1.Size = new System.Drawing.Size(884, 469);
            this.splitContainer1.SplitterDistance = 396;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 11;
            // 
            // indexPropertites
            // 
            this.indexPropertites.BrowsableProperties = null;
            this.indexPropertites.Dock = System.Windows.Forms.DockStyle.Fill;
            this.indexPropertites.HelpVisible = false;
            this.indexPropertites.HiddenAttributes = null;
            this.indexPropertites.HiddenProperties = null;
            this.indexPropertites.Location = new System.Drawing.Point(0, 0);
            this.indexPropertites.Margin = new System.Windows.Forms.Padding(4);
            this.indexPropertites.Name = "indexPropertites";
            this.indexPropertites.Size = new System.Drawing.Size(884, 67);
            this.indexPropertites.TabIndex = 1;
            // 
            // UC_TableIndexes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UC_TableIndexes";
            this.Size = new System.Drawing.Size(884, 469);
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndexes)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvIndexes;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiDeleteIndex;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private FilteredPropertyGrid indexPropertites;
        private System.Windows.Forms.ToolStripMenuItem tsmiGenerateChangeScripts;
        private System.Windows.Forms.DataGridViewComboBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndexName;
        private System.Windows.Forms.DataGridViewButtonColumn colColumns;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
    }
}
