namespace DatabaseManager.Forms
{
    partial class frmDataTypeMappingSpecial
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDataTypeMappingSpecial));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbAdd = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tsbDelete = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            tsbCommit = new System.Windows.Forms.ToolStripButton();
            dgvData = new System.Windows.Forms.DataGridView();
            colCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPrecision = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colScale = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colTargetType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colTargetMaxLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colNoLength = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colSubstitute = new System.Windows.Forms.DataGridViewTextBoxColumn();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbAdd, toolStripSeparator1, tsbDelete, toolStripSeparator3, tsbCommit });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(922, 28);
            toolStrip1.TabIndex = 61;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbAdd
            // 
            tsbAdd.AutoSize = false;
            tsbAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbAdd.Image = (System.Drawing.Image)resources.GetObject("tsbAdd.Image");
            tsbAdd.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbAdd.Name = "tsbAdd";
            tsbAdd.Size = new System.Drawing.Size(25, 25);
            tsbAdd.Text = "Add";
            tsbAdd.Click += tsbAdd_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbDelete
            // 
            tsbDelete.AutoSize = false;
            tsbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbDelete.Image = (System.Drawing.Image)resources.GetObject("tsbDelete.Image");
            tsbDelete.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbDelete.Name = "tsbDelete";
            tsbDelete.Size = new System.Drawing.Size(23, 25);
            tsbDelete.Text = "Delete";
            tsbDelete.Click += tsbDelete_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbCommit
            // 
            tsbCommit.AutoSize = false;
            tsbCommit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbCommit.Image = (System.Drawing.Image)resources.GetObject("tsbCommit.Image");
            tsbCommit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbCommit.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbCommit.Name = "tsbCommit";
            tsbCommit.Size = new System.Drawing.Size(23, 25);
            tsbCommit.Text = "Commit";
            tsbCommit.Click += tsbCommit_Click;
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
            dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colCheck, colName, colValue, colPrecision, colScale, colTargetType, colTargetMaxLength, colNoLength, colSubstitute });
            dgvData.Location = new System.Drawing.Point(0, 32);
            dgvData.Margin = new System.Windows.Forms.Padding(4);
            dgvData.Name = "dgvData";
            dgvData.RowHeadersVisible = false;
            dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgvData.Size = new System.Drawing.Size(922, 290);
            dgvData.TabIndex = 62;
            dgvData.CellValidating += dgvData_CellValidating;
            dgvData.DataBindingComplete += dgvData_DataBindingComplete;
            dgvData.DataError += dgvData_DataError;
            // 
            // colCheck
            // 
            colCheck.HeaderText = "";
            colCheck.Name = "colCheck";
            colCheck.Width = 40;
            // 
            // colName
            // 
            colName.FillWeight = 120F;
            colName.HeaderText = "Name";
            colName.Name = "colName";
            colName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // colValue
            // 
            colValue.FillWeight = 120F;
            colValue.HeaderText = "Value";
            colValue.Name = "colValue";
            // 
            // colPrecision
            // 
            colPrecision.HeaderText = "Precision";
            colPrecision.Name = "colPrecision";
            // 
            // colScale
            // 
            colScale.HeaderText = "Scale";
            colScale.Name = "colScale";
            // 
            // colTargetType
            // 
            colTargetType.FillWeight = 120F;
            colTargetType.HeaderText = "Target Type";
            colTargetType.Name = "colTargetType";
            colTargetType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // colTargetMaxLength
            // 
            colTargetMaxLength.HeaderText = "Target Max Length";
            colTargetMaxLength.Name = "colTargetMaxLength";
            colTargetMaxLength.Width = 150;
            // 
            // colNoLength
            // 
            colNoLength.HeaderText = "No Length";
            colNoLength.Name = "colNoLength";
            colNoLength.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colNoLength.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // colSubstitute
            // 
            colSubstitute.HeaderText = "Substitute";
            colSubstitute.Name = "colSubstitute";
            // 
            // frmDataTypeMappingSpecial
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(922, 325);
            Controls.Add(dgvData);
            Controls.Add(toolStrip1);
            Name = "frmDataTypeMappingSpecial";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Data Type Mapping Special";
            Load += frmDataTypeMappingSpecial_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbCommit;
        private System.Windows.Forms.ToolStripButton tsbAdd;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrecision;
        private System.Windows.Forms.DataGridViewTextBoxColumn colScale;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetMaxLength;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colNoLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSubstitute;
    }
}