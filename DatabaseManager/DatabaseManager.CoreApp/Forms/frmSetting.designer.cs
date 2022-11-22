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
            this.btnConfirm = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabMySql = new System.Windows.Forms.TabPage();
            this.txtMySqlCharsetCollation = new System.Windows.Forms.TextBox();
            this.txtMySqlCharset = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblMySqlCharset = new System.Windows.Forms.Label();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.chkValidateScriptsAfterTranslated = new System.Windows.Forms.CheckBox();
            this.btnSelectTargetDatabaseTypesForConcatChar = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.lblOutputFolder = new System.Windows.Forms.Label();
            this.btnOutputFolder = new System.Windows.Forms.Button();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.chkEditorEnableIntellisence = new System.Windows.Forms.CheckBox();
            this.chkEnableEditorHighlighting = new System.Windows.Forms.CheckBox();
            this.txtLockPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.chkRememberPasswordDuringSession = new System.Windows.Forms.CheckBox();
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
            this.label2 = new System.Windows.Forms.Label();
            this.numDataBatchSize = new System.Windows.Forms.NumericUpDown();
            this.lblCommandTimeout = new System.Windows.Forms.Label();
            this.numCommandTimeout = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPostgres = new System.Windows.Forms.TabPage();
            this.chkExcludePostgresExtensionObjects = new System.Windows.Forms.CheckBox();
            this.dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.tabMySql.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDataBatchSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCommandTimeout)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPostgres.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConfirm
            // 
            this.btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnConfirm.Location = new System.Drawing.Point(176, 527);
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
            this.btnCancel.Location = new System.Drawing.Point(297, 527);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 33);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabMySql
            // 
            this.tabMySql.BackColor = System.Drawing.SystemColors.Control;
            this.tabMySql.Controls.Add(this.txtMySqlCharsetCollation);
            this.tabMySql.Controls.Add(this.txtMySqlCharset);
            this.tabMySql.Controls.Add(this.label3);
            this.tabMySql.Controls.Add(this.lblMySqlCharset);
            this.tabMySql.Location = new System.Drawing.Point(4, 26);
            this.tabMySql.Margin = new System.Windows.Forms.Padding(4);
            this.tabMySql.Name = "tabMySql";
            this.tabMySql.Padding = new System.Windows.Forms.Padding(4);
            this.tabMySql.Size = new System.Drawing.Size(509, 483);
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
            // txtMySqlCharset
            // 
            this.txtMySqlCharset.Location = new System.Drawing.Point(156, 27);
            this.txtMySqlCharset.Margin = new System.Windows.Forms.Padding(4);
            this.txtMySqlCharset.Name = "txtMySqlCharset";
            this.txtMySqlCharset.Size = new System.Drawing.Size(116, 23);
            this.txtMySqlCharset.TabIndex = 1;
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
            // tabGeneral
            // 
            this.tabGeneral.BackColor = System.Drawing.SystemColors.Control;
            this.tabGeneral.Controls.Add(this.chkValidateScriptsAfterTranslated);
            this.tabGeneral.Controls.Add(this.btnSelectTargetDatabaseTypesForConcatChar);
            this.tabGeneral.Controls.Add(this.label6);
            this.tabGeneral.Controls.Add(this.lblOutputFolder);
            this.tabGeneral.Controls.Add(this.btnOutputFolder);
            this.tabGeneral.Controls.Add(this.txtOutputFolder);
            this.tabGeneral.Controls.Add(this.chkEditorEnableIntellisence);
            this.tabGeneral.Controls.Add(this.chkEnableEditorHighlighting);
            this.tabGeneral.Controls.Add(this.txtLockPassword);
            this.tabGeneral.Controls.Add(this.lblPassword);
            this.tabGeneral.Controls.Add(this.chkRememberPasswordDuringSession);
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
            this.tabGeneral.Size = new System.Drawing.Size(509, 483);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            // 
            // chkValidateScriptsAfterTranslated
            // 
            this.chkValidateScriptsAfterTranslated.AutoSize = true;
            this.chkValidateScriptsAfterTranslated.Location = new System.Drawing.Point(13, 293);
            this.chkValidateScriptsAfterTranslated.Name = "chkValidateScriptsAfterTranslated";
            this.chkValidateScriptsAfterTranslated.Size = new System.Drawing.Size(209, 21);
            this.chkValidateScriptsAfterTranslated.TabIndex = 38;
            this.chkValidateScriptsAfterTranslated.Text = "Validate scripts after translated";
            this.chkValidateScriptsAfterTranslated.UseVisualStyleBackColor = true;
            // 
            // btnSelectTargetDatabaseTypesForConcatChar
            // 
            this.btnSelectTargetDatabaseTypesForConcatChar.Location = new System.Drawing.Point(265, 321);
            this.btnSelectTargetDatabaseTypesForConcatChar.Name = "btnSelectTargetDatabaseTypesForConcatChar";
            this.btnSelectTargetDatabaseTypesForConcatChar.Size = new System.Drawing.Size(75, 26);
            this.btnSelectTargetDatabaseTypesForConcatChar.TabIndex = 37;
            this.btnSelectTargetDatabaseTypesForConcatChar.Text = "Set";
            this.btnSelectTargetDatabaseTypesForConcatChar.UseVisualStyleBackColor = true;
            this.btnSelectTargetDatabaseTypesForConcatChar.Click += new System.EventHandler(this.btnSelectTargetDatabaseTypesForConcatChar_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 324);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(248, 17);
            this.label6.TabIndex = 36;
            this.label6.Text = "Convert concat char if target database is:";
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.Location = new System.Drawing.Point(10, 455);
            this.lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(131, 17);
            this.lblOutputFolder.TabIndex = 34;
            this.lblOutputFolder.Text = "Scripts output folder:";
            // 
            // btnOutputFolder
            // 
            this.btnOutputFolder.Location = new System.Drawing.Point(433, 449);
            this.btnOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            this.btnOutputFolder.Name = "btnOutputFolder";
            this.btnOutputFolder.Size = new System.Drawing.Size(42, 24);
            this.btnOutputFolder.TabIndex = 33;
            this.btnOutputFolder.Text = "...";
            this.btnOutputFolder.UseVisualStyleBackColor = true;
            this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Location = new System.Drawing.Point(149, 450);
            this.txtOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(277, 23);
            this.txtOutputFolder.TabIndex = 32;
            // 
            // chkEditorEnableIntellisence
            // 
            this.chkEditorEnableIntellisence.AutoSize = true;
            this.chkEditorEnableIntellisence.Checked = true;
            this.chkEditorEnableIntellisence.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEditorEnableIntellisence.Location = new System.Drawing.Point(248, 262);
            this.chkEditorEnableIntellisence.Margin = new System.Windows.Forms.Padding(4);
            this.chkEditorEnableIntellisence.Name = "chkEditorEnableIntellisence";
            this.chkEditorEnableIntellisence.Size = new System.Drawing.Size(214, 21);
            this.chkEditorEnableIntellisence.TabIndex = 31;
            this.chkEditorEnableIntellisence.Text = "Enable intellisence for sql editor";
            this.chkEditorEnableIntellisence.UseVisualStyleBackColor = true;
            // 
            // chkEnableEditorHighlighting
            // 
            this.chkEnableEditorHighlighting.AutoSize = true;
            this.chkEnableEditorHighlighting.Checked = true;
            this.chkEnableEditorHighlighting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableEditorHighlighting.Location = new System.Drawing.Point(13, 262);
            this.chkEnableEditorHighlighting.Margin = new System.Windows.Forms.Padding(4);
            this.chkEnableEditorHighlighting.Name = "chkEnableEditorHighlighting";
            this.chkEnableEditorHighlighting.Size = new System.Drawing.Size(219, 21);
            this.chkEnableEditorHighlighting.TabIndex = 30;
            this.chkEnableEditorHighlighting.Text = "Enable highlighting for sql editor";
            this.chkEnableEditorHighlighting.UseVisualStyleBackColor = true;
            // 
            // txtLockPassword
            // 
            this.txtLockPassword.Location = new System.Drawing.Point(115, 419);
            this.txtLockPassword.Margin = new System.Windows.Forms.Padding(4);
            this.txtLockPassword.Name = "txtLockPassword";
            this.txtLockPassword.PasswordChar = '*';
            this.txtLockPassword.Size = new System.Drawing.Size(164, 23);
            this.txtLockPassword.TabIndex = 28;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(10, 422);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(99, 17);
            this.lblPassword.TabIndex = 27;
            this.lblPassword.Text = "Lock password:";
            // 
            // chkRememberPasswordDuringSession
            // 
            this.chkRememberPasswordDuringSession.AutoSize = true;
            this.chkRememberPasswordDuringSession.Checked = true;
            this.chkRememberPasswordDuringSession.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRememberPasswordDuringSession.Location = new System.Drawing.Point(13, 388);
            this.chkRememberPasswordDuringSession.Margin = new System.Windows.Forms.Padding(4);
            this.chkRememberPasswordDuringSession.Name = "chkRememberPasswordDuringSession";
            this.chkRememberPasswordDuringSession.Size = new System.Drawing.Size(495, 21);
            this.chkRememberPasswordDuringSession.TabIndex = 26;
            this.chkRememberPasswordDuringSession.Text = "Remember password during application session(store it in memory temporarily)";
            this.chkRememberPasswordDuringSession.UseVisualStyleBackColor = true;
            // 
            // chkUseOriginalDataTypeIfUdtHasOnlyOneAttr
            // 
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.AutoSize = true;
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Checked = true;
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Location = new System.Drawing.Point(13, 166);
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
            this.cboPreferredDatabase.Location = new System.Drawing.Point(149, 355);
            this.cboPreferredDatabase.Margin = new System.Windows.Forms.Padding(4);
            this.cboPreferredDatabase.Name = "cboPreferredDatabase";
            this.cboPreferredDatabase.Size = new System.Drawing.Size(126, 25);
            this.cboPreferredDatabase.TabIndex = 24;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 359);
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
            this.chkLogError.Location = new System.Drawing.Point(294, 228);
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
            this.chkLogInfo.Location = new System.Drawing.Point(194, 228);
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
            this.lblLogType.Location = new System.Drawing.Point(117, 229);
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
            this.chkNotCreateIfExists.Location = new System.Drawing.Point(13, 197);
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
            this.chkEnableLog.Location = new System.Drawing.Point(13, 228);
            this.chkEnableLog.Margin = new System.Windows.Forms.Padding(4);
            this.chkEnableLog.Name = "chkEnableLog";
            this.chkEnableLog.Size = new System.Drawing.Size(89, 21);
            this.chkEnableLog.TabIndex = 15;
            this.chkEnableLog.Text = "Enable log";
            this.chkEnableLog.UseVisualStyleBackColor = true;
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
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabMySql);
            this.tabControl1.Controls.Add(this.tabPostgres);
            this.tabControl1.Location = new System.Drawing.Point(1, 3);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(517, 513);
            this.tabControl1.TabIndex = 15;
            // 
            // tabPostgres
            // 
            this.tabPostgres.BackColor = System.Drawing.SystemColors.Control;
            this.tabPostgres.Controls.Add(this.chkExcludePostgresExtensionObjects);
            this.tabPostgres.Location = new System.Drawing.Point(4, 26);
            this.tabPostgres.Name = "tabPostgres";
            this.tabPostgres.Padding = new System.Windows.Forms.Padding(3);
            this.tabPostgres.Size = new System.Drawing.Size(509, 483);
            this.tabPostgres.TabIndex = 2;
            this.tabPostgres.Text = "Postgres";
            // 
            // chkExcludePostgresExtensionObjects
            // 
            this.chkExcludePostgresExtensionObjects.AutoSize = true;
            this.chkExcludePostgresExtensionObjects.Checked = true;
            this.chkExcludePostgresExtensionObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExcludePostgresExtensionObjects.Location = new System.Drawing.Point(18, 12);
            this.chkExcludePostgresExtensionObjects.Name = "chkExcludePostgresExtensionObjects";
            this.chkExcludePostgresExtensionObjects.Size = new System.Drawing.Size(176, 21);
            this.chkExcludePostgresExtensionObjects.TabIndex = 0;
            this.chkExcludePostgresExtensionObjects.Text = "Exclude extension objects";
            this.chkExcludePostgresExtensionObjects.UseVisualStyleBackColor = true;
            // 
            // frmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(523, 574);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Setting";
            this.Load += new System.EventHandler(this.frmSetting_Load);
            this.tabMySql.ResumeLayout(false);
            this.tabMySql.PerformLayout();
            this.tabGeneral.ResumeLayout(false);
            this.tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDataBatchSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCommandTimeout)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPostgres.ResumeLayout(false);
            this.tabPostgres.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabPage tabMySql;
        private System.Windows.Forms.TextBox txtMySqlCharsetCollation;
        private System.Windows.Forms.TextBox txtMySqlCharset;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblMySqlCharset;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.CheckBox chkEnableEditorHighlighting;
        private System.Windows.Forms.TextBox txtLockPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.CheckBox chkRememberPasswordDuringSession;
        private System.Windows.Forms.CheckBox chkUseOriginalDataTypeIfUdtHasOnlyOneAttr;
        private System.Windows.Forms.ComboBox cboPreferredDatabase;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkLogError;
        private System.Windows.Forms.CheckBox chkLogInfo;
        private System.Windows.Forms.Label lblLogType;
        private System.Windows.Forms.ComboBox cboDbObjectNameMode;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkNotCreateIfExists;
        private System.Windows.Forms.CheckBox chkShowBuiltinDatabase;
        private System.Windows.Forms.CheckBox chkEnableLog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numDataBatchSize;
        private System.Windows.Forms.Label lblCommandTimeout;
        private System.Windows.Forms.NumericUpDown numCommandTimeout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.CheckBox chkEditorEnableIntellisence;
        private System.Windows.Forms.TabPage tabPostgres;
        private System.Windows.Forms.CheckBox chkExcludePostgresExtensionObjects;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.Button btnOutputFolder;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSelectTargetDatabaseTypesForConcatChar;
        private System.Windows.Forms.CheckBox chkValidateScriptsAfterTranslated;
    }
}