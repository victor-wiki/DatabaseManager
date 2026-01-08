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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            columnSpliter = new System.Windows.Forms.SplitContainer();
            dgvColumns = new System.Windows.Forms.DataGridView();
            colColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDataType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            colLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPrimary = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colNullable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colDefaultValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colIdentity = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnPropertites = new FilteredPropertyGrid();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiInsertColumn = new System.Windows.Forms.ToolStripMenuItem();
            tsmiDeleteColumn = new System.Windows.Forms.ToolStripMenuItem();
            tsmiGenerateChangeScripts = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)columnSpliter).BeginInit();
            columnSpliter.Panel1.SuspendLayout();
            columnSpliter.Panel2.SuspendLayout();
            columnSpliter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvColumns).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // columnSpliter
            // 
            columnSpliter.Dock = System.Windows.Forms.DockStyle.Fill;
            columnSpliter.Location = new System.Drawing.Point(0, 0);
            columnSpliter.Margin = new System.Windows.Forms.Padding(4);
            columnSpliter.Name = "columnSpliter";
            columnSpliter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // columnSpliter.Panel1
            // 
            columnSpliter.Panel1.Controls.Add(dgvColumns);
            // 
            // columnSpliter.Panel2
            // 
            columnSpliter.Panel2.Controls.Add(columnPropertites);
            columnSpliter.Size = new System.Drawing.Size(1133, 608);
            columnSpliter.SplitterDistance = 442;
            columnSpliter.SplitterWidth = 6;
            columnSpliter.TabIndex = 1;
            // 
            // dgvColumns
            // 
            dgvColumns.AllowDrop = true;
            dgvColumns.BackgroundColor = System.Drawing.Color.White;
            dgvColumns.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvColumns.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colColumnName, colDataType, colLength, colPrimary, colNullable, colDefaultValue, colIdentity, colComment });
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgvColumns.DefaultCellStyle = dataGridViewCellStyle2;
            dgvColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvColumns.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            dgvColumns.Location = new System.Drawing.Point(0, 0);
            dgvColumns.Margin = new System.Windows.Forms.Padding(4);
            dgvColumns.MultiSelect = false;
            dgvColumns.Name = "dgvColumns";
            dgvColumns.RowHeadersWidth = 25;
            dgvColumns.Size = new System.Drawing.Size(1133, 442);
            dgvColumns.TabIndex = 7;
            dgvColumns.CellContentClick += dgvColumns_CellContentClick;
            dgvColumns.CellEnter += dgvColumns_CellEnter;
            dgvColumns.CellLeave += dgvColumns_CellLeave;
            dgvColumns.CellValueChanged += dgvColumns_CellValueChanged;
            dgvColumns.DataBindingComplete += dgvColumns_DataBindingComplete;
            dgvColumns.DataError += dgvColumns_DataError;
            dgvColumns.DefaultValuesNeeded += dgvColumns_DefaultValuesNeeded;
            dgvColumns.EditingControlShowing += dgvColumns_EditingControlShowing;
            dgvColumns.RowHeaderMouseClick += dgvColumns_RowHeaderMouseClick;
            dgvColumns.UserAddedRow += dgvColumns_UserAddedRow;
            dgvColumns.SizeChanged += UC_TableColumns_SizeChanged;
            dgvColumns.DragDrop += dgvColumns_DragDrop;
            dgvColumns.DragOver += dgvColumns_DragOver;
            dgvColumns.KeyDown += dgvColumns_KeyDown;
            dgvColumns.MouseDown += dgvColumns_MouseDown;
            dgvColumns.MouseMove += dgvColumns_MouseMove;
            dgvColumns.MouseUp += dgvColumns_MouseUp;
            // 
            // colColumnName
            // 
            colColumnName.DataPropertyName = "Name";
            colColumnName.HeaderText = "Name";
            colColumnName.Name = "colColumnName";
            colColumnName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colColumnName.Width = 200;
            // 
            // colDataType
            // 
            colDataType.DataPropertyName = "DataType";
            colDataType.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            colDataType.HeaderText = "Data Type";
            colDataType.Name = "colDataType";
            colDataType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colDataType.Width = 180;
            // 
            // colLength
            // 
            colLength.DataPropertyName = "Length";
            colLength.HeaderText = "Length";
            colLength.Name = "colLength";
            colLength.Width = 60;
            // 
            // colPrimary
            // 
            colPrimary.DataPropertyName = "IsPrimary";
            colPrimary.HeaderText = "Primary";
            colPrimary.Name = "colPrimary";
            colPrimary.Width = 60;
            // 
            // colNullable
            // 
            colNullable.DataPropertyName = "IsNullable";
            colNullable.HeaderText = "Nullable";
            colNullable.Name = "colNullable";
            colNullable.Width = 60;
            // 
            // colDefaultValue
            // 
            colDefaultValue.DataPropertyName = "DefaultValue";
            colDefaultValue.HeaderText = "Default Value";
            colDefaultValue.Name = "colDefaultValue";
            colDefaultValue.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colDefaultValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colDefaultValue.Width = 150;
            // 
            // colIdentity
            // 
            colIdentity.DataPropertyName = "IsIdentity";
            colIdentity.HeaderText = "Identity";
            colIdentity.Name = "colIdentity";
            colIdentity.Width = 60;
            // 
            // colComment
            // 
            colComment.DataPropertyName = "Comment";
            colComment.HeaderText = "Comment";
            colComment.Name = "colComment";
            colComment.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colComment.Width = 200;
            // 
            // columnPropertites
            // 
            columnPropertites.BrowsableProperties = null;
            columnPropertites.Dock = System.Windows.Forms.DockStyle.Fill;
            columnPropertites.HelpVisible = false;
            columnPropertites.HiddenAttributes = null;
            columnPropertites.HiddenProperties = null;
            columnPropertites.Location = new System.Drawing.Point(0, 0);
            columnPropertites.Margin = new System.Windows.Forms.Padding(4);
            columnPropertites.Name = "columnPropertites";
            columnPropertites.Size = new System.Drawing.Size(1133, 160);
            columnPropertites.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiInsertColumn, tsmiDeleteColumn, tsmiGenerateChangeScripts });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(221, 70);
            // 
            // tsmiInsertColumn
            // 
            tsmiInsertColumn.Name = "tsmiInsertColumn";
            tsmiInsertColumn.Size = new System.Drawing.Size(220, 22);
            tsmiInsertColumn.Text = "Insert Column";
            tsmiInsertColumn.Click += tsmiInsertColumn_Click;
            // 
            // tsmiDeleteColumn
            // 
            tsmiDeleteColumn.Name = "tsmiDeleteColumn";
            tsmiDeleteColumn.Size = new System.Drawing.Size(220, 22);
            tsmiDeleteColumn.Text = "Delete Column";
            tsmiDeleteColumn.Click += tsmiDeleteColumn_Click;
            // 
            // tsmiGenerateChangeScripts
            // 
            tsmiGenerateChangeScripts.Name = "tsmiGenerateChangeScripts";
            tsmiGenerateChangeScripts.Size = new System.Drawing.Size(220, 22);
            tsmiGenerateChangeScripts.Text = "Generate Change Scripts";
            tsmiGenerateChangeScripts.Click += tsmiGenerateChangeScripts_Click;
            // 
            // UC_TableColumns
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            Controls.Add(columnSpliter);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "UC_TableColumns";
            Size = new System.Drawing.Size(1133, 608);
            Load += UC_TableColumns_Load;
            columnSpliter.Panel1.ResumeLayout(false);
            columnSpliter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)columnSpliter).EndInit();
            columnSpliter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvColumns).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);

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
