namespace DatabaseManager.Controls
{
    partial class UC_TableIndexes
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvIndexes = new System.Windows.Forms.DataGridView();
            this.colColumnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPrimary = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Columns = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndexes)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvIndexes
            // 
            this.dgvIndexes.AllowDrop = true;
            this.dgvIndexes.BackgroundColor = System.Drawing.Color.White;
            this.dgvIndexes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvIndexes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvIndexes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIndexes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colColumnName,
            this.colPrimary,
            this.Type,
            this.Columns});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvIndexes.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvIndexes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvIndexes.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvIndexes.Location = new System.Drawing.Point(0, 0);
            this.dgvIndexes.MultiSelect = false;
            this.dgvIndexes.Name = "dgvIndexes";
            this.dgvIndexes.RowTemplate.Height = 23;
            this.dgvIndexes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvIndexes.Size = new System.Drawing.Size(758, 331);
            this.dgvIndexes.TabIndex = 8;
            // 
            // colColumnName
            // 
            this.colColumnName.DataPropertyName = "Name";
            this.colColumnName.HeaderText = "Name";
            this.colColumnName.Name = "colColumnName";
            this.colColumnName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colColumnName.Width = 200;
            // 
            // colPrimary
            // 
            this.colPrimary.DataPropertyName = "IsUnique";
            this.colPrimary.HeaderText = "Unique";
            this.colPrimary.Name = "colPrimary";
            this.colPrimary.Width = 60;
            // 
            // Type
            // 
            this.Type.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Type.HeaderText = "Type";
            this.Type.Name = "Type";
            // 
            // Columns
            // 
            this.Columns.HeaderText = "Columns";
            this.Columns.Name = "Columns";
            this.Columns.Width = 300;
            // 
            // UC_TableIndexes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvIndexes);
            this.Name = "UC_TableIndexes";
            this.Size = new System.Drawing.Size(758, 331);
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndexes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvIndexes;
        private System.Windows.Forms.DataGridViewTextBoxColumn colColumnName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colPrimary;
        private System.Windows.Forms.DataGridViewComboBoxColumn Type;
        private System.Windows.Forms.DataGridViewButtonColumn Columns;
    }
}
