namespace DatabaseManager.Controls
{
    partial class UC_TableConstraints
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dgvConstraints = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiDeleteConstraint = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateChangeScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.colColumnName = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDefinition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvConstraints.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvConstraints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvConstraints.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colColumnName,
            this.colName,
            this.colDefinition,
            this.colComment});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.GradientActiveCaption;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvConstraints.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvConstraints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvConstraints.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvConstraints.Location = new System.Drawing.Point(0, 0);
            this.dgvConstraints.Margin = new System.Windows.Forms.Padding(4);
            this.dgvConstraints.MultiSelect = false;
            this.dgvConstraints.Name = "dgvConstraints";
            this.dgvConstraints.RowHeadersWidth = 25;
            this.dgvConstraints.Size = new System.Drawing.Size(845, 547);
            this.dgvConstraints.TabIndex = 10;
            this.dgvConstraints.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvConstraints_CellContentClick);
            this.dgvConstraints.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.dgvConstraints_DataError);
            this.dgvConstraints.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvConstraints_RowHeaderMouseClick);
            this.dgvConstraints.SizeChanged += new System.EventHandler(this.dgvConstraints_SizeChanged);
            this.dgvConstraints.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvConstraints_KeyDown);
            this.dgvConstraints.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgvConstraints_MouseUp);
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
            // colColumnName
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            this.colColumnName.DefaultCellStyle = dataGridViewCellStyle2;
            this.colColumnName.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colColumnName.HeaderText = "Column Name";
            this.colColumnName.Name = "colColumnName";
            this.colColumnName.Visible = false;
            this.colColumnName.Width = 120;
            // 
            // colName
            // 
            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "Constraint Name";
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
        private System.Windows.Forms.DataGridViewButtonColumn colColumnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefinition;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
    }
}
