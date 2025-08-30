namespace DatabaseManager.Forms
{
    partial class frmTableDiagnoseResult
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTableDiagnoseResult));
            dgvResult = new System.Windows.Forms.DataGridView();
            colTableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colObjectType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colInvalidRecordCount = new System.Windows.Forms.DataGridViewLinkColumn();
            btnClose = new System.Windows.Forms.Button();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCopyWithHeader = new System.Windows.Forms.ToolStripMenuItem();
            tsmiSave = new System.Windows.Forms.ToolStripMenuItem();
            dlgSave = new System.Windows.Forms.SaveFileDialog();
            btnSave = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvResult).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // dgvResult
            // 
            dgvResult.AllowUserToAddRows = false;
            dgvResult.AllowUserToDeleteRows = false;
            dgvResult.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvResult.BackgroundColor = System.Drawing.Color.White;
            dgvResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colTableName, colObjectType, colObjectName, colInvalidRecordCount });
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgvResult.DefaultCellStyle = dataGridViewCellStyle3;
            dgvResult.Location = new System.Drawing.Point(0, 0);
            dgvResult.Margin = new System.Windows.Forms.Padding(4);
            dgvResult.Name = "dgvResult";
            dgvResult.ReadOnly = true;
            dgvResult.RowHeadersVisible = false;
            dgvResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvResult.Size = new System.Drawing.Size(694, 322);
            dgvResult.TabIndex = 1;
            dgvResult.CellContentClick += dgvResult_CellContentClick;
            dgvResult.DataBindingComplete += dgvResult_DataBindingComplete;
            dgvResult.RowsAdded += dgvResult_RowsAdded;
            dgvResult.MouseUp += dgvResult_MouseUp;
            // 
            // colTableName
            // 
            colTableName.DataPropertyName = "TableName";
            colTableName.HeaderText = "Table Name";
            colTableName.Name = "colTableName";
            colTableName.ReadOnly = true;
            colTableName.Width = 180;
            // 
            // colObjectType
            // 
            colObjectType.HeaderText = "Object Type";
            colObjectType.Name = "colObjectType";
            colObjectType.ReadOnly = true;
            colObjectType.Width = 150;
            // 
            // colObjectName
            // 
            colObjectName.HeaderText = "Object Name";
            colObjectName.Name = "colObjectName";
            colObjectName.ReadOnly = true;
            colObjectName.Width = 200;
            // 
            // colInvalidRecordCount
            // 
            colInvalidRecordCount.ActiveLinkColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            colInvalidRecordCount.DefaultCellStyle = dataGridViewCellStyle2;
            colInvalidRecordCount.HeaderText = "Invalid Record Count";
            colInvalidRecordCount.Name = "colInvalidRecordCount";
            colInvalidRecordCount.ReadOnly = true;
            colInvalidRecordCount.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colInvalidRecordCount.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colInvalidRecordCount.Width = 150;
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnClose.Location = new System.Drawing.Point(353, 334);
            btnClose.Margin = new System.Windows.Forms.Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(88, 33);
            btnClose.TabIndex = 9;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiCopy, tsmiCopyWithHeader, tsmiSave });
            contextMenuStrip1.Name = "contextMenuStrip2";
            contextMenuStrip1.Size = new System.Drawing.Size(179, 70);
            // 
            // tsmiCopy
            // 
            tsmiCopy.Name = "tsmiCopy";
            tsmiCopy.Size = new System.Drawing.Size(178, 22);
            tsmiCopy.Text = "Copy";
            tsmiCopy.Click += tsmiCopy_Click;
            // 
            // tsmiCopyWithHeader
            // 
            tsmiCopyWithHeader.Name = "tsmiCopyWithHeader";
            tsmiCopyWithHeader.Size = new System.Drawing.Size(178, 22);
            tsmiCopyWithHeader.Text = "Copy with header";
            tsmiCopyWithHeader.Click += tsmiCopyWithHeader_Click;
            // 
            // tsmiSave
            // 
            tsmiSave.Name = "tsmiSave";
            tsmiSave.Size = new System.Drawing.Size(178, 22);
            tsmiSave.Text = "Save";
            tsmiSave.Click += tsmiSave_Click;
            // 
            // dlgSave
            // 
            dlgSave.Filter = "excel file|*.xlsx";
            // 
            // btnSave
            // 
            btnSave.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnSave.Location = new System.Drawing.Point(227, 334);
            btnSave.Margin = new System.Windows.Forms.Padding(4);
            btnSave.Name = "btnSave";
            btnSave.Size = new System.Drawing.Size(88, 33);
            btnSave.TabIndex = 10;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // frmTableDiagnoseResult
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(697, 376);
            Controls.Add(btnSave);
            Controls.Add(btnClose);
            Controls.Add(dgvResult);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmTableDiagnoseResult";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Table Diagnose Result";
            Load += frmTableDiagnoseResult_Load;
            ((System.ComponentModel.ISupportInitialize)dgvResult).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DataGridView dgvResult;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyWithHeader;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTableName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectName;
        private System.Windows.Forms.DataGridViewLinkColumn colInvalidRecordCount;
        private System.Windows.Forms.ToolStripMenuItem tsmiSave;
        private System.Windows.Forms.SaveFileDialog dlgSave;
        private System.Windows.Forms.Button btnSave;
    }
}