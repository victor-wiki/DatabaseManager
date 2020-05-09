namespace DatabaseManager.Controls
{
    partial class UC_TableDesigner
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtTableName = new System.Windows.Forms.TextBox();
            this.lblOwner = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTableComment = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabColumns = new System.Windows.Forms.TabPage();
            this.tabForeignKeys = new System.Windows.Forms.TabPage();
            this.dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.ucColumns = new DatabaseManager.Controls.UC_TableColumns();
            this.cboOwner = new System.Windows.Forms.ComboBox();
            this.tabIndex = new System.Windows.Forms.TabPage();
            this.tabConstraint = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabColumns.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Table Name:";
            // 
            // txtTableName
            // 
            this.txtTableName.Location = new System.Drawing.Point(81, 4);
            this.txtTableName.Name = "txtTableName";
            this.txtTableName.Size = new System.Drawing.Size(358, 21);
            this.txtTableName.TabIndex = 2;
            // 
            // lblOwner
            // 
            this.lblOwner.AutoSize = true;
            this.lblOwner.Location = new System.Drawing.Point(515, 10);
            this.lblOwner.Name = "lblOwner";
            this.lblOwner.Size = new System.Drawing.Size(41, 12);
            this.lblOwner.TabIndex = 3;
            this.lblOwner.Text = "Owner:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "Comment:";
            // 
            // txtTableComment
            // 
            this.txtTableComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTableComment.Location = new System.Drawing.Point(81, 36);
            this.txtTableComment.Name = "txtTableComment";
            this.txtTableComment.Size = new System.Drawing.Size(613, 21);
            this.txtTableComment.TabIndex = 6;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabColumns);
            this.tabControl1.Controls.Add(this.tabForeignKeys);
            this.tabControl1.Controls.Add(this.tabIndex);
            this.tabControl1.Controls.Add(this.tabConstraint);
            this.tabControl1.Location = new System.Drawing.Point(6, 63);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(695, 372);
            this.tabControl1.TabIndex = 8;
            // 
            // tabColumns
            // 
            this.tabColumns.Controls.Add(this.ucColumns);
            this.tabColumns.Location = new System.Drawing.Point(4, 22);
            this.tabColumns.Name = "tabColumns";
            this.tabColumns.Padding = new System.Windows.Forms.Padding(3);
            this.tabColumns.Size = new System.Drawing.Size(687, 346);
            this.tabColumns.TabIndex = 0;
            this.tabColumns.Text = "Columns";
            this.tabColumns.UseVisualStyleBackColor = true;
            // 
            // tabForeignKeys
            // 
            this.tabForeignKeys.Location = new System.Drawing.Point(4, 22);
            this.tabForeignKeys.Name = "tabForeignKeys";
            this.tabForeignKeys.Padding = new System.Windows.Forms.Padding(3);
            this.tabForeignKeys.Size = new System.Drawing.Size(687, 346);
            this.tabForeignKeys.TabIndex = 1;
            this.tabForeignKeys.Text = "Foreign Keys";
            this.tabForeignKeys.UseVisualStyleBackColor = true;
            // 
            // dataGridViewComboBoxColumn1
            // 
            this.dataGridViewComboBoxColumn1.DataPropertyName = "DataType";
            this.dataGridViewComboBoxColumn1.HeaderText = "Data Type";
            this.dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
            this.dataGridViewComboBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewComboBoxColumn1.Width = 120;
            // 
            // ucColumns
            // 
            this.ucColumns.BackColor = System.Drawing.Color.White;
            this.ucColumns.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            this.ucColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucColumns.Location = new System.Drawing.Point(3, 3);
            this.ucColumns.Name = "ucColumns";
            this.ucColumns.Size = new System.Drawing.Size(681, 340);
            this.ucColumns.TabIndex = 0;
            // 
            // cboOwner
            // 
            this.cboOwner.FormattingEnabled = true;
            this.cboOwner.Location = new System.Drawing.Point(562, 5);
            this.cboOwner.Name = "cboOwner";
            this.cboOwner.Size = new System.Drawing.Size(132, 20);
            this.cboOwner.TabIndex = 9;
            // 
            // tabIndex
            // 
            this.tabIndex.Location = new System.Drawing.Point(4, 22);
            this.tabIndex.Name = "tabIndex";
            this.tabIndex.Size = new System.Drawing.Size(687, 346);
            this.tabIndex.TabIndex = 2;
            this.tabIndex.Text = "Indexes";
            this.tabIndex.UseVisualStyleBackColor = true;
            // 
            // tabConstraint
            // 
            this.tabConstraint.Location = new System.Drawing.Point(4, 22);
            this.tabConstraint.Name = "tabConstraint";
            this.tabConstraint.Size = new System.Drawing.Size(687, 346);
            this.tabConstraint.TabIndex = 3;
            this.tabConstraint.Text = "Constraints";
            this.tabConstraint.UseVisualStyleBackColor = true;
            // 
            // UC_TableDesigner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.cboOwner);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.txtTableComment);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblOwner);
            this.Controls.Add(this.txtTableName);
            this.Controls.Add(this.label1);
            this.Name = "UC_TableDesigner";
            this.Size = new System.Drawing.Size(704, 438);
            this.Load += new System.EventHandler(this.UC_TableDesigner_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabColumns.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTableName;
        private System.Windows.Forms.Label lblOwner;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTableComment;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabForeignKeys;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
        private System.Windows.Forms.TabPage tabColumns;
        private UC_TableColumns ucColumns;
        private System.Windows.Forms.ComboBox cboOwner;
        private System.Windows.Forms.TabPage tabIndex;
        private System.Windows.Forms.TabPage tabConstraint;
    }
}
