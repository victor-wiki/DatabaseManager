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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_DataViewer));
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.btnFilter = new System.Windows.Forms.Button();
            this.pagination = new DatabaseManager.Controls.UC_Pagination();
            this.cellContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewGeometry = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiShowContent = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            this.cellContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.AllowUserToDeleteRows = false;
            this.dgvData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvData.Location = new System.Drawing.Point(0, 0);
            this.dgvData.Margin = new System.Windows.Forms.Padding(4);
            this.dgvData.Name = "dgvData";
            this.dgvData.ReadOnly = true;
            this.dgvData.RowHeadersVisible = false;
            this.dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvData.Size = new System.Drawing.Size(818, 428);
            this.dgvData.TabIndex = 5;
            this.dgvData.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvData_CellFormatting);
            this.dgvData.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvData_CellMouseClick);
            this.dgvData.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvData_DataError);
            this.dgvData.Sorted += new System.EventHandler(this.dgvData_Sorted);
            // 
            // btnFilter
            // 
            this.btnFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnFilter.FlatAppearance.BorderSize = 0;
            this.btnFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFilter.Image = ((System.Drawing.Image)(resources.GetObject("btnFilter.Image")));
            this.btnFilter.Location = new System.Drawing.Point(4, 446);
            this.btnFilter.Margin = new System.Windows.Forms.Padding(4);
            this.btnFilter.Name = "btnFilter";
            this.btnFilter.Size = new System.Drawing.Size(36, 25);
            this.btnFilter.TabIndex = 7;
            this.btnFilter.UseVisualStyleBackColor = true;
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // pagination
            // 
            this.pagination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pagination.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pagination.Location = new System.Drawing.Point(47, 442);
            this.pagination.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.pagination.Name = "pagination";
            this.pagination.PageCount = ((long)(0));
            this.pagination.PageNum = ((long)(1));
            this.pagination.PageSize = 10;
            this.pagination.Size = new System.Drawing.Size(769, 37);
            this.pagination.TabIndex = 6;
            this.pagination.TotalCount = ((long)(0));
            this.pagination.OnPageNumberChanged += new DatabaseManager.Controls.UC_Pagination.PageNumberChangeHandler(this.pagination_OnPageNumberChanged);
            // 
            // cellContextMenu
            // 
            this.cellContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCopy,
            this.tsmiShowContent,
            this.tsmiViewGeometry});
            this.cellContextMenu.Name = "cellContextMenu";
            this.cellContextMenu.Size = new System.Drawing.Size(181, 92);
            // 
            // tsmiCopy
            // 
            this.tsmiCopy.Name = "tsmiCopy";
            this.tsmiCopy.Size = new System.Drawing.Size(164, 22);
            this.tsmiCopy.Text = "Copy Content";
            this.tsmiCopy.Click += new System.EventHandler(this.tsmiCopy_Click);
            // 
            // tsmiViewGeometry
            // 
            this.tsmiViewGeometry.Name = "tsmiViewGeometry";
            this.tsmiViewGeometry.Size = new System.Drawing.Size(164, 22);
            this.tsmiViewGeometry.Text = "View Geometry";
            this.tsmiViewGeometry.Click += new System.EventHandler(this.tsmiViewGeometry_Click);
            // 
            // tsmiShowContent
            // 
            this.tsmiShowContent.Name = "tsmiShowContent";
            this.tsmiShowContent.Size = new System.Drawing.Size(180, 22);
            this.tsmiShowContent.Text = "Show Content";
            this.tsmiShowContent.Click += new System.EventHandler(this.tsmiShowContent_Click);
            // 
            // UC_DataViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.btnFilter);
            this.Controls.Add(this.pagination);
            this.Controls.Add(this.dgvData);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UC_DataViewer";
            this.Size = new System.Drawing.Size(825, 479);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            this.cellContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvData;
        private UC_Pagination pagination;
        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.ContextMenuStrip cellContextMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewGeometry;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowContent;
    }
}
