namespace DatabaseManager.Forms
{
    partial class frmDbConnectionManage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDbConnectionManage));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            dgvDbConnection = new System.Windows.Forms.DataGridView();
            label1 = new System.Windows.Forms.Label();
            cboDbType = new System.Windows.Forms.ComboBox();
            btnSelect = new System.Windows.Forms.Button();
            panelDbType = new System.Windows.Forms.Panel();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbSave = new System.Windows.Forms.ToolStripButton();
            tssSave = new System.Windows.Forms.ToolStripSeparator();
            tsbAdd = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tsbEdit = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            tsbDelete = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            tsbClear = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            tsbManageProfile = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            tsbManageVisibility = new System.Windows.Forms.ToolStripButton();
            panelInfo = new System.Windows.Forms.Panel();
            label2 = new System.Windows.Forms.Label();
            picInfo = new System.Windows.Forms.PictureBox();
            colCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colServer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colIntegratedSecurity = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colUserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDatabase = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPriority = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dgvDbConnection).BeginInit();
            panelDbType.SuspendLayout();
            toolStrip1.SuspendLayout();
            panelInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picInfo).BeginInit();
            SuspendLayout();
            // 
            // dgvDbConnection
            // 
            dgvDbConnection.AllowDrop = true;
            dgvDbConnection.AllowUserToAddRows = false;
            dgvDbConnection.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvDbConnection.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDbConnection.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colCheck, colId, colServer, colPort, colIntegratedSecurity, colUserName, colName, colDatabase, colPriority });
            dgvDbConnection.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            dgvDbConnection.Location = new System.Drawing.Point(5, 70);
            dgvDbConnection.Margin = new System.Windows.Forms.Padding(4);
            dgvDbConnection.MultiSelect = false;
            dgvDbConnection.Name = "dgvDbConnection";
            dgvDbConnection.RowHeadersVisible = false;
            dgvDbConnection.RowHeadersWidth = 20;
            dgvDbConnection.RowTemplate.Height = 23;
            dgvDbConnection.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvDbConnection.Size = new System.Drawing.Size(798, 380);
            dgvDbConnection.TabIndex = 20;
            dgvDbConnection.DataBindingComplete += dgvDbConnection_DataBindingComplete;
            dgvDbConnection.DragDrop += dgvDbConnection_DragDrop;
            dgvDbConnection.DragOver += dgvDbConnection_DragOver;
            dgvDbConnection.DoubleClick += dgvDbConnection_DoubleClick;
            dgvDbConnection.MouseDown += dgvDbConnection_MouseDown;
            dgvDbConnection.MouseMove += dgvDbConnection_MouseMove;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(6, 8);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(98, 17);
            label1.TabIndex = 24;
            label1.Text = "Database Type:";
            // 
            // cboDbType
            // 
            cboDbType.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDbType.FormattingEnabled = true;
            cboDbType.Location = new System.Drawing.Point(119, 4);
            cboDbType.Margin = new System.Windows.Forms.Padding(4);
            cboDbType.Name = "cboDbType";
            cboDbType.Size = new System.Drawing.Size(106, 25);
            cboDbType.TabIndex = 37;
            cboDbType.SelectedIndexChanged += cboDbType_SelectedIndexChanged;
            // 
            // btnSelect
            // 
            btnSelect.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnSelect.Location = new System.Drawing.Point(715, 455);
            btnSelect.Margin = new System.Windows.Forms.Padding(4);
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new System.Drawing.Size(88, 33);
            btnSelect.TabIndex = 39;
            btnSelect.Text = "Select";
            btnSelect.UseVisualStyleBackColor = true;
            btnSelect.Visible = false;
            btnSelect.Click += btnSelect_Click;
            // 
            // panelDbType
            // 
            panelDbType.Controls.Add(label1);
            panelDbType.Controls.Add(cboDbType);
            panelDbType.Location = new System.Drawing.Point(5, 29);
            panelDbType.Margin = new System.Windows.Forms.Padding(4);
            panelDbType.Name = "panelDbType";
            panelDbType.Size = new System.Drawing.Size(422, 34);
            panelDbType.TabIndex = 41;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbSave, tssSave, tsbAdd, toolStripSeparator1, tsbEdit, toolStripSeparator2, tsbDelete, toolStripSeparator3, tsbClear, toolStripSeparator4, tsbManageProfile, toolStripSeparator5, tsbManageVisibility });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(808, 28);
            toolStrip1.TabIndex = 42;
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
            tsbSave.Size = new System.Drawing.Size(23, 25);
            tsbSave.Text = "Save";
            tsbSave.Click += tsbSave_Click;
            // 
            // tssSave
            // 
            tssSave.Name = "tssSave";
            tssSave.Size = new System.Drawing.Size(6, 28);
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
            // tsbManageProfile
            // 
            tsbManageProfile.AutoSize = false;
            tsbManageProfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbManageProfile.Image = (System.Drawing.Image)resources.GetObject("tsbManageProfile.Image");
            tsbManageProfile.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbManageProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbManageProfile.Name = "tsbManageProfile";
            tsbManageProfile.Size = new System.Drawing.Size(25, 25);
            tsbManageProfile.Text = "Manage Profiles";
            tsbManageProfile.Click += tsbManageProfile_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbManageVisibility
            // 
            tsbManageVisibility.AutoSize = false;
            tsbManageVisibility.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbManageVisibility.Image = (System.Drawing.Image)resources.GetObject("tsbManageVisibility.Image");
            tsbManageVisibility.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbManageVisibility.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbManageVisibility.Name = "tsbManageVisibility";
            tsbManageVisibility.Size = new System.Drawing.Size(25, 25);
            tsbManageVisibility.Text = "Manage Visibility";
            tsbManageVisibility.Click += tsbManageVisibility_Click;
            // 
            // panelInfo
            // 
            panelInfo.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            panelInfo.Controls.Add(label2);
            panelInfo.Controls.Add(picInfo);
            panelInfo.Location = new System.Drawing.Point(5, 463);
            panelInfo.Name = "panelInfo";
            panelInfo.Size = new System.Drawing.Size(371, 29);
            panelInfo.TabIndex = 43;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(24, 5);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(326, 17);
            label2.TabIndex = 47;
            label2.Text = "You can drag and drop the row to reorder the record.";
            // 
            // picInfo
            // 
            picInfo.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            picInfo.Location = new System.Drawing.Point(2, 5);
            picInfo.Name = "picInfo";
            picInfo.Size = new System.Drawing.Size(20, 20);
            picInfo.TabIndex = 46;
            picInfo.TabStop = false;
            // 
            // colCheck
            // 
            colCheck.HeaderText = "";
            colCheck.Name = "colCheck";
            colCheck.Resizable = System.Windows.Forms.DataGridViewTriState.True;
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
            // colIntegratedSecurity
            // 
            colIntegratedSecurity.DataPropertyName = "IntegratedSecurity";
            colIntegratedSecurity.HeaderText = "Integrated Security";
            colIntegratedSecurity.Name = "colIntegratedSecurity";
            colIntegratedSecurity.ReadOnly = true;
            colIntegratedSecurity.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colIntegratedSecurity.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            colIntegratedSecurity.Width = 150;
            // 
            // colUserName
            // 
            colUserName.DataPropertyName = "UserId";
            colUserName.HeaderText = "User Name";
            colUserName.Name = "colUserName";
            colUserName.ReadOnly = true;
            colUserName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colUserName.Width = 150;
            // 
            // colName
            // 
            colName.HeaderText = "Name";
            colName.Name = "colName";
            colName.ReadOnly = true;
            colName.Visible = false;
            colName.Width = 150;
            // 
            // colDatabase
            // 
            colDatabase.HeaderText = "File Path";
            colDatabase.Name = "colDatabase";
            colDatabase.ReadOnly = true;
            colDatabase.Visible = false;
            colDatabase.Width = 400;
            // 
            // colPriority
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            colPriority.DefaultCellStyle = dataGridViewCellStyle1;
            colPriority.HeaderText = "Priority";
            colPriority.Name = "colPriority";
            colPriority.ReadOnly = true;
            // 
            // frmDbConnectionManage
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(808, 494);
            Controls.Add(panelInfo);
            Controls.Add(toolStrip1);
            Controls.Add(panelDbType);
            Controls.Add(btnSelect);
            Controls.Add(dgvDbConnection);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmDbConnectionManage";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Connection Manage";
            Load += frmDbConnectionManage_Load;
            ((System.ComponentModel.ISupportInitialize)dgvDbConnection).EndInit();
            panelDbType.ResumeLayout(false);
            panelDbType.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panelInfo.ResumeLayout(false);
            panelInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picInfo).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.DataGridView dgvDbConnection;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboDbType;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Panel panelDbType;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbAdd;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbEdit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton tsbDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton tsbClear;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripSeparator tssSave;
        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox picInfo;
        private System.Windows.Forms.ToolStripButton tsbManageProfile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton tsbManageVisibility;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServer;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPort;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIntegratedSecurity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDatabase;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPriority;
    }
}