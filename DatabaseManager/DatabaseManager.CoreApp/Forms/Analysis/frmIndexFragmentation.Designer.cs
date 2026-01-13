namespace DatabaseManager.Forms
{
    partial class frmIndexFragmentation
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            loadingPanel = new DatabaseManager.Controls.UC_LoadingPanel();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiRefresh = new System.Windows.Forms.ToolStripMenuItem();
            tsmiSave = new System.Windows.Forms.ToolStripMenuItem();
            tsmiRebuildIndex = new System.Windows.Forms.ToolStripMenuItem();
            dgvData = new System.Windows.Forms.DataGridView();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            dlgSave = new System.Windows.Forms.SaveFileDialog();
            colTableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colIndexName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colFragmentationPercent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            SuspendLayout();
            // 
            // loadingPanel
            // 
            loadingPanel.BackColor = System.Drawing.SystemColors.Control;
            loadingPanel.BackgroundColor = System.Drawing.SystemColors.ControlDark;
            loadingPanel.CancellationTokenSource = null;
            loadingPanel.InterruptButtonVisible = true;
            loadingPanel.Location = new System.Drawing.Point(8, 27);
            loadingPanel.Name = "loadingPanel";
            loadingPanel.Size = new System.Drawing.Size(811, 415);
            loadingPanel.TabIndex = 20;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiRefresh, tsmiSave, tsmiRebuildIndex });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(157, 70);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // tsmiRefresh
            // 
            tsmiRefresh.Name = "tsmiRefresh";
            tsmiRefresh.Size = new System.Drawing.Size(156, 22);
            tsmiRefresh.Text = "Refresh";
            tsmiRefresh.Click += tsmiRefresh_Click;
            // 
            // tsmiSave
            // 
            tsmiSave.Name = "tsmiSave";
            tsmiSave.Size = new System.Drawing.Size(156, 22);
            tsmiSave.Text = "Save";
            tsmiSave.Click += tsmiSave_Click;
            // 
            // tsmiRebuildIndex
            // 
            tsmiRebuildIndex.Name = "tsmiRebuildIndex";
            tsmiRebuildIndex.Size = new System.Drawing.Size(156, 22);
            tsmiRebuildIndex.Text = "Rebuild Index";
            tsmiRebuildIndex.Click += tsmiRebuildIndex_Click;
            // 
            // dgvData
            // 
            dgvData.AllowUserToAddRows = false;
            dgvData.AllowUserToDeleteRows = false;
            dgvData.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colTableName, colIndexName, colFragmentationPercent });
            dgvData.ContextMenuStrip = contextMenuStrip1;
            dgvData.Location = new System.Drawing.Point(2, 1);
            dgvData.Margin = new System.Windows.Forms.Padding(4);
            dgvData.MultiSelect = false;
            dgvData.Name = "dgvData";
            dgvData.ReadOnly = true;
            dgvData.RowHeadersVisible = false;
            dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvData.Size = new System.Drawing.Size(827, 508);
            dgvData.TabIndex = 19;
            dgvData.CellFormatting += dgvData_CellFormatting;
            dgvData.DataBindingComplete += dgvData_DataBindingComplete;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(4, 515);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(40, 17);
            label2.TabIndex = 22;
            label2.Text = "Note:";
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label1.Location = new System.Drawing.Point(59, 515);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(615, 17);
            label1.TabIndex = 21;
            label1.Text = "For Postgres, the extension \"pgstattuple\" must be installed.";
            // 
            // dlgSave
            // 
            dlgSave.Filter = "excel file|*.xlsx";
            // 
            // colTableName
            // 
            colTableName.DataPropertyName = "TableName";
            colTableName.HeaderText = "Table Name";
            colTableName.Name = "colTableName";
            colTableName.ReadOnly = true;
            colTableName.Width = 250;
            // 
            // colIndexName
            // 
            colIndexName.DataPropertyName = "IndexName";
            colIndexName.HeaderText = "Index Name";
            colIndexName.Name = "colIndexName";
            colIndexName.ReadOnly = true;
            colIndexName.Width = 250;
            // 
            // colFragmentationPercent
            // 
            colFragmentationPercent.DataPropertyName = "FragmentationPercent";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            colFragmentationPercent.DefaultCellStyle = dataGridViewCellStyle2;
            colFragmentationPercent.HeaderText = "Fragmentation Percent(%)";
            colFragmentationPercent.Name = "colFragmentationPercent";
            colFragmentationPercent.ReadOnly = true;
            colFragmentationPercent.Width = 180;
            // 
            // frmIndexFragmentation
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(831, 539);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(loadingPanel);
            Controls.Add(dgvData);
            Name = "frmIndexFragmentation";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Index Fragmentation";
            Load += frmIndexFragmentation_Load;
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Controls.UC_LoadingPanel loadingPanel;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiRefresh;
        private System.Windows.Forms.ToolStripMenuItem tsmiSave;
        private System.Windows.Forms.SaveFileDialog dlgSave;
        private System.Windows.Forms.ToolStripMenuItem tsmiRebuildIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTableName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndexName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFragmentationPercent;
    }
}