namespace DatabaseManager.Controls
{
    partial class UC_ProfilingResultGrid
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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            dgvData = new System.Windows.Forms.DataGridView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiCopyContent = new System.Windows.Forms.ToolStripMenuItem();
            tsmiShowContent = new System.Windows.Forms.ToolStripMenuItem();
            colExecuteType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colSQL = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDuration = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // dgvData
            // 
            dgvData.AllowUserToAddRows = false;
            dgvData.AllowUserToDeleteRows = false;
            dgvData.BackgroundColor = System.Drawing.Color.White;
            dgvData.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colExecuteType, colSQL, colDuration });
            dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvData.Location = new System.Drawing.Point(0, 0);
            dgvData.Margin = new System.Windows.Forms.Padding(4);
            dgvData.Name = "dgvData";
            dgvData.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvData.RowHeadersVisible = false;
            dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgvData.Size = new System.Drawing.Size(647, 310);
            dgvData.TabIndex = 7;
            dgvData.CellFormatting += dgvData_CellFormatting;
            dgvData.DataBindingComplete += dgvData_DataBindingComplete;
            dgvData.MouseUp += dgvData_MouseUp;
            dgvData.Resize += dgvData_Resize;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiCopyContent, tsmiShowContent });
            contextMenuStrip1.Name = "contextMenuStrip2";
            contextMenuStrip1.Size = new System.Drawing.Size(155, 48);
            // 
            // tsmiCopyContent
            // 
            tsmiCopyContent.Name = "tsmiCopyContent";
            tsmiCopyContent.Size = new System.Drawing.Size(154, 22);
            tsmiCopyContent.Text = "Copy content";
            tsmiCopyContent.Click += tsmiCopyContent_Click;
            // 
            // tsmiShowContent
            // 
            tsmiShowContent.Name = "tsmiShowContent";
            tsmiShowContent.Size = new System.Drawing.Size(154, 22);
            tsmiShowContent.Text = "Show content";
            tsmiShowContent.Click += tsmiShowContent_Click;
            // 
            // colExecuteType
            // 
            colExecuteType.DataPropertyName = "ExecuteType";
            colExecuteType.HeaderText = "ExecuteType";
            colExecuteType.Name = "colExecuteType";
            colExecuteType.ReadOnly = true;
            colExecuteType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colSQL
            // 
            colSQL.DataPropertyName = "Sql";
            colSQL.HeaderText = "SQL";
            colSQL.Name = "colSQL";
            colSQL.ReadOnly = true;
            colSQL.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colSQL.Width = 400;
            // 
            // colDuration
            // 
            colDuration.DataPropertyName = "Duration";
            colDuration.HeaderText = "Duration(ms)";
            colDuration.Name = "colDuration";
            colDuration.ReadOnly = true;
            colDuration.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colDuration.Width = 120;
            // 
            // UC_ProfilingResultGrid
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(dgvData);
            Name = "UC_ProfilingResultGrid";
            Size = new System.Drawing.Size(647, 310);
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyContent;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowContent;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExecuteType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSQL;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDuration;
    }
}
