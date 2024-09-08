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
            btnConfirm = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            tabMySql = new System.Windows.Forms.TabPage();
            txtMySqlCharsetCollation = new System.Windows.Forms.TextBox();
            txtMySqlCharset = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            lblMySqlCharset = new System.Windows.Forms.Label();
            tabGeneral = new System.Windows.Forms.TabPage();
            chkValidateScriptsAfterTranslated = new System.Windows.Forms.CheckBox();
            btnSelectTargetDatabaseTypesForConcatChar = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            lblOutputFolder = new System.Windows.Forms.Label();
            btnOutputFolder = new System.Windows.Forms.Button();
            txtOutputFolder = new System.Windows.Forms.TextBox();
            chkEditorEnableIntellisence = new System.Windows.Forms.CheckBox();
            chkEnableEditorHighlighting = new System.Windows.Forms.CheckBox();
            txtLockPassword = new System.Windows.Forms.TextBox();
            lblPassword = new System.Windows.Forms.Label();
            chkRememberPasswordDuringSession = new System.Windows.Forms.CheckBox();
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr = new System.Windows.Forms.CheckBox();
            cboPreferredDatabase = new System.Windows.Forms.ComboBox();
            label5 = new System.Windows.Forms.Label();
            chkLogError = new System.Windows.Forms.CheckBox();
            chkLogInfo = new System.Windows.Forms.CheckBox();
            lblLogType = new System.Windows.Forms.Label();
            cboDbObjectNameMode = new System.Windows.Forms.ComboBox();
            label4 = new System.Windows.Forms.Label();
            chkNotCreateIfExists = new System.Windows.Forms.CheckBox();
            chkShowBuiltinDatabase = new System.Windows.Forms.CheckBox();
            chkEnableLog = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            numDataBatchSize = new System.Windows.Forms.NumericUpDown();
            lblCommandTimeout = new System.Windows.Forms.Label();
            numCommandTimeout = new System.Windows.Forms.NumericUpDown();
            label1 = new System.Windows.Forms.Label();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabApperance = new System.Windows.Forms.TabPage();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label8 = new System.Windows.Forms.Label();
            numTextEditorFontSize = new System.Windows.Forms.NumericUpDown();
            cboTextEditorFontSize = new System.Windows.Forms.Label();
            cboTextEditorFontName = new System.Windows.Forms.ComboBox();
            chkShowTextEditorLineNumber = new System.Windows.Forms.CheckBox();
            label7 = new System.Windows.Forms.Label();
            tabPostgres = new System.Windows.Forms.TabPage();
            chkExcludePostgresExtensionObjects = new System.Windows.Forms.CheckBox();
            dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            groupBox2 = new System.Windows.Forms.GroupBox();
            cboThemeType = new System.Windows.Forms.ComboBox();
            label9 = new System.Windows.Forms.Label();
            tabMySql.SuspendLayout();
            tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numDataBatchSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCommandTimeout).BeginInit();
            tabControl1.SuspendLayout();
            tabApperance.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTextEditorFontSize).BeginInit();
            tabPostgres.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // btnConfirm
            // 
            btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            btnConfirm.Location = new System.Drawing.Point(176, 527);
            btnConfirm.Margin = new System.Windows.Forms.Padding(4);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new System.Drawing.Size(88, 33);
            btnConfirm.TabIndex = 10;
            btnConfirm.Text = "OK";
            btnConfirm.UseVisualStyleBackColor = true;
            btnConfirm.Click += btnConfirm_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(297, 527);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 33);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabMySql
            // 
            tabMySql.BackColor = System.Drawing.SystemColors.Control;
            tabMySql.Controls.Add(txtMySqlCharsetCollation);
            tabMySql.Controls.Add(txtMySqlCharset);
            tabMySql.Controls.Add(label3);
            tabMySql.Controls.Add(lblMySqlCharset);
            tabMySql.Location = new System.Drawing.Point(4, 26);
            tabMySql.Margin = new System.Windows.Forms.Padding(4);
            tabMySql.Name = "tabMySql";
            tabMySql.Padding = new System.Windows.Forms.Padding(4);
            tabMySql.Size = new System.Drawing.Size(509, 483);
            tabMySql.TabIndex = 1;
            tabMySql.Text = "MySql";
            // 
            // txtMySqlCharsetCollation
            // 
            txtMySqlCharsetCollation.Location = new System.Drawing.Point(156, 71);
            txtMySqlCharsetCollation.Margin = new System.Windows.Forms.Padding(4);
            txtMySqlCharsetCollation.Name = "txtMySqlCharsetCollation";
            txtMySqlCharsetCollation.Size = new System.Drawing.Size(116, 23);
            txtMySqlCharsetCollation.TabIndex = 3;
            // 
            // txtMySqlCharset
            // 
            txtMySqlCharset.Location = new System.Drawing.Point(156, 27);
            txtMySqlCharset.Margin = new System.Windows.Forms.Padding(4);
            txtMySqlCharset.Name = "txtMySqlCharset";
            txtMySqlCharset.Size = new System.Drawing.Size(116, 23);
            txtMySqlCharset.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(18, 75);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(110, 17);
            label3.TabIndex = 2;
            label3.Text = "Charset Collation:";
            // 
            // lblMySqlCharset
            // 
            lblMySqlCharset.AutoSize = true;
            lblMySqlCharset.Location = new System.Drawing.Point(18, 31);
            lblMySqlCharset.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblMySqlCharset.Name = "lblMySqlCharset";
            lblMySqlCharset.Size = new System.Drawing.Size(55, 17);
            lblMySqlCharset.TabIndex = 0;
            lblMySqlCharset.Text = "Charset:";
            // 
            // tabGeneral
            // 
            tabGeneral.BackColor = System.Drawing.SystemColors.Control;
            tabGeneral.Controls.Add(chkValidateScriptsAfterTranslated);
            tabGeneral.Controls.Add(btnSelectTargetDatabaseTypesForConcatChar);
            tabGeneral.Controls.Add(label6);
            tabGeneral.Controls.Add(lblOutputFolder);
            tabGeneral.Controls.Add(btnOutputFolder);
            tabGeneral.Controls.Add(txtOutputFolder);
            tabGeneral.Controls.Add(chkEditorEnableIntellisence);
            tabGeneral.Controls.Add(chkEnableEditorHighlighting);
            tabGeneral.Controls.Add(txtLockPassword);
            tabGeneral.Controls.Add(lblPassword);
            tabGeneral.Controls.Add(chkRememberPasswordDuringSession);
            tabGeneral.Controls.Add(chkUseOriginalDataTypeIfUdtHasOnlyOneAttr);
            tabGeneral.Controls.Add(cboPreferredDatabase);
            tabGeneral.Controls.Add(label5);
            tabGeneral.Controls.Add(chkLogError);
            tabGeneral.Controls.Add(chkLogInfo);
            tabGeneral.Controls.Add(lblLogType);
            tabGeneral.Controls.Add(cboDbObjectNameMode);
            tabGeneral.Controls.Add(label4);
            tabGeneral.Controls.Add(chkNotCreateIfExists);
            tabGeneral.Controls.Add(chkShowBuiltinDatabase);
            tabGeneral.Controls.Add(chkEnableLog);
            tabGeneral.Controls.Add(label2);
            tabGeneral.Controls.Add(numDataBatchSize);
            tabGeneral.Controls.Add(lblCommandTimeout);
            tabGeneral.Controls.Add(numCommandTimeout);
            tabGeneral.Controls.Add(label1);
            tabGeneral.Location = new System.Drawing.Point(4, 26);
            tabGeneral.Margin = new System.Windows.Forms.Padding(4);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new System.Windows.Forms.Padding(4);
            tabGeneral.Size = new System.Drawing.Size(509, 483);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "General";
            // 
            // chkValidateScriptsAfterTranslated
            // 
            chkValidateScriptsAfterTranslated.AutoSize = true;
            chkValidateScriptsAfterTranslated.Location = new System.Drawing.Point(13, 293);
            chkValidateScriptsAfterTranslated.Name = "chkValidateScriptsAfterTranslated";
            chkValidateScriptsAfterTranslated.Size = new System.Drawing.Size(209, 21);
            chkValidateScriptsAfterTranslated.TabIndex = 38;
            chkValidateScriptsAfterTranslated.Text = "Validate scripts after translated";
            chkValidateScriptsAfterTranslated.UseVisualStyleBackColor = true;
            // 
            // btnSelectTargetDatabaseTypesForConcatChar
            // 
            btnSelectTargetDatabaseTypesForConcatChar.Location = new System.Drawing.Point(265, 321);
            btnSelectTargetDatabaseTypesForConcatChar.Name = "btnSelectTargetDatabaseTypesForConcatChar";
            btnSelectTargetDatabaseTypesForConcatChar.Size = new System.Drawing.Size(75, 26);
            btnSelectTargetDatabaseTypesForConcatChar.TabIndex = 37;
            btnSelectTargetDatabaseTypesForConcatChar.Text = "Set";
            btnSelectTargetDatabaseTypesForConcatChar.UseVisualStyleBackColor = true;
            btnSelectTargetDatabaseTypesForConcatChar.Click += btnSelectTargetDatabaseTypesForConcatChar_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(11, 324);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(248, 17);
            label6.TabIndex = 36;
            label6.Text = "Convert concat char if target database is:";
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Location = new System.Drawing.Point(10, 455);
            lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new System.Drawing.Size(131, 17);
            lblOutputFolder.TabIndex = 34;
            lblOutputFolder.Text = "Scripts output folder:";
            // 
            // btnOutputFolder
            // 
            btnOutputFolder.Location = new System.Drawing.Point(433, 449);
            btnOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            btnOutputFolder.Name = "btnOutputFolder";
            btnOutputFolder.Size = new System.Drawing.Size(42, 24);
            btnOutputFolder.TabIndex = 33;
            btnOutputFolder.Text = "...";
            btnOutputFolder.UseVisualStyleBackColor = true;
            btnOutputFolder.Click += btnOutputFolder_Click;
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.Location = new System.Drawing.Point(149, 450);
            txtOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.Size = new System.Drawing.Size(277, 23);
            txtOutputFolder.TabIndex = 32;
            // 
            // chkEditorEnableIntellisence
            // 
            chkEditorEnableIntellisence.AutoSize = true;
            chkEditorEnableIntellisence.Checked = true;
            chkEditorEnableIntellisence.CheckState = System.Windows.Forms.CheckState.Checked;
            chkEditorEnableIntellisence.Location = new System.Drawing.Point(248, 262);
            chkEditorEnableIntellisence.Margin = new System.Windows.Forms.Padding(4);
            chkEditorEnableIntellisence.Name = "chkEditorEnableIntellisence";
            chkEditorEnableIntellisence.Size = new System.Drawing.Size(214, 21);
            chkEditorEnableIntellisence.TabIndex = 31;
            chkEditorEnableIntellisence.Text = "Enable intellisence for sql editor";
            chkEditorEnableIntellisence.UseVisualStyleBackColor = true;
            // 
            // chkEnableEditorHighlighting
            // 
            chkEnableEditorHighlighting.AutoSize = true;
            chkEnableEditorHighlighting.Checked = true;
            chkEnableEditorHighlighting.CheckState = System.Windows.Forms.CheckState.Checked;
            chkEnableEditorHighlighting.Location = new System.Drawing.Point(13, 262);
            chkEnableEditorHighlighting.Margin = new System.Windows.Forms.Padding(4);
            chkEnableEditorHighlighting.Name = "chkEnableEditorHighlighting";
            chkEnableEditorHighlighting.Size = new System.Drawing.Size(219, 21);
            chkEnableEditorHighlighting.TabIndex = 30;
            chkEnableEditorHighlighting.Text = "Enable highlighting for sql editor";
            chkEnableEditorHighlighting.UseVisualStyleBackColor = true;
            // 
            // txtLockPassword
            // 
            txtLockPassword.Location = new System.Drawing.Point(115, 419);
            txtLockPassword.Margin = new System.Windows.Forms.Padding(4);
            txtLockPassword.Name = "txtLockPassword";
            txtLockPassword.PasswordChar = '*';
            txtLockPassword.Size = new System.Drawing.Size(164, 23);
            txtLockPassword.TabIndex = 28;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new System.Drawing.Point(10, 422);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(99, 17);
            lblPassword.TabIndex = 27;
            lblPassword.Text = "Lock password:";
            // 
            // chkRememberPasswordDuringSession
            // 
            chkRememberPasswordDuringSession.AutoSize = true;
            chkRememberPasswordDuringSession.Checked = true;
            chkRememberPasswordDuringSession.CheckState = System.Windows.Forms.CheckState.Checked;
            chkRememberPasswordDuringSession.Location = new System.Drawing.Point(13, 388);
            chkRememberPasswordDuringSession.Margin = new System.Windows.Forms.Padding(4);
            chkRememberPasswordDuringSession.Name = "chkRememberPasswordDuringSession";
            chkRememberPasswordDuringSession.Size = new System.Drawing.Size(495, 21);
            chkRememberPasswordDuringSession.TabIndex = 26;
            chkRememberPasswordDuringSession.Text = "Remember password during application session(store it in memory temporarily)";
            chkRememberPasswordDuringSession.UseVisualStyleBackColor = true;
            // 
            // chkUseOriginalDataTypeIfUdtHasOnlyOneAttr
            // 
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.AutoSize = true;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Checked = true;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.CheckState = System.Windows.Forms.CheckState.Checked;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Location = new System.Drawing.Point(13, 166);
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Margin = new System.Windows.Forms.Padding(4);
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Name = "chkUseOriginalDataTypeIfUdtHasOnlyOneAttr";
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Size = new System.Drawing.Size(404, 21);
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.TabIndex = 25;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Text = "Use original data type if user defined type has only one attribute";
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.UseVisualStyleBackColor = true;
            // 
            // cboPreferredDatabase
            // 
            cboPreferredDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboPreferredDatabase.FormattingEnabled = true;
            cboPreferredDatabase.Location = new System.Drawing.Point(149, 355);
            cboPreferredDatabase.Margin = new System.Windows.Forms.Padding(4);
            cboPreferredDatabase.Name = "cboPreferredDatabase";
            cboPreferredDatabase.Size = new System.Drawing.Size(126, 25);
            cboPreferredDatabase.TabIndex = 24;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(11, 359);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(124, 17);
            label5.TabIndex = 23;
            label5.Text = "Preferred database:";
            // 
            // chkLogError
            // 
            chkLogError.AutoSize = true;
            chkLogError.Checked = true;
            chkLogError.CheckState = System.Windows.Forms.CheckState.Checked;
            chkLogError.Location = new System.Drawing.Point(294, 228);
            chkLogError.Margin = new System.Windows.Forms.Padding(4);
            chkLogError.Name = "chkLogError";
            chkLogError.Size = new System.Drawing.Size(57, 21);
            chkLogError.TabIndex = 22;
            chkLogError.Text = "Error";
            chkLogError.UseVisualStyleBackColor = true;
            // 
            // chkLogInfo
            // 
            chkLogInfo.AutoSize = true;
            chkLogInfo.Checked = true;
            chkLogInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            chkLogInfo.Location = new System.Drawing.Point(194, 228);
            chkLogInfo.Margin = new System.Windows.Forms.Padding(4);
            chkLogInfo.Name = "chkLogInfo";
            chkLogInfo.Size = new System.Drawing.Size(90, 21);
            chkLogInfo.TabIndex = 21;
            chkLogInfo.Text = "Infomation";
            chkLogInfo.UseVisualStyleBackColor = true;
            // 
            // lblLogType
            // 
            lblLogType.AutoSize = true;
            lblLogType.Location = new System.Drawing.Point(117, 229);
            lblLogType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLogType.Name = "lblLogType";
            lblLogType.Size = new System.Drawing.Size(62, 17);
            lblLogType.TabIndex = 20;
            lblLogType.Text = "Log type:";
            // 
            // cboDbObjectNameMode
            // 
            cboDbObjectNameMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDbObjectNameMode.FormattingEnabled = true;
            cboDbObjectNameMode.Location = new System.Drawing.Point(204, 94);
            cboDbObjectNameMode.Margin = new System.Windows.Forms.Padding(4);
            cboDbObjectNameMode.Name = "cboDbObjectNameMode";
            cboDbObjectNameMode.Size = new System.Drawing.Size(138, 25);
            cboDbObjectNameMode.TabIndex = 19;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(10, 99);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(180, 17);
            label4.TabIndex = 18;
            label4.Text = "Database object name mode:";
            // 
            // chkNotCreateIfExists
            // 
            chkNotCreateIfExists.AutoSize = true;
            chkNotCreateIfExists.Location = new System.Drawing.Point(13, 197);
            chkNotCreateIfExists.Margin = new System.Windows.Forms.Padding(4);
            chkNotCreateIfExists.Name = "chkNotCreateIfExists";
            chkNotCreateIfExists.Size = new System.Drawing.Size(176, 21);
            chkNotCreateIfExists.TabIndex = 17;
            chkNotCreateIfExists.Text = "Not create if object exists";
            chkNotCreateIfExists.UseVisualStyleBackColor = true;
            // 
            // chkShowBuiltinDatabase
            // 
            chkShowBuiltinDatabase.AutoSize = true;
            chkShowBuiltinDatabase.Location = new System.Drawing.Point(13, 135);
            chkShowBuiltinDatabase.Margin = new System.Windows.Forms.Padding(4);
            chkShowBuiltinDatabase.Name = "chkShowBuiltinDatabase";
            chkShowBuiltinDatabase.Size = new System.Drawing.Size(155, 21);
            chkShowBuiltinDatabase.TabIndex = 16;
            chkShowBuiltinDatabase.Text = "Show builtin database";
            chkShowBuiltinDatabase.UseVisualStyleBackColor = true;
            // 
            // chkEnableLog
            // 
            chkEnableLog.AutoSize = true;
            chkEnableLog.Location = new System.Drawing.Point(13, 228);
            chkEnableLog.Margin = new System.Windows.Forms.Padding(4);
            chkEnableLog.Name = "chkEnableLog";
            chkEnableLog.Size = new System.Drawing.Size(89, 21);
            chkEnableLog.TabIndex = 15;
            chkEnableLog.Text = "Enable log";
            chkEnableLog.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(10, 58);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(100, 17);
            label2.TabIndex = 13;
            label2.Text = "Data batch size:";
            // 
            // numDataBatchSize
            // 
            numDataBatchSize.Location = new System.Drawing.Point(135, 55);
            numDataBatchSize.Margin = new System.Windows.Forms.Padding(4);
            numDataBatchSize.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numDataBatchSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numDataBatchSize.Name = "numDataBatchSize";
            numDataBatchSize.Size = new System.Drawing.Size(106, 23);
            numDataBatchSize.TabIndex = 14;
            numDataBatchSize.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // lblCommandTimeout
            // 
            lblCommandTimeout.AutoSize = true;
            lblCommandTimeout.Location = new System.Drawing.Point(10, 17);
            lblCommandTimeout.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblCommandTimeout.Name = "lblCommandTimeout";
            lblCommandTimeout.Size = new System.Drawing.Size(119, 17);
            lblCommandTimeout.TabIndex = 0;
            lblCommandTimeout.Text = "Command timeout:";
            // 
            // numCommandTimeout
            // 
            numCommandTimeout.Location = new System.Drawing.Point(135, 14);
            numCommandTimeout.Margin = new System.Windows.Forms.Padding(4);
            numCommandTimeout.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numCommandTimeout.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCommandTimeout.Name = "numCommandTimeout";
            numCommandTimeout.Size = new System.Drawing.Size(106, 23);
            numCommandTimeout.TabIndex = 1;
            numCommandTimeout.Value = new decimal(new int[] { 600, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(248, 17);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(58, 17);
            label1.TabIndex = 12;
            label1.Text = "(second)";
            // 
            // tabControl1
            // 
            tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl1.Controls.Add(tabGeneral);
            tabControl1.Controls.Add(tabApperance);
            tabControl1.Controls.Add(tabMySql);
            tabControl1.Controls.Add(tabPostgres);
            tabControl1.Location = new System.Drawing.Point(1, 3);
            tabControl1.Margin = new System.Windows.Forms.Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(517, 513);
            tabControl1.TabIndex = 15;
            // 
            // tabApperance
            // 
            tabApperance.BackColor = System.Drawing.SystemColors.Control;
            tabApperance.Controls.Add(groupBox2);
            tabApperance.Controls.Add(groupBox1);
            tabApperance.Location = new System.Drawing.Point(4, 26);
            tabApperance.Name = "tabApperance";
            tabApperance.Size = new System.Drawing.Size(509, 483);
            tabApperance.TabIndex = 3;
            tabApperance.Text = "Apperance";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(numTextEditorFontSize);
            groupBox1.Controls.Add(cboTextEditorFontSize);
            groupBox1.Controls.Add(cboTextEditorFontName);
            groupBox1.Controls.Add(chkShowTextEditorLineNumber);
            groupBox1.Controls.Add(label7);
            groupBox1.Location = new System.Drawing.Point(6, 83);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(500, 120);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Text Editor";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(228, 86);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(20, 17);
            label8.TabIndex = 8;
            label8.Text = "pt";
            // 
            // numTextEditorFontSize
            // 
            numTextEditorFontSize.Location = new System.Drawing.Point(83, 84);
            numTextEditorFontSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numTextEditorFontSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numTextEditorFontSize.Name = "numTextEditorFontSize";
            numTextEditorFontSize.Size = new System.Drawing.Size(139, 23);
            numTextEditorFontSize.TabIndex = 7;
            numTextEditorFontSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // cboTextEditorFontSize
            // 
            cboTextEditorFontSize.AutoSize = true;
            cboTextEditorFontSize.Location = new System.Drawing.Point(6, 88);
            cboTextEditorFontSize.Name = "cboTextEditorFontSize";
            cboTextEditorFontSize.Size = new System.Drawing.Size(62, 17);
            cboTextEditorFontSize.TabIndex = 6;
            cboTextEditorFontSize.Text = "Font size:";
            // 
            // cboTextEditorFontName
            // 
            cboTextEditorFontName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboTextEditorFontName.FormattingEnabled = true;
            cboTextEditorFontName.Location = new System.Drawing.Point(83, 48);
            cboTextEditorFontName.Name = "cboTextEditorFontName";
            cboTextEditorFontName.Size = new System.Drawing.Size(139, 25);
            cboTextEditorFontName.TabIndex = 3;
            // 
            // chkShowTextEditorLineNumber
            // 
            chkShowTextEditorLineNumber.AutoSize = true;
            chkShowTextEditorLineNumber.Checked = true;
            chkShowTextEditorLineNumber.CheckState = System.Windows.Forms.CheckState.Checked;
            chkShowTextEditorLineNumber.Location = new System.Drawing.Point(6, 22);
            chkShowTextEditorLineNumber.Name = "chkShowTextEditorLineNumber";
            chkShowTextEditorLineNumber.Size = new System.Drawing.Size(131, 21);
            chkShowTextEditorLineNumber.TabIndex = 2;
            chkShowTextEditorLineNumber.Text = "Show line number";
            chkShowTextEditorLineNumber.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(5, 51);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(72, 17);
            label7.TabIndex = 0;
            label7.Text = "Font name:";
            // 
            // tabPostgres
            // 
            tabPostgres.BackColor = System.Drawing.SystemColors.Control;
            tabPostgres.Controls.Add(chkExcludePostgresExtensionObjects);
            tabPostgres.Location = new System.Drawing.Point(4, 26);
            tabPostgres.Name = "tabPostgres";
            tabPostgres.Padding = new System.Windows.Forms.Padding(3);
            tabPostgres.Size = new System.Drawing.Size(509, 483);
            tabPostgres.TabIndex = 2;
            tabPostgres.Text = "Postgres";
            // 
            // chkExcludePostgresExtensionObjects
            // 
            chkExcludePostgresExtensionObjects.AutoSize = true;
            chkExcludePostgresExtensionObjects.Checked = true;
            chkExcludePostgresExtensionObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            chkExcludePostgresExtensionObjects.Location = new System.Drawing.Point(18, 12);
            chkExcludePostgresExtensionObjects.Name = "chkExcludePostgresExtensionObjects";
            chkExcludePostgresExtensionObjects.Size = new System.Drawing.Size(176, 21);
            chkExcludePostgresExtensionObjects.TabIndex = 0;
            chkExcludePostgresExtensionObjects.Text = "Exclude extension objects";
            chkExcludePostgresExtensionObjects.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(cboThemeType);
            groupBox2.Controls.Add(label9);
            groupBox2.Location = new System.Drawing.Point(6, 8);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(500, 69);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Theme";
            // 
            // cboThemeType
            // 
            cboThemeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboThemeType.FormattingEnabled = true;
            cboThemeType.Location = new System.Drawing.Point(84, 29);
            cboThemeType.Name = "cboThemeType";
            cboThemeType.Size = new System.Drawing.Size(139, 25);
            cboThemeType.TabIndex = 5;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(6, 32);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(39, 17);
            label9.TabIndex = 4;
            label9.Text = "Type:";
            // 
            // frmSetting
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(523, 574);
            Controls.Add(tabControl1);
            Controls.Add(btnCancel);
            Controls.Add(btnConfirm);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmSetting";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Setting";
            Load += frmSetting_Load;
            tabMySql.ResumeLayout(false);
            tabMySql.PerformLayout();
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numDataBatchSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCommandTimeout).EndInit();
            tabControl1.ResumeLayout(false);
            tabApperance.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numTextEditorFontSize).EndInit();
            tabPostgres.ResumeLayout(false);
            tabPostgres.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.TabPage tabApperance;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox chkShowTextEditorLineNumber;
        private System.Windows.Forms.NumericUpDown numTextEditorFontSize;
        private System.Windows.Forms.Label cboTextEditorFontSize;
        private System.Windows.Forms.ComboBox cboTextEditorFontName;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cboThemeType;
        private System.Windows.Forms.Label label9;
    }
}