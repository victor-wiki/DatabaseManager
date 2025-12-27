namespace DatabaseManager.Forms
{
    partial class frmImportData
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            btnOpenFile = new System.Windows.Forms.Button();
            txtFilePath = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            chkFirstRowIsColumnName = new System.Windows.Forms.CheckBox();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            chkUseColumnMapping = new System.Windows.Forms.CheckBox();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            btnCancel = new System.Windows.Forms.Button();
            btnOK = new System.Windows.Forms.Button();
            gbColumnMappings = new System.Windows.Forms.GroupBox();
            dgvColumns = new System.Windows.Forms.DataGridView();
            txtMessage = new System.Windows.Forms.TextBox();
            colTableColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colFileColumnName = new System.Windows.Forms.DataGridViewComboBoxColumn();
            colReferencedTable = new System.Windows.Forms.DataGridViewComboBoxColumn();
            colReferencedTableColumn = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            gbColumnMappings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvColumns).BeginInit();
            SuspendLayout();
            // 
            // btnOpenFile
            // 
            btnOpenFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnOpenFile.Location = new System.Drawing.Point(777, 8);
            btnOpenFile.Name = "btnOpenFile";
            btnOpenFile.Size = new System.Drawing.Size(40, 23);
            btnOpenFile.TabIndex = 14;
            btnOpenFile.Text = "...";
            btnOpenFile.UseVisualStyleBackColor = true;
            btnOpenFile.Click += btnOpenFile_Click;
            // 
            // txtFilePath
            // 
            txtFilePath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtFilePath.Location = new System.Drawing.Point(85, 8);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new System.Drawing.Size(686, 23);
            txtFilePath.TabIndex = 13;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(14, 11);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(59, 17);
            label2.TabIndex = 12;
            label2.Text = "File Path:";
            // 
            // chkFirstRowIsColumnName
            // 
            chkFirstRowIsColumnName.AutoSize = true;
            chkFirstRowIsColumnName.Location = new System.Drawing.Point(17, 42);
            chkFirstRowIsColumnName.Name = "chkFirstRowIsColumnName";
            chkFirstRowIsColumnName.Size = new System.Drawing.Size(172, 21);
            chkFirstRowIsColumnName.TabIndex = 11;
            chkFirstRowIsColumnName.Text = "First row is column name";
            chkFirstRowIsColumnName.UseVisualStyleBackColor = true;
            chkFirstRowIsColumnName.CheckedChanged += chkFirstRowIsColumnName_CheckedChanged;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            openFileDialog1.Filter = "csv file|*.csv|excel file|*.xlsx|excel file|*.xls";
            // 
            // chkUseColumnMapping
            // 
            chkUseColumnMapping.AutoSize = true;
            chkUseColumnMapping.Enabled = false;
            chkUseColumnMapping.Location = new System.Drawing.Point(249, 42);
            chkUseColumnMapping.Name = "chkUseColumnMapping";
            chkUseColumnMapping.Size = new System.Drawing.Size(179, 21);
            chkUseColumnMapping.TabIndex = 20;
            chkUseColumnMapping.Text = "Custom column mappings";
            chkUseColumnMapping.UseVisualStyleBackColor = true;
            chkUseColumnMapping.CheckedChanged += chkUseColumMapping_CheckedChanged;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(btnCancel);
            splitContainer1.Panel1.Controls.Add(btnOK);
            splitContainer1.Panel1.Controls.Add(gbColumnMappings);
            splitContainer1.Panel1.Controls.Add(txtFilePath);
            splitContainer1.Panel1.Controls.Add(chkUseColumnMapping);
            splitContainer1.Panel1.Controls.Add(chkFirstRowIsColumnName);
            splitContainer1.Panel1.Controls.Add(label2);
            splitContainer1.Panel1.Controls.Add(btnOpenFile);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(txtMessage);
            splitContainer1.Size = new System.Drawing.Size(829, 584);
            splitContainer1.SplitterDistance = 494;
            splitContainer1.TabIndex = 21;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnCancel.Location = new System.Drawing.Point(433, 458);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 23;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnOK.Location = new System.Drawing.Point(322, 458);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 22;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // gbColumnMappings
            // 
            gbColumnMappings.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbColumnMappings.Controls.Add(dgvColumns);
            gbColumnMappings.Location = new System.Drawing.Point(14, 79);
            gbColumnMappings.Name = "gbColumnMappings";
            gbColumnMappings.Size = new System.Drawing.Size(812, 373);
            gbColumnMappings.TabIndex = 21;
            gbColumnMappings.TabStop = false;
            gbColumnMappings.Text = "Column Mappings";
            // 
            // dgvColumns
            // 
            dgvColumns.AllowDrop = true;
            dgvColumns.AllowUserToAddRows = false;
            dgvColumns.AllowUserToDeleteRows = false;
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
            dgvColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colTableColumnName, colFileColumnName, colReferencedTable, colReferencedTableColumn });
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
            dgvColumns.Enabled = false;
            dgvColumns.Location = new System.Drawing.Point(3, 19);
            dgvColumns.Margin = new System.Windows.Forms.Padding(4);
            dgvColumns.MultiSelect = false;
            dgvColumns.Name = "dgvColumns";
            dgvColumns.RowHeadersVisible = false;
            dgvColumns.RowHeadersWidth = 25;
            dgvColumns.Size = new System.Drawing.Size(806, 351);
            dgvColumns.TabIndex = 17;
            dgvColumns.CellContentClick += dgvColumns_CellContentClick;
            dgvColumns.CellLeave += dgvColumns_CellLeave;
            dgvColumns.EditingControlShowing += dgvColumns_EditingControlShowing;
            // 
            // txtMessage
            // 
            txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            txtMessage.Location = new System.Drawing.Point(0, 0);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtMessage.Size = new System.Drawing.Size(829, 86);
            txtMessage.TabIndex = 20;
            // 
            // colTableColumnName
            // 
            colTableColumnName.DataPropertyName = "Name";
            colTableColumnName.HeaderText = "Table Column Name";
            colTableColumnName.Name = "colTableColumnName";
            colTableColumnName.ReadOnly = true;
            colTableColumnName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colTableColumnName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colTableColumnName.Width = 180;
            // 
            // colFileColumnName
            // 
            colFileColumnName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            colFileColumnName.HeaderText = "File Column Name";
            colFileColumnName.Name = "colFileColumnName";
            colFileColumnName.Width = 180;
            // 
            // colReferencedTable
            // 
            colReferencedTable.DataPropertyName = "Name";
            colReferencedTable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            colReferencedTable.HeaderText = "Referenced Table";
            colReferencedTable.Name = "colReferencedTable";
            colReferencedTable.Width = 250;
            // 
            // colReferencedTableColumn
            // 
            colReferencedTableColumn.HeaderText = "Referenced Table Column";
            colReferencedTableColumn.Name = "colReferencedTableColumn";
            colReferencedTableColumn.Width = 180;
            // 
            // frmImportData
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(829, 584);
            Controls.Add(splitContainer1);
            Name = "frmImportData";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Import Data";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            gbColumnMappings.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvColumns).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkFirstRowIsColumnName;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.CheckBox chkUseColumnMapping;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox gbColumnMappings;
        private System.Windows.Forms.DataGridView dgvColumns;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTableColumnName;
        private System.Windows.Forms.DataGridViewComboBoxColumn colFileColumnName;
        private System.Windows.Forms.DataGridViewComboBoxColumn colReferencedTable;
        private System.Windows.Forms.DataGridViewButtonColumn colReferencedTableColumn;
    }
}