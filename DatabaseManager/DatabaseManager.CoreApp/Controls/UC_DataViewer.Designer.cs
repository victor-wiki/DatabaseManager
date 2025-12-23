namespace DatabaseManager.Controls
{
    partial class UC_DataViewer
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
            dgvData = new System.Windows.Forms.DataGridView();
            btnFilter = new System.Windows.Forms.Button();
            pagination = new UC_Pagination();
            cellContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            tsmiShowContent = new System.Windows.Forms.ToolStripMenuItem();
            tsmiViewGeometry = new System.Windows.Forms.ToolStripMenuItem();
            uc_QuickFilter = new UC_QuickFilter();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            loadingPanel = new UC_LoadingPanel();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            cellContextMenu.SuspendLayout();
            SuspendLayout();
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
            dgvData.Location = new System.Drawing.Point(0, 29);
            dgvData.Margin = new System.Windows.Forms.Padding(4);
            dgvData.Name = "dgvData";
            dgvData.ReadOnly = true;
            dgvData.RowHeadersVisible = false;
            dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgvData.Size = new System.Drawing.Size(818, 408);
            dgvData.TabIndex = 5;
            dgvData.CellFormatting += dgvData_CellFormatting;
            dgvData.CellMouseClick += dgvData_CellMouseClick;
            dgvData.DataBindingComplete += dgvData_DataBindingComplete;
            dgvData.DataError += dgvData_DataError;
            dgvData.Sorted += dgvData_Sorted;
            dgvData.SizeChanged += dgvData_SizeChanged;
            // 
            // btnFilter
            // 
            btnFilter.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFilter.Location = new System.Drawing.Point(4, 445);
            btnFilter.Margin = new System.Windows.Forms.Padding(4);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new System.Drawing.Size(36, 25);
            btnFilter.TabIndex = 7;
            toolTip1.SetToolTip(btnFilter, "Filter");
            btnFilter.UseVisualStyleBackColor = true;
            btnFilter.Click += btnFilter_Click;
            // 
            // pagination
            // 
            pagination.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pagination.BackColor = System.Drawing.Color.WhiteSmoke;
            pagination.Location = new System.Drawing.Point(47, 442);
            pagination.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            pagination.Name = "pagination";
            pagination.PageCount = 0L;
            pagination.PageNumber = 1L;
            pagination.PageSize = 10;
            pagination.Size = new System.Drawing.Size(769, 37);
            pagination.TabIndex = 6;
            pagination.TotalCount = 0L;
            pagination.OnPageNumberChanged += pagination_OnPageNumberChanged;
            // 
            // cellContextMenu
            // 
            cellContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiCopy, tsmiShowContent, tsmiViewGeometry });
            cellContextMenu.Name = "cellContextMenu";
            cellContextMenu.Size = new System.Drawing.Size(165, 70);
            // 
            // tsmiCopy
            // 
            tsmiCopy.Name = "tsmiCopy";
            tsmiCopy.Size = new System.Drawing.Size(164, 22);
            tsmiCopy.Text = "Copy Content";
            tsmiCopy.Click += tsmiCopy_Click;
            // 
            // tsmiShowContent
            // 
            tsmiShowContent.Name = "tsmiShowContent";
            tsmiShowContent.Size = new System.Drawing.Size(164, 22);
            tsmiShowContent.Text = "Show Content";
            tsmiShowContent.Click += tsmiShowContent_Click;
            // 
            // tsmiViewGeometry
            // 
            tsmiViewGeometry.Name = "tsmiViewGeometry";
            tsmiViewGeometry.Size = new System.Drawing.Size(164, 22);
            tsmiViewGeometry.Text = "View Geometry";
            tsmiViewGeometry.Click += tsmiViewGeometry_Click;
            // 
            // uc_QuickFilter
            // 
            uc_QuickFilter.Location = new System.Drawing.Point(-3, -1);
            uc_QuickFilter.Name = "uc_QuickFilter";
            uc_QuickFilter.Size = new System.Drawing.Size(250, 30);
            uc_QuickFilter.TabIndex = 17;
            // 
            // loadingPanel
            // 
            loadingPanel.BackColor = System.Drawing.SystemColors.Control;
            loadingPanel.BackgroundColor = System.Drawing.SystemColors.ControlDark;
            loadingPanel.CancellationTokenSource = null;
            loadingPanel.InterruptButtonVisible = false;
            loadingPanel.Location = new System.Drawing.Point(4, 35);
            loadingPanel.Name = "loadingPanel";
            loadingPanel.Size = new System.Drawing.Size(812, 398);
            loadingPanel.TabIndex = 18;
            // 
            // UC_DataViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.WhiteSmoke;
            Controls.Add(loadingPanel);
            Controls.Add(uc_QuickFilter);
            Controls.Add(btnFilter);
            Controls.Add(pagination);
            Controls.Add(dgvData);
            Margin = new System.Windows.Forms.Padding(0);
            Name = "UC_DataViewer";
            Size = new System.Drawing.Size(825, 479);
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            cellContextMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DataGridView dgvData;
        private UC_Pagination pagination;
        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.ContextMenuStrip cellContextMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewGeometry;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowContent;
        private UC_QuickFilter uc_QuickFilter;
        private System.Windows.Forms.ToolTip toolTip1;
        private UC_LoadingPanel loadingPanel;
    }
}
