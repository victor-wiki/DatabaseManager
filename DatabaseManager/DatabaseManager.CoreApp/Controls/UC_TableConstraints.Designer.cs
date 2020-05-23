namespace DatabaseManager.Controls
{
    partial class UC_TableConstraints
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvConstraints = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDefinition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiDeleteConstraint = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateChangeScripts = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvConstraints)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvConstraints
            // 
            this.dgvConstraints.AllowDrop = true;
            this.dgvConstraints.BackgroundColor = System.Drawing.Color.White;
            this.dgvConstraints.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvConstraints.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvConstraints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvConstraints.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colDefinition,
            this.colComment});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvConstraints.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvConstraints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvConstraints.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvConstraints.Location = new System.Drawing.Point(0, 0);
            this.dgvConstraints.Margin = new System.Windows.Forms.Padding(4);
            this.dgvConstraints.MultiSelect = false;
            this.dgvConstraints.Name = "dgvConstraints";
            this.dgvConstraints.RowHeadersWidth = 25;
            this.dgvConstraints.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvConstraints.Size = new System.Drawing.Size(845, 547);
            this.dgvConstraints.TabIndex = 10;
            this.dgvConstraints.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvConstraints_DataError);
            this.dgvConstraints.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvConstraints_RowHeaderMouseClick);
            this.dgvConstraints.SizeChanged += new System.EventHandler(this.dgvConstraints_SizeChanged);
            this.dgvConstraints.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvConstraints_KeyDown);
            this.dgvConstraints.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgvConstraints_MouseUp);
            // 
            // colName
            // 
            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.Width = 200;
            // 
            // colDefinition
            // 
            this.colDefinition.DataPropertyName = "Definition";
            this.colDefinition.HeaderText = "Expression";
            this.colDefinition.Name = "colDefinition";
            this.colDefinition.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colDefinition.Width = 300;
            // 
            // colComment
            // 
            this.colComment.DataPropertyName = "Comment";
            this.colComment.HeaderText = "Comment";
            this.colComment.Name = "colComment";
            this.colComment.Width = 150;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiDeleteConstraint,
            this.tsmiGenerateChangeScripts});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(229, 48);
            // 
            // tsmiDeleteConstraint
            // 
            this.tsmiDeleteConstraint.Name = "tsmiDeleteConstraint";
            this.tsmiDeleteConstraint.Size = new System.Drawing.Size(228, 22);
            this.tsmiDeleteConstraint.Text = "Delete Constraint";
            this.tsmiDeleteConstraint.Click += new System.EventHandler(this.tsmiDeleteConstraint_Click);
            // 
            // tsmiGenerateChangeScripts
            // 
            this.tsmiGenerateChangeScripts.Name = "tsmiGenerateChangeScripts";
            this.tsmiGenerateChangeScripts.Size = new System.Drawing.Size(228, 22);
            this.tsmiGenerateChangeScripts.Text = "Generate Changed Scripts";
            this.tsmiGenerateChangeScripts.Click += new System.EventHandler(this.tsmiGenerateChangeScripts_Click);
            // 
            // UC_TableConstraints
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvConstraints);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UC_TableConstraints";
            this.Size = new System.Drawing.Size(845, 547);
            this.Load += new System.EventHandler(this.UC_TableConstraints_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvConstraints)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvConstraints;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiDeleteConstraint;
        private System.Windows.Forms.ToolStripMenuItem tsmiGenerateChangeScripts;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefinition;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
    }
}
