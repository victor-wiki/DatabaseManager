namespace DatabaseManager.Controls
{
    partial class UC_DataEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_DataEditor));
            dgvData = new System.Windows.Forms.DataGridView();
            btnFilter = new System.Windows.Forms.Button();
            pagination = new UC_Pagination();
            cellContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiSetCellValueToNull = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            tsmiShowContent = new System.Windows.Forms.ToolStripMenuItem();
            tsmiViewGeometry = new System.Windows.Forms.ToolStripMenuItem();
            cboAddMultipleRows = new System.Windows.Forms.ComboBox();
            btnAdd = new System.Windows.Forms.Button();
            btnRemove = new System.Windows.Forms.Button();
            btnRevert = new System.Windows.Forms.Button();
            btnCommit = new System.Windows.Forms.Button();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            cboAddMode = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            cellContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // dgvData
            // 
            dgvData.AllowUserToAddRows = false;
            dgvData.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Location = new System.Drawing.Point(0, 33);
            dgvData.Margin = new System.Windows.Forms.Padding(4);
            dgvData.Name = "dgvData";
            dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgvData.Size = new System.Drawing.Size(818, 395);
            dgvData.TabIndex = 5;
            dgvData.CellFormatting += dgvData_CellFormatting;
            dgvData.CellLeave += dgvData_CellLeave;
            dgvData.CellMouseClick += dgvData_CellMouseClick;
            dgvData.CellValidating += dgvData_CellValidating;
            dgvData.CellValueChanged += dgvData_CellValueChanged;
            dgvData.DataError += dgvData_DataError;
            dgvData.Sorted += dgvData_Sorted;          
            dgvData.KeyUp += dgvData_KeyUp;
            // 
            // btnFilter
            // 
            btnFilter.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnFilter.FlatAppearance.BorderSize = 0;
            btnFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFilter.Image = (System.Drawing.Image)resources.GetObject("btnFilter.Image");
            btnFilter.Location = new System.Drawing.Point(4, 446);
            btnFilter.Margin = new System.Windows.Forms.Padding(4);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new System.Drawing.Size(36, 25);
            btnFilter.TabIndex = 7;
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
            pagination.PageNum = 1L;
            pagination.PageSize = 10;
            pagination.Size = new System.Drawing.Size(769, 37);
            pagination.TabIndex = 6;
            pagination.TotalCount = 0L;
            pagination.OnPageNumberChanged += pagination_OnPageNumberChanged;
            // 
            // cellContextMenu
            // 
            cellContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiSetCellValueToNull, tsmiCopy, tsmiShowContent, tsmiViewGeometry });
            cellContextMenu.Name = "cellContextMenu";
            cellContextMenu.Size = new System.Drawing.Size(165, 92);
            // 
            // tsmiSetCellValueToNull
            // 
            tsmiSetCellValueToNull.Name = "tsmiSetCellValueToNull";
            tsmiSetCellValueToNull.Size = new System.Drawing.Size(164, 22);
            tsmiSetCellValueToNull.Text = "Set NULL";
            tsmiSetCellValueToNull.Click += tsmiSetCellValueToNull_Click;
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
            // cboAddMultipleRows
            // 
            cboAddMultipleRows.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboAddMultipleRows.DropDownWidth = 180;
            cboAddMultipleRows.FormattingEnabled = true;
            cboAddMultipleRows.Items.AddRange(new object[] { "Insert multiple rows" });
            cboAddMultipleRows.Location = new System.Drawing.Point(6, 2);
            cboAddMultipleRows.Name = "cboAddMultipleRows";
            cboAddMultipleRows.Size = new System.Drawing.Size(50, 25);
            cboAddMultipleRows.TabIndex = 8;
            cboAddMultipleRows.SelectedIndexChanged += cboAddModes_SelectedIndexChanged;
            // 
            // btnAdd
            // 
            btnAdd.Image = Resources.Add;
            btnAdd.Location = new System.Drawing.Point(4, 1);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(38, 27);
            btnAdd.TabIndex = 9;
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnRemove
            // 
            btnRemove.Image = Resources.Remove;
            btnRemove.Location = new System.Drawing.Point(61, 1);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new System.Drawing.Size(34, 26);
            btnRemove.TabIndex = 10;
            toolTip1.SetToolTip(btnRemove, "Remove");
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // btnRevert
            // 
            btnRevert.Enabled = false;
            btnRevert.Image = Resources.Revert;
            btnRevert.Location = new System.Drawing.Point(101, 1);
            btnRevert.Name = "btnRevert";
            btnRevert.Size = new System.Drawing.Size(37, 26);
            btnRevert.TabIndex = 11;
            toolTip1.SetToolTip(btnRevert, "Revert");
            btnRevert.UseVisualStyleBackColor = true;
            btnRevert.Click += btnRevert_Click;
            // 
            // btnCommit
            // 
            btnCommit.Enabled = false;
            btnCommit.Image = Resources.Check;
            btnCommit.Location = new System.Drawing.Point(144, 1);
            btnCommit.Name = "btnCommit";
            btnCommit.Size = new System.Drawing.Size(37, 26);
            btnCommit.TabIndex = 12;
            toolTip1.SetToolTip(btnCommit, "Commit");
            btnCommit.UseVisualStyleBackColor = true;
            btnCommit.Click += btnCommit_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // cboAddMode
            // 
            cboAddMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboAddMode.DropDownWidth = 220;
            cboAddMode.FormattingEnabled = true;
            cboAddMode.Items.AddRange(new object[] { "Place new rows above selected row", "Place new rows below selected row", "Place new rows at the bottom" });
            cboAddMode.Location = new System.Drawing.Point(399, 1);
            cboAddMode.Name = "cboAddMode";
            cboAddMode.Size = new System.Drawing.Size(217, 25);
            cboAddMode.TabIndex = 14;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(296, 5);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(99, 17);
            label1.TabIndex = 15;
            label1.Text = "Add row mode:";
            // 
            // UC_DataEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.WhiteSmoke;
            Controls.Add(label1);
            Controls.Add(cboAddMode);
            Controls.Add(btnCommit);
            Controls.Add(btnRevert);
            Controls.Add(btnRemove);
            Controls.Add(btnAdd);
            Controls.Add(cboAddMultipleRows);
            Controls.Add(btnFilter);
            Controls.Add(pagination);
            Controls.Add(dgvData);
            Margin = new System.Windows.Forms.Padding(0);
            Name = "UC_DataEditor";
            Size = new System.Drawing.Size(825, 479);
            MouseMove += UC_DataEditor_MouseMove;
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            cellContextMenu.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataGridView dgvData;
        private UC_Pagination pagination;
        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.ContextMenuStrip cellContextMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewGeometry;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowContent;
        private System.Windows.Forms.ComboBox cboAddMultipleRows;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.Button btnCommit;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ComboBox cboAddMode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem tsmiSetCellValueToNull;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
