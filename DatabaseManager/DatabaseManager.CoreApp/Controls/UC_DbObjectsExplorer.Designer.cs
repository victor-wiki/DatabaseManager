namespace DatabaseManager.Controls
{
    partial class UC_DbObjectsExplorer
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_DbObjectsExplorer));
            cboDbType = new System.Windows.Forms.ComboBox();
            cboAccount = new System.Windows.Forms.ComboBox();
            btnConnect = new System.Windows.Forms.Button();
            btnAddAccount = new System.Windows.Forms.Button();
            tvDbObjects = new UC_DbObjectsComplexTree();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            SuspendLayout();
            // 
            // cboDbType
            // 
            cboDbType.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboDbType.BackColor = System.Drawing.Color.White;
            cboDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDbType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cboDbType.FormattingEnabled = true;
            cboDbType.Location = new System.Drawing.Point(4, 4);
            cboDbType.Margin = new System.Windows.Forms.Padding(4);
            cboDbType.Name = "cboDbType";
            cboDbType.Size = new System.Drawing.Size(215, 25);
            cboDbType.TabIndex = 36;
            cboDbType.SelectedIndexChanged += cboDbType_SelectedIndexChanged;
            // 
            // cboAccount
            // 
            cboAccount.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboAccount.BackColor = System.Drawing.Color.White;
            cboAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboAccount.DropDownWidth = 215;
            cboAccount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cboAccount.FormattingEnabled = true;
            cboAccount.Location = new System.Drawing.Point(4, 35);
            cboAccount.Margin = new System.Windows.Forms.Padding(4);
            cboAccount.MaxDropDownItems = 100;
            cboAccount.Name = "cboAccount";
            cboAccount.Size = new System.Drawing.Size(215, 25);
            cboAccount.TabIndex = 40;
            // 
            // btnConnect
            // 
            btnConnect.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnConnect.Enabled = false;
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnConnect.Image = (System.Drawing.Image)resources.GetObject("btnConnect.Image");
            btnConnect.Location = new System.Drawing.Point(222, 37);
            btnConnect.Margin = new System.Windows.Forms.Padding(4);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(36, 22);
            btnConnect.TabIndex = 45;
            btnConnect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnAddAccount
            // 
            btnAddAccount.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnAddAccount.FlatAppearance.BorderSize = 0;
            btnAddAccount.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnAddAccount.Location = new System.Drawing.Point(222, 4);
            btnAddAccount.Margin = new System.Windows.Forms.Padding(4);
            btnAddAccount.Name = "btnAddAccount";
            btnAddAccount.Size = new System.Drawing.Size(36, 25);
            btnAddAccount.TabIndex = 39;
            btnAddAccount.UseVisualStyleBackColor = true;
            btnAddAccount.Click += btnAddAccount_Click;
            // 
            // tvDbObjects
            // 
            tvDbObjects.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tvDbObjects.Location = new System.Drawing.Point(4, 66);
            tvDbObjects.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tvDbObjects.Name = "tvDbObjects";
            tvDbObjects.Size = new System.Drawing.Size(254, 400);
            tvDbObjects.TabIndex = 46;
            // 
            // UC_DbObjectsExplorer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tvDbObjects);
            Controls.Add(btnConnect);
            Controls.Add(cboAccount);
            Controls.Add(btnAddAccount);
            Controls.Add(cboDbType);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "UC_DbObjectsExplorer";
            Size = new System.Drawing.Size(262, 466);
            Load += UC_DbObjectsNavigator_Load;
            SizeChanged += UC_DbObjectsNavigator_SizeChanged;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ComboBox cboDbType;
        private System.Windows.Forms.Button btnAddAccount;
        private System.Windows.Forms.ComboBox cboAccount;
        private System.Windows.Forms.Button btnConnect;
        private UC_DbObjectsComplexTree tvDbObjects;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
