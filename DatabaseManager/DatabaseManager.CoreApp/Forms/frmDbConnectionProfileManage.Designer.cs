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
            this.dgvDbConnectionProfile = new System.Windows.Forms.DataGridView();
            this.colId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colServer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPort = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDatabase = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelOperation = new System.Windows.Forms.Panel();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDbConnectionProfile)).BeginInit();
            this.panelOperation.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvDbConnectionProfile
            // 
            this.dgvDbConnectionProfile.AllowUserToAddRows = false;
            this.dgvDbConnectionProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDbConnectionProfile.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDbConnectionProfile.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colId,
            this.colName,
            this.colServer,
            this.colPort,
            this.colDatabase});
            this.dgvDbConnectionProfile.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgvDbConnectionProfile.Location = new System.Drawing.Point(5, 3);
            this.dgvDbConnectionProfile.Margin = new System.Windows.Forms.Padding(4);
            this.dgvDbConnectionProfile.Name = "dgvDbConnectionProfile";
            this.dgvDbConnectionProfile.ReadOnly = true;
            this.dgvDbConnectionProfile.RowHeadersVisible = false;
            this.dgvDbConnectionProfile.RowHeadersWidth = 20;
            this.dgvDbConnectionProfile.RowTemplate.Height = 23;
            this.dgvDbConnectionProfile.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDbConnectionProfile.Size = new System.Drawing.Size(762, 335);
            this.dgvDbConnectionProfile.TabIndex = 21;
            // 
            // colId
            // 
            this.colId.DataPropertyName = "Id";
            this.colId.HeaderText = "Id";
            this.colId.Name = "colId";
            this.colId.ReadOnly = true;
            this.colId.Visible = false;
            // 
            // colName
            // 
            this.colName.HeaderText = "Profile Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Width = 250;
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
            // colDatabase
            // 
            this.colDatabase.HeaderText = "Database";
            this.colDatabase.Name = "colDatabase";
            this.colDatabase.ReadOnly = true;
            this.colDatabase.Width = 150;
            // 
            // panelOperation
            // 
            this.panelOperation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelOperation.Controls.Add(this.btnDelete);
            this.panelOperation.Controls.Add(this.btnClear);
            this.panelOperation.Location = new System.Drawing.Point(5, 346);
            this.panelOperation.Margin = new System.Windows.Forms.Padding(4);
            this.panelOperation.Name = "panelOperation";
            this.panelOperation.Size = new System.Drawing.Size(225, 44);
            this.panelOperation.TabIndex = 42;
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Location = new System.Drawing.Point(8, 4);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(88, 33);
            this.btnDelete.TabIndex = 22;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(123, 4);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(88, 33);
            this.btnClear.TabIndex = 23;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(679, 350);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 33);
            this.btnClose.TabIndex = 41;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // frmDbConnectionProfileManage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(773, 403);
            this.Controls.Add(this.panelOperation);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dgvDbConnectionProfile);
            this.Name = "frmDbConnectionProfileManage";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Connection Profile Manage";
            this.Load += new System.EventHandler(this.frmDbConnectionProfile_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDbConnectionProfile)).EndInit();
            this.panelOperation.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDbConnectionProfile;
        private System.Windows.Forms.Panel panelOperation;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.DataGridViewTextBoxColumn colId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colServer;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPort;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDatabase;
    }
}