namespace DatabaseManager.Forms
{
    partial class frmDbConnectionProfileManage
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDbConnectionProfileManage));
            dgvDbConnectionProfile = new System.Windows.Forms.DataGridView();
            colCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colServer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDatabase = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPriority = new System.Windows.Forms.DataGridViewTextBoxColumn();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbSave = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            tsbDelete = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tsbEdit = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            tsbClear = new System.Windows.Forms.ToolStripButton();
            picInfo = new System.Windows.Forms.PictureBox();
            label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)dgvDbConnectionProfile).BeginInit();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picInfo).BeginInit();
            SuspendLayout();
            // 
            // dgvDbConnectionProfile
            // 
            dgvDbConnectionProfile.AllowDrop = true;
            dgvDbConnectionProfile.AllowUserToAddRows = false;
            dgvDbConnectionProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvDbConnectionProfile.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDbConnectionProfile.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colCheck, colId, colName, colServer, colPort, colDatabase, colPriority });
            dgvDbConnectionProfile.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            dgvDbConnectionProfile.Location = new System.Drawing.Point(5, 42);
            dgvDbConnectionProfile.Margin = new System.Windows.Forms.Padding(4);
            dgvDbConnectionProfile.MultiSelect = false;
            dgvDbConnectionProfile.Name = "dgvDbConnectionProfile";
            dgvDbConnectionProfile.RowHeadersVisible = false;
            dgvDbConnectionProfile.RowHeadersWidth = 20;
            dgvDbConnectionProfile.RowTemplate.Height = 23;
            dgvDbConnectionProfile.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvDbConnectionProfile.Size = new System.Drawing.Size(896, 337);
            dgvDbConnectionProfile.TabIndex = 21;
            dgvDbConnectionProfile.DragDrop += dgvDbConnectionProfile_DragDrop;
            dgvDbConnectionProfile.DragOver += dgvDbConnectionProfile_DragOver;
            dgvDbConnectionProfile.MouseDown += dgvDbConnectionProfile_MouseDown;
            dgvDbConnectionProfile.MouseMove += dgvDbConnectionProfile_MouseMove;
            // 
            // colCheck
            // 
            colCheck.HeaderText = "";
            colCheck.Name = "colCheck";
            colCheck.Width = 40;
            // 
            // colId
            // 
            colId.DataPropertyName = "Id";
            colId.HeaderText = "Id";
            colId.Name = "colId";
            colId.ReadOnly = true;
            colId.Visible = false;
            // 
            // colName
            // 
            colName.HeaderText = "Profile Name";
            colName.Name = "colName";
            colName.ReadOnly = true;
            colName.Width = 250;
            // 
            // colServer
            // 
            colServer.DataPropertyName = "Server";
            colServer.HeaderText = "Server";
            colServer.Name = "colServer";
            colServer.ReadOnly = true;
            colServer.Width = 250;
            // 
            // colPort
            // 
            colPort.HeaderText = "Port";
            colPort.Name = "colPort";
            colPort.ReadOnly = true;
            // 
            // colDatabase
            // 
            colDatabase.HeaderText = "Database";
            colDatabase.Name = "colDatabase";
            colDatabase.ReadOnly = true;
            colDatabase.Width = 150;
            // 
            // colPriority
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            colPriority.DefaultCellStyle = dataGridViewCellStyle1;
            colPriority.HeaderText = "Priority";
            colPriority.Name = "colPriority";
            colPriority.ReadOnly = true;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbSave, toolStripSeparator2, tsbDelete, toolStripSeparator1, tsbEdit, toolStripSeparator3, tsbClear });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(907, 28);
            toolStrip1.TabIndex = 43;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbSave
            // 
            tsbSave.AutoSize = false;
            tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbSave.Enabled = false;
            tsbSave.Image = (System.Drawing.Image)resources.GetObject("tsbSave.Image");
            tsbSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbSave.Name = "tsbSave";
            tsbSave.Size = new System.Drawing.Size(25, 25);
            tsbSave.Text = "Save";
            tsbSave.Click += tsbSave_Click;
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
            tsbDelete.Size = new System.Drawing.Size(25, 25);
            tsbDelete.Text = "Delete";
            tsbDelete.Click += tsbDelete_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbEdit
            // 
            tsbEdit.AutoSize = false;
            tsbEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbEdit.Image = (System.Drawing.Image)resources.GetObject("tsbEdit.Image");
            tsbEdit.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbEdit.Name = "tsbEdit";
            tsbEdit.Size = new System.Drawing.Size(25, 25);
            tsbEdit.Text = "Edit";
            tsbEdit.Click += tsbEdit_Click;
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
            tsbClear.Size = new System.Drawing.Size(25, 25);
            tsbClear.Text = "Clear";
            tsbClear.Click += tsbClear_Click;
            // 
            // picInfo
            // 
            picInfo.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            picInfo.Location = new System.Drawing.Point(6, 383);
            picInfo.Name = "picInfo";
            picInfo.Size = new System.Drawing.Size(20, 20);
            picInfo.TabIndex = 44;
            picInfo.TabStop = false;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(28, 383);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(326, 17);
            label1.TabIndex = 45;
            label1.Text = "You can drag and drop the row to reorder the record.";
            // 
            // frmDbConnectionProfileManage
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(907, 403);
            Controls.Add(label1);
            Controls.Add(picInfo);
            Controls.Add(toolStrip1);
            Controls.Add(dgvDbConnectionProfile);
            Name = "frmDbConnectionProfileManage";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Connection Profile Manage";
            Load += frmDbConnectionProfile_Load;
            ((System.ComponentModel.ISupportInitialize)dgvDbConnectionProfile).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picInfo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDbConnectionProfile;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbDelete;
        private System.Windows.Forms.ToolStripButton tsbClear;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServer;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPort;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDatabase;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPriority;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.PictureBox picInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripButton tsbEdit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}