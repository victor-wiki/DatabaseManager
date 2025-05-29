namespace DatabaseManager.Forms
{
    partial class frmDatabaseVisibility
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDatabaseVisibility));
            dgvDatabases = new System.Windows.Forms.DataGridView();
            colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDatabase = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colVisible = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbInvisible = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tsbVisible = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            tsbDelete = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            tsbClear = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            tsbRefresh = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)dgvDatabases).BeginInit();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // dgvDatabases
            // 
            dgvDatabases.AllowUserToAddRows = false;
            dgvDatabases.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvDatabases.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDatabases.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colId, colDatabase, colVisible });
            dgvDatabases.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            dgvDatabases.Location = new System.Drawing.Point(2, 33);
            dgvDatabases.Margin = new System.Windows.Forms.Padding(4);
            dgvDatabases.Name = "dgvDatabases";
            dgvDatabases.ReadOnly = true;
            dgvDatabases.RowHeadersVisible = false;
            dgvDatabases.RowHeadersWidth = 20;
            dgvDatabases.RowTemplate.Height = 23;
            dgvDatabases.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvDatabases.Size = new System.Drawing.Size(618, 428);
            dgvDatabases.TabIndex = 21;
            // 
            // colId
            // 
            colId.DataPropertyName = "Id";
            colId.HeaderText = "Id";
            colId.Name = "colId";
            colId.ReadOnly = true;
            colId.Visible = false;
            // 
            // colDatabase
            // 
            colDatabase.HeaderText = "Database";
            colDatabase.Name = "colDatabase";
            colDatabase.ReadOnly = true;
            colDatabase.Width = 250;
            // 
            // colVisible
            // 
            colVisible.HeaderText = "Visible";
            colVisible.Name = "colVisible";
            colVisible.ReadOnly = true;
            colVisible.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbInvisible, toolStripSeparator1, tsbVisible, toolStripSeparator2, tsbDelete, toolStripSeparator3, tsbClear, toolStripSeparator4, tsbRefresh });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(623, 28);
            toolStrip1.TabIndex = 43;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbInvisible
            // 
            tsbInvisible.AutoSize = false;
            tsbInvisible.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbInvisible.Image = (System.Drawing.Image)resources.GetObject("tsbInvisible.Image");
            tsbInvisible.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbInvisible.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbInvisible.Name = "tsbInvisible";
            tsbInvisible.Size = new System.Drawing.Size(25, 25);
            tsbInvisible.Text = "Add";
            tsbInvisible.Click += tsbInvisible_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbVisible
            // 
            tsbVisible.AutoSize = false;
            tsbVisible.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbVisible.Image = (System.Drawing.Image)resources.GetObject("tsbVisible.Image");
            tsbVisible.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbVisible.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbVisible.Name = "tsbVisible";
            tsbVisible.Size = new System.Drawing.Size(25, 25);
            tsbVisible.Text = "Edit";
            tsbVisible.Click += tsbVisible_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
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
            // tsbClear
            // 
            tsbClear.AutoSize = false;
            tsbClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbClear.Image = (System.Drawing.Image)resources.GetObject("tsbClear.Image");
            tsbClear.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbClear.Name = "tsbClear";
            tsbClear.Size = new System.Drawing.Size(23, 25);
            tsbClear.Text = "Clear";
            tsbClear.Click += tsbClear_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbRefresh
            // 
            tsbRefresh.AutoSize = false;
            tsbRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbRefresh.Image = (System.Drawing.Image)resources.GetObject("tsbRefresh.Image");
            tsbRefresh.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbRefresh.Name = "tsbRefresh";
            tsbRefresh.Size = new System.Drawing.Size(25, 25);
            tsbRefresh.Text = "Manage Prifiles";
            tsbRefresh.Click += tsbRefresh_Click;
            // 
            // frmDatabaseVisibility
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(623, 465);
            Controls.Add(toolStrip1);
            Controls.Add(dgvDatabases);
            Name = "frmDatabaseVisibility";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Visibility";
            Load += frmDatabaseVisibility_Load;
            ((System.ComponentModel.ISupportInitialize)dgvDatabases).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDatabases;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDatabase;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colVisible;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbInvisible;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbVisible;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsbClear;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton tsbRefresh;
    }
}