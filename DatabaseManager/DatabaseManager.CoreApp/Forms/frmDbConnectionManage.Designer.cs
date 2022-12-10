namespace DatabaseManager
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
            this.btnClose = new System.Windows.Forms.Button();
            this.dgvDbConnection = new System.Windows.Forms.DataGridView();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboDbType = new System.Windows.Forms.ComboBox();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.panelOperation = new System.Windows.Forms.Panel();
            this.panelDbType = new System.Windows.Forms.Panel();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIntegratedSecurity = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colUserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDatabase = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProfiles = new System.Windows.Forms.DataGridViewButtonColumn();
            this.colDatabaseVisibility = new System.Windows.Forms.DataGridViewButtonColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDbConnection)).BeginInit();
            this.panelOperation.SuspendLayout();
            this.panelDbType.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(773, 407);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 33);
            this.btnClose.TabIndex = 19;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // dgvDbConnection
            // 
            this.dgvDbConnection.AllowUserToAddRows = false;
            this.dgvDbConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDbConnection.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDbConnection.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colServer,
            this.colPort,
            this.colIntegratedSecurity,
            this.colUserName,
            this.colName,
            this.colDatabase,
            this.colProfiles,
            this.colDatabaseVisibility});
            this.dgvDbConnection.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvDbConnection.Location = new System.Drawing.Point(5, 45);
            this.dgvDbConnection.Margin = new System.Windows.Forms.Padding(4);
            this.dgvDbConnection.Name = "dgvDbConnection";
            this.dgvDbConnection.ReadOnly = true;
            this.dgvDbConnection.RowHeadersVisible = false;
            this.dgvDbConnection.RowHeadersWidth = 20;
            this.dgvDbConnection.RowTemplate.Height = 23;
            this.dgvDbConnection.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDbConnection.Size = new System.Drawing.Size(865, 351);
            this.dgvDbConnection.TabIndex = 20;
            this.dgvDbConnection.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDbConnection_CellContentClick);
            this.dgvDbConnection.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgvDbConnection_DataBindingComplete);
            this.dgvDbConnection.DoubleClick += new System.EventHandler(this.dgvDbConnection_DoubleClick);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(340, 4);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(88, 33);
            this.btnClear.TabIndex = 23;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(230, 4);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(88, 33);
            this.btnDelete.TabIndex = 22;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(10, 4);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(88, 33);
            this.btnAdd.TabIndex = 21;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 17);
            this.label1.TabIndex = 24;
            this.label1.Text = "Database Type:";
            // 
            // cboDbType
            // 
            this.cboDbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDbType.FormattingEnabled = true;
            this.cboDbType.Location = new System.Drawing.Point(119, 3);
            this.cboDbType.Margin = new System.Windows.Forms.Padding(4);
            this.cboDbType.Name = "cboDbType";
            this.cboDbType.Size = new System.Drawing.Size(106, 25);
            this.cboDbType.TabIndex = 37;
            this.cboDbType.SelectedIndexChanged += new System.EventHandler(this.cboDbType_SelectedIndexChanged);
            // 
            // btnEdit
            // 
            this.btnEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnEdit.Location = new System.Drawing.Point(120, 4);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(4);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(88, 33);
            this.btnEdit.TabIndex = 38;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelect.Location = new System.Drawing.Point(679, 407);
            this.btnSelect.Margin = new System.Windows.Forms.Padding(4);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(88, 33);
            this.btnSelect.TabIndex = 39;
            this.btnSelect.Text = "Select";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Visible = false;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // panelOperation
            // 
            this.panelOperation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelOperation.Controls.Add(this.btnEdit);
            this.panelOperation.Controls.Add(this.btnAdd);
            this.panelOperation.Controls.Add(this.btnDelete);
            this.panelOperation.Controls.Add(this.btnClear);
            this.panelOperation.Location = new System.Drawing.Point(5, 403);
            this.panelOperation.Margin = new System.Windows.Forms.Padding(4);
            this.panelOperation.Name = "panelOperation";
            this.panelOperation.Size = new System.Drawing.Size(446, 44);
            this.panelOperation.TabIndex = 40;
            // 
            // panelDbType
            // 
            this.panelDbType.Controls.Add(this.label1);
            this.panelDbType.Controls.Add(this.cboDbType);
            this.panelDbType.Location = new System.Drawing.Point(5, 3);
            this.panelDbType.Margin = new System.Windows.Forms.Padding(4);
            this.panelDbType.Name = "panelDbType";
            this.panelDbType.Size = new System.Drawing.Size(422, 34);
            this.panelDbType.TabIndex = 41;
            // 
            // colId
            // 
            this.colId.DataPropertyName = "Id";
            this.colId.HeaderText = "Id";
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Visible = false;
            // 
            // colServer
            // 
            this.colServer.DataPropertyName = "Server";
            this.colServer.HeaderText = "Server";
            this.colServer.Name = "colServer";
            this.colServer.ReadOnly = true;
            this.colServer.Width = 250;
            // 
            // colPort
            // 
            this.colPort.HeaderText = "Port";
            this.colPort.Name = "colPort";
            this.colPort.ReadOnly = true;
            // 
            // colIntegratedSecurity
            // 
            this.colIntegratedSecurity.DataPropertyName = "IntegratedSecurity";
            this.colIntegratedSecurity.HeaderText = "Integrated Security";
            this.colIntegratedSecurity.Name = "colIntegratedSecurity";
            this.colIntegratedSecurity.ReadOnly = true;
            this.colIntegratedSecurity.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIntegratedSecurity.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colIntegratedSecurity.Width = 150;
            // 
            // colUserName
            // 
            this.colUserName.DataPropertyName = "UserId";
            this.colUserName.HeaderText = "User Name";
            this.colUserName.Name = "colUserName";
            this.colUserName.ReadOnly = true;
            this.colUserName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colUserName.Width = 150;
            // 
            // colName
            // 
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Visible = false;
            this.colName.Width = 150;
            // 
            // colDatabase
            // 
            this.colDatabase.HeaderText = "File Path";
            this.colDatabase.Name = "colDatabase";
            this.colDatabase.ReadOnly = true;
            this.colDatabase.Visible = false;
            this.colDatabase.Width = 400;
            // 
            // colProfiles
            // 
            this.colProfiles.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colProfiles.HeaderText = "Profiles";
            this.colProfiles.Name = "colProfiles";
            this.colProfiles.ReadOnly = true;
            // 
            // colDatabaseVisibility
            // 
            this.colDatabaseVisibility.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.colDatabaseVisibility.HeaderText = "Database Visibility";
            this.colDatabaseVisibility.Name = "colDatabaseVisibility";
            this.colDatabaseVisibility.ReadOnly = true;
            // 
            // frmDbConnectionManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(875, 457);
            this.Controls.Add(this.panelDbType);
            this.Controls.Add(this.panelOperation);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.dgvDbConnection);
            this.Controls.Add(this.btnClose);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmDbConnectionManage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Connection Manage";
            this.Load += new System.EventHandler(this.frmDbConnectionManage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDbConnection)).EndInit();
            this.panelOperation.ResumeLayout(false);
            this.panelDbType.ResumeLayout(false);
            this.panelDbType.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridView dgvDbConnection;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboDbType;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Panel panelOperation;
        private System.Windows.Forms.Panel panelDbType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServer;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPort;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIntegratedSecurity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDatabase;
        private System.Windows.Forms.DataGridViewButtonColumn colProfiles;
        private System.Windows.Forms.DataGridViewButtonColumn colDatabaseVisibility;
    }
}