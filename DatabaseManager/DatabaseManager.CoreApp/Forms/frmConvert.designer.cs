using DatabaseInterpreter.Model;

namespace DatabaseManager
{
    partial class frmConvert
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConvert));
            btnSaveMessage = new System.Windows.Forms.Button();
            btnCopyMessage = new System.Windows.Forms.Button();
            txtMessage = new System.Windows.Forms.RichTextBox();
            dlgSaveLog = new System.Windows.Forms.SaveFileDialog();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            tvDbObjects = new Controls.UC_DbObjectsSimpleTree();
            gbConfiguration = new System.Windows.Forms.GroupBox();
            chkGenerateCheckConstraint = new System.Windows.Forms.CheckBox();
            cboMode = new System.Windows.Forms.ComboBox();
            chkNcharToDoubleChar = new System.Windows.Forms.CheckBox();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            chkCreateSchemaIfNotExists = new System.Windows.Forms.CheckBox();
            chkExcludeGeometryForData = new System.Windows.Forms.CheckBox();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            btnSetSchemaMappings = new System.Windows.Forms.Button();
            chkGenerateComment = new System.Windows.Forms.CheckBox();
            chkTreatBytesAsNull = new System.Windows.Forms.CheckBox();
            chkOnlyCommentComputeExpression = new System.Windows.Forms.CheckBox();
            chkComputeColumn = new System.Windows.Forms.CheckBox();
            chkContinueWhenErrorOccurs = new System.Windows.Forms.CheckBox();
            chkUseTransaction = new System.Windows.Forms.CheckBox();
            chkBulkCopy = new System.Windows.Forms.CheckBox();
            chkGenerateIdentity = new System.Windows.Forms.CheckBox();
            lblDbSchemaMappings = new System.Windows.Forms.Label();
            chkGenerateSourceScripts = new System.Windows.Forms.CheckBox();
            chkExecuteOnTarget = new System.Windows.Forms.CheckBox();
            lblOutputFolder = new System.Windows.Forms.Label();
            btnOutputFolder = new System.Windows.Forms.Button();
            txtOutputFolder = new System.Windows.Forms.TextBox();
            lblScriptsMode = new System.Windows.Forms.Label();
            chkOutputScripts = new System.Windows.Forms.CheckBox();
            targetDbProfile = new Controls.UC_DbConnectionProfile();
            sourceDbProfile = new Controls.UC_DbConnectionProfile();
            btnFetch = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            btnExecute = new System.Windows.Forms.Button();
            dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiCopySelection = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            gbConfiguration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnSaveMessage
            // 
            btnSaveMessage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnSaveMessage.Image = (System.Drawing.Image)resources.GetObject("btnSaveMessage.Image");
            btnSaveMessage.Location = new System.Drawing.Point(863, 40);
            btnSaveMessage.Margin = new System.Windows.Forms.Padding(4);
            btnSaveMessage.Name = "btnSaveMessage";
            btnSaveMessage.Size = new System.Drawing.Size(31, 28);
            btnSaveMessage.TabIndex = 9;
            toolTip1.SetToolTip(btnSaveMessage, "Save message");
            btnSaveMessage.UseVisualStyleBackColor = true;
            btnSaveMessage.Click += btnSaveMessage_Click;
            // 
            // btnCopyMessage
            // 
            btnCopyMessage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnCopyMessage.Image = (System.Drawing.Image)resources.GetObject("btnCopyMessage.Image");
            btnCopyMessage.Location = new System.Drawing.Point(862, 10);
            btnCopyMessage.Margin = new System.Windows.Forms.Padding(4);
            btnCopyMessage.Name = "btnCopyMessage";
            btnCopyMessage.Size = new System.Drawing.Size(31, 28);
            btnCopyMessage.TabIndex = 8;
            toolTip1.SetToolTip(btnCopyMessage, "Copy message to clipboard");
            btnCopyMessage.UseVisualStyleBackColor = true;
            btnCopyMessage.Click += btnCopyMessage_Click;
            // 
            // txtMessage
            // 
            txtMessage.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtMessage.BackColor = System.Drawing.Color.White;
            txtMessage.Location = new System.Drawing.Point(4, 4);
            txtMessage.Margin = new System.Windows.Forms.Padding(4);
            txtMessage.Name = "txtMessage";
            txtMessage.ReadOnly = true;
            txtMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            txtMessage.Size = new System.Drawing.Size(852, 69);
            txtMessage.TabIndex = 0;
            txtMessage.Text = "";
            txtMessage.MouseUp += txtMessage_MouseUp;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(1, 7);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            splitContainer1.Panel1.Controls.Add(targetDbProfile);
            splitContainer1.Panel1.Controls.Add(sourceDbProfile);
            splitContainer1.Panel1.Controls.Add(btnFetch);
            splitContainer1.Panel1.Controls.Add(btnCancel);
            splitContainer1.Panel1.Controls.Add(btnExecute);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(btnSaveMessage);
            splitContainer1.Panel2.Controls.Add(txtMessage);
            splitContainer1.Panel2.Controls.Add(btnCopyMessage);
            splitContainer1.Size = new System.Drawing.Size(898, 700);
            splitContainer1.SplitterDistance = 526;
            splitContainer1.TabIndex = 21;
            // 
            // splitContainer2
            // 
            splitContainer2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer2.Location = new System.Drawing.Point(4, 63);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(tvDbObjects);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(gbConfiguration);
            splitContainer2.Size = new System.Drawing.Size(882, 411);
            splitContainer2.SplitterDistance = 294;
            splitContainer2.TabIndex = 39;
            // 
            // tvDbObjects
            // 
            tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            tvDbObjects.Location = new System.Drawing.Point(0, 0);
            tvDbObjects.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            tvDbObjects.Name = "tvDbObjects";
            tvDbObjects.Size = new System.Drawing.Size(294, 411);
            tvDbObjects.TabIndex = 38;
            // 
            // gbConfiguration
            // 
            gbConfiguration.Controls.Add(chkGenerateCheckConstraint);
            gbConfiguration.Controls.Add(cboMode);
            gbConfiguration.Controls.Add(chkNcharToDoubleChar);
            gbConfiguration.Controls.Add(pictureBox1);
            gbConfiguration.Controls.Add(chkCreateSchemaIfNotExists);
            gbConfiguration.Controls.Add(chkExcludeGeometryForData);
            gbConfiguration.Controls.Add(pictureBox2);
            gbConfiguration.Controls.Add(btnSetSchemaMappings);
            gbConfiguration.Controls.Add(chkGenerateComment);
            gbConfiguration.Controls.Add(chkTreatBytesAsNull);
            gbConfiguration.Controls.Add(chkOnlyCommentComputeExpression);
            gbConfiguration.Controls.Add(chkComputeColumn);
            gbConfiguration.Controls.Add(chkContinueWhenErrorOccurs);
            gbConfiguration.Controls.Add(chkUseTransaction);
            gbConfiguration.Controls.Add(chkBulkCopy);
            gbConfiguration.Controls.Add(chkGenerateIdentity);
            gbConfiguration.Controls.Add(lblDbSchemaMappings);
            gbConfiguration.Controls.Add(chkGenerateSourceScripts);
            gbConfiguration.Controls.Add(chkExecuteOnTarget);
            gbConfiguration.Controls.Add(lblOutputFolder);
            gbConfiguration.Controls.Add(btnOutputFolder);
            gbConfiguration.Controls.Add(txtOutputFolder);
            gbConfiguration.Controls.Add(lblScriptsMode);
            gbConfiguration.Controls.Add(chkOutputScripts);
            gbConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
            gbConfiguration.Location = new System.Drawing.Point(0, 0);
            gbConfiguration.Margin = new System.Windows.Forms.Padding(4);
            gbConfiguration.Name = "gbConfiguration";
            gbConfiguration.Padding = new System.Windows.Forms.Padding(4);
            gbConfiguration.Size = new System.Drawing.Size(584, 411);
            gbConfiguration.TabIndex = 21;
            gbConfiguration.TabStop = false;
            gbConfiguration.Text = "Configuration";
            // 
            // chkGenerateCheckConstraint
            // 
            chkGenerateCheckConstraint.AutoSize = true;
            chkGenerateCheckConstraint.Checked = true;
            chkGenerateCheckConstraint.CheckState = System.Windows.Forms.CheckState.Checked;
            chkGenerateCheckConstraint.Location = new System.Drawing.Point(293, 175);
            chkGenerateCheckConstraint.Margin = new System.Windows.Forms.Padding(4);
            chkGenerateCheckConstraint.Name = "chkGenerateCheckConstraint";
            chkGenerateCheckConstraint.Size = new System.Drawing.Size(178, 21);
            chkGenerateCheckConstraint.TabIndex = 65;
            chkGenerateCheckConstraint.Tag = "Schema";
            chkGenerateCheckConstraint.Text = "Generate check constraint";
            chkGenerateCheckConstraint.UseVisualStyleBackColor = true;
            // 
            // cboMode
            // 
            cboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboMode.FormattingEnabled = true;
            cboMode.Items.AddRange(new object[] { "Schema", "Data", "Schema & Data" });
            cboMode.Location = new System.Drawing.Point(139, 24);
            cboMode.Name = "cboMode";
            cboMode.Size = new System.Drawing.Size(121, 25);
            cboMode.TabIndex = 64;
            cboMode.SelectedIndexChanged += cboMode_SelectedIndexChanged;
            // 
            // chkNcharToDoubleChar
            // 
            chkNcharToDoubleChar.Checked = true;
            chkNcharToDoubleChar.CheckState = System.Windows.Forms.CheckState.Checked;
            chkNcharToDoubleChar.Enabled = false;
            chkNcharToDoubleChar.Location = new System.Drawing.Point(293, 233);
            chkNcharToDoubleChar.Margin = new System.Windows.Forms.Padding(4);
            chkNcharToDoubleChar.Name = "chkNcharToDoubleChar";
            chkNcharToDoubleChar.Size = new System.Drawing.Size(249, 21);
            chkNcharToDoubleChar.TabIndex = 63;
            chkNcharToDoubleChar.Tag = "Schema";
            chkNcharToDoubleChar.Text = "One nchar to two chars for data type";
            chkNcharToDoubleChar.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Resources.Info16;
            pictureBox1.Location = new System.Drawing.Point(448, 286);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(17, 19);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox1.TabIndex = 62;
            pictureBox1.TabStop = false;
            toolTip1.SetToolTip(pictureBox1, "It works without transaction");
            // 
            // chkCreateSchemaIfNotExists
            // 
            chkCreateSchemaIfNotExists.AutoSize = true;
            chkCreateSchemaIfNotExists.Checked = true;
            chkCreateSchemaIfNotExists.CheckState = System.Windows.Forms.CheckState.Checked;
            chkCreateSchemaIfNotExists.Location = new System.Drawing.Point(292, 91);
            chkCreateSchemaIfNotExists.Margin = new System.Windows.Forms.Padding(4);
            chkCreateSchemaIfNotExists.Name = "chkCreateSchemaIfNotExists";
            chkCreateSchemaIfNotExists.Size = new System.Drawing.Size(183, 21);
            chkCreateSchemaIfNotExists.TabIndex = 61;
            chkCreateSchemaIfNotExists.Tag = "Schema";
            chkCreateSchemaIfNotExists.Text = "Create schema if not exists";
            chkCreateSchemaIfNotExists.UseVisualStyleBackColor = true;
            // 
            // chkExcludeGeometryForData
            // 
            chkExcludeGeometryForData.AutoSize = true;
            chkExcludeGeometryForData.Location = new System.Drawing.Point(293, 204);
            chkExcludeGeometryForData.Margin = new System.Windows.Forms.Padding(4);
            chkExcludeGeometryForData.Name = "chkExcludeGeometryForData";
            chkExcludeGeometryForData.Size = new System.Drawing.Size(156, 21);
            chkExcludeGeometryForData.TabIndex = 60;
            chkExcludeGeometryForData.Tag = "Data";
            chkExcludeGeometryForData.Text = "Ignore geometry data";
            chkExcludeGeometryForData.UseVisualStyleBackColor = true;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = Resources.Info16;
            pictureBox2.Location = new System.Drawing.Point(214, 61);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(17, 19);
            pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBox2.TabIndex = 59;
            pictureBox2.TabStop = false;
            toolTip1.SetToolTip(pictureBox2, "If the target database is Oracle or MySql, no need to set.");
            // 
            // btnSetSchemaMappings
            // 
            btnSetSchemaMappings.Location = new System.Drawing.Point(138, 58);
            btnSetSchemaMappings.Margin = new System.Windows.Forms.Padding(4);
            btnSetSchemaMappings.Name = "btnSetSchemaMappings";
            btnSetSchemaMappings.Size = new System.Drawing.Size(67, 25);
            btnSetSchemaMappings.TabIndex = 58;
            btnSetSchemaMappings.Text = "Set";
            btnSetSchemaMappings.UseVisualStyleBackColor = true;
            btnSetSchemaMappings.Click += btnSetSchemaMappings_Click;
            // 
            // chkGenerateComment
            // 
            chkGenerateComment.AutoSize = true;
            chkGenerateComment.Checked = true;
            chkGenerateComment.CheckState = System.Windows.Forms.CheckState.Checked;
            chkGenerateComment.Location = new System.Drawing.Point(12, 201);
            chkGenerateComment.Margin = new System.Windows.Forms.Padding(4);
            chkGenerateComment.Name = "chkGenerateComment";
            chkGenerateComment.Size = new System.Drawing.Size(138, 21);
            chkGenerateComment.TabIndex = 57;
            chkGenerateComment.Tag = "Schema";
            chkGenerateComment.Text = "Generate comment";
            chkGenerateComment.UseVisualStyleBackColor = true;
            // 
            // chkTreatBytesAsNull
            // 
            chkTreatBytesAsNull.AutoSize = true;
            chkTreatBytesAsNull.Location = new System.Drawing.Point(12, 229);
            chkTreatBytesAsNull.Margin = new System.Windows.Forms.Padding(4);
            chkTreatBytesAsNull.Name = "chkTreatBytesAsNull";
            chkTreatBytesAsNull.Size = new System.Drawing.Size(263, 21);
            chkTreatBytesAsNull.TabIndex = 55;
            chkTreatBytesAsNull.Tag = "Data";
            chkTreatBytesAsNull.Text = "Treat bytes data as null for data transfer";
            chkTreatBytesAsNull.UseVisualStyleBackColor = true;
            // 
            // chkOnlyCommentComputeExpression
            // 
            chkOnlyCommentComputeExpression.AutoSize = true;
            chkOnlyCommentComputeExpression.Enabled = false;
            chkOnlyCommentComputeExpression.Location = new System.Drawing.Point(293, 262);
            chkOnlyCommentComputeExpression.Margin = new System.Windows.Forms.Padding(4);
            chkOnlyCommentComputeExpression.Name = "chkOnlyCommentComputeExpression";
            chkOnlyCommentComputeExpression.Size = new System.Drawing.Size(228, 21);
            chkOnlyCommentComputeExpression.TabIndex = 22;
            chkOnlyCommentComputeExpression.Tag = "Schema";
            chkOnlyCommentComputeExpression.Text = "Only comment expression in script";
            chkOnlyCommentComputeExpression.UseVisualStyleBackColor = true;
            // 
            // chkComputeColumn
            // 
            chkComputeColumn.AutoSize = true;
            chkComputeColumn.Checked = true;
            chkComputeColumn.CheckState = System.Windows.Forms.CheckState.Checked;
            chkComputeColumn.Location = new System.Drawing.Point(12, 257);
            chkComputeColumn.Margin = new System.Windows.Forms.Padding(4);
            chkComputeColumn.Name = "chkComputeColumn";
            chkComputeColumn.Size = new System.Drawing.Size(249, 21);
            chkComputeColumn.TabIndex = 18;
            chkComputeColumn.Tag = "Schema";
            chkComputeColumn.Text = "Convert compute column's expression";
            chkComputeColumn.UseVisualStyleBackColor = true;
            chkComputeColumn.Click += chkComputeColumn_CheckedChanged;
            // 
            // chkContinueWhenErrorOccurs
            // 
            chkContinueWhenErrorOccurs.AutoSize = true;
            chkContinueWhenErrorOccurs.Location = new System.Drawing.Point(12, 286);
            chkContinueWhenErrorOccurs.Margin = new System.Windows.Forms.Padding(4);
            chkContinueWhenErrorOccurs.Name = "chkContinueWhenErrorOccurs";
            chkContinueWhenErrorOccurs.Size = new System.Drawing.Size(429, 21);
            chkContinueWhenErrorOccurs.TabIndex = 17;
            chkContinueWhenErrorOccurs.Tag = "Schema";
            chkContinueWhenErrorOccurs.Text = "Continue when error occurs for function, procedure, trigger and view";
            chkContinueWhenErrorOccurs.UseVisualStyleBackColor = true;
            // 
            // chkUseTransaction
            // 
            chkUseTransaction.AutoSize = true;
            chkUseTransaction.Checked = true;
            chkUseTransaction.CheckState = System.Windows.Forms.CheckState.Checked;
            chkUseTransaction.Location = new System.Drawing.Point(12, 117);
            chkUseTransaction.Margin = new System.Windows.Forms.Padding(4);
            chkUseTransaction.Name = "chkUseTransaction";
            chkUseTransaction.Size = new System.Drawing.Size(117, 21);
            chkUseTransaction.TabIndex = 16;
            chkUseTransaction.Text = "Use transaction";
            chkUseTransaction.UseVisualStyleBackColor = true;
            // 
            // chkBulkCopy
            // 
            chkBulkCopy.AutoSize = true;
            chkBulkCopy.Checked = true;
            chkBulkCopy.CheckState = System.Windows.Forms.CheckState.Checked;
            chkBulkCopy.Enabled = false;
            chkBulkCopy.Location = new System.Drawing.Point(12, 145);
            chkBulkCopy.Margin = new System.Windows.Forms.Padding(4);
            chkBulkCopy.Name = "chkBulkCopy";
            chkBulkCopy.Size = new System.Drawing.Size(190, 21);
            chkBulkCopy.TabIndex = 15;
            chkBulkCopy.Tag = "Data";
            chkBulkCopy.Text = "Use BulkCopy to insert data";
            chkBulkCopy.UseVisualStyleBackColor = true;
            // 
            // chkGenerateIdentity
            // 
            chkGenerateIdentity.AutoSize = true;
            chkGenerateIdentity.Checked = true;
            chkGenerateIdentity.CheckState = System.Windows.Forms.CheckState.Checked;
            chkGenerateIdentity.Location = new System.Drawing.Point(12, 173);
            chkGenerateIdentity.Margin = new System.Windows.Forms.Padding(4);
            chkGenerateIdentity.Name = "chkGenerateIdentity";
            chkGenerateIdentity.Size = new System.Drawing.Size(126, 21);
            chkGenerateIdentity.TabIndex = 14;
            chkGenerateIdentity.Tag = "Schema";
            chkGenerateIdentity.Text = "Generate identity";
            chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // lblDbSchemaMappings
            // 
            lblDbSchemaMappings.AutoSize = true;
            lblDbSchemaMappings.Location = new System.Drawing.Point(8, 61);
            lblDbSchemaMappings.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblDbSchemaMappings.Name = "lblDbSchemaMappings";
            lblDbSchemaMappings.Size = new System.Drawing.Size(118, 17);
            lblDbSchemaMappings.TabIndex = 9;
            lblDbSchemaMappings.Text = "Schema mappings:";
            // 
            // chkGenerateSourceScripts
            // 
            chkGenerateSourceScripts.AutoSize = true;
            chkGenerateSourceScripts.Location = new System.Drawing.Point(180, 315);
            chkGenerateSourceScripts.Margin = new System.Windows.Forms.Padding(4);
            chkGenerateSourceScripts.Name = "chkGenerateSourceScripts";
            chkGenerateSourceScripts.Size = new System.Drawing.Size(269, 21);
            chkGenerateSourceScripts.TabIndex = 8;
            chkGenerateSourceScripts.Text = "Output scripts of source database as well";
            chkGenerateSourceScripts.UseVisualStyleBackColor = true;
            // 
            // chkExecuteOnTarget
            // 
            chkExecuteOnTarget.AutoSize = true;
            chkExecuteOnTarget.Checked = true;
            chkExecuteOnTarget.CheckState = System.Windows.Forms.CheckState.Checked;
            chkExecuteOnTarget.Location = new System.Drawing.Point(12, 89);
            chkExecuteOnTarget.Margin = new System.Windows.Forms.Padding(4);
            chkExecuteOnTarget.Name = "chkExecuteOnTarget";
            chkExecuteOnTarget.Size = new System.Drawing.Size(229, 21);
            chkExecuteOnTarget.TabIndex = 7;
            chkExecuteOnTarget.Text = "Execute scripts on target database";
            chkExecuteOnTarget.UseVisualStyleBackColor = true;
            // 
            // lblOutputFolder
            // 
            lblOutputFolder.AutoSize = true;
            lblOutputFolder.Location = new System.Drawing.Point(9, 366);
            lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblOutputFolder.Name = "lblOutputFolder";
            lblOutputFolder.Size = new System.Drawing.Size(131, 17);
            lblOutputFolder.TabIndex = 6;
            lblOutputFolder.Text = "Scripts output folder:";
            // 
            // btnOutputFolder
            // 
            btnOutputFolder.Location = new System.Drawing.Point(431, 363);
            btnOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            btnOutputFolder.Name = "btnOutputFolder";
            btnOutputFolder.Size = new System.Drawing.Size(42, 24);
            btnOutputFolder.TabIndex = 4;
            btnOutputFolder.Text = "...";
            btnOutputFolder.UseVisualStyleBackColor = true;
            btnOutputFolder.Click += btnOutputFolder_Click;
            // 
            // txtOutputFolder
            // 
            txtOutputFolder.Location = new System.Drawing.Point(150, 364);
            txtOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            txtOutputFolder.Name = "txtOutputFolder";
            txtOutputFolder.Size = new System.Drawing.Size(277, 23);
            txtOutputFolder.TabIndex = 3;
            // 
            // lblScriptsMode
            // 
            lblScriptsMode.AutoSize = true;
            lblScriptsMode.Location = new System.Drawing.Point(8, 27);
            lblScriptsMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblScriptsMode.Name = "lblScriptsMode";
            lblScriptsMode.Size = new System.Drawing.Size(129, 17);
            lblScriptsMode.TabIndex = 1;
            lblScriptsMode.Text = "Mode of conversion:";
            // 
            // chkOutputScripts
            // 
            chkOutputScripts.AutoSize = true;
            chkOutputScripts.Location = new System.Drawing.Point(12, 315);
            chkOutputScripts.Margin = new System.Windows.Forms.Padding(4);
            chkOutputScripts.Name = "chkOutputScripts";
            chkOutputScripts.Size = new System.Drawing.Size(146, 21);
            chkOutputScripts.TabIndex = 0;
            chkOutputScripts.Text = "Output scripts to file";
            chkOutputScripts.UseVisualStyleBackColor = true;
            chkOutputScripts.CheckedChanged += chkOutputScripts_CheckedChanged;
            // 
            // targetDbProfile
            // 
            targetDbProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            targetDbProfile.DatabaseType = DatabaseType.Unknown;
            targetDbProfile.EnableDatabaseType = true;
            targetDbProfile.Location = new System.Drawing.Point(13, 31);
            targetDbProfile.Margin = new System.Windows.Forms.Padding(0);
            targetDbProfile.Name = "targetDbProfile";
            targetDbProfile.Size = new System.Drawing.Size(792, 29);
            targetDbProfile.TabIndex = 37;
            targetDbProfile.Title = "Target:";
            targetDbProfile.OnSelectedChanged += targetDbProfile_OnSelectedChanged;
            // 
            // sourceDbProfile
            // 
            sourceDbProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            sourceDbProfile.DatabaseType = DatabaseType.Unknown;
            sourceDbProfile.EnableDatabaseType = true;
            sourceDbProfile.Location = new System.Drawing.Point(13, 1);
            sourceDbProfile.Margin = new System.Windows.Forms.Padding(0);
            sourceDbProfile.Name = "sourceDbProfile";
            sourceDbProfile.Size = new System.Drawing.Size(792, 28);
            sourceDbProfile.TabIndex = 36;
            sourceDbProfile.Title = "Source:";
            sourceDbProfile.OnSelectedChanged += sourceDbProfile_OnSelectedChanged;
            // 
            // btnFetch
            // 
            btnFetch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnFetch.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F);
            btnFetch.Location = new System.Drawing.Point(806, 3);
            btnFetch.Margin = new System.Windows.Forms.Padding(4);
            btnFetch.Name = "btnFetch";
            btnFetch.Size = new System.Drawing.Size(83, 54);
            btnFetch.TabIndex = 35;
            btnFetch.Text = "Fetch";
            btnFetch.UseVisualStyleBackColor = true;
            btnFetch.Click += btnFetch_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnCancel.Enabled = false;
            btnCancel.Location = new System.Drawing.Point(479, 484);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 33);
            btnCancel.TabIndex = 22;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnExecute
            // 
            btnExecute.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnExecute.Enabled = false;
            btnExecute.Location = new System.Drawing.Point(366, 484);
            btnExecute.Margin = new System.Windows.Forms.Padding(4);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new System.Drawing.Size(88, 33);
            btnExecute.TabIndex = 21;
            btnExecute.Text = "Execute";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiCopySelection });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(162, 26);
            // 
            // tsmiCopySelection
            // 
            tsmiCopySelection.Name = "tsmiCopySelection";
            tsmiCopySelection.Size = new System.Drawing.Size(161, 22);
            tsmiCopySelection.Text = "Copy selection";
            tsmiCopySelection.Click += tsmiCopySelection_Click;
            // 
            // frmConvert
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(899, 611);
            Controls.Add(splitContainer1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmConvert";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Convert";
            FormClosing += frmMain_FormClosing;
            Load += frmMain_Load;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            gbConfiguration.ResumeLayout(false);
            gbConfiguration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.RichTextBox txtMessage;
        private System.Windows.Forms.Button btnSaveMessage;
        private System.Windows.Forms.Button btnCopyMessage;
        private System.Windows.Forms.SaveFileDialog dlgSaveLog;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnFetch;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
        private System.Windows.Forms.ToolTip toolTip1;
        private Controls.UC_DbConnectionProfile sourceDbProfile;
        private Controls.UC_DbConnectionProfile targetDbProfile;
        private Controls.UC_DbObjectsSimpleTree tvDbObjects;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopySelection;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox gbConfiguration;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button btnSetSchemaMappings;
        private System.Windows.Forms.CheckBox chkGenerateComment;
        private System.Windows.Forms.CheckBox chkTreatBytesAsNull;
        private System.Windows.Forms.CheckBox chkOnlyCommentComputeExpression;
        private System.Windows.Forms.CheckBox chkComputeColumn;
        private System.Windows.Forms.CheckBox chkContinueWhenErrorOccurs;
        private System.Windows.Forms.CheckBox chkUseTransaction;
        private System.Windows.Forms.CheckBox chkBulkCopy;
        private System.Windows.Forms.CheckBox chkGenerateIdentity;
        private System.Windows.Forms.Label lblDbSchemaMappings;
        private System.Windows.Forms.CheckBox chkGenerateSourceScripts;
        private System.Windows.Forms.CheckBox chkExecuteOnTarget;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.Button btnOutputFolder;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.Label lblScriptsMode;
        private System.Windows.Forms.CheckBox chkOutputScripts;
        private System.Windows.Forms.CheckBox chkExcludeGeometryForData;
        private System.Windows.Forms.CheckBox chkCreateSchemaIfNotExists;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox chkNcharToDoubleChar;
        private System.Windows.Forms.ComboBox cboMode;
        private System.Windows.Forms.CheckBox chkGenerateCheckConstraint;
    }
}

