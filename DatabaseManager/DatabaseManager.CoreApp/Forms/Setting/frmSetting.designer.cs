namespace DatabaseManager.Forms
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
            tabDatabase = new System.Windows.Forms.TabPage();
            groupBox4 = new System.Windows.Forms.GroupBox();
            chkExcludePostgresExtensionObjects = new System.Windows.Forms.CheckBox();
            groupBox3 = new System.Windows.Forms.GroupBox();
            txtMySqlCharset = new System.Windows.Forms.TextBox();
            lblMySqlCharset = new System.Windows.Forms.Label();
            txtMySqlCharsetCollation = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            panel_Database = new System.Windows.Forms.Panel();
            lblCommandTimeout = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            numCommandTimeout = new System.Windows.Forms.NumericUpDown();
            numDataBatchSize = new System.Windows.Forms.NumericUpDown();
            label2 = new System.Windows.Forms.Label();
            chkShowBuiltinDatabase = new System.Windows.Forms.CheckBox();
            label4 = new System.Windows.Forms.Label();
            cboDbObjectNameMode = new System.Windows.Forms.ComboBox();
            tabGeneral = new System.Windows.Forms.TabPage();
            lblOutputFolder = new System.Windows.Forms.Label();
            txtLockPassword = new System.Windows.Forms.TextBox();
            txtScriptOutputFolder = new System.Windows.Forms.TextBox();
            btnScriptOutputFolder = new System.Windows.Forms.Button();
            chkRememberPasswordDuringSession = new System.Windows.Forms.CheckBox();
            lblPassword = new System.Windows.Forms.Label();
            chkEnableEditorHighlighting = new System.Windows.Forms.CheckBox();
            chkEditorEnableIntellisence = new System.Windows.Forms.CheckBox();
            chkEnableLog = new System.Windows.Forms.CheckBox();
            chkLogError = new System.Windows.Forms.CheckBox();
            lblLogType = new System.Windows.Forms.Label();
            cboPreferredDatabase = new System.Windows.Forms.ComboBox();
            chkLogInfo = new System.Windows.Forms.CheckBox();
            label5 = new System.Windows.Forms.Label();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabApperance = new System.Windows.Forms.TabPage();
            panel_Apperance = new System.Windows.Forms.Panel();
            groupBox2 = new System.Windows.Forms.GroupBox();
            cboThemeType = new System.Windows.Forms.ComboBox();
            label9 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            label8 = new System.Windows.Forms.Label();
            numTextEditorFontSize = new System.Windows.Forms.NumericUpDown();
            cboTextEditorFontSize = new System.Windows.Forms.Label();
            cboTextEditorFontName = new System.Windows.Forms.ComboBox();
            chkShowTextEditorLineNumber = new System.Windows.Forms.CheckBox();
            label7 = new System.Windows.Forms.Label();
            tabConvert = new System.Windows.Forms.TabPage();
            panel_Convert = new System.Windows.Forms.Panel();
            chkAutoMapSchema = new System.Windows.Forms.CheckBox();
            chkShowCurrentColumnContentMaxLength = new System.Windows.Forms.CheckBox();
            btnSelectTargetDatabaseTypesForConcatChar = new System.Windows.Forms.Button();
            label6 = new System.Windows.Forms.Label();
            chkNotCreateIfExists = new System.Windows.Forms.CheckBox();
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr = new System.Windows.Forms.CheckBox();
            panel_Translate = new System.Windows.Forms.Panel();
            chkValidateScriptsAfterTranslated = new System.Windows.Forms.CheckBox();
            chkNeedPreviewBeforeConvert = new System.Windows.Forms.CheckBox();
            txtCustomMappingFolder = new System.Windows.Forms.TextBox();
            btnCustomMappingFolder = new System.Windows.Forms.Button();
            label11 = new System.Windows.Forms.Label();
            btnSetDataTypeMapping = new System.Windows.Forms.Button();
            label10 = new System.Windows.Forms.Label();
            dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            tabDatabase.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox3.SuspendLayout();
            panel_Database.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCommandTimeout).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDataBatchSize).BeginInit();
            tabGeneral.SuspendLayout();
            tabControl1.SuspendLayout();
            tabApperance.SuspendLayout();
            panel_Apperance.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTextEditorFontSize).BeginInit();
            tabConvert.SuspendLayout();
            panel_Convert.SuspendLayout();
            panel_Translate.SuspendLayout();
            SuspendLayout();
            // 
            // btnConfirm
            // 
            btnConfirm.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnConfirm.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnConfirm.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            btnConfirm.Location = new System.Drawing.Point(275, 527);
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
            btnCancel.Location = new System.Drawing.Point(396, 527);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 33);
            btnCancel.TabIndex = 11;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // tabDatabase
            // 
            tabDatabase.BackColor = System.Drawing.SystemColors.Control;
            tabDatabase.Controls.Add(groupBox4);
            tabDatabase.Controls.Add(groupBox3);
            tabDatabase.Controls.Add(panel_Database);
            tabDatabase.Location = new System.Drawing.Point(4, 26);
            tabDatabase.Margin = new System.Windows.Forms.Padding(4);
            tabDatabase.Name = "tabDatabase";
            tabDatabase.Padding = new System.Windows.Forms.Padding(4);
            tabDatabase.Size = new System.Drawing.Size(705, 483);
            tabDatabase.TabIndex = 1;
            tabDatabase.Text = "Database";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(chkExcludePostgresExtensionObjects);
            groupBox4.Location = new System.Drawing.Point(7, 306);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new System.Drawing.Size(691, 85);
            groupBox4.TabIndex = 6;
            groupBox4.TabStop = false;
            groupBox4.Text = "Postgres";
            // 
            // chkExcludePostgresExtensionObjects
            // 
            chkExcludePostgresExtensionObjects.AutoSize = true;
            chkExcludePostgresExtensionObjects.Checked = true;
            chkExcludePostgresExtensionObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            chkExcludePostgresExtensionObjects.Location = new System.Drawing.Point(11, 34);
            chkExcludePostgresExtensionObjects.Name = "chkExcludePostgresExtensionObjects";
            chkExcludePostgresExtensionObjects.Size = new System.Drawing.Size(176, 21);
            chkExcludePostgresExtensionObjects.TabIndex = 1;
            chkExcludePostgresExtensionObjects.Text = "Exclude extension objects";
            chkExcludePostgresExtensionObjects.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(txtMySqlCharset);
            groupBox3.Controls.Add(lblMySqlCharset);
            groupBox3.Controls.Add(txtMySqlCharsetCollation);
            groupBox3.Controls.Add(label3);
            groupBox3.Location = new System.Drawing.Point(7, 189);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new System.Drawing.Size(691, 100);
            groupBox3.TabIndex = 5;
            groupBox3.TabStop = false;
            groupBox3.Text = "MySql";
            // 
            // txtMySqlCharset
            // 
            txtMySqlCharset.Location = new System.Drawing.Point(135, 23);
            txtMySqlCharset.Margin = new System.Windows.Forms.Padding(4);
            txtMySqlCharset.Name = "txtMySqlCharset";
            txtMySqlCharset.Size = new System.Drawing.Size(116, 23);
            txtMySqlCharset.TabIndex = 1;
            // 
            // lblMySqlCharset
            // 
            lblMySqlCharset.AutoSize = true;
            lblMySqlCharset.Location = new System.Drawing.Point(11, 27);
            lblMySqlCharset.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblMySqlCharset.Name = "lblMySqlCharset";
            lblMySqlCharset.Size = new System.Drawing.Size(55, 17);
            lblMySqlCharset.TabIndex = 0;
            lblMySqlCharset.Text = "Charset:";
            // 
            // txtMySqlCharsetCollation
            // 
            txtMySqlCharsetCollation.Location = new System.Drawing.Point(134, 67);
            txtMySqlCharsetCollation.Margin = new System.Windows.Forms.Padding(4);
            txtMySqlCharsetCollation.Name = "txtMySqlCharsetCollation";
            txtMySqlCharsetCollation.Size = new System.Drawing.Size(116, 23);
            txtMySqlCharsetCollation.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(10, 71);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(110, 17);
            label3.TabIndex = 2;
            label3.Text = "Charset Collation:";
            // 
            // panel_Database
            // 
            panel_Database.AutoScroll = true;
            panel_Database.Controls.Add(lblCommandTimeout);
            panel_Database.Controls.Add(label1);
            panel_Database.Controls.Add(numCommandTimeout);
            panel_Database.Controls.Add(numDataBatchSize);
            panel_Database.Controls.Add(label2);
            panel_Database.Controls.Add(chkShowBuiltinDatabase);
            panel_Database.Controls.Add(label4);
            panel_Database.Controls.Add(cboDbObjectNameMode);
            panel_Database.Location = new System.Drawing.Point(7, 12);
            panel_Database.Name = "panel_Database";
            panel_Database.Size = new System.Drawing.Size(514, 161);
            panel_Database.TabIndex = 4;
            // 
            // lblCommandTimeout
            // 
            lblCommandTimeout.AutoSize = true;
            lblCommandTimeout.Location = new System.Drawing.Point(9, 10);
            lblCommandTimeout.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblCommandTimeout.Name = "lblCommandTimeout";
            lblCommandTimeout.Size = new System.Drawing.Size(119, 17);
            lblCommandTimeout.TabIndex = 0;
            lblCommandTimeout.Text = "Command timeout:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(247, 10);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(58, 17);
            label1.TabIndex = 12;
            label1.Text = "(second)";
            // 
            // numCommandTimeout
            // 
            numCommandTimeout.Location = new System.Drawing.Point(134, 7);
            numCommandTimeout.Margin = new System.Windows.Forms.Padding(4);
            numCommandTimeout.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numCommandTimeout.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCommandTimeout.Name = "numCommandTimeout";
            numCommandTimeout.Size = new System.Drawing.Size(106, 23);
            numCommandTimeout.TabIndex = 1;
            numCommandTimeout.Value = new decimal(new int[] { 600, 0, 0, 0 });
            // 
            // numDataBatchSize
            // 
            numDataBatchSize.Location = new System.Drawing.Point(134, 48);
            numDataBatchSize.Margin = new System.Windows.Forms.Padding(4);
            numDataBatchSize.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            numDataBatchSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numDataBatchSize.Name = "numDataBatchSize";
            numDataBatchSize.Size = new System.Drawing.Size(106, 23);
            numDataBatchSize.TabIndex = 14;
            numDataBatchSize.Value = new decimal(new int[] { 500, 0, 0, 0 });
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(9, 51);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(100, 17);
            label2.TabIndex = 13;
            label2.Text = "Data batch size:";
            // 
            // chkShowBuiltinDatabase
            // 
            chkShowBuiltinDatabase.AutoSize = true;
            chkShowBuiltinDatabase.Location = new System.Drawing.Point(12, 128);
            chkShowBuiltinDatabase.Margin = new System.Windows.Forms.Padding(4);
            chkShowBuiltinDatabase.Name = "chkShowBuiltinDatabase";
            chkShowBuiltinDatabase.Size = new System.Drawing.Size(155, 21);
            chkShowBuiltinDatabase.TabIndex = 16;
            chkShowBuiltinDatabase.Text = "Show builtin database";
            chkShowBuiltinDatabase.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(9, 92);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(180, 17);
            label4.TabIndex = 18;
            label4.Text = "Database object name mode:";
            // 
            // cboDbObjectNameMode
            // 
            cboDbObjectNameMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDbObjectNameMode.FormattingEnabled = true;
            cboDbObjectNameMode.Location = new System.Drawing.Point(203, 87);
            cboDbObjectNameMode.Margin = new System.Windows.Forms.Padding(4);
            cboDbObjectNameMode.Name = "cboDbObjectNameMode";
            cboDbObjectNameMode.Size = new System.Drawing.Size(138, 25);
            cboDbObjectNameMode.TabIndex = 19;
            // 
            // tabGeneral
            // 
            tabGeneral.BackColor = System.Drawing.SystemColors.Control;
            tabGeneral.Controls.Add(lblOutputFolder);
            tabGeneral.Controls.Add(txtLockPassword);
            tabGeneral.Controls.Add(txtScriptOutputFolder);
            tabGeneral.Controls.Add(btnScriptOutputFolder);
            tabGeneral.Controls.Add(chkRememberPasswordDuringSession);
            tabGeneral.Controls.Add(lblPassword);
            tabGeneral.Controls.Add(chkEnableEditorHighlighting);
            tabGeneral.Controls.Add(chkEditorEnableIntellisence);
            tabGeneral.Controls.Add(chkEnableLog);
            tabGeneral.Controls.Add(chkLogError);
            tabGeneral.Controls.Add(lblLogType);
            tabGeneral.Controls.Add(cboPreferredDatabase);
            tabGeneral.Controls.Add(chkLogInfo);
            tabGeneral.Controls.Add(label5);
            tabGeneral.Location = new System.Drawing.Point(4, 26);
            tabGeneral.Margin = new System.Windows.Forms.Padding(4);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new System.Windows.Forms.Padding(4);
            tabGeneral.Size = new System.Drawing.Size(705, 483);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "General";
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Location = new System.Drawing.Point(10, 232);
            lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new System.Drawing.Size(131, 17);
            lblOutputFolder.TabIndex = 34;
            lblOutputFolder.Text = "Scripts output folder:";
            // 
            // txtLockPassword
            // 
            txtLockPassword.Location = new System.Drawing.Point(114, 196);
            txtLockPassword.Margin = new System.Windows.Forms.Padding(4);
            txtLockPassword.Name = "txtLockPassword";
            txtLockPassword.PasswordChar = '*';
            txtLockPassword.Size = new System.Drawing.Size(164, 23);
            txtLockPassword.TabIndex = 28;
            // 
            // txtScriptOutputFolder
            // 
            txtScriptOutputFolder.Location = new System.Drawing.Point(146, 229);
            txtScriptOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            txtScriptOutputFolder.Name = "txtScriptOutputFolder";
            txtScriptOutputFolder.Size = new System.Drawing.Size(277, 23);
            txtScriptOutputFolder.TabIndex = 32;
            // 
            // btnScriptOutputFolder
            // 
            btnScriptOutputFolder.Location = new System.Drawing.Point(430, 228);
            btnScriptOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            btnScriptOutputFolder.Name = "btnScriptOutputFolder";
            btnScriptOutputFolder.Size = new System.Drawing.Size(42, 24);
            btnScriptOutputFolder.TabIndex = 33;
            btnScriptOutputFolder.Text = "...";
            btnScriptOutputFolder.UseVisualStyleBackColor = true;
            // 
            // chkRememberPasswordDuringSession
            // 
            chkRememberPasswordDuringSession.AutoSize = true;
            chkRememberPasswordDuringSession.Checked = true;
            chkRememberPasswordDuringSession.CheckState = System.Windows.Forms.CheckState.Checked;
            chkRememberPasswordDuringSession.Location = new System.Drawing.Point(12, 160);
            chkRememberPasswordDuringSession.Margin = new System.Windows.Forms.Padding(4);
            chkRememberPasswordDuringSession.Name = "chkRememberPasswordDuringSession";
            chkRememberPasswordDuringSession.Size = new System.Drawing.Size(495, 21);
            chkRememberPasswordDuringSession.TabIndex = 26;
            chkRememberPasswordDuringSession.Text = "Remember password during application session(store it in memory temporarily)";
            chkRememberPasswordDuringSession.UseVisualStyleBackColor = true;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new System.Drawing.Point(10, 198);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(99, 17);
            lblPassword.TabIndex = 27;
            lblPassword.Text = "Lock password:";
            // 
            // chkEnableEditorHighlighting
            // 
            chkEnableEditorHighlighting.AutoSize = true;
            chkEnableEditorHighlighting.Checked = true;
            chkEnableEditorHighlighting.CheckState = System.Windows.Forms.CheckState.Checked;
            chkEnableEditorHighlighting.Location = new System.Drawing.Point(12, 84);
            chkEnableEditorHighlighting.Margin = new System.Windows.Forms.Padding(4);
            chkEnableEditorHighlighting.Name = "chkEnableEditorHighlighting";
            chkEnableEditorHighlighting.Size = new System.Drawing.Size(219, 21);
            chkEnableEditorHighlighting.TabIndex = 30;
            chkEnableEditorHighlighting.Text = "Enable highlighting for sql editor";
            chkEnableEditorHighlighting.UseVisualStyleBackColor = true;
            // 
            // chkEditorEnableIntellisence
            // 
            chkEditorEnableIntellisence.AutoSize = true;
            chkEditorEnableIntellisence.Checked = true;
            chkEditorEnableIntellisence.CheckState = System.Windows.Forms.CheckState.Checked;
            chkEditorEnableIntellisence.Location = new System.Drawing.Point(12, 122);
            chkEditorEnableIntellisence.Margin = new System.Windows.Forms.Padding(4);
            chkEditorEnableIntellisence.Name = "chkEditorEnableIntellisence";
            chkEditorEnableIntellisence.Size = new System.Drawing.Size(214, 21);
            chkEditorEnableIntellisence.TabIndex = 31;
            chkEditorEnableIntellisence.Text = "Enable intellisense for sql editor";
            chkEditorEnableIntellisence.UseVisualStyleBackColor = true;
            // 
            // chkEnableLog
            // 
            chkEnableLog.AutoSize = true;
            chkEnableLog.Location = new System.Drawing.Point(12, 46);
            chkEnableLog.Margin = new System.Windows.Forms.Padding(4);
            chkEnableLog.Name = "chkEnableLog";
            chkEnableLog.Size = new System.Drawing.Size(89, 21);
            chkEnableLog.TabIndex = 15;
            chkEnableLog.Text = "Enable log";
            chkEnableLog.UseVisualStyleBackColor = true;
            // 
            // chkLogError
            // 
            chkLogError.AutoSize = true;
            chkLogError.Checked = true;
            chkLogError.CheckState = System.Windows.Forms.CheckState.Checked;
            chkLogError.Location = new System.Drawing.Point(338, 48);
            chkLogError.Margin = new System.Windows.Forms.Padding(4);
            chkLogError.Name = "chkLogError";
            chkLogError.Size = new System.Drawing.Size(57, 21);
            chkLogError.TabIndex = 22;
            chkLogError.Text = "Error";
            chkLogError.UseVisualStyleBackColor = true;
            // 
            // lblLogType
            // 
            lblLogType.AutoSize = true;
            lblLogType.Location = new System.Drawing.Point(164, 48);
            lblLogType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLogType.Name = "lblLogType";
            lblLogType.Size = new System.Drawing.Size(62, 17);
            lblLogType.TabIndex = 20;
            lblLogType.Text = "Log type:";
            // 
            // cboPreferredDatabase
            // 
            cboPreferredDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboPreferredDatabase.FormattingEnabled = true;
            cboPreferredDatabase.Location = new System.Drawing.Point(146, 8);
            cboPreferredDatabase.Margin = new System.Windows.Forms.Padding(4);
            cboPreferredDatabase.Name = "cboPreferredDatabase";
            cboPreferredDatabase.Size = new System.Drawing.Size(126, 25);
            cboPreferredDatabase.TabIndex = 49;
            // 
            // chkLogInfo
            // 
            chkLogInfo.AutoSize = true;
            chkLogInfo.Checked = true;
            chkLogInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            chkLogInfo.Location = new System.Drawing.Point(240, 48);
            chkLogInfo.Margin = new System.Windows.Forms.Padding(4);
            chkLogInfo.Name = "chkLogInfo";
            chkLogInfo.Size = new System.Drawing.Size(90, 21);
            chkLogInfo.TabIndex = 21;
            chkLogInfo.Text = "Infomation";
            chkLogInfo.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(10, 12);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(124, 17);
            label5.TabIndex = 48;
            label5.Text = "Preferred database:";
            // 
            // tabControl1
            // 
            tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl1.Controls.Add(tabGeneral);
            tabControl1.Controls.Add(tabDatabase);
            tabControl1.Controls.Add(tabApperance);
            tabControl1.Controls.Add(tabConvert);
            tabControl1.Location = new System.Drawing.Point(1, 3);
            tabControl1.Margin = new System.Windows.Forms.Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(713, 513);
            tabControl1.TabIndex = 15;
            // 
            // tabApperance
            // 
            tabApperance.BackColor = System.Drawing.SystemColors.Control;
            tabApperance.Controls.Add(panel_Apperance);
            tabApperance.Location = new System.Drawing.Point(4, 26);
            tabApperance.Name = "tabApperance";
            tabApperance.Padding = new System.Windows.Forms.Padding(3);
            tabApperance.Size = new System.Drawing.Size(705, 483);
            tabApperance.TabIndex = 2;
            tabApperance.Text = "Apperance";
            // 
            // panel_Apperance
            // 
            panel_Apperance.Controls.Add(groupBox2);
            panel_Apperance.Controls.Add(groupBox1);
            panel_Apperance.Location = new System.Drawing.Point(7, 6);
            panel_Apperance.Name = "panel_Apperance";
            panel_Apperance.Size = new System.Drawing.Size(692, 471);
            panel_Apperance.TabIndex = 47;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(cboThemeType);
            groupBox2.Controls.Add(label9);
            groupBox2.Location = new System.Drawing.Point(3, 3);
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
            label9.Location = new System.Drawing.Point(8, 32);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(39, 17);
            label9.TabIndex = 4;
            label9.Text = "Type:";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label8);
            groupBox1.Controls.Add(numTextEditorFontSize);
            groupBox1.Controls.Add(cboTextEditorFontSize);
            groupBox1.Controls.Add(cboTextEditorFontName);
            groupBox1.Controls.Add(chkShowTextEditorLineNumber);
            groupBox1.Controls.Add(label7);
            groupBox1.Location = new System.Drawing.Point(3, 82);
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
            chkShowTextEditorLineNumber.Location = new System.Drawing.Point(9, 22);
            chkShowTextEditorLineNumber.Name = "chkShowTextEditorLineNumber";
            chkShowTextEditorLineNumber.Size = new System.Drawing.Size(131, 21);
            chkShowTextEditorLineNumber.TabIndex = 2;
            chkShowTextEditorLineNumber.Text = "Show line number";
            chkShowTextEditorLineNumber.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(6, 51);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(72, 17);
            label7.TabIndex = 0;
            label7.Text = "Font name:";
            // 
            // tabConvert
            // 
            tabConvert.BackColor = System.Drawing.SystemColors.Control;
            tabConvert.Controls.Add(panel_Convert);
            tabConvert.Location = new System.Drawing.Point(4, 26);
            tabConvert.Name = "tabConvert";
            tabConvert.Size = new System.Drawing.Size(705, 483);
            tabConvert.TabIndex = 3;
            tabConvert.Text = "Convert";
            // 
            // panel_Convert
            // 
            panel_Convert.Controls.Add(chkAutoMapSchema);
            panel_Convert.Controls.Add(chkShowCurrentColumnContentMaxLength);
            panel_Convert.Controls.Add(btnSelectTargetDatabaseTypesForConcatChar);
            panel_Convert.Controls.Add(label6);
            panel_Convert.Controls.Add(chkNotCreateIfExists);
            panel_Convert.Controls.Add(chkUseOriginalDataTypeIfUdtHasOnlyOneAttr);
            panel_Convert.Controls.Add(panel_Translate);
            panel_Convert.Controls.Add(chkNeedPreviewBeforeConvert);
            panel_Convert.Controls.Add(txtCustomMappingFolder);
            panel_Convert.Controls.Add(btnCustomMappingFolder);
            panel_Convert.Controls.Add(label11);
            panel_Convert.Controls.Add(btnSetDataTypeMapping);
            panel_Convert.Controls.Add(label10);
            panel_Convert.Location = new System.Drawing.Point(7, 12);
            panel_Convert.Name = "panel_Convert";
            panel_Convert.Size = new System.Drawing.Size(695, 402);
            panel_Convert.TabIndex = 48;
            // 
            // chkAutoMapSchema
            // 
            chkAutoMapSchema.AutoSize = true;
            chkAutoMapSchema.Checked = true;
            chkAutoMapSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            chkAutoMapSchema.Location = new System.Drawing.Point(10, 125);
            chkAutoMapSchema.Margin = new System.Windows.Forms.Padding(4);
            chkAutoMapSchema.Name = "chkAutoMapSchema";
            chkAutoMapSchema.Size = new System.Drawing.Size(464, 21);
            chkAutoMapSchema.TabIndex = 46;
            chkAutoMapSchema.Text = "Auto map schema if source database type is equal to target database type";
            chkAutoMapSchema.UseVisualStyleBackColor = true;
            // 
            // chkShowCurrentColumnContentMaxLength
            // 
            chkShowCurrentColumnContentMaxLength.AutoSize = true;
            chkShowCurrentColumnContentMaxLength.Checked = true;
            chkShowCurrentColumnContentMaxLength.CheckState = System.Windows.Forms.CheckState.Checked;
            chkShowCurrentColumnContentMaxLength.Location = new System.Drawing.Point(10, 66);
            chkShowCurrentColumnContentMaxLength.Name = "chkShowCurrentColumnContentMaxLength";
            chkShowCurrentColumnContentMaxLength.Size = new System.Drawing.Size(430, 21);
            chkShowCurrentColumnContentMaxLength.TabIndex = 45;
            chkShowCurrentColumnContentMaxLength.Text = "Show current database column content max length before converting";
            chkShowCurrentColumnContentMaxLength.UseVisualStyleBackColor = true;
            // 
            // btnSelectTargetDatabaseTypesForConcatChar
            // 
            btnSelectTargetDatabaseTypesForConcatChar.Location = new System.Drawing.Point(265, 235);
            btnSelectTargetDatabaseTypesForConcatChar.Name = "btnSelectTargetDatabaseTypesForConcatChar";
            btnSelectTargetDatabaseTypesForConcatChar.Size = new System.Drawing.Size(75, 26);
            btnSelectTargetDatabaseTypesForConcatChar.TabIndex = 44;
            btnSelectTargetDatabaseTypesForConcatChar.Text = "Set";
            btnSelectTargetDatabaseTypesForConcatChar.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(8, 238);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(248, 17);
            label6.TabIndex = 43;
            label6.Text = "Convert concat char if target database is:";
            // 
            // chkNotCreateIfExists
            // 
            chkNotCreateIfExists.AutoSize = true;
            chkNotCreateIfExists.Location = new System.Drawing.Point(10, 8);
            chkNotCreateIfExists.Margin = new System.Windows.Forms.Padding(4);
            chkNotCreateIfExists.Name = "chkNotCreateIfExists";
            chkNotCreateIfExists.Size = new System.Drawing.Size(176, 21);
            chkNotCreateIfExists.TabIndex = 41;
            chkNotCreateIfExists.Text = "Not create if object exists";
            chkNotCreateIfExists.UseVisualStyleBackColor = true;
            // 
            // chkUseOriginalDataTypeIfUdtHasOnlyOneAttr
            // 
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.AutoSize = true;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Checked = true;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.CheckState = System.Windows.Forms.CheckState.Checked;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Location = new System.Drawing.Point(10, 97);
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Margin = new System.Windows.Forms.Padding(4);
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Name = "chkUseOriginalDataTypeIfUdtHasOnlyOneAttr";
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Size = new System.Drawing.Size(404, 21);
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.TabIndex = 42;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Text = "Use original data type if user defined type has only one attribute";
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.UseVisualStyleBackColor = true;
            // 
            // panel_Translate
            // 
            panel_Translate.Controls.Add(chkValidateScriptsAfterTranslated);
            panel_Translate.Location = new System.Drawing.Point(5, 284);
            panel_Translate.Name = "panel_Translate";
            panel_Translate.Size = new System.Drawing.Size(491, 36);
            panel_Translate.TabIndex = 40;
            // 
            // chkValidateScriptsAfterTranslated
            // 
            chkValidateScriptsAfterTranslated.AutoSize = true;
            chkValidateScriptsAfterTranslated.Location = new System.Drawing.Point(6, 7);
            chkValidateScriptsAfterTranslated.Name = "chkValidateScriptsAfterTranslated";
            chkValidateScriptsAfterTranslated.Size = new System.Drawing.Size(209, 21);
            chkValidateScriptsAfterTranslated.TabIndex = 38;
            chkValidateScriptsAfterTranslated.Text = "Validate scripts after translated";
            chkValidateScriptsAfterTranslated.UseVisualStyleBackColor = true;
            // 
            // chkNeedPreviewBeforeConvert
            // 
            chkNeedPreviewBeforeConvert.AutoSize = true;
            chkNeedPreviewBeforeConvert.Checked = true;
            chkNeedPreviewBeforeConvert.CheckState = System.Windows.Forms.CheckState.Checked;
            chkNeedPreviewBeforeConvert.Location = new System.Drawing.Point(10, 39);
            chkNeedPreviewBeforeConvert.Name = "chkNeedPreviewBeforeConvert";
            chkNeedPreviewBeforeConvert.Size = new System.Drawing.Size(216, 21);
            chkNeedPreviewBeforeConvert.TabIndex = 36;
            chkNeedPreviewBeforeConvert.Text = "Need preview before converting";
            chkNeedPreviewBeforeConvert.UseVisualStyleBackColor = true;
            // 
            // txtCustomMappingFolder
            // 
            txtCustomMappingFolder.Location = new System.Drawing.Point(233, 151);
            txtCustomMappingFolder.Margin = new System.Windows.Forms.Padding(4);
            txtCustomMappingFolder.Name = "txtCustomMappingFolder";
            txtCustomMappingFolder.Size = new System.Drawing.Size(273, 23);
            txtCustomMappingFolder.TabIndex = 34;
            // 
            // btnCustomMappingFolder
            // 
            btnCustomMappingFolder.Location = new System.Drawing.Point(515, 149);
            btnCustomMappingFolder.Margin = new System.Windows.Forms.Padding(4);
            btnCustomMappingFolder.Name = "btnCustomMappingFolder";
            btnCustomMappingFolder.Size = new System.Drawing.Size(42, 24);
            btnCustomMappingFolder.TabIndex = 35;
            btnCustomMappingFolder.Text = "...";
            btnCustomMappingFolder.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(8, 153);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(219, 17);
            label11.TabIndex = 2;
            label11.Text = "Custom mapping config root folder:";
            // 
            // btnSetDataTypeMapping
            // 
            btnSetDataTypeMapping.Location = new System.Drawing.Point(162, 188);
            btnSetDataTypeMapping.Name = "btnSetDataTypeMapping";
            btnSetDataTypeMapping.Size = new System.Drawing.Size(75, 23);
            btnSetDataTypeMapping.TabIndex = 1;
            btnSetDataTypeMapping.Text = "Set";
            btnSetDataTypeMapping.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(8, 190);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(129, 17);
            label10.TabIndex = 0;
            label10.Text = "Data type mappings:";
            // 
            // frmSetting
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(721, 574);
            Controls.Add(tabControl1);
            Controls.Add(btnCancel);
            Controls.Add(btnConfirm);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmSetting";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Setting";
            Load += frmSetting_Load;
            tabDatabase.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            panel_Database.ResumeLayout(false);
            panel_Database.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCommandTimeout).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDataBatchSize).EndInit();
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabApperance.ResumeLayout(false);
            panel_Apperance.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numTextEditorFontSize).EndInit();
            tabConvert.ResumeLayout(false);
            panel_Convert.ResumeLayout(false);
            panel_Convert.PerformLayout();
            panel_Translate.ResumeLayout(false);
            panel_Translate.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TabPage tabDatabase;
        private System.Windows.Forms.TextBox txtMySqlCharsetCollation;
        private System.Windows.Forms.TextBox txtMySqlCharset;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblMySqlCharset;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TextBox txtLockPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.CheckBox chkRememberPasswordDuringSession;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabApperance;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;    
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox chkExcludePostgresExtensionObjects;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel panel_Database;
        private System.Windows.Forms.Label lblCommandTimeout;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numCommandTimeout;
        private System.Windows.Forms.NumericUpDown numDataBatchSize;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkShowBuiltinDatabase;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboDbObjectNameMode;
        private System.Windows.Forms.Panel panel_Apperance;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox cboThemeType;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numTextEditorFontSize;
        private System.Windows.Forms.Label cboTextEditorFontSize;
        private System.Windows.Forms.ComboBox cboTextEditorFontName;
        private System.Windows.Forms.CheckBox chkShowTextEditorLineNumber;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage tabConvert;
        private System.Windows.Forms.ComboBox cboPreferredDatabase;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.TextBox txtScriptOutputFolder;
        private System.Windows.Forms.Button btnScriptOutputFolder;
        private System.Windows.Forms.CheckBox chkEnableLog;
        private System.Windows.Forms.CheckBox chkLogError;
        private System.Windows.Forms.Label lblLogType;
        private System.Windows.Forms.CheckBox chkLogInfo;
        private System.Windows.Forms.Panel panel_Convert;
        private System.Windows.Forms.Button btnSelectTargetDatabaseTypesForConcatChar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkNotCreateIfExists;
        private System.Windows.Forms.CheckBox chkUseOriginalDataTypeIfUdtHasOnlyOneAttr;
        private System.Windows.Forms.Panel panel_Translate;
        private System.Windows.Forms.CheckBox chkValidateScriptsAfterTranslated;
        private System.Windows.Forms.CheckBox chkNeedPreviewBeforeConvert;
        private System.Windows.Forms.TextBox txtCustomMappingFolder;
        private System.Windows.Forms.Button btnCustomMappingFolder;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnSetDataTypeMapping;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox chkEnableEditorHighlighting;
        private System.Windows.Forms.CheckBox chkEditorEnableIntellisence;
        private System.Windows.Forms.CheckBox chkShowCurrentColumnContentMaxLength;
        private System.Windows.Forms.CheckBox chkAutoMapSchema;
    }
}