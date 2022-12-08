namespace DatabaseManager.Controls
{
    partial class UC_TableColumns
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.columnSpliter = new System.Windows.Forms.SplitContainer();
            this.dgvColumns = new System.Windows.Forms.DataGridView();
            this.colColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDataType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPrimary = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colNullable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDefaultValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIdentity = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnPropertites = new DatabaseManager.Controls.FilteredPropertyGrid();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiInsertColumn = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDeleteColumn = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateChangeScripts = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.columnSpliter)).BeginInit();
            this.columnSpliter.Panel1.SuspendLayout();
            this.columnSpliter.Panel2.SuspendLayout();
            this.columnSpliter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnSpliter
            // 
            this.columnSpliter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.columnSpliter.Location = new System.Drawing.Point(0, 0);
            this.columnSpliter.Margin = new System.Windows.Forms.Padding(4);
            this.columnSpliter.Name = "columnSpliter";
            this.columnSpliter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // columnSpliter.Panel1
            // 
            this.columnSpliter.Panel1.Controls.Add(this.dgvColumns);
            // 
            // columnSpliter.Panel2
            // 
            this.columnSpliter.Panel2.Controls.Add(this.columnPropertites);
            this.columnSpliter.Size = new System.Drawing.Size(1133, 608);
            this.columnSpliter.SplitterDistance = 442;
            this.columnSpliter.SplitterWidth = 6;
            this.columnSpliter.TabIndex = 1;
            // 
            // dgvColumns
            // 
            this.dgvColumns.AllowDrop = true;
            this.dgvColumns.BackgroundColor = System.Drawing.Color.White;
            this.dgvColumns.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvColumns.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colColumnName,
            this.colDataType,
            this.colLength,
            this.colPrimary,
            this.colNullable,
            this.colDefaultValue,
            this.colIdentity,
            this.colComment});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvColumns.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvColumns.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvColumns.Location = new System.Drawing.Point(0, 0);
            this.dgvColumns.Margin = new System.Windows.Forms.Padding(4);
            this.dgvColumns.MultiSelect = false;
            this.dgvColumns.Name = "dgvColumns";
            this.dgvColumns.RowHeadersWidth = 25;
            this.dgvColumns.RowTemplate.Height = 25;
            this.dgvColumns.Size = new System.Drawing.Size(1133, 442);
            this.dgvColumns.TabIndex = 7;
            this.dgvColumns.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvColumns_CellContentClick);
            this.dgvColumns.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvColumns_CellEnter);
            this.dgvColumns.CellLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvColumns_CellLeave);
            this.dgvColumns.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvColumns_CellValueChanged);
            this.dgvColumns.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgvColumns_DataBindingComplete);
            this.dgvColumns.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvColumns_DataError);
            this.dgvColumns.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.dgvColumns_DefaultValuesNeeded);
            this.dgvColumns.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgvColumns_EditingControlShowing);
            this.dgvColumns.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvColumns_RowHeaderMouseClick);
            this.dgvColumns.SelectionChanged += new System.EventHandler(this.dgvColumns_SelectionChanged);
            this.dgvColumns.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dgvColumns_UserAddedRow);
            this.dgvColumns.SizeChanged += new System.EventHandler(this.UC_TableColumns_SizeChanged);
            this.dgvColumns.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgvColumns_DragDrop);
            this.dgvColumns.DragOver += new System.Windows.Forms.DragEventHandler(this.dgvColumns_DragOver);
            this.dgvColumns.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvColumns_KeyDown);
            this.dgvColumns.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgvColumns_MouseDown);
            this.dgvColumns.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgvColumns_MouseMove);
            this.dgvColumns.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgvColumns_MouseUp);
            // 
            // colColumnName
            // 
            this.colColumnName.DataPropertyName = "Name";
            this.colColumnName.HeaderText = "Name";
            this.colColumnName.Name = "colColumnName";
            this.colColumnName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colColumnName.Width = 200;
            // 
            // colDataType
            // 
            this.colDataType.DataPropertyName = "DataType";
            this.colDataType.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colDataType.HeaderText = "Data Type";
            this.colDataType.Name = "colDataType";
            this.colDataType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colDataType.Width = 180;
            // 
            // colLength
            // 
            this.colLength.DataPropertyName = "Length";
            this.colLength.HeaderText = "Length";
            this.colLength.Name = "colLength";
            this.colLength.Width = 60;
            // 
            // colPrimary
            // 
            this.colPrimary.DataPropertyName = "IsPrimary";
            this.colPrimary.HeaderText = "Primary";
            this.colPrimary.Name = "colPrimary";
            this.colPrimary.Width = 60;
            // 
            // colNullable
            // 
            this.colNullable.DataPropertyName = "IsNullable";
            this.colNullable.HeaderText = "Nullable";
            this.colNullable.Name = "colNullable";
            this.colNullable.Width = 60;
            // 
            // colDefaultValue
            // 
            this.colDefaultValue.DataPropertyName = "DefaultValue";
            this.colDefaultValue.HeaderText = "Default Value";
            this.colDefaultValue.Name = "colDefaultValue";
            this.colDefaultValue.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colDefaultValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colDefaultValue.Width = 150;
            // 
            // colIdentity
            // 
            this.colIdentity.DataPropertyName = "IsIdentity";
            this.colIdentity.HeaderText = "Identity";
            this.colIdentity.Name = "colIdentity";
            this.colIdentity.Width = 60;
            // 
            // colComment
            // 
            this.colComment.DataPropertyName = "Comment";
            this.colComment.HeaderText = "Comment";
            this.colComment.Name = "colComment";
            this.colComment.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colComment.Width = 200;
            // 
            // columnPropertites
            // 
            this.columnPropertites.BrowsableProperties = null;
            this.columnPropertites.Dock = System.Windows.Forms.DockStyle.Fill;
            this.columnPropertites.HelpVisible = false;
            this.columnPropertites.HiddenAttributes = null;
            this.columnPropertites.HiddenProperties = null;
            this.columnPropertites.Location = new System.Drawing.Point(0, 0);
            this.columnPropertites.Margin = new System.Windows.Forms.Padding(4);
            this.columnPropertites.Name = "columnPropertites";
            this.columnPropertites.Size = new System.Drawing.Size(1133, 160);
            this.columnPropertites.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiInsertColumn,
            this.tsmiDeleteColumn,
            this.tsmiGenerateChangeScripts});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(221, 70);
            // 
            // tsmiInsertColumn
            // 
            this.tsmiInsertColumn.Name = "tsmiInsertColumn";
            this.tsmiInsertColumn.Size = new System.Drawing.Size(220, 22);
            this.tsmiInsertColumn.Text = "Insert Column";
            this.tsmiInsertColumn.Click += new System.EventHandler(this.tsmiInsertColumn_Click);
            // 
            // tsmiDeleteColumn
            // 
            this.tsmiDeleteColumn.Name = "tsmiDeleteColumn";
            this.tsmiDeleteColumn.Size = new System.Drawing.Size(220, 22);
            this.tsmiDeleteColumn.Text = "Delete Column";
            this.tsmiDeleteColumn.Click += new System.EventHandler(this.tsmiDeleteColumn_Click);
            // 
            // tsmiGenerateChangeScripts
            // 
            this.tsmiGenerateChangeScripts.Name = "tsmiGenerateChangeScripts";
            this.tsmiGenerateChangeScripts.Size = new System.Drawing.Size(220, 22);
            this.tsmiGenerateChangeScripts.Text = "Generate Change Scripts";
            this.tsmiGenerateChangeScripts.Click += new System.EventHandler(this.tsmiGenerateChangeScripts_Click);
            // 
            // UC_TableColumns
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.columnSpliter);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UC_TableColumns";
            this.Size = new System.Drawing.Size(1133, 608);
            this.Load += new System.EventHandler(this.UC_TableColumns_Load);
            this.columnSpliter.Panel1.ResumeLayout(false);
            this.columnSpliter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.columnSpliter)).EndInit();
            this.columnSpliter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvColumns)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer columnSpliter;
        private System.Windows.Forms.DataGridView dgvColumns;
        private FilteredPropertyGrid columnPropertites;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiInsertColumn;
        private System.Windows.Forms.ToolStripMenuItem tsmiDeleteColumn;
        private System.Windows.Forms.ToolStripMenuItem tsmiGenerateChangeScripts;
        private System.Windows.Forms.DataGridViewTextBoxColumn colColumnName;
        private System.Windows.Forms.DataGridViewComboBoxColumn colDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLength;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colPrimary;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colNullable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefaultValue;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIdentity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
    }
}
