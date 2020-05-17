namespace DatabaseManager.Controls
{
    partial class UC_DbAccountInfo
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
            this.txtPort = new System.Windows.Forms.TextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.chkRememberPassword = new System.Windows.Forms.CheckBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUserId = new System.Windows.Forms.TextBox();
            this.lblUserId = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.cboAuthentication = new System.Windows.Forms.ComboBox();
            this.lblAuthentication = new System.Windows.Forms.Label();
            this.lblServerName = new System.Windows.Forms.Label();
            this.cboServer = new System.Windows.Forms.ComboBox();
            this.chkAsDba = new System.Windows.Forms.CheckBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.chkUseSsl = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(324, 7);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(49, 21);
            this.txtPort.TabIndex = 2;
            this.txtPort.Visible = false;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(288, 10);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(35, 12);
            this.lblPort.TabIndex = 28;
            this.lblPort.Text = "Port:";
            this.lblPort.Visible = false;
            // 
            // chkRememberPassword
            // 
            this.chkRememberPassword.AutoSize = true;
            this.chkRememberPassword.Location = new System.Drawing.Point(118, 141);
            this.chkRememberPassword.Name = "chkRememberPassword";
            this.chkRememberPassword.Size = new System.Drawing.Size(126, 16);
            this.chkRememberPassword.TabIndex = 6;
            this.chkRememberPassword.Text = "Remember password";
            this.chkRememberPassword.UseVisualStyleBackColor = true;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(118, 109);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(166, 21);
            this.txtPassword.TabIndex = 5;
            // 
            // txtUserId
            // 
            this.txtUserId.Location = new System.Drawing.Point(118, 78);
            this.txtUserId.Name = "txtUserId";
            this.txtUserId.Size = new System.Drawing.Size(166, 21);
            this.txtUserId.TabIndex = 4;
            // 
            // lblUserId
            // 
            this.lblUserId.AutoSize = true;
            this.lblUserId.Location = new System.Drawing.Point(10, 81);
            this.lblUserId.Name = "lblUserId";
            this.lblUserId.Size = new System.Drawing.Size(65, 12);
            this.lblUserId.TabIndex = 26;
            this.lblUserId.Text = "User name:";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(10, 112);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(59, 12);
            this.lblPassword.TabIndex = 24;
            this.lblPassword.Text = "Password:";
            // 
            // cboAuthentication
            // 
            this.cboAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboAuthentication.FormattingEnabled = true;
            this.cboAuthentication.Location = new System.Drawing.Point(118, 43);
            this.cboAuthentication.Name = "cboAuthentication";
            this.cboAuthentication.Size = new System.Drawing.Size(166, 20);
            this.cboAuthentication.TabIndex = 3;
            this.cboAuthentication.SelectedIndexChanged += new System.EventHandler(this.cboAuthentication_SelectedIndexChanged);
            // 
            // lblAuthentication
            // 
            this.lblAuthentication.AutoSize = true;
            this.lblAuthentication.Location = new System.Drawing.Point(10, 46);
            this.lblAuthentication.Name = "lblAuthentication";
            this.lblAuthentication.Size = new System.Drawing.Size(95, 12);
            this.lblAuthentication.TabIndex = 21;
            this.lblAuthentication.Text = "Authentication:";
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = true;
            this.lblServerName.Location = new System.Drawing.Point(10, 10);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Size = new System.Drawing.Size(77, 12);
            this.lblServerName.TabIndex = 18;
            this.lblServerName.Text = "Server name:";
            // 
            // cboServer
            // 
            this.cboServer.FormattingEnabled = true;
            this.cboServer.Location = new System.Drawing.Point(118, 7);
            this.cboServer.Name = "cboServer";
            this.cboServer.Size = new System.Drawing.Size(166, 20);
            this.cboServer.TabIndex = 1;
            // 
            // chkAsDba
            // 
            this.chkAsDba.AutoSize = true;
            this.chkAsDba.Location = new System.Drawing.Point(288, 46);
            this.chkAsDba.Name = "chkAsDba";
            this.chkAsDba.Size = new System.Drawing.Size(78, 16);
            this.chkAsDba.TabIndex = 30;
            this.chkAsDba.Text = "as sysdba";
            this.chkAsDba.UseVisualStyleBackColor = true;
            this.chkAsDba.Visible = false;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(288, 108);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 31;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // chkUseSsl
            // 
            this.chkUseSsl.AutoSize = true;
            this.chkUseSsl.Location = new System.Drawing.Point(250, 141);
            this.chkUseSsl.Name = "chkUseSsl";
            this.chkUseSsl.Size = new System.Drawing.Size(66, 16);
            this.chkUseSsl.TabIndex = 32;
            this.chkUseSsl.Text = "Use ssl";
            this.chkUseSsl.UseVisualStyleBackColor = true;
            this.chkUseSsl.Visible = false;
            // 
            // UC_DbAccountInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkUseSsl);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.chkAsDba);
            this.Controls.Add(this.cboServer);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.chkRememberPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUserId);
            this.Controls.Add(this.lblUserId);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.cboAuthentication);
            this.Controls.Add(this.lblAuthentication);
            this.Controls.Add(this.lblServerName);
            this.Name = "UC_DbAccountInfo";
            this.Size = new System.Drawing.Size(389, 163);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.CheckBox chkRememberPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUserId;
        private System.Windows.Forms.Label lblUserId;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.ComboBox cboAuthentication;
        private System.Windows.Forms.Label lblAuthentication;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.ComboBox cboServer;
        private System.Windows.Forms.CheckBox chkAsDba;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.CheckBox chkUseSsl;
    }
}
