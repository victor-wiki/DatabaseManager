namespace DatabaseManager.Forms
{
    partial class frmSchemaPreviewer
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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            splitter1 = new System.Windows.Forms.Splitter();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tvDbObjects = new Controls.UC_DbObjectsSimpleTree();
            dgvColumn = new System.Windows.Forms.DataGridView();
            cellContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiSetCellValueToNull = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            colColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colTargetDataType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPrecision = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colScale = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDefaultValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colCurrentColumnContentMaxLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvColumn).BeginInit();
            cellContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // splitter1
            // 
            splitter1.Location = new System.Drawing.Point(0, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(3, 513);
            splitter1.TabIndex = 0;
            splitter1.TabStop = false;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(3, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tvDbObjects);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(dgvColumn);
            splitContainer1.Size = new System.Drawing.Size(1331, 513);
            splitContainer1.SplitterDistance = 280;
            splitContainer1.TabIndex = 1;
            // 
            // tvDbObjects
            // 
            tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            tvDbObjects.Location = new System.Drawing.Point(0, 0);
            tvDbObjects.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tvDbObjects.Name = "tvDbObjects";
            tvDbObjects.ShowCheckBox = true;
            tvDbObjects.Size = new System.Drawing.Size(280, 513);
            tvDbObjects.TabIndex = 39;
            // 
            // dgvColumn
            // 
            dgvColumn.AllowUserToAddRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvColumn.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvColumn.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvColumn.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colColumnName, colTargetDataType, colLength, colPrecision, colScale, colDefaultValue, colCurrentColumnContentMaxLength });
            dgvColumn.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvColumn.Location = new System.Drawing.Point(0, 0);
            dgvColumn.Margin = new System.Windows.Forms.Padding(4);
            dgvColumn.Name = "dgvColumn";
            dgvColumn.RowHeadersVisible = false;
            dgvColumn.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgvColumn.Size = new System.Drawing.Size(1047, 513);
            dgvColumn.TabIndex = 56;
            dgvColumn.CellMouseClick += dgvColumn_CellMouseClick;
            dgvColumn.CellValidating += dgvColumn_CellValidating;
            dgvColumn.ColumnHeaderMouseClick += dgvColumn_ColumnHeaderMouseClick;
            dgvColumn.DataError += dgvColumn_DataError;
            dgvColumn.KeyUp += dgvColumn_KeyUp;
            // 
            // cellContextMenu
            // 
            cellContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiSetCellValueToNull, tsmiCopy });
            cellContextMenu.Name = "cellContextMenu";
            cellContextMenu.Size = new System.Drawing.Size(156, 48);
            // 
            // tsmiSetCellValueToNull
            // 
            tsmiSetCellValueToNull.Name = "tsmiSetCellValueToNull";
            tsmiSetCellValueToNull.Size = new System.Drawing.Size(155, 22);
            tsmiSetCellValueToNull.Text = "Set NULL";
            tsmiSetCellValueToNull.Click += tsmiSetCellValueToNull_Click;
            // 
            // tsmiCopy
            // 
            tsmiCopy.Name = "tsmiCopy";
            tsmiCopy.Size = new System.Drawing.Size(155, 22);
            tsmiCopy.Text = "Copy Content";
            tsmiCopy.Click += tsmiCopy_Click;
            // 
            // colColumnName
            // 
            colColumnName.HeaderText = "Column Name";
            colColumnName.Name = "colColumnName";
            colColumnName.ReadOnly = true;
            colColumnName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colColumnName.Width = 180;
            // 
            // colTargetDataType
            // 
            colTargetDataType.HeaderText = "Data Type";
            colTargetDataType.Name = "colTargetDataType";
            colTargetDataType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colTargetDataType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colTargetDataType.Width = 150;
            // 
            // colLength
            // 
            colLength.HeaderText = "Length";
            colLength.Name = "colLength";
            colLength.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colLength.Width = 120;
            // 
            // colPrecision
            // 
            colPrecision.HeaderText = "Precision";
            colPrecision.Name = "colPrecision";
            colPrecision.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colPrecision.Width = 120;
            // 
            // colScale
            // 
            colScale.HeaderText = "Scale";
            colScale.Name = "colScale";
            colScale.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colScale.Width = 120;
            // 
            // colDefaultValue
            // 
            colDefaultValue.HeaderText = "Default Value";
            colDefaultValue.Name = "colDefaultValue";
            colDefaultValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colDefaultValue.Width = 150;
            // 
            // colCurrentColumnContentMaxLength
            // 
            colCurrentColumnContentMaxLength.HeaderText = "Current Content Max Length";
            colCurrentColumnContentMaxLength.Name = "colCurrentColumnContentMaxLength";
            colCurrentColumnContentMaxLength.ReadOnly = true;
            colCurrentColumnContentMaxLength.Width = 200;
            // 
            // frmSchemaPreviewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1334, 513);
            Controls.Add(splitContainer1);
            Controls.Add(splitter1);
            Name = "frmSchemaPreviewer";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Schema Previewer";
            FormClosing += frmSchemaPreviewer_FormClosing;
            Load += frmSchemaPreviewer_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvColumn).EndInit();
            cellContextMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private Controls.UC_DbObjectsSimpleTree tvDbObjects;
        private System.Windows.Forms.DataGridView dgvColumn;
        private System.Windows.Forms.ContextMenuStrip cellContextMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiSetCellValueToNull;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.DataGridViewTextBoxColumn colColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetDataType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrecision;
        private System.Windows.Forms.DataGridViewTextBoxColumn colScale;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefaultValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCurrentColumnContentMaxLength;
    }
}