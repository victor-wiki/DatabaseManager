namespace DatabaseManager.Controls
{
    partial class UC_DbObjectsNavigator
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_DbObjectsNavigator));
            this.cboDbType = new System.Windows.Forms.ComboBox();
            this.cboAccount = new System.Windows.Forms.ComboBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnAddAccount = new System.Windows.Forms.Button();
            this.tvDbObjects = new DatabaseManager.Controls.UC_DbObjectsComplexTree();
            this.SuspendLayout();
            // 
            // cboDbType
            // 
            this.cboDbType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDbType.FormattingEnabled = true;
            this.cboDbType.Location = new System.Drawing.Point(3, 3);
            this.cboDbType.Name = "cboDbType";
            this.cboDbType.Size = new System.Drawing.Size(185, 20);
            this.cboDbType.TabIndex = 36;
            this.cboDbType.SelectedIndexChanged += new System.EventHandler(this.cboDbType_SelectedIndexChanged);
            // 
            // cboAccount
            // 
            this.cboAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAccount.FormattingEnabled = true;
            this.cboAccount.Location = new System.Drawing.Point(3, 29);
            this.cboAccount.Name = "cboAccount";
            this.cboAccount.Size = new System.Drawing.Size(185, 20);
            this.cboAccount.TabIndex = 40;
            // 
            // btnConnect
            // 
            this.btnConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConnect.Enabled = false;
            this.btnConnect.Image = global::DatabaseManager.Properties.Resources.DbConnect16;
            this.btnConnect.Location = new System.Drawing.Point(190, 28);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(31, 23);
            this.btnConnect.TabIndex = 45;
            this.btnConnect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnAddAccount
            // 
            this.btnAddAccount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddAccount.Image = ((System.Drawing.Image)(resources.GetObject("btnAddAccount.Image")));
            this.btnAddAccount.Location = new System.Drawing.Point(190, 2);
            this.btnAddAccount.Name = "btnAddAccount";
            this.btnAddAccount.Size = new System.Drawing.Size(31, 22);
            this.btnAddAccount.TabIndex = 39;
            this.btnAddAccount.UseVisualStyleBackColor = true;
            this.btnAddAccount.Click += new System.EventHandler(this.btnAddAccount_Click);
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvDbObjects.Location = new System.Drawing.Point(3, 55);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(218, 274);
            this.tvDbObjects.TabIndex = 46;
            // 
            // UC_DbObjectsNavigator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvDbObjects);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.cboAccount);
            this.Controls.Add(this.btnAddAccount);
            this.Controls.Add(this.cboDbType);
            this.Name = "UC_DbObjectsNavigator";
            this.Size = new System.Drawing.Size(225, 329);
            this.Load += new System.EventHandler(this.UC_DbObjectsNavigator_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cboDbType;
        private System.Windows.Forms.Button btnAddAccount;
        private System.Windows.Forms.ComboBox cboAccount;
        private System.Windows.Forms.Button btnConnect;
        private UC_DbObjectsComplexTree tvDbObjects;
    }
}
