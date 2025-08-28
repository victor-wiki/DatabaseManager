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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Database");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Apperance");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Editor");
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Log");
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Security");
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Script");
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Translate");
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Convert");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSetting));
            btnConfirm = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            tabMySql = new System.Windows.Forms.TabPage();
            txtMySqlCharsetCollation = new System.Windows.Forms.TextBox();
            txtMySqlCharset = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            lblMySqlCharset = new System.Windows.Forms.Label();
            tabGeneral = new System.Windows.Forms.TabPage();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            lvOption = new System.Windows.Forms.ListView();
            panel_Convert = new System.Windows.Forms.Panel();
            txtCustomMappingFolder = new System.Windows.Forms.TextBox();
            btnCustomMappingFolder = new System.Windows.Forms.Button();
            label11 = new System.Windows.Forms.Label();
            btnSetDataTypeMapping = new System.Windows.Forms.Button();
            label10 = new System.Windows.Forms.Label();
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
            panel_Translate = new System.Windows.Forms.Panel();
            chkValidateScriptsAfterTranslated = new System.Windows.Forms.CheckBox();
            panel_Script = new System.Windows.Forms.Panel();
            lblOutputFolder = new System.Windows.Forms.Label();
            txtScriptOutputFolder = new System.Windows.Forms.TextBox();
            btnScriptOutputFolder = new System.Windows.Forms.Button();
            panel_Database = new System.Windows.Forms.Panel();
            lblCommandTimeout = new System.Windows.Forms.Label();
            btnSelectTargetDatabaseTypesForConcatChar = new System.Windows.Forms.Button();
            cboPreferredDatabase = new System.Windows.Forms.ComboBox();
            label5 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            numCommandTimeout = new System.Windows.Forms.NumericUpDown();
            label6 = new System.Windows.Forms.Label();
            numDataBatchSize = new System.Windows.Forms.NumericUpDown();
            label2 = new System.Windows.Forms.Label();
            chkNotCreateIfExists = new System.Windows.Forms.CheckBox();
            chkShowBuiltinDatabase = new System.Windows.Forms.CheckBox();
            label4 = new System.Windows.Forms.Label();
            cboDbObjectNameMode = new System.Windows.Forms.ComboBox();
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr = new System.Windows.Forms.CheckBox();
            panel_Security = new System.Windows.Forms.Panel();
            chkRememberPasswordDuringSession = new System.Windows.Forms.CheckBox();
            txtLockPassword = new System.Windows.Forms.TextBox();
            lblPassword = new System.Windows.Forms.Label();
            panel_Editor = new System.Windows.Forms.Panel();
            chkEnableEditorHighlighting = new System.Windows.Forms.CheckBox();
            chkEditorEnableIntellisence = new System.Windows.Forms.CheckBox();
            panel_Log = new System.Windows.Forms.Panel();
            chkEnableLog = new System.Windows.Forms.CheckBox();
            chkLogError = new System.Windows.Forms.CheckBox();
            lblLogType = new System.Windows.Forms.Label();
            chkLogInfo = new System.Windows.Forms.CheckBox();
            tabControl1 = new System.Windows.Forms.TabControl();
            tabPostgres = new System.Windows.Forms.TabPage();
            chkExcludePostgresExtensionObjects = new System.Windows.Forms.CheckBox();
            dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            tabMySql.SuspendLayout();
            tabGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel_Convert.SuspendLayout();
            panel_Apperance.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numTextEditorFontSize).BeginInit();
            panel_Translate.SuspendLayout();
            panel_Script.SuspendLayout();
            panel_Database.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCommandTimeout).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numDataBatchSize).BeginInit();
            panel_Security.SuspendLayout();
            panel_Editor.SuspendLayout();
            panel_Log.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPostgres.SuspendLayout();
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
            tabMySql.Size = new System.Drawing.Size(705, 483);
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
            tabGeneral.Controls.Add(splitContainer1);
            tabGeneral.Location = new System.Drawing.Point(4, 26);
            tabGeneral.Margin = new System.Windows.Forms.Padding(4);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new System.Windows.Forms.Padding(4);
            tabGeneral.Size = new System.Drawing.Size(705, 483);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "General";
            // 
            // splitContainer1
            // 
            splitContainer1.Location = new System.Drawing.Point(5, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lvOption);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.AutoScroll = true;
            splitContainer1.Panel2.AutoScrollMinSize = new System.Drawing.Size(0, 1000);
            splitContainer1.Panel2.Controls.Add(panel_Convert);
            splitContainer1.Panel2.Controls.Add(panel_Apperance);
            splitContainer1.Panel2.Controls.Add(panel_Translate);
            splitContainer1.Panel2.Controls.Add(panel_Script);
            splitContainer1.Panel2.Controls.Add(panel_Database);
            splitContainer1.Panel2.Controls.Add(panel_Security);
            splitContainer1.Panel2.Controls.Add(panel_Editor);
            splitContainer1.Panel2.Controls.Add(panel_Log);
            splitContainer1.Size = new System.Drawing.Size(697, 473);
            splitContainer1.SplitterDistance = 154;
            splitContainer1.TabIndex = 40;
            // 
            // lvOption
            // 
            lvOption.BackColor = System.Drawing.SystemColors.Control;
            lvOption.Dock = System.Windows.Forms.DockStyle.Fill;
            lvOption.FullRowSelect = true;
            lvOption.Items.AddRange(new System.Windows.Forms.ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4, listViewItem5, listViewItem6, listViewItem7, listViewItem8 });
            lvOption.Location = new System.Drawing.Point(0, 0);
            lvOption.MultiSelect = false;
            lvOption.Name = "lvOption";
            lvOption.Size = new System.Drawing.Size(154, 473);
            lvOption.TabIndex = 39;
            lvOption.UseCompatibleStateImageBehavior = false;
            lvOption.View = System.Windows.Forms.View.List;
            lvOption.SelectedIndexChanged += lvOption_SelectedIndexChanged;
            // 
            // panel_Convert
            // 
            panel_Convert.Controls.Add(txtCustomMappingFolder);
            panel_Convert.Controls.Add(btnCustomMappingFolder);
            panel_Convert.Controls.Add(label11);
            panel_Convert.Controls.Add(btnSetDataTypeMapping);
            panel_Convert.Controls.Add(label10);
            panel_Convert.Location = new System.Drawing.Point(3, 821);
            panel_Convert.Name = "panel_Convert";
            panel_Convert.Size = new System.Drawing.Size(516, 120);
            panel_Convert.TabIndex = 47;
            // 
            // txtCustomMappingFolder
            // 
            txtCustomMappingFolder.Location = new System.Drawing.Point(163, 6);
            txtCustomMappingFolder.Margin = new System.Windows.Forms.Padding(4);
            txtCustomMappingFolder.Name = "txtCustomMappingFolder";
            txtCustomMappingFolder.Size = new System.Drawing.Size(273, 23);
            txtCustomMappingFolder.TabIndex = 34;
            // 
            // btnCustomMappingFolder
            // 
            btnCustomMappingFolder.Location = new System.Drawing.Point(445, 6);
            btnCustomMappingFolder.Margin = new System.Windows.Forms.Padding(4);
            btnCustomMappingFolder.Name = "btnCustomMappingFolder";
            btnCustomMappingFolder.Size = new System.Drawing.Size(42, 24);
            btnCustomMappingFolder.TabIndex = 35;
            btnCustomMappingFolder.Text = "...";
            btnCustomMappingFolder.UseVisualStyleBackColor = true;
            btnCustomMappingFolder.Click += btnCustomFolderMapping_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(6, 8);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(150, 17);
            label11.TabIndex = 2;
            label11.Text = "Custom mapping folder:";
            // 
            // btnSetDataTypeMapping
            // 
            btnSetDataTypeMapping.Location = new System.Drawing.Point(162, 35);
            btnSetDataTypeMapping.Name = "btnSetDataTypeMapping";
            btnSetDataTypeMapping.Size = new System.Drawing.Size(75, 23);
            btnSetDataTypeMapping.TabIndex = 1;
            btnSetDataTypeMapping.Text = "Set";
            btnSetDataTypeMapping.UseVisualStyleBackColor = true;
            btnSetDataTypeMapping.Click += btnSetDataTypeMapping_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(5, 37);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(129, 17);
            label10.TabIndex = 0;
            label10.Text = "Data type mappings:";
            // 
            // panel_Apperance
            // 
            panel_Apperance.Controls.Add(groupBox2);
            panel_Apperance.Controls.Add(groupBox1);
            panel_Apperance.Location = new System.Drawing.Point(3, 605);
            panel_Apperance.Name = "panel_Apperance";
            panel_Apperance.Size = new System.Drawing.Size(516, 210);
            panel_Apperance.TabIndex = 46;
            panel_Apperance.Visible = false;
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
            groupBox1.Location = new System.Drawing.Point(3, 78);
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
            // panel_Translate
            // 
            panel_Translate.Controls.Add(chkValidateScriptsAfterTranslated);
            panel_Translate.Location = new System.Drawing.Point(3, 308);
            panel_Translate.Name = "panel_Translate";
            panel_Translate.Size = new System.Drawing.Size(516, 36);
            panel_Translate.TabIndex = 39;
            panel_Translate.Visible = false;
            // 
            // chkValidateScriptsAfterTranslated
            // 
            chkValidateScriptsAfterTranslated.AutoSize = true;
            chkValidateScriptsAfterTranslated.Location = new System.Drawing.Point(7, 7);
            chkValidateScriptsAfterTranslated.Name = "chkValidateScriptsAfterTranslated";
            chkValidateScriptsAfterTranslated.Size = new System.Drawing.Size(209, 21);
            chkValidateScriptsAfterTranslated.TabIndex = 38;
            chkValidateScriptsAfterTranslated.Text = "Validate scripts after translated";
            chkValidateScriptsAfterTranslated.UseVisualStyleBackColor = true;
            // 
            // panel_Script
            // 
            panel_Script.Controls.Add(lblOutputFolder);
            panel_Script.Controls.Add(txtScriptOutputFolder);
            panel_Script.Controls.Add(btnScriptOutputFolder);
            panel_Script.Location = new System.Drawing.Point(2, 563);
            panel_Script.Name = "panel_Script";
            panel_Script.Size = new System.Drawing.Size(517, 36);
            panel_Script.TabIndex = 45;
            panel_Script.Visible = false;
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Location = new System.Drawing.Point(4, 9);
            lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new System.Drawing.Size(131, 17);
            lblOutputFolder.TabIndex = 34;
            lblOutputFolder.Text = "Scripts output folder:";
            // 
            // txtScriptOutputFolder
            // 
            txtScriptOutputFolder.Location = new System.Drawing.Point(143, 6);
            txtScriptOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            txtScriptOutputFolder.Name = "txtScriptOutputFolder";
            txtScriptOutputFolder.Size = new System.Drawing.Size(277, 23);
            txtScriptOutputFolder.TabIndex = 32;
            // 
            // btnScriptOutputFolder
            // 
            btnScriptOutputFolder.Location = new System.Drawing.Point(427, 5);
            btnScriptOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            btnScriptOutputFolder.Name = "btnScriptOutputFolder";
            btnScriptOutputFolder.Size = new System.Drawing.Size(42, 24);
            btnScriptOutputFolder.TabIndex = 33;
            btnScriptOutputFolder.Text = "...";
            btnScriptOutputFolder.UseVisualStyleBackColor = true;
            btnScriptOutputFolder.Click += btnScriptOutputFolder_Click;
            // 
            // panel_Database
            // 
            panel_Database.AutoScroll = true;
            panel_Database.Controls.Add(lblCommandTimeout);
            panel_Database.Controls.Add(btnSelectTargetDatabaseTypesForConcatChar);
            panel_Database.Controls.Add(cboPreferredDatabase);
            panel_Database.Controls.Add(label5);
            panel_Database.Controls.Add(label1);
            panel_Database.Controls.Add(numCommandTimeout);
            panel_Database.Controls.Add(label6);
            panel_Database.Controls.Add(numDataBatchSize);
            panel_Database.Controls.Add(label2);
            panel_Database.Controls.Add(chkNotCreateIfExists);
            panel_Database.Controls.Add(chkShowBuiltinDatabase);
            panel_Database.Controls.Add(label4);
            panel_Database.Controls.Add(cboDbObjectNameMode);
            panel_Database.Controls.Add(chkUseOriginalDataTypeIfUdtHasOnlyOneAttr);
            panel_Database.Location = new System.Drawing.Point(5, 3);
            panel_Database.Name = "panel_Database";
            panel_Database.Size = new System.Drawing.Size(514, 294);
            panel_Database.TabIndex = 0;
            panel_Database.Visible = false;
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
            // btnSelectTargetDatabaseTypesForConcatChar
            // 
            btnSelectTargetDatabaseTypesForConcatChar.Location = new System.Drawing.Point(264, 220);
            btnSelectTargetDatabaseTypesForConcatChar.Name = "btnSelectTargetDatabaseTypesForConcatChar";
            btnSelectTargetDatabaseTypesForConcatChar.Size = new System.Drawing.Size(75, 26);
            btnSelectTargetDatabaseTypesForConcatChar.TabIndex = 37;
            btnSelectTargetDatabaseTypesForConcatChar.Text = "Set";
            btnSelectTargetDatabaseTypesForConcatChar.UseVisualStyleBackColor = true;
            btnSelectTargetDatabaseTypesForConcatChar.Click += btnSelectTargetDatabaseTypesForConcatChar_Click;
            // 
            // cboPreferredDatabase
            // 
            cboPreferredDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboPreferredDatabase.FormattingEnabled = true;
            cboPreferredDatabase.Location = new System.Drawing.Point(147, 253);
            cboPreferredDatabase.Margin = new System.Windows.Forms.Padding(4);
            cboPreferredDatabase.Name = "cboPreferredDatabase";
            cboPreferredDatabase.Size = new System.Drawing.Size(126, 25);
            cboPreferredDatabase.TabIndex = 24;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(9, 257);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(124, 17);
            label5.TabIndex = 23;
            label5.Text = "Preferred database:";
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
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(9, 223);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(248, 17);
            label6.TabIndex = 36;
            label6.Text = "Convert concat char if target database is:";
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
            // chkNotCreateIfExists
            // 
            chkNotCreateIfExists.AutoSize = true;
            chkNotCreateIfExists.Location = new System.Drawing.Point(9, 188);
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
            chkShowBuiltinDatabase.Location = new System.Drawing.Point(9, 128);
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
            // chkUseOriginalDataTypeIfUdtHasOnlyOneAttr
            // 
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.AutoSize = true;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Checked = true;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.CheckState = System.Windows.Forms.CheckState.Checked;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Location = new System.Drawing.Point(9, 159);
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Margin = new System.Windows.Forms.Padding(4);
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Name = "chkUseOriginalDataTypeIfUdtHasOnlyOneAttr";
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Size = new System.Drawing.Size(404, 21);
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.TabIndex = 25;
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.Text = "Use original data type if user defined type has only one attribute";
            chkUseOriginalDataTypeIfUdtHasOnlyOneAttr.UseVisualStyleBackColor = true;
            // 
            // panel_Security
            // 
            panel_Security.Controls.Add(chkRememberPasswordDuringSession);
            panel_Security.Controls.Add(txtLockPassword);
            panel_Security.Controls.Add(lblPassword);
            panel_Security.Location = new System.Drawing.Point(6, 349);
            panel_Security.Name = "panel_Security";
            panel_Security.Size = new System.Drawing.Size(513, 69);
            panel_Security.TabIndex = 44;
            panel_Security.Visible = false;
            // 
            // chkRememberPasswordDuringSession
            // 
            chkRememberPasswordDuringSession.AutoSize = true;
            chkRememberPasswordDuringSession.Checked = true;
            chkRememberPasswordDuringSession.CheckState = System.Windows.Forms.CheckState.Checked;
            chkRememberPasswordDuringSession.Location = new System.Drawing.Point(5, 8);
            chkRememberPasswordDuringSession.Margin = new System.Windows.Forms.Padding(4);
            chkRememberPasswordDuringSession.Name = "chkRememberPasswordDuringSession";
            chkRememberPasswordDuringSession.Size = new System.Drawing.Size(495, 21);
            chkRememberPasswordDuringSession.TabIndex = 26;
            chkRememberPasswordDuringSession.Text = "Remember password during application session(store it in memory temporarily)";
            chkRememberPasswordDuringSession.UseVisualStyleBackColor = true;
            // 
            // txtLockPassword
            // 
            txtLockPassword.Location = new System.Drawing.Point(108, 35);
            txtLockPassword.Margin = new System.Windows.Forms.Padding(4);
            txtLockPassword.Name = "txtLockPassword";
            txtLockPassword.PasswordChar = '*';
            txtLockPassword.Size = new System.Drawing.Size(164, 23);
            txtLockPassword.TabIndex = 28;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new System.Drawing.Point(4, 38);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(99, 17);
            lblPassword.TabIndex = 27;
            lblPassword.Text = "Lock password:";
            // 
            // panel_Editor
            // 
            panel_Editor.Controls.Add(chkEnableEditorHighlighting);
            panel_Editor.Controls.Add(chkEditorEnableIntellisence);
            panel_Editor.Location = new System.Drawing.Point(6, 421);
            panel_Editor.Name = "panel_Editor";
            panel_Editor.Size = new System.Drawing.Size(513, 63);
            panel_Editor.TabIndex = 43;
            panel_Editor.Visible = false;
            // 
            // chkEnableEditorHighlighting
            // 
            chkEnableEditorHighlighting.AutoSize = true;
            chkEnableEditorHighlighting.Checked = true;
            chkEnableEditorHighlighting.CheckState = System.Windows.Forms.CheckState.Checked;
            chkEnableEditorHighlighting.Location = new System.Drawing.Point(4, 4);
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
            chkEditorEnableIntellisence.Location = new System.Drawing.Point(4, 33);
            chkEditorEnableIntellisence.Margin = new System.Windows.Forms.Padding(4);
            chkEditorEnableIntellisence.Name = "chkEditorEnableIntellisence";
            chkEditorEnableIntellisence.Size = new System.Drawing.Size(214, 21);
            chkEditorEnableIntellisence.TabIndex = 31;
            chkEditorEnableIntellisence.Text = "Enable intellisence for sql editor";
            chkEditorEnableIntellisence.UseVisualStyleBackColor = true;
            // 
            // panel_Log
            // 
            panel_Log.Controls.Add(chkEnableLog);
            panel_Log.Controls.Add(chkLogError);
            panel_Log.Controls.Add(lblLogType);
            panel_Log.Controls.Add(chkLogInfo);
            panel_Log.Location = new System.Drawing.Point(2, 495);
            panel_Log.Name = "panel_Log";
            panel_Log.Size = new System.Drawing.Size(517, 62);
            panel_Log.TabIndex = 42;
            panel_Log.Visible = false;
            // 
            // chkEnableLog
            // 
            chkEnableLog.AutoSize = true;
            chkEnableLog.Location = new System.Drawing.Point(8, 4);
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
            chkLogError.Location = new System.Drawing.Point(183, 32);
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
            lblLogType.Location = new System.Drawing.Point(7, 32);
            lblLogType.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblLogType.Name = "lblLogType";
            lblLogType.Size = new System.Drawing.Size(62, 17);
            lblLogType.TabIndex = 20;
            lblLogType.Text = "Log type:";
            // 
            // chkLogInfo
            // 
            chkLogInfo.AutoSize = true;
            chkLogInfo.Checked = true;
            chkLogInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            chkLogInfo.Location = new System.Drawing.Point(83, 32);
            chkLogInfo.Margin = new System.Windows.Forms.Padding(4);
            chkLogInfo.Name = "chkLogInfo";
            chkLogInfo.Size = new System.Drawing.Size(90, 21);
            chkLogInfo.TabIndex = 21;
            chkLogInfo.Text = "Infomation";
            chkLogInfo.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl1.Controls.Add(tabGeneral);
            tabControl1.Controls.Add(tabMySql);
            tabControl1.Controls.Add(tabPostgres);
            tabControl1.Location = new System.Drawing.Point(1, 3);
            tabControl1.Margin = new System.Windows.Forms.Padding(4);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(713, 513);
            tabControl1.TabIndex = 15;
            // 
            // tabPostgres
            // 
            tabPostgres.BackColor = System.Drawing.SystemColors.Control;
            tabPostgres.Controls.Add(chkExcludePostgresExtensionObjects);
            tabPostgres.Location = new System.Drawing.Point(4, 26);
            tabPostgres.Name = "tabPostgres";
            tabPostgres.Padding = new System.Windows.Forms.Padding(3);
            tabPostgres.Size = new System.Drawing.Size(705, 483);
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
            tabMySql.ResumeLayout(false);
            tabMySql.PerformLayout();
            tabGeneral.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel_Convert.ResumeLayout(false);
            panel_Convert.PerformLayout();
            panel_Apperance.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numTextEditorFontSize).EndInit();
            panel_Translate.ResumeLayout(false);
            panel_Translate.PerformLayout();
            panel_Script.ResumeLayout(false);
            panel_Script.PerformLayout();
            panel_Database.ResumeLayout(false);
            panel_Database.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCommandTimeout).EndInit();
            ((System.ComponentModel.ISupportInitialize)numDataBatchSize).EndInit();
            panel_Security.ResumeLayout(false);
            panel_Security.PerformLayout();
            panel_Editor.ResumeLayout(false);
            panel_Editor.PerformLayout();
            panel_Log.ResumeLayout(false);
            panel_Log.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPostgres.ResumeLayout(false);
            tabPostgres.PerformLayout();
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
        private System.Windows.Forms.Button btnScriptOutputFolder;
        private System.Windows.Forms.TextBox txtScriptOutputFolder;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSelectTargetDatabaseTypesForConcatChar;
        private System.Windows.Forms.CheckBox chkValidateScriptsAfterTranslated;
        private System.Windows.Forms.TabPage tabApperance;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView lvOption;
        private System.Windows.Forms.Panel panel_Script;
        private System.Windows.Forms.Panel panel_Editor;
        private System.Windows.Forms.Panel panel_Log;
        private System.Windows.Forms.Panel panel_Translate;
        private System.Windows.Forms.Panel panel_Apperance;
        private System.Windows.Forms.Panel panel_Database;
        private System.Windows.Forms.Panel panel_Security;
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
        private System.Windows.Forms.Panel panel_Convert;
        private System.Windows.Forms.Button btnSetDataTypeMapping;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtCustomMappingFolder;
        private System.Windows.Forms.Button btnCustomMappingFolder;
    }
}