using DatabaseInterpreter.Model;

namespace DatabaseManager
{
    partial class frmConvert
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
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
            this.tvDbObjects = new DatabaseManager.Controls.UC_DbObjectsSimpleTree();
            this.targetDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            this.sourceDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            this.btnFetch = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnExecute = new System.Windows.Forms.Button();
            this.gbOption = new System.Windows.Forms.GroupBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.btnSetSchemaMappings = new System.Windows.Forms.Button();
            this.chkGenerateComment = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.chkTreatBytesAsNull = new System.Windows.Forms.CheckBox();
            this.chkOnlyCommentComputeExpression = new System.Windows.Forms.CheckBox();
            this.chkComputeColumn = new System.Windows.Forms.CheckBox();
            this.chkContinueWhenErrorOccurs = new System.Windows.Forms.CheckBox();
            this.chkUseTransaction = new System.Windows.Forms.CheckBox();
            this.chkBulkCopy = new System.Windows.Forms.CheckBox();
            this.chkGenerateIdentity = new System.Windows.Forms.CheckBox();
            this.chkScriptData = new System.Windows.Forms.CheckBox();
            this.chkScriptSchema = new System.Windows.Forms.CheckBox();
            this.lblDbSchemaMappings = new System.Windows.Forms.Label();
            this.chkGenerateSourceScripts = new System.Windows.Forms.CheckBox();
            this.chkExecuteOnTarget = new System.Windows.Forms.CheckBox();
            this.lblOutputFolder = new System.Windows.Forms.Label();
            this.btnOutputFolder = new System.Windows.Forms.Button();
            this.txtOutputFolder = new System.Windows.Forms.TextBox();
            this.lblScriptsMode = new System.Windows.Forms.Label();
            this.chkOutputScripts = new System.Windows.Forms.CheckBox();
            this.dlgOutputFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCopySelection = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.gbOption.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
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
            this.splitContainer1.Panel1.Controls.Add(this.tvDbObjects);
            this.splitContainer1.Panel1.Controls.Add(this.targetDbProfile);
            this.splitContainer1.Panel1.Controls.Add(this.sourceDbProfile);
            this.splitContainer1.Panel1.Controls.Add(this.btnFetch);
            this.splitContainer1.Panel1.Controls.Add(this.btnCancel);
            this.splitContainer1.Panel1.Controls.Add(this.btnExecute);
            this.splitContainer1.Panel1.Controls.Add(this.gbOption);
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
            // tvDbObjects
            // 
            this.tvDbObjects.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvDbObjects.Location = new System.Drawing.Point(13, 66);
            this.tvDbObjects.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(277, 407);
            this.tvDbObjects.TabIndex = 38;
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
            this.btnFetch.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
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
            // gbOption
            // 
            this.gbOption.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbOption.Controls.Add(this.pictureBox2);
            this.gbOption.Controls.Add(this.btnSetSchemaMappings);
            this.gbOption.Controls.Add(this.chkGenerateComment);
            this.gbOption.Controls.Add(this.pictureBox1);
            this.gbOption.Controls.Add(this.chkTreatBytesAsNull);
            this.gbOption.Controls.Add(this.chkOnlyCommentComputeExpression);
            this.gbOption.Controls.Add(this.chkComputeColumn);
            this.gbOption.Controls.Add(this.chkContinueWhenErrorOccurs);
            this.gbOption.Controls.Add(this.chkUseTransaction);
            this.gbOption.Controls.Add(this.chkBulkCopy);
            this.gbOption.Controls.Add(this.chkGenerateIdentity);
            this.gbOption.Controls.Add(this.chkScriptData);
            this.gbOption.Controls.Add(this.chkScriptSchema);
            this.gbOption.Controls.Add(this.lblDbSchemaMappings);
            this.gbOption.Controls.Add(this.chkGenerateSourceScripts);
            this.gbOption.Controls.Add(this.chkExecuteOnTarget);
            this.gbOption.Controls.Add(this.lblOutputFolder);
            this.gbOption.Controls.Add(this.btnOutputFolder);
            this.gbOption.Controls.Add(this.txtOutputFolder);
            this.gbOption.Controls.Add(this.lblScriptsMode);
            this.gbOption.Controls.Add(this.chkOutputScripts);
            this.gbOption.Location = new System.Drawing.Point(297, 64);
            this.gbOption.Margin = new System.Windows.Forms.Padding(4);
            this.gbOption.Name = "gbOption";
            this.gbOption.Padding = new System.Windows.Forms.Padding(4);
            this.gbOption.Size = new System.Drawing.Size(591, 410);
            this.gbOption.TabIndex = 20;
            this.gbOption.TabStop = false;
            this.gbOption.Text = "Options";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::DatabaseManager.Resources.Info16;
            this.pictureBox2.Location = new System.Drawing.Point(211, 53);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(17, 19);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox2.TabIndex = 59;
            this.pictureBox2.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox2, "If the target database is Oracle or MySql, no need to set.");
            // 
            // btnSetSchemaMappings
            // 
            this.btnSetSchemaMappings.Location = new System.Drawing.Point(136, 49);
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
            this.chkGenerateComment.Location = new System.Drawing.Point(10, 262);
            this.chkGenerateComment.Margin = new System.Windows.Forms.Padding(4);
            this.chkGenerateComment.Name = "chkGenerateComment";
            this.chkGenerateComment.Size = new System.Drawing.Size(140, 21);
            this.chkGenerateComment.TabIndex = 57;
            this.chkGenerateComment.Text = "Generate Comment";
            this.chkGenerateComment.UseVisualStyleBackColor = true;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::DatabaseManager.Resources.Info16;
            this.pictureBox1.Location = new System.Drawing.Point(202, 146);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(17, 19);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 56;
            this.pictureBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox1, "If the target database is Oracle and it has geometry data type, don\'t check this." +
        "");
            // 
            // chkTreatBytesAsNull
            // 
            this.chkTreatBytesAsNull.AutoSize = true;
            this.chkTreatBytesAsNull.Location = new System.Drawing.Point(10, 174);
            this.chkTreatBytesAsNull.Margin = new System.Windows.Forms.Padding(4);
            this.chkTreatBytesAsNull.Name = "chkTreatBytesAsNull";
            this.chkTreatBytesAsNull.Size = new System.Drawing.Size(263, 21);
            this.chkTreatBytesAsNull.TabIndex = 55;
            this.chkTreatBytesAsNull.Text = "Treat bytes data as null for data transfer";
            this.chkTreatBytesAsNull.UseVisualStyleBackColor = true;
            // 
            // chkOnlyCommentComputeExpression
            // 
            this.chkOnlyCommentComputeExpression.AutoSize = true;
            this.chkOnlyCommentComputeExpression.Enabled = false;
            this.chkOnlyCommentComputeExpression.Location = new System.Drawing.Point(292, 205);
            this.chkOnlyCommentComputeExpression.Margin = new System.Windows.Forms.Padding(4);
            this.chkOnlyCommentComputeExpression.Name = "chkOnlyCommentComputeExpression";
            this.chkOnlyCommentComputeExpression.Size = new System.Drawing.Size(228, 21);
            this.chkOnlyCommentComputeExpression.TabIndex = 22;
            this.chkOnlyCommentComputeExpression.Text = "Only comment expression in script";
            this.chkOnlyCommentComputeExpression.UseVisualStyleBackColor = true;
            // 
            // chkComputeColumn
            // 
            this.chkComputeColumn.AutoSize = true;
            this.chkComputeColumn.Checked = true;
            this.chkComputeColumn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkComputeColumn.Location = new System.Drawing.Point(10, 204);
            this.chkComputeColumn.Margin = new System.Windows.Forms.Padding(4);
            this.chkComputeColumn.Name = "chkComputeColumn";
            this.chkComputeColumn.Size = new System.Drawing.Size(249, 21);
            this.chkComputeColumn.TabIndex = 18;
            this.chkComputeColumn.Text = "Convert compute column\'s expression";
            this.chkComputeColumn.UseVisualStyleBackColor = true;
            this.chkComputeColumn.CheckedChanged += new System.EventHandler(this.chkComputeColumn_CheckedChanged);
            // 
            // chkContinueWhenErrorOccurs
            // 
            this.chkContinueWhenErrorOccurs.AutoSize = true;
            this.chkContinueWhenErrorOccurs.Location = new System.Drawing.Point(10, 293);
            this.chkContinueWhenErrorOccurs.Margin = new System.Windows.Forms.Padding(4);
            this.chkContinueWhenErrorOccurs.Name = "chkContinueWhenErrorOccurs";
            this.chkContinueWhenErrorOccurs.Size = new System.Drawing.Size(429, 21);
            this.chkContinueWhenErrorOccurs.TabIndex = 17;
            this.chkContinueWhenErrorOccurs.Text = "Continue when error occurs for function, procedure, trigger and view";
            this.chkContinueWhenErrorOccurs.UseVisualStyleBackColor = true;
            // 
            // chkUseTransaction
            // 
            this.chkUseTransaction.AutoSize = true;
            this.chkUseTransaction.Checked = true;
            this.chkUseTransaction.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseTransaction.Location = new System.Drawing.Point(10, 113);
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
            this.chkBulkCopy.Location = new System.Drawing.Point(10, 145);
            this.chkBulkCopy.Margin = new System.Windows.Forms.Padding(4);
            this.chkBulkCopy.Name = "chkBulkCopy";
            this.chkBulkCopy.Size = new System.Drawing.Size(190, 21);
            this.chkBulkCopy.TabIndex = 15;
            this.chkBulkCopy.Text = "Use BulkCopy to insert data";
            this.chkBulkCopy.UseVisualStyleBackColor = true;
            // 
            // chkGenerateIdentity
            // 
            this.chkGenerateIdentity.AutoSize = true;
            this.chkGenerateIdentity.Checked = true;
            this.chkGenerateIdentity.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateIdentity.Location = new System.Drawing.Point(10, 233);
            this.chkGenerateIdentity.Margin = new System.Windows.Forms.Padding(4);
            this.chkGenerateIdentity.Name = "chkGenerateIdentity";
            this.chkGenerateIdentity.Size = new System.Drawing.Size(126, 21);
            this.chkGenerateIdentity.TabIndex = 14;
            this.chkGenerateIdentity.Text = "Generate identity";
            this.chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // chkScriptData
            // 
            this.chkScriptData.AutoSize = true;
            this.chkScriptData.Checked = true;
            this.chkScriptData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptData.Location = new System.Drawing.Point(150, 25);
            this.chkScriptData.Margin = new System.Windows.Forms.Padding(4);
            this.chkScriptData.Name = "chkScriptData";
            this.chkScriptData.Size = new System.Drawing.Size(53, 21);
            this.chkScriptData.TabIndex = 12;
            this.chkScriptData.Text = "data";
            this.chkScriptData.UseVisualStyleBackColor = true;
            // 
            // chkScriptSchema
            // 
            this.chkScriptSchema.AutoSize = true;
            this.chkScriptSchema.Checked = true;
            this.chkScriptSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptSchema.Location = new System.Drawing.Point(57, 25);
            this.chkScriptSchema.Margin = new System.Windows.Forms.Padding(4);
            this.chkScriptSchema.Name = "chkScriptSchema";
            this.chkScriptSchema.Size = new System.Drawing.Size(71, 21);
            this.chkScriptSchema.TabIndex = 11;
            this.chkScriptSchema.Text = "schema";
            this.chkScriptSchema.UseVisualStyleBackColor = true;
            // 
            // lblDbSchemaMappings
            // 
            this.lblDbSchemaMappings.AutoSize = true;
            this.lblDbSchemaMappings.Location = new System.Drawing.Point(7, 52);
            this.lblDbSchemaMappings.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDbSchemaMappings.Name = "lblDbSchemaMappings";
            this.lblDbSchemaMappings.Size = new System.Drawing.Size(118, 17);
            this.lblDbSchemaMappings.TabIndex = 9;
            this.lblDbSchemaMappings.Text = "Schema mappings:";
            // 
            // chkGenerateSourceScripts
            // 
            this.chkGenerateSourceScripts.AutoSize = true;
            this.chkGenerateSourceScripts.Location = new System.Drawing.Point(182, 323);
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
            this.chkExecuteOnTarget.Location = new System.Drawing.Point(10, 82);
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
            this.lblOutputFolder.Location = new System.Drawing.Point(9, 359);
            this.lblOutputFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(131, 17);
            this.lblOutputFolder.TabIndex = 6;
            this.lblOutputFolder.Text = "Scripts output folder:";
            // 
            // btnOutputFolder
            // 
            this.btnOutputFolder.Location = new System.Drawing.Point(431, 357);
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
            this.txtOutputFolder.Location = new System.Drawing.Point(150, 358);
            this.txtOutputFolder.Margin = new System.Windows.Forms.Padding(4);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(277, 23);
            this.txtOutputFolder.TabIndex = 3;
            // 
            // lblScriptsMode
            // 
            this.lblScriptsMode.AutoSize = true;
            this.lblScriptsMode.Location = new System.Drawing.Point(8, 26);
            this.lblScriptsMode.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScriptsMode.Name = "lblScriptsMode";
            this.lblScriptsMode.Size = new System.Drawing.Size(46, 17);
            this.lblScriptsMode.TabIndex = 1;
            this.lblScriptsMode.Text = "Mode:";
            // 
            // chkOutputScripts
            // 
            this.chkOutputScripts.AutoSize = true;
            this.chkOutputScripts.Location = new System.Drawing.Point(10, 322);
            this.chkOutputScripts.Margin = new System.Windows.Forms.Padding(4);
            this.chkOutputScripts.Name = "chkOutputScripts";
            this.chkOutputScripts.Size = new System.Drawing.Size(146, 21);
            this.chkOutputScripts.TabIndex = 0;
            this.chkOutputScripts.Text = "Output scripts to file";
            this.chkOutputScripts.UseVisualStyleBackColor = true;
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
            this.gbOption.ResumeLayout(false);
            this.gbOption.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
        private System.Windows.Forms.GroupBox gbOption;
        private System.Windows.Forms.CheckBox chkBulkCopy;
        private System.Windows.Forms.CheckBox chkGenerateIdentity;
        private System.Windows.Forms.CheckBox chkScriptData;
        private System.Windows.Forms.CheckBox chkScriptSchema;
        private System.Windows.Forms.Label lblDbSchemaMappings;
        private System.Windows.Forms.CheckBox chkGenerateSourceScripts;
        private System.Windows.Forms.CheckBox chkExecuteOnTarget;
        private System.Windows.Forms.Label lblOutputFolder;
        private System.Windows.Forms.Button btnOutputFolder;
        private System.Windows.Forms.TextBox txtOutputFolder;
        private System.Windows.Forms.Label lblScriptsMode;
        private System.Windows.Forms.CheckBox chkOutputScripts;
        private System.Windows.Forms.FolderBrowserDialog dlgOutputFolder;
        private System.Windows.Forms.CheckBox chkUseTransaction;
        private System.Windows.Forms.ToolTip toolTip1;
        private Controls.UC_DbConnectionProfile sourceDbProfile;
        private Controls.UC_DbConnectionProfile targetDbProfile;
        private Controls.UC_DbObjectsSimpleTree tvDbObjects;
        private System.Windows.Forms.CheckBox chkContinueWhenErrorOccurs;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopySelection;
        private System.Windows.Forms.CheckBox chkComputeColumn;
        private System.Windows.Forms.CheckBox chkOnlyCommentComputeExpression;
        private System.Windows.Forms.CheckBox chkTreatBytesAsNull;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox chkGenerateComment;
        private System.Windows.Forms.Button btnSetSchemaMappings;
        private System.Windows.Forms.PictureBox pictureBox2;
    }
}

