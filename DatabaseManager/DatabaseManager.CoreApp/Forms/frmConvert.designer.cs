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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConvert));
            this.btnSaveMessage = new System.Windows.Forms.Button();
            this.btnCopyMessage = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.RichTextBox();
            this.dlgSaveLog = new System.Windows.Forms.SaveFileDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tvDbObjects = new DatabaseManager.Controls.UC_DbObjectsSimpleTree();
            this.gbConfiguration = new System.Windows.Forms.GroupBox();
            this.cboMode = new System.Windows.Forms.ComboBox();
            this.chkNcharToDoubleChar = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chkCreateSchemaIfNotExists = new System.Windows.Forms.CheckBox();
            this.chkExcludeGeometryForData = new System.Windows.Forms.CheckBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnSetSchemaMappings = new System.Windows.Forms.Button();
            this.chkGenerateComment = new System.Windows.Forms.CheckBox();
            this.chkTreatBytesAsNull = new System.Windows.Forms.CheckBox();
            this.chkOnlyCommentComputeExpression = new System.Windows.Forms.CheckBox();
            this.chkComputeColumn = new System.Windows.Forms.CheckBox();
            this.chkContinueWhenErrorOccurs = new System.Windows.Forms.CheckBox();
            this.chkUseTransaction = new System.Windows.Forms.CheckBox();
            this.chkBulkCopy = new System.Windows.Forms.CheckBox();
            this.chkGenerateIdentity = new System.Windows.Forms.CheckBox();
            this.lblDbSchemaMappings = new System.Windows.Forms.Label();
            this.chkGenerateSourceScripts = new System.Windows.Forms.CheckBox();
            this.chkExecuteOnTarget = new System.Windows.Forms.CheckBox();
            this.lblOutputFolder = new System.Windows.Forms.Label();
            this.btnOutputFolder = new System.Windows.Forms.Button();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.lblScriptsMode = new System.Windows.Forms.Label();
            this.chkOutputScripts = new System.Windows.Forms.CheckBox();
            this.targetDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            this.sourceDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            this.btnFetch = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCopySelection = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.gbConfiguration.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSaveMessage
            // 
            this.btnSaveMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveMessage.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveMessage.Image")));
            this.btnSaveMessage.Location = new System.Drawing.Point(863, 40);
            this.btnSaveMessage.Margin = new System.Windows.Forms.Padding(4);
            this.btnSaveMessage.Name = "btnSaveMessage";
            this.btnSaveMessage.Size = new System.Drawing.Size(31, 28);
            this.btnSaveMessage.TabIndex = 9;
            this.toolTip1.SetToolTip(this.btnSaveMessage, "Save message");
            this.btnSaveMessage.UseVisualStyleBackColor = true;
            this.btnSaveMessage.Click += new System.EventHandler(this.btnSaveMessage_Click);
            // 
            // btnCopyMessage
            // 
            this.btnCopyMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyMessage.Image = ((System.Drawing.Image)(resources.GetObject("btnCopyMessage.Image")));
            this.btnCopyMessage.Location = new System.Drawing.Point(862, 10);
            this.btnCopyMessage.Margin = new System.Windows.Forms.Padding(4);
            this.btnCopyMessage.Name = "btnCopyMessage";
            this.btnCopyMessage.Size = new System.Drawing.Size(31, 28);
            this.btnCopyMessage.TabIndex = 8;
            this.toolTip1.SetToolTip(this.btnCopyMessage, "Copy message to clipboard");
            this.btnCopyMessage.UseVisualStyleBackColor = true;
            this.btnCopyMessage.Click += new System.EventHandler(this.btnCopyMessage_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessage.BackColor = System.Drawing.Color.White;
            this.txtMessage.Location = new System.Drawing.Point(4, 4);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(4);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(852, 69);
            this.txtMessage.TabIndex = 0;
            this.txtMessage.Text = "";
            this.txtMessage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtMessage_MouseUp);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(1, 7);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1.Controls.Add(this.targetDbProfile);
            this.splitContainer1.Panel1.Controls.Add(this.sourceDbProfile);
            this.splitContainer1.Panel1.Controls.Add(this.btnFetch);
            this.splitContainer1.Panel1.Controls.Add(this.btnCancel);
            this.splitContainer1.Panel1.Controls.Add(this.btnExecute);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnSaveMessage);
            this.splitContainer1.Panel2.Controls.Add(this.txtMessage);
            this.splitContainer1.Panel2.Controls.Add(this.btnCopyMessage);
            this.splitContainer1.Size = new System.Drawing.Size(898, 700);
            this.splitContainer1.SplitterDistance = 526;
            this.splitContainer1.TabIndex = 21;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.Location = new System.Drawing.Point(4, 63);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tvDbObjects);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.gbConfiguration);
            this.splitContainer2.Size = new System.Drawing.Size(882, 411);
            this.splitContainer2.SplitterDistance = 294;
            this.splitContainer2.TabIndex = 39;
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDbObjects.Location = new System.Drawing.Point(0, 0);
            this.tvDbObjects.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(294, 411);
            this.tvDbObjects.TabIndex = 38;
            // 
            // gbConfiguration
            // 
            this.gbConfiguration.Controls.Add(this.cboMode);
            this.gbConfiguration.Controls.Add(this.chkNcharToDoubleChar);
            this.gbConfiguration.Controls.Add(this.pictureBox1);
            this.gbConfiguration.Controls.Add(this.chkCreateSchemaIfNotExists);
            this.gbConfiguration.Controls.Add(this.chkExcludeGeometryForData);
            this.gbConfiguration.Controls.Add(this.pictureBox2);
            this.gbConfiguration.Controls.Add(this.btnSetSchemaMappings);
            this.gbConfiguration.Controls.Add(this.chkGenerateComment);
            this.gbConfiguration.Controls.Add(this.chkTreatBytesAsNull);
            this.gbConfiguration.Controls.Add(this.chkOnlyCommentComputeExpression);
            this.gbConfiguration.Controls.Add(this.chkComputeColumn);
            this.gbConfiguration.Controls.Add(this.chkContinueWhenErrorOccurs);
            this.gbConfiguration.Controls.Add(this.chkUseTransaction);
            this.gbConfiguration.Controls.Add(this.chkBulkCopy);
            this.gbConfiguration.Controls.Add(this.chkGenerateIdentity);
            this.gbConfiguration.Controls.Add(this.lblDbSchemaMappings);
            this.gbConfiguration.Controls.Add(this.chkGenerateSourceScripts);
            this.gbConfiguration.Controls.Add(this.chkExecuteOnTarget);
            this.gbConfiguration.Controls.Add(this.lblOutputFolder);
            this.gbConfiguration.Controls.Add(this.btnOutputFolder);
            this.gbConfiguration.Controls.Add(this.txtOutputFolder);
            this.gbConfiguration.Controls.Add(this.lblScriptsMode);
            this.gbConfiguration.Controls.Add(this.chkOutputScripts);
            this.gbConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbConfiguration.Location = new System.Drawing.Point(0, 0);
            this.gbConfiguration.Margin = new System.Windows.Forms.Padding(4);
            this.gbConfiguration.Name = "gbConfiguration";
            this.gbConfiguration.Padding = new System.Windows.Forms.Padding(4);
            this.gbConfiguration.Size = new System.Drawing.Size(584, 411);
            this.gbConfiguration.TabIndex = 21;
            this.gbConfiguration.TabStop = false;
            this.gbConfiguration.Text = "Configuration";
            // 
            // cboMode
            // 
            this.cboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMode.FormattingEnabled = true;
            this.cboMode.Items.AddRange(new object[] {
            "Schema",
            "Data",
            "Schema & Data"});
            this.cboMode.Location = new System.Drawing.Point(139, 24);
            this.cboMode.Name = "cboMode";
            this.cboMode.Size = new System.Drawing.Size(121, 25);
            this.cboMode.TabIndex = 64;
            this.cboMode.SelectedIndexChanged += new System.EventHandler(this.cboMode_SelectedIndexChanged);
            // 
            // chkNcharToDoubleChar
            // 
            this.chkNcharToDoubleChar.Checked = true;
            this.chkNcharToDoubleChar.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNcharToDoubleChar.Enabled = false;
            this.chkNcharToDoubleChar.Location = new System.Drawing.Point(11, 270);
            this.chkNcharToDoubleChar.Margin = new System.Windows.Forms.Padding(4);
            this.chkNcharToDoubleChar.Name = "chkNcharToDoubleChar";
            this.chkNcharToDoubleChar.Size = new System.Drawing.Size(302, 21);
            this.chkNcharToDoubleChar.TabIndex = 63;
            this.chkNcharToDoubleChar.Tag = "Schema";
            this.chkNcharToDoubleChar.Text = "One nchar to two chars for data type";
            this.chkNcharToDoubleChar.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::DatabaseManager.Resources.Info16;
            this.pictureBox1.Location = new System.Drawing.Point(445, 300);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(17, 19);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 62;
            this.pictureBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox1, "It works without transaction");
            // 
            // chkCreateSchemaIfNotExists
            // 
            this.chkCreateSchemaIfNotExists.AutoSize = true;
            this.chkCreateSchemaIfNotExists.Checked = true;
            this.chkCreateSchemaIfNotExists.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCreateSchemaIfNotExists.Location = new System.Drawing.Point(246, 61);
            this.chkCreateSchemaIfNotExists.Margin = new System.Windows.Forms.Padding(4);
            this.chkCreateSchemaIfNotExists.Name = "chkCreateSchemaIfNotExists";
            this.chkCreateSchemaIfNotExists.Size = new System.Drawing.Size(183, 21);
            this.chkCreateSchemaIfNotExists.TabIndex = 61;
            this.chkCreateSchemaIfNotExists.Tag = "Schema";
            this.chkCreateSchemaIfNotExists.Text = "Create schema if not exists";
            this.chkCreateSchemaIfNotExists.UseVisualStyleBackColor = true;
            // 
            // chkExcludeGeometryForData
            // 
            this.chkExcludeGeometryForData.AutoSize = true;
            this.chkExcludeGeometryForData.Location = new System.Drawing.Point(293, 181);
            this.chkExcludeGeometryForData.Margin = new System.Windows.Forms.Padding(4);
            this.chkExcludeGeometryForData.Name = "chkExcludeGeometryForData";
            this.chkExcludeGeometryForData.Size = new System.Drawing.Size(156, 21);
            this.chkExcludeGeometryForData.TabIndex = 60;
            this.chkExcludeGeometryForData.Tag = "Data";
            this.chkExcludeGeometryForData.Text = "Ignore geometry data";
            this.chkExcludeGeometryForData.UseVisualStyleBackColor = true;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::DatabaseManager.Resources.Info16;
            this.pictureBox2.Location = new System.Drawing.Point(214, 61);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(17, 19);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox2.TabIndex = 59;
            this.pictureBox2.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox2, "If the target database is Oracle or MySql, no need to set.");
            // 
            // btnSetSchemaMappings
            // 
            this.btnSetSchemaMappings.Location = new System.Drawing.Point(138, 58);
            this.btnSetSchemaMappings.Margin = new System.Windows.Forms.Padding(4);
            this.btnSetSchemaMappings.Name = "btnSetSchemaMappings";
            this.btnSetSchemaMappings.Size = new System.Drawing.Size(67, 25);
            this.btnSetSchemaMappings.TabIndex = 58;
            this.btnSetSchemaMappings.Text = "Set";
            this.btnSetSchemaMappings.UseVisualStyleBackColor = true;
            this.btnSetSchemaMappings.Click += new System.EventHandler(this.btnSetSchemaMappings_Click);
            // 
            // chkGenerateComment
            // 
            this.chkGenerateComment.AutoSize = true;
            this.chkGenerateComment.Checked = true;
            this.chkGenerateComment.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateComment.Location = new System.Drawing.Point(165, 241);
            this.chkGenerateComment.Margin = new System.Windows.Forms.Padding(4);
            this.chkGenerateComment.Name = "chkGenerateComment";
            this.chkGenerateComment.Size = new System.Drawing.Size(138, 21);
            this.chkGenerateComment.TabIndex = 57;
            this.chkGenerateComment.Tag = "Schema";
            this.chkGenerateComment.Text = "Generate comment";
            this.chkGenerateComment.UseVisualStyleBackColor = true;
            // 
            // chkTreatBytesAsNull
            // 
            this.chkTreatBytesAsNull.AutoSize = true;
            this.chkTreatBytesAsNull.Location = new System.Drawing.Point(11, 181);
            this.chkTreatBytesAsNull.Margin = new System.Windows.Forms.Padding(4);
            this.chkTreatBytesAsNull.Name = "chkTreatBytesAsNull";
            this.chkTreatBytesAsNull.Size = new System.Drawing.Size(263, 21);
            this.chkTreatBytesAsNull.TabIndex = 55;
            this.chkTreatBytesAsNull.Tag = "Data";
            this.chkTreatBytesAsNull.Text = "Treat bytes data as null for data transfer";
            this.chkTreatBytesAsNull.UseVisualStyleBackColor = true;
            // 
            // chkOnlyCommentComputeExpression
            // 
            this.chkOnlyCommentComputeExpression.AutoSize = true;
            this.chkOnlyCommentComputeExpression.Enabled = false;
            this.chkOnlyCommentComputeExpression.Location = new System.Drawing.Point(293, 212);
            this.chkOnlyCommentComputeExpression.Margin = new System.Windows.Forms.Padding(4);
            this.chkOnlyCommentComputeExpression.Name = "chkOnlyCommentComputeExpression";
            this.chkOnlyCommentComputeExpression.Size = new System.Drawing.Size(228, 21);
            this.chkOnlyCommentComputeExpression.TabIndex = 22;
            this.chkOnlyCommentComputeExpression.Tag = "Schema";
            this.chkOnlyCommentComputeExpression.Text = "Only comment expression in script";
            this.chkOnlyCommentComputeExpression.UseVisualStyleBackColor = true;
            // 
            // chkComputeColumn
            // 
            this.chkComputeColumn.AutoSize = true;
            this.chkComputeColumn.Checked = true;
            this.chkComputeColumn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkComputeColumn.Location = new System.Drawing.Point(11, 211);
            this.chkComputeColumn.Margin = new System.Windows.Forms.Padding(4);
            this.chkComputeColumn.Name = "chkComputeColumn";
            this.chkComputeColumn.Size = new System.Drawing.Size(249, 21);
            this.chkComputeColumn.TabIndex = 18;
            this.chkComputeColumn.Tag = "Schema";
            this.chkComputeColumn.Text = "Convert compute column\'s expression";
            this.chkComputeColumn.UseVisualStyleBackColor = true;
            this.chkComputeColumn.Click += new System.EventHandler(this.chkComputeColumn_CheckedChanged);
            // 
            // chkContinueWhenErrorOccurs
            // 
            this.chkContinueWhenErrorOccurs.AutoSize = true;
            this.chkContinueWhenErrorOccurs.Location = new System.Drawing.Point(11, 300);
            this.chkContinueWhenErrorOccurs.Margin = new System.Windows.Forms.Padding(4);
            this.chkContinueWhenErrorOccurs.Name = "chkContinueWhenErrorOccurs";
            this.chkContinueWhenErrorOccurs.Size = new System.Drawing.Size(429, 21);
            this.chkContinueWhenErrorOccurs.TabIndex = 17;
            this.chkContinueWhenErrorOccurs.Tag = "Schema";
            this.chkContinueWhenErrorOccurs.Text = "Continue when error occurs for function, procedure, trigger and view";
            this.chkContinueWhenErrorOccurs.UseVisualStyleBackColor = true;
            // 
            // chkUseTransaction
            // 
            this.chkUseTransaction.AutoSize = true;
            this.chkUseTransaction.Checked = true;
            this.chkUseTransaction.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseTransaction.Location = new System.Drawing.Point(11, 120);
            this.chkUseTransaction.Margin = new System.Windows.Forms.Padding(4);
            this.chkUseTransaction.Name = "chkUseTransaction";
            this.chkUseTransaction.Size = new System.Drawing.Size(117, 21);
            this.chkUseTransaction.TabIndex = 16;
            this.chkUseTransaction.Text = "Use transaction";
            this.chkUseTransaction.UseVisualStyleBackColor = true;
            // 
            // chkBulkCopy
            // 
            this.chkBulkCopy.AutoSize = true;
            this.chkBulkCopy.Checked = true;
            this.chkBulkCopy.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBulkCopy.Location = new System.Drawing.Point(11, 152);
            this.chkBulkCopy.Margin = new System.Windows.Forms.Padding(4);
            this.chkBulkCopy.Name = "chkBulkCopy";
            this.chkBulkCopy.Size = new System.Drawing.Size(190, 21);
            this.chkBulkCopy.TabIndex = 15;
            this.chkBulkCopy.Tag = "Data";
            this.chkBulkCopy.Text = "Use BulkCopy to insert data";
            this.chkBulkCopy.UseVisualStyleBackColor = true;
            // 
            // chkGenerateIdentity
            // 
            this.chkGenerateIdentity.AutoSize = true;
            this.chkGenerateIdentity.Checked = true;
            this.chkGenerateIdentity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateIdentity.Location = new System.Drawing.Point(11, 240);
            this.chkGenerateIdentity.Margin = new System.Windows.Forms.Padding(4);
            this.chkGenerateIdentity.Name = "chkGenerateIdentity";
            this.chkGenerateIdentity.Size = new System.Drawing.Size(126, 21);
            this.chkGenerateIdentity.TabIndex = 14;
            this.chkGenerateIdentity.Tag = "Schema";
            this.chkGenerateIdentity.Text = "Generate identity";
            this.chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // lblDbSchemaMappings
            // 
            this.lblDbSchemaMappings.AutoSize = true;
            this.lblDbSchemaMappings.Location = new System.Drawing.Point(8, 61);
            this.lblDbSchemaMappings.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDbSchemaMappings.Name = "lblDbSchemaMappings";
            this.lblDbSchemaMappings.Size = new System.Drawing.Size(118, 17);
            this.lblDbSchemaMappings.TabIndex = 9;
            this.lblDbSchemaMappings.Text = "Schema mappings:";
            // 
            // chkGenerateSourceScripts
            // 
            this.chkGenerateSourceScripts.AutoSize = true;
            this.chkGenerateSourceScripts.Location = new System.Drawing.Point(183, 330);
            this.chkGenerateSourceScripts.Margin = new System.Windows.Forms.Padding(4);
            this.chkGenerateSourceScripts.Name = "chkGenerateSourceScripts";
            this.chkGenerateSourceScripts.Size = new System.Drawing.Size(269, 21);
            this.chkGenerateSourceScripts.TabIndex = 8;
            this.chkGenerateSourceScripts.Text = "Output scripts of source database as well";
            this.chkGenerateSourceScripts.UseVisualStyleBackColor = true;
            // 
            // chkExecuteOnTarget
            // 
            this.chkExecuteOnTarget.AutoSize = true;
            this.chkExecuteOnTarget.Checked = true;
            this.chkExecuteOnTarget.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExecuteOnTarget.Location = new System.Drawing.Point(11, 89);
            this.chkExecuteOnTarget.Margin = new System.Windows.Forms.Padding(4);
            this.chkExecuteOnTarget.Name = "chkExecuteOnTarget";
            this.chkExecuteOnTarget.Size = new System.Drawing.Size(229, 21);
            this.chkExecuteOnTarget.TabIndex = 7;
            this.chkExecuteOnTarget.Text = "Execute scripts on target database";
            this.chkExecuteOnTarget.UseVisualStyleBackColor = true;
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.Location = new System.Drawing.Point(9, 366);
            this.lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(131, 17);
            this.lblOutputFolder.TabIndex = 6;
            this.lblOutputFolder.Text = "Scripts output folder:";
            // 
            // btnOutputFolder
            // 
            this.btnOutputFolder.Location = new System.Drawing.Point(431, 363);
            this.btnOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            this.btnOutputFolder.Name = "btnOutputFolder";
            this.btnOutputFolder.Size = new System.Drawing.Size(42, 24);
            this.btnOutputFolder.TabIndex = 4;
            this.btnOutputFolder.Text = "...";
            this.btnOutputFolder.UseVisualStyleBackColor = true;
            this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Location = new System.Drawing.Point(150, 364);
            this.txtOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(277, 23);
            this.txtOutputFolder.TabIndex = 3;
            // 
            // lblScriptsMode
            // 
            this.lblScriptsMode.AutoSize = true;
            this.lblScriptsMode.Location = new System.Drawing.Point(8, 27);
            this.lblScriptsMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScriptsMode.Name = "lblScriptsMode";
            this.lblScriptsMode.Size = new System.Drawing.Size(129, 17);
            this.lblScriptsMode.TabIndex = 1;
            this.lblScriptsMode.Text = "Mode of conversion:";
            // 
            // chkOutputScripts
            // 
            this.chkOutputScripts.AutoSize = true;
            this.chkOutputScripts.Location = new System.Drawing.Point(11, 329);
            this.chkOutputScripts.Margin = new System.Windows.Forms.Padding(4);
            this.chkOutputScripts.Name = "chkOutputScripts";
            this.chkOutputScripts.Size = new System.Drawing.Size(146, 21);
            this.chkOutputScripts.TabIndex = 0;
            this.chkOutputScripts.Text = "Output scripts to file";
            this.chkOutputScripts.UseVisualStyleBackColor = true;
            this.chkOutputScripts.CheckedChanged += new System.EventHandler(this.chkOutputScripts_CheckedChanged);
            // 
            // targetDbProfile
            // 
            this.targetDbProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetDbProfile.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            this.targetDbProfile.EnableDatabaseType = true;
            this.targetDbProfile.Location = new System.Drawing.Point(13, 31);
            this.targetDbProfile.Margin = new System.Windows.Forms.Padding(0);
            this.targetDbProfile.Name = "targetDbProfile";
            this.targetDbProfile.Size = new System.Drawing.Size(792, 29);
            this.targetDbProfile.TabIndex = 37;
            this.targetDbProfile.Title = "Target:";
            this.targetDbProfile.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.targetDbProfile_OnSelectedChanged);
            // 
            // sourceDbProfile
            // 
            this.sourceDbProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceDbProfile.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            this.sourceDbProfile.EnableDatabaseType = true;
            this.sourceDbProfile.Location = new System.Drawing.Point(13, 1);
            this.sourceDbProfile.Margin = new System.Windows.Forms.Padding(0);
            this.sourceDbProfile.Name = "sourceDbProfile";
            this.sourceDbProfile.Size = new System.Drawing.Size(792, 28);
            this.sourceDbProfile.TabIndex = 36;
            this.sourceDbProfile.Title = "Source:";
            this.sourceDbProfile.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.sourceDbProfile_OnSelectedChanged);
            // 
            // btnFetch
            // 
            this.btnFetch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFetch.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnFetch.Location = new System.Drawing.Point(806, 3);
            this.btnFetch.Margin = new System.Windows.Forms.Padding(4);
            this.btnFetch.Name = "btnFetch";
            this.btnFetch.Size = new System.Drawing.Size(83, 54);
            this.btnFetch.TabIndex = 35;
            this.btnFetch.Text = "Fetch";
            this.btnFetch.UseVisualStyleBackColor = true;
            this.btnFetch.Click += new System.EventHandler(this.btnFetch_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(479, 484);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 33);
            this.btnCancel.TabIndex = 22;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnExecute.Enabled = false;
            this.btnExecute.Location = new System.Drawing.Point(366, 484);
            this.btnExecute.Margin = new System.Windows.Forms.Padding(4);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(88, 33);
            this.btnExecute.TabIndex = 21;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCopySelection});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(162, 26);
            // 
            // tsmiCopySelection
            // 
            this.tsmiCopySelection.Name = "tsmiCopySelection";
            this.tsmiCopySelection.Size = new System.Drawing.Size(161, 22);
            this.tsmiCopySelection.Text = "Copy selection";
            this.tsmiCopySelection.Click += new System.EventHandler(this.tsmiCopySelection_Click);
            // 
            // frmConvert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(899, 611);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmConvert";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Convert";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.gbConfiguration.ResumeLayout(false);
            this.gbConfiguration.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

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
    }
}

