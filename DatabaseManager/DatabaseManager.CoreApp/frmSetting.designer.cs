namespace DatabaseManager
{
    partial class frmSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetting));
            this.lblCommandTimeout = new System.Windows.Forms.Label();
            this.numCommandTimeout = new System.Windows.Forms.NumericUpDown();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numDataBatchSize = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr = new System.Windows.Forms.CheckBox();
            this.cboPreferredDatabase = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chkLogError = new System.Windows.Forms.CheckBox();
            this.chkLogInfo = new System.Windows.Forms.CheckBox();
            this.lblLogType = new System.Windows.Forms.Label();
            this.cboDbObjectNameMode = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkNotCreateIfExists = new System.Windows.Forms.CheckBox();
            this.chkShowBuiltinDatabase = new System.Windows.Forms.CheckBox();
            this.chkEnableLog = new System.Windows.Forms.CheckBox();
            this.tabMySql = new System.Windows.Forms.TabPage();
            this.txtMySqlCharsetCollation = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtMySqlCharset = new System.Windows.Forms.TextBox();
            this.lblMySqlCharset = new System.Windows.Forms.Label();
            this.tabOracle = new System.Windows.Forms.TabPage();
            this.cboOracleGeometryType = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numCommandTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDataBatchSize)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabMySql.SuspendLayout();
            this.tabOracle.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCommandTimeout
            // 
            this.lblCommandTimeout.AutoSize = true;
            this.lblCommandTimeout.Location = new System.Drawing.Point(10, 17);
            this.lblCommandTimeout.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCommandTimeout.Name = "lblCommandTimeout";
            this.lblCommandTimeout.Size = new System.Drawing.Size(119, 17);
            this.lblCommandTimeout.TabIndex = 0;
            this.lblCommandTimeout.Text = "Command timeout:";
            // 
            // numCommandTimeout
            // 
            this.numCommandTimeout.Location = new System.Drawing.Point(135, 14);
            this.numCommandTimeout.Margin = new System.Windows.Forms.Padding(4);
            this.numCommandTimeout.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numCommandTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numCommandTimeout.Name = "numCommandTimeout";
            this.numCommandTimeout.Size = new System.Drawing.Size(106, 23);
            this.numCommandTimeout.TabIndex = 1;
            this.numCommandTimeout.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            // 
            // btnConfirm
            // 
            this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConfirm.Location = new System.Drawing.Point(168, 381);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(4);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(88, 33);
            this.btnConfirm.TabIndex = 10;
            this.btnConfirm.Text = "OK";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(289, 381);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 33);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(248, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 17);
            this.label1.TabIndex = 12;
            this.label1.Text = "(second)";
            // 
            // numDataBatchSize
            // 
            this.numDataBatchSize.Location = new System.Drawing.Point(135, 55);
            this.numDataBatchSize.Margin = new System.Windows.Forms.Padding(4);
            this.numDataBatchSize.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numDataBatchSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numDataBatchSize.Name = "numDataBatchSize";
            this.numDataBatchSize.Size = new System.Drawing.Size(106, 23);
            this.numDataBatchSize.TabIndex = 14;
            this.numDataBatchSize.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 58);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 17);
            this.label2.TabIndex = 13;
            this.label2.Text = "Data batch size:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabMySql);
            this.tabControl1.Controls.Add(this.tabOracle);
            this.tabControl1.Location = new System.Drawing.Point(1, 3);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(501, 367);
            this.tabControl1.TabIndex = 15;
            // 
            // tabGeneral
            // 
            this.tabGeneral.BackColor = System.Drawing.SystemColors.Control;
            this.tabGeneral.Controls.Add(this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr);
            this.tabGeneral.Controls.Add(this.cboPreferredDatabase);
            this.tabGeneral.Controls.Add(this.label5);
            this.tabGeneral.Controls.Add(this.chkLogError);
            this.tabGeneral.Controls.Add(this.chkLogInfo);
            this.tabGeneral.Controls.Add(this.lblLogType);
            this.tabGeneral.Controls.Add(this.cboDbObjectNameMode);
            this.tabGeneral.Controls.Add(this.label4);
            this.tabGeneral.Controls.Add(this.chkNotCreateIfExists);
            this.tabGeneral.Controls.Add(this.chkShowBuiltinDatabase);
            this.tabGeneral.Controls.Add(this.chkEnableLog);
            this.tabGeneral.Controls.Add(this.label2);
            this.tabGeneral.Controls.Add(this.numDataBatchSize);
            this.tabGeneral.Controls.Add(this.lblCommandTimeout);
            this.tabGeneral.Controls.Add(this.numCommandTimeout);
            this.tabGeneral.Controls.Add(this.label1);
            this.tabGeneral.Location = new System.Drawing.Point(4, 26);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(4);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(4);
            this.tabGeneral.Size = new System.Drawing.Size(493, 337);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            // 
            // chkUseOriginalDataTypeIfUdtHasOnlyOneAttr
            // 
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.AutoSize = true;
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Checked = true;
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Location = new System.Drawing.Point(13, 164);
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Margin = new System.Windows.Forms.Padding(4);
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Name = "chkUseOriginalDataTypeIfUdtHasOnlyOneAttr";
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Size = new System.Drawing.Size(404, 21);
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.TabIndex = 25;
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Text = "Use original data type if user defined type has only one attribute";
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.UseVisualStyleBackColor = true;
            // 
            // cboPreferredDatabase
            // 
            this.cboPreferredDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPreferredDatabase.FormattingEnabled = true;
            this.cboPreferredDatabase.Location = new System.Drawing.Point(154, 295);
            this.cboPreferredDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.cboPreferredDatabase.Name = "cboPreferredDatabase";
            this.cboPreferredDatabase.Size = new System.Drawing.Size(126, 25);
            this.cboPreferredDatabase.TabIndex = 24;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 299);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(124, 17);
            this.label5.TabIndex = 23;
            this.label5.Text = "Preferred database:";
            // 
            // chkLogError
            // 
            this.chkLogError.AutoSize = true;
            this.chkLogError.Checked = true;
            this.chkLogError.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogError.Location = new System.Drawing.Point(206, 259);
            this.chkLogError.Margin = new System.Windows.Forms.Padding(4);
            this.chkLogError.Name = "chkLogError";
            this.chkLogError.Size = new System.Drawing.Size(57, 21);
            this.chkLogError.TabIndex = 22;
            this.chkLogError.Text = "Error";
            this.chkLogError.UseVisualStyleBackColor = true;
            // 
            // chkLogInfo
            // 
            this.chkLogInfo.AutoSize = true;
            this.chkLogInfo.Checked = true;
            this.chkLogInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLogInfo.Location = new System.Drawing.Point(92, 259);
            this.chkLogInfo.Margin = new System.Windows.Forms.Padding(4);
            this.chkLogInfo.Name = "chkLogInfo";
            this.chkLogInfo.Size = new System.Drawing.Size(90, 21);
            this.chkLogInfo.TabIndex = 21;
            this.chkLogInfo.Text = "Infomation";
            this.chkLogInfo.UseVisualStyleBackColor = true;
            // 
            // lblLogType
            // 
            this.lblLogType.AutoSize = true;
            this.lblLogType.Location = new System.Drawing.Point(11, 259);
            this.lblLogType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblLogType.Name = "lblLogType";
            this.lblLogType.Size = new System.Drawing.Size(62, 17);
            this.lblLogType.TabIndex = 20;
            this.lblLogType.Text = "Log type:";
            // 
            // cboDbObjectNameMode
            // 
            this.cboDbObjectNameMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDbObjectNameMode.FormattingEnabled = true;
            this.cboDbObjectNameMode.Location = new System.Drawing.Point(204, 94);
            this.cboDbObjectNameMode.Margin = new System.Windows.Forms.Padding(4);
            this.cboDbObjectNameMode.Name = "cboDbObjectNameMode";
            this.cboDbObjectNameMode.Size = new System.Drawing.Size(138, 25);
            this.cboDbObjectNameMode.TabIndex = 19;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 99);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(180, 17);
            this.label4.TabIndex = 18;
            this.label4.Text = "Database object name mode:";
            // 
            // chkNotCreateIfExists
            // 
            this.chkNotCreateIfExists.AutoSize = true;
            this.chkNotCreateIfExists.Location = new System.Drawing.Point(13, 192);
            this.chkNotCreateIfExists.Margin = new System.Windows.Forms.Padding(4);
            this.chkNotCreateIfExists.Name = "chkNotCreateIfExists";
            this.chkNotCreateIfExists.Size = new System.Drawing.Size(176, 21);
            this.chkNotCreateIfExists.TabIndex = 17;
            this.chkNotCreateIfExists.Text = "Not create if object exists";
            this.chkNotCreateIfExists.UseVisualStyleBackColor = true;
            // 
            // chkShowBuiltinDatabase
            // 
            this.chkShowBuiltinDatabase.AutoSize = true;
            this.chkShowBuiltinDatabase.Location = new System.Drawing.Point(13, 135);
            this.chkShowBuiltinDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.chkShowBuiltinDatabase.Name = "chkShowBuiltinDatabase";
            this.chkShowBuiltinDatabase.Size = new System.Drawing.Size(155, 21);
            this.chkShowBuiltinDatabase.TabIndex = 16;
            this.chkShowBuiltinDatabase.Text = "Show builtin database";
            this.chkShowBuiltinDatabase.UseVisualStyleBackColor = true;
            // 
            // chkEnableLog
            // 
            this.chkEnableLog.AutoSize = true;
            this.chkEnableLog.Location = new System.Drawing.Point(13, 225);
            this.chkEnableLog.Margin = new System.Windows.Forms.Padding(4);
            this.chkEnableLog.Name = "chkEnableLog";
            this.chkEnableLog.Size = new System.Drawing.Size(89, 21);
            this.chkEnableLog.TabIndex = 15;
            this.chkEnableLog.Text = "Enable log";
            this.chkEnableLog.UseVisualStyleBackColor = true;
            // 
            // tabMySql
            // 
            this.tabMySql.BackColor = System.Drawing.SystemColors.Control;
            this.tabMySql.Controls.Add(this.txtMySqlCharsetCollation);
            this.tabMySql.Controls.Add(this.label3);
            this.tabMySql.Controls.Add(this.txtMySqlCharset);
            this.tabMySql.Controls.Add(this.lblMySqlCharset);
            this.tabMySql.Location = new System.Drawing.Point(4, 26);
            this.tabMySql.Margin = new System.Windows.Forms.Padding(4);
            this.tabMySql.Name = "tabMySql";
            this.tabMySql.Padding = new System.Windows.Forms.Padding(4);
            this.tabMySql.Size = new System.Drawing.Size(493, 337);
            this.tabMySql.TabIndex = 1;
            this.tabMySql.Text = "MySql";
            // 
            // txtMySqlCharsetCollation
            // 
            this.txtMySqlCharsetCollation.Location = new System.Drawing.Point(156, 71);
            this.txtMySqlCharsetCollation.Margin = new System.Windows.Forms.Padding(4);
            this.txtMySqlCharsetCollation.Name = "txtMySqlCharsetCollation";
            this.txtMySqlCharsetCollation.Size = new System.Drawing.Size(116, 23);
            this.txtMySqlCharsetCollation.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 75);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Charset Collation:";
            // 
            // txtMySqlCharset
            // 
            this.txtMySqlCharset.Location = new System.Drawing.Point(156, 27);
            this.txtMySqlCharset.Margin = new System.Windows.Forms.Padding(4);
            this.txtMySqlCharset.Name = "txtMySqlCharset";
            this.txtMySqlCharset.Size = new System.Drawing.Size(116, 23);
            this.txtMySqlCharset.TabIndex = 1;
            // 
            // lblMySqlCharset
            // 
            this.lblMySqlCharset.AutoSize = true;
            this.lblMySqlCharset.Location = new System.Drawing.Point(18, 31);
            this.lblMySqlCharset.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMySqlCharset.Name = "lblMySqlCharset";
            this.lblMySqlCharset.Size = new System.Drawing.Size(55, 17);
            this.lblMySqlCharset.TabIndex = 0;
            this.lblMySqlCharset.Text = "Charset:";
            // 
            // tabOracle
            // 
            this.tabOracle.BackColor = System.Drawing.SystemColors.Control;
            this.tabOracle.Controls.Add(this.cboOracleGeometryType);
            this.tabOracle.Controls.Add(this.label6);
            this.tabOracle.Location = new System.Drawing.Point(4, 26);
            this.tabOracle.Name = "tabOracle";
            this.tabOracle.Padding = new System.Windows.Forms.Padding(3);
            this.tabOracle.Size = new System.Drawing.Size(493, 337);
            this.tabOracle.TabIndex = 2;
            this.tabOracle.Text = "Oracle";
            // 
            // cboOracleGeometryType
            // 
            this.cboOracleGeometryType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOracleGeometryType.FormattingEnabled = true;
            this.cboOracleGeometryType.Items.AddRange(new object[] {
            "MDSYS",
            "SDE"});
            this.cboOracleGeometryType.Location = new System.Drawing.Point(125, 11);
            this.cboOracleGeometryType.Margin = new System.Windows.Forms.Padding(4);
            this.cboOracleGeometryType.Name = "cboOracleGeometryType";
            this.cboOracleGeometryType.Size = new System.Drawing.Size(138, 25);
            this.cboOracleGeometryType.TabIndex = 20;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 14);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(100, 17);
            this.label6.TabIndex = 2;
            this.label6.Text = "Geometry Type:";
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(507, 428);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Setting";
            this.Load += new System.EventHandler(this.frmSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numCommandTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDataBatchSize)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            this.tabMySql.ResumeLayout(false);
            this.tabMySql.PerformLayout();
            this.tabOracle.ResumeLayout(false);
            this.tabOracle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblCommandTimeout;
        private System.Windows.Forms.NumericUpDown numCommandTimeout;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numDataBatchSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabMySql;
        private System.Windows.Forms.TextBox txtMySqlCharset;
        private System.Windows.Forms.Label lblMySqlCharset;
        private System.Windows.Forms.TextBox txtMySqlCharsetCollation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkEnableLog;
        private System.Windows.Forms.CheckBox chkShowBuiltinDatabase;
        private System.Windows.Forms.CheckBox chkNotCreateIfExists;
        private System.Windows.Forms.ComboBox cboDbObjectNameMode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkLogError;
        private System.Windows.Forms.CheckBox chkLogInfo;
        private System.Windows.Forms.Label lblLogType;
        private System.Windows.Forms.ComboBox cboPreferredDatabase;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabPage tabOracle;
        private System.Windows.Forms.ComboBox cboOracleGeometryType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkUseOriginalDataTypeIfUdtHasOnlyOneAttr;
    }
}