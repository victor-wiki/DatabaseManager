namespace DatabaseManager.Forms
{
    partial class frmTableColumnsRelation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTableColumnsRelation));
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            lblPKTableName = new System.Windows.Forms.Label();
            ucPKTableColumns = new DatabaseManager.Controls.UC_TableColumnDetails();
            lblFKTableName = new System.Windows.Forms.Label();
            ucFKTableColumns = new DatabaseManager.Controls.UC_TableColumnDetails();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lblPKTableName);
            splitContainer1.Panel1.Controls.Add(ucPKTableColumns);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(lblFKTableName);
            splitContainer1.Panel2.Controls.Add(ucFKTableColumns);
            splitContainer1.Size = new System.Drawing.Size(509, 267);
            splitContainer1.SplitterDistance = 245;
            splitContainer1.TabIndex = 0;
            // 
            // lblPKTableName
            // 
            lblPKTableName.AutoSize = true;
            lblPKTableName.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
            lblPKTableName.Location = new System.Drawing.Point(6, 5);
            lblPKTableName.Name = "lblPKTableName";
            lblPKTableName.Size = new System.Drawing.Size(45, 17);
            lblPKTableName.TabIndex = 1;
            lblPKTableName.Text = "label1";
            // 
            // ucPKTableColumns
            // 
            ucPKTableColumns.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ucPKTableColumns.Location = new System.Drawing.Point(3, 25);
            ucPKTableColumns.Name = "ucPKTableColumns";
            ucPKTableColumns.Size = new System.Drawing.Size(246, 238);
            ucPKTableColumns.TabIndex = 0;
            // 
            // lblFKTableName
            // 
            lblFKTableName.AutoSize = true;
            lblFKTableName.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 134);
            lblFKTableName.Location = new System.Drawing.Point(6, 5);
            lblFKTableName.Name = "lblFKTableName";
            lblFKTableName.Size = new System.Drawing.Size(45, 17);
            lblFKTableName.TabIndex = 2;
            lblFKTableName.Text = "label2";
            // 
            // ucFKTableColumns
            // 
            ucFKTableColumns.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ucFKTableColumns.Location = new System.Drawing.Point(3, 25);
            ucFKTableColumns.Name = "ucFKTableColumns";
            ucFKTableColumns.Size = new System.Drawing.Size(254, 238);
            ucFKTableColumns.TabIndex = 0;
            // 
            // frmTableColumnsRelation
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(509, 267);
            Controls.Add(splitContainer1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmTableColumnsRelation";
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Load += frmTableColumnsRelation_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private Controls.UC_TableColumnDetails ucPKTableColumns;
        private Controls.UC_TableColumnDetails ucFKTableColumns;
        private System.Windows.Forms.Label lblPKTableName;
        private System.Windows.Forms.Label lblFKTableName;
    }
}