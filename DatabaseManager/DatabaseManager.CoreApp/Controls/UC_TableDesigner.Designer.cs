using DatabaseInterpreter.Model;

namespace DatabaseManager.Controls
{
    partial class UC_TableDesigner
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
            label1 = new System.Windows.Forms.Label();
            txtTableName = new System.Windows.Forms.TextBox();
            lblSchema = new System.Windows.Forms.Label();
            lblComment = new System.Windows.Forms.Label();
            txtTableComment = new System.Windows.Forms.TextBox();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabColumns = new System.Windows.Forms.TabPage();
            ucColumns = new UC_TableColumns();
            tabForeignKeys = new System.Windows.Forms.TabPage();
            ucForeignKeys = new UC_TableForeignKeys();
            tabIndexes = new System.Windows.Forms.TabPage();
            ucIndexes = new UC_TableIndexes();
            tabConstraints = new System.Windows.Forms.TabPage();
            ucConstraints = new UC_TableConstraints();
            dataGridViewComboBoxColumn1 = new System.Windows.Forms.DataGridViewComboBoxColumn();
            cboSchema = new System.Windows.Forms.ComboBox();
            tabControl1.SuspendLayout();
            tabColumns.SuspendLayout();
            tabForeignKeys.SuspendLayout();
            tabIndexes.SuspendLayout();
            tabConstraints.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(5, 9);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(82, 17);
            label1.TabIndex = 1;
            label1.Text = "Table Name:";
            // 
            // txtTableName
            // 
            txtTableName.Location = new System.Drawing.Point(94, 6);
            txtTableName.Margin = new System.Windows.Forms.Padding(4);
            txtTableName.Name = "txtTableName";
            txtTableName.Size = new System.Drawing.Size(417, 23);
            txtTableName.TabIndex = 2;
            // 
            // lblSchema
            // 
            lblSchema.Location = new System.Drawing.Point(566, 9);
            lblSchema.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblSchema.Name = "lblSchema";
            lblSchema.Size = new System.Drawing.Size(81, 17);
            lblSchema.TabIndex = 3;
            lblSchema.Text = "Schema:";
            lblSchema.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblComment
            // 
            lblComment.AutoSize = true;
            lblComment.Location = new System.Drawing.Point(5, 46);
            lblComment.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblComment.Name = "lblComment";
            lblComment.Size = new System.Drawing.Size(67, 17);
            lblComment.TabIndex = 5;
            lblComment.Text = "Comment:";
            // 
            // txtTableComment
            // 
            txtTableComment.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtTableComment.Location = new System.Drawing.Point(94, 45);
            txtTableComment.Margin = new System.Windows.Forms.Padding(4);
            txtTableComment.Name = "txtTableComment";
            txtTableComment.Size = new System.Drawing.Size(808, 23);
            txtTableComment.TabIndex = 6;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl1.Controls.Add(tabColumns);
            tabControl1.Controls.Add(tabForeignKeys);
            tabControl1.Controls.Add(tabIndexes);
            tabControl1.Controls.Add(tabConstraints);
            tabControl1.Location = new System.Drawing.Point(7, 76);
            tabControl1.Margin = new System.Windows.Forms.Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(905, 540);
            tabControl1.TabIndex = 8;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // 
            // tabColumns
            // 
            tabColumns.Controls.Add(ucColumns);
            tabColumns.Location = new System.Drawing.Point(4, 26);
            tabColumns.Margin = new System.Windows.Forms.Padding(4);
            tabColumns.Name = "tabColumns";
            tabColumns.Padding = new System.Windows.Forms.Padding(4);
            tabColumns.Size = new System.Drawing.Size(897, 510);
            tabColumns.TabIndex = 0;
            tabColumns.Text = "Columns";
            tabColumns.UseVisualStyleBackColor = true;
            // 
            // ucColumns
            // 
            ucColumns.BackColor = System.Drawing.Color.White;
            ucColumns.DatabaseType = DatabaseType.Unknown;
            ucColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            ucColumns.Location = new System.Drawing.Point(4, 4);
            ucColumns.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            ucColumns.Name = "ucColumns";
            ucColumns.Size = new System.Drawing.Size(889, 502);
            ucColumns.TabIndex = 0;
            ucColumns.UserDefinedTypes = null;
            // 
            // tabForeignKeys
            // 
            tabForeignKeys.Controls.Add(ucForeignKeys);
            tabForeignKeys.Location = new System.Drawing.Point(4, 26);
            tabForeignKeys.Margin = new System.Windows.Forms.Padding(4);
            tabForeignKeys.Name = "tabForeignKeys";
            tabForeignKeys.Padding = new System.Windows.Forms.Padding(4);
            tabForeignKeys.Size = new System.Drawing.Size(897, 510);
            tabForeignKeys.TabIndex = 1;
            tabForeignKeys.Text = "Foreign Keys";
            tabForeignKeys.UseVisualStyleBackColor = true;
            // 
            // ucForeignKeys
            // 
            ucForeignKeys.DatabaseType = DatabaseType.Unknown;
            ucForeignKeys.DefaultSchema = null;
            ucForeignKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            ucForeignKeys.Location = new System.Drawing.Point(4, 4);
            ucForeignKeys.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            ucForeignKeys.Name = "ucForeignKeys";
            ucForeignKeys.Size = new System.Drawing.Size(889, 502);
            ucForeignKeys.TabIndex = 0;
            ucForeignKeys.Table = null;
            // 
            // tabIndexes
            // 
            tabIndexes.Controls.Add(ucIndexes);
            tabIndexes.Location = new System.Drawing.Point(4, 26);
            tabIndexes.Margin = new System.Windows.Forms.Padding(4);
            tabIndexes.Name = "tabIndexes";
            tabIndexes.Size = new System.Drawing.Size(897, 510);
            tabIndexes.TabIndex = 2;
            tabIndexes.Text = "Indexes";
            tabIndexes.UseVisualStyleBackColor = true;
            // 
            // ucIndexes
            // 
            ucIndexes.DatabaseType = DatabaseType.Unknown;
            ucIndexes.Dock = System.Windows.Forms.DockStyle.Fill;
            ucIndexes.Location = new System.Drawing.Point(0, 0);
            ucIndexes.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            ucIndexes.Name = "ucIndexes";
            ucIndexes.Size = new System.Drawing.Size(897, 510);
            ucIndexes.TabIndex = 0;
            ucIndexes.Table = null;
            // 
            // tabConstraints
            // 
            tabConstraints.Controls.Add(ucConstraints);
            tabConstraints.Location = new System.Drawing.Point(4, 26);
            tabConstraints.Margin = new System.Windows.Forms.Padding(4);
            tabConstraints.Name = "tabConstraints";
            tabConstraints.Size = new System.Drawing.Size(897, 510);
            tabConstraints.TabIndex = 3;
            tabConstraints.Text = "Constraints";
            tabConstraints.UseVisualStyleBackColor = true;
            // 
            // ucConstraints
            // 
            ucConstraints.DatabaseType = DatabaseType.Unknown;
            ucConstraints.Dock = System.Windows.Forms.DockStyle.Fill;
            ucConstraints.Location = new System.Drawing.Point(0, 0);
            ucConstraints.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            ucConstraints.Name = "ucConstraints";
            ucConstraints.Size = new System.Drawing.Size(897, 510);
            ucConstraints.TabIndex = 0;
            ucConstraints.Table = null;
            // 
            // dataGridViewComboBoxColumn1
            // 
            dataGridViewComboBoxColumn1.DataPropertyName = "DataType";
            dataGridViewComboBoxColumn1.HeaderText = "Data Type";
            dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
            dataGridViewComboBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            dataGridViewComboBoxColumn1.Width = 120;
            // 
            // cboSchema
            // 
            cboSchema.FormattingEnabled = true;
            cboSchema.Location = new System.Drawing.Point(655, 6);
            cboSchema.Margin = new System.Windows.Forms.Padding(4);
            cboSchema.Name = "cboSchema";
            cboSchema.Size = new System.Drawing.Size(247, 25);
            cboSchema.TabIndex = 9;
            // 
            // UC_TableDesigner
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            Controls.Add(cboSchema);
            Controls.Add(tabControl1);
            Controls.Add(txtTableComment);
            Controls.Add(lblComment);
            Controls.Add(lblSchema);
            Controls.Add(txtTableName);
            Controls.Add(label1);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "UC_TableDesigner";
            Size = new System.Drawing.Size(915, 620);
            Load += UC_TableDesigner_Load;
            tabControl1.ResumeLayout(false);
            tabColumns.ResumeLayout(false);
            tabForeignKeys.ResumeLayout(false);
            tabIndexes.ResumeLayout(false);
            tabConstraints.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTableName;
        private System.Windows.Forms.Label lblSchema;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.TextBox txtTableComment;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabForeignKeys;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
        private System.Windows.Forms.TabPage tabColumns;
        private UC_TableColumns ucColumns;
        private System.Windows.Forms.ComboBox cboSchema;
        private System.Windows.Forms.TabPage tabIndexes;
        private System.Windows.Forms.TabPage tabConstraints;
        private UC_TableIndexes ucIndexes;
        private UC_TableForeignKeys ucForeignKeys;
        private UC_TableConstraints ucConstraints;
    }
}
