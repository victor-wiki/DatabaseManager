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
            this.chkTreatBytesAsNull = new System.Windows.Forms.CheckBox();
            this.chkOnlyCommentComputeExpression = new System.Windows.Forms.CheckBox();
            this.chkComputeColumn = new System.Windows.Forms.CheckBox();
            this.chkSkipScriptError = new System.Windows.Forms.CheckBox();
            this.chkUseTransaction = new System.Windows.Forms.CheckBox();
            this.chkBulkCopy = new System.Windows.Forms.CheckBox();
            this.chkGenerateIdentity = new System.Windows.Forms.CheckBox();
            this.chkPickup = new System.Windows.Forms.CheckBox();
            this.chkScriptData = new System.Windows.Forms.CheckBox();
            this.chkScriptSchema = new System.Windows.Forms.CheckBox();
            this.txtTargetDbOwner = new System.Windows.Forms.TextBox();
            this.lblTargetDbOwner = new System.Windows.Forms.Label();
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
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnSaveMessage
            // 
            this.btnSaveMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveMessage.Image = global::DatabaseManager.Properties.Resources.Save;
            this.btnSaveMessage.Location = new System.Drawing.Point(738, 42);
            this.btnSaveMessage.Name = "btnSaveMessage";
            this.btnSaveMessage.Size = new System.Drawing.Size(27, 23);
            this.btnSaveMessage.TabIndex = 9;
            this.toolTip1.SetToolTip(this.btnSaveMessage, "Save message");
            this.btnSaveMessage.UseVisualStyleBackColor = true;
            this.btnSaveMessage.Click += new System.EventHandler(this.btnSaveMessage_Click);
            // 
            // btnCopyMessage
            // 
            this.btnCopyMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyMessage.Image = global::DatabaseManager.Properties.Resources.Copy;
            this.btnCopyMessage.Location = new System.Drawing.Point(739, 13);
            this.btnCopyMessage.Name = "btnCopyMessage";
            this.btnCopyMessage.Size = new System.Drawing.Size(27, 23);
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
            this.txtMessage.Location = new System.Drawing.Point(3, 2);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(731, 87);
            this.txtMessage.TabIndex = 0;
            this.txtMessage.Text = "";
            this.txtMessage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtMessage_MouseUp);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(1, 5);
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
            this.splitContainer1.Size = new System.Drawing.Size(770, 542);
            this.splitContainer1.SplitterDistance = 452;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 21;
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.tvDbObjects.Location = new System.Drawing.Point(11, 58);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(269, 359);
            this.tvDbObjects.TabIndex = 38;
            // 
            // targetDbProfile
            // 
            this.targetDbProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetDbProfile.Location = new System.Drawing.Point(11, 28);
            this.targetDbProfile.Margin = new System.Windows.Forms.Padding(0);
            this.targetDbProfile.Name = "targetDbProfile";
            this.targetDbProfile.Size = new System.Drawing.Size(679, 27);
            this.targetDbProfile.TabIndex = 37;
            this.targetDbProfile.Title = "Target:";
            this.targetDbProfile.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.targetDbProfile_OnSelectedChanged);
            // 
            // sourceDbProfile
            // 
            this.sourceDbProfile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceDbProfile.Location = new System.Drawing.Point(11, 1);
            this.sourceDbProfile.Margin = new System.Windows.Forms.Padding(0);
            this.sourceDbProfile.Name = "sourceDbProfile";
            this.sourceDbProfile.Size = new System.Drawing.Size(679, 27);
            this.sourceDbProfile.TabIndex = 36;
            this.sourceDbProfile.Title = "Source:";
            this.sourceDbProfile.OnSelectedChanged += new DatabaseManager.Controls.SelectedChangeHandler(this.sourceDbProfile_OnSelectedChanged);
            // 
            // btnFetch
            // 
            this.btnFetch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFetch.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnFetch.Location = new System.Drawing.Point(690, 2);
            this.btnFetch.Name = "btnFetch";
            this.btnFetch.Size = new System.Drawing.Size(71, 50);
            this.btnFetch.TabIndex = 35;
            this.btnFetch.Text = "Fetch";
            this.btnFetch.UseVisualStyleBackColor = true;
            this.btnFetch.Click += new System.EventHandler(this.btnFetch_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(415, 423);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 22;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnExecute.Enabled = false;
            this.btnExecute.Location = new System.Drawing.Point(318, 423);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
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
            this.gbOption.Controls.Add(this.chkTreatBytesAsNull);
            this.gbOption.Controls.Add(this.chkOnlyCommentComputeExpression);
            this.gbOption.Controls.Add(this.chkComputeColumn);
            this.gbOption.Controls.Add(this.chkSkipScriptError);
            this.gbOption.Controls.Add(this.chkUseTransaction);
            this.gbOption.Controls.Add(this.chkBulkCopy);
            this.gbOption.Controls.Add(this.chkGenerateIdentity);
            this.gbOption.Controls.Add(this.chkPickup);
            this.gbOption.Controls.Add(this.chkScriptData);
            this.gbOption.Controls.Add(this.chkScriptSchema);
            this.gbOption.Controls.Add(this.txtTargetDbOwner);
            this.gbOption.Controls.Add(this.lblTargetDbOwner);
            this.gbOption.Controls.Add(this.chkGenerateSourceScripts);
            this.gbOption.Controls.Add(this.chkExecuteOnTarget);
            this.gbOption.Controls.Add(this.lblOutputFolder);
            this.gbOption.Controls.Add(this.btnOutputFolder);
            this.gbOption.Controls.Add(this.txtOutputFolder);
            this.gbOption.Controls.Add(this.lblScriptsMode);
            this.gbOption.Controls.Add(this.chkOutputScripts);
            this.gbOption.Location = new System.Drawing.Point(286, 54);
            this.gbOption.Name = "gbOption";
            this.gbOption.Size = new System.Drawing.Size(475, 363);
            this.gbOption.TabIndex = 20;
            this.gbOption.TabStop = false;
            this.gbOption.Text = "Options";
            // 
            // chkTreatBytesAsNull
            // 
            this.chkTreatBytesAsNull.AutoSize = true;
            this.chkTreatBytesAsNull.Location = new System.Drawing.Point(8, 155);
            this.chkTreatBytesAsNull.Name = "chkTreatBytesAsNull";
            this.chkTreatBytesAsNull.Size = new System.Drawing.Size(276, 16);
            this.chkTreatBytesAsNull.TabIndex = 55;
            this.chkTreatBytesAsNull.Text = "Treat bytes data as null for data transfer";
            this.chkTreatBytesAsNull.UseVisualStyleBackColor = true;
            // 
            // chkOnlyCommentComputeExpression
            // 
            this.chkOnlyCommentComputeExpression.AutoSize = true;
            this.chkOnlyCommentComputeExpression.Enabled = false;
            this.chkOnlyCommentComputeExpression.Location = new System.Drawing.Point(250, 203);
            this.chkOnlyCommentComputeExpression.Name = "chkOnlyCommentComputeExpression";
            this.chkOnlyCommentComputeExpression.Size = new System.Drawing.Size(222, 16);
            this.chkOnlyCommentComputeExpression.TabIndex = 22;
            this.chkOnlyCommentComputeExpression.Text = "Only comment expression in script";
            this.chkOnlyCommentComputeExpression.UseVisualStyleBackColor = true;
            // 
            // chkComputeColumn
            // 
            this.chkComputeColumn.AutoSize = true;
            this.chkComputeColumn.Checked = true;
            this.chkComputeColumn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkComputeColumn.Location = new System.Drawing.Point(8, 203);
            this.chkComputeColumn.Name = "chkComputeColumn";
            this.chkComputeColumn.Size = new System.Drawing.Size(234, 16);
            this.chkComputeColumn.TabIndex = 18;
            this.chkComputeColumn.Text = "Convert compute column\'s expression";
            this.chkComputeColumn.UseVisualStyleBackColor = true;
            this.chkComputeColumn.CheckedChanged += new System.EventHandler(this.chkComputeColumn_CheckedChanged);
            // 
            // chkSkipScriptError
            // 
            this.chkSkipScriptError.AutoSize = true;
            this.chkSkipScriptError.Location = new System.Drawing.Point(8, 254);
            this.chkSkipScriptError.Name = "chkSkipScriptError";
            this.chkSkipScriptError.Size = new System.Drawing.Size(336, 16);
            this.chkSkipScriptError.TabIndex = 17;
            this.chkSkipScriptError.Text = "Skip error for function, procedure, trigger and view";
            this.chkSkipScriptError.UseVisualStyleBackColor = true;
            // 
            // chkUseTransaction
            // 
            this.chkUseTransaction.AutoSize = true;
            this.chkUseTransaction.Checked = true;
            this.chkUseTransaction.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseTransaction.Location = new System.Drawing.Point(8, 104);
            this.chkUseTransaction.Name = "chkUseTransaction";
            this.chkUseTransaction.Size = new System.Drawing.Size(114, 16);
            this.chkUseTransaction.TabIndex = 16;
            this.chkUseTransaction.Text = "Use transaction";
            this.chkUseTransaction.UseVisualStyleBackColor = true;
            // 
            // chkBulkCopy
            // 
            this.chkBulkCopy.AutoSize = true;
            this.chkBulkCopy.Checked = true;
            this.chkBulkCopy.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBulkCopy.Location = new System.Drawing.Point(8, 133);
            this.chkBulkCopy.Name = "chkBulkCopy";
            this.chkBulkCopy.Size = new System.Drawing.Size(186, 16);
            this.chkBulkCopy.TabIndex = 15;
            this.chkBulkCopy.Text = "Use BulkCopy to insert data";
            this.chkBulkCopy.UseVisualStyleBackColor = true;
            // 
            // chkGenerateIdentity
            // 
            this.chkGenerateIdentity.AutoSize = true;
            this.chkGenerateIdentity.Location = new System.Drawing.Point(8, 226);
            this.chkGenerateIdentity.Name = "chkGenerateIdentity";
            this.chkGenerateIdentity.Size = new System.Drawing.Size(126, 16);
            this.chkGenerateIdentity.TabIndex = 14;
            this.chkGenerateIdentity.Text = "Generate identity";
            this.chkGenerateIdentity.UseVisualStyleBackColor = true;
            // 
            // chkPickup
            // 
            this.chkPickup.AutoSize = true;
            this.chkPickup.Location = new System.Drawing.Point(8, 180);
            this.chkPickup.Name = "chkPickup";
            this.chkPickup.Size = new System.Drawing.Size(294, 16);
            this.chkPickup.TabIndex = 13;
            this.chkPickup.Text = "Continue from last error of data sync(if any)";
            this.chkPickup.UseVisualStyleBackColor = true;
            // 
            // chkScriptData
            // 
            this.chkScriptData.AutoSize = true;
            this.chkScriptData.Checked = true;
            this.chkScriptData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptData.Location = new System.Drawing.Point(129, 23);
            this.chkScriptData.Name = "chkScriptData";
            this.chkScriptData.Size = new System.Drawing.Size(48, 16);
            this.chkScriptData.TabIndex = 12;
            this.chkScriptData.Text = "data";
            this.chkScriptData.UseVisualStyleBackColor = true;
            // 
            // chkScriptSchema
            // 
            this.chkScriptSchema.AutoSize = true;
            this.chkScriptSchema.Checked = true;
            this.chkScriptSchema.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptSchema.Location = new System.Drawing.Point(49, 23);
            this.chkScriptSchema.Name = "chkScriptSchema";
            this.chkScriptSchema.Size = new System.Drawing.Size(60, 16);
            this.chkScriptSchema.TabIndex = 11;
            this.chkScriptSchema.Text = "schema";
            this.chkScriptSchema.UseVisualStyleBackColor = true;
            // 
            // txtTargetDbOwner
            // 
            this.txtTargetDbOwner.Location = new System.Drawing.Point(150, 47);
            this.txtTargetDbOwner.Name = "txtTargetDbOwner";
            this.txtTargetDbOwner.Size = new System.Drawing.Size(130, 21);
            this.txtTargetDbOwner.TabIndex = 10;
            // 
            // lblTargetDbOwner
            // 
            this.lblTargetDbOwner.AutoSize = true;
            this.lblTargetDbOwner.Location = new System.Drawing.Point(6, 50);
            this.lblTargetDbOwner.Name = "lblTargetDbOwner";
            this.lblTargetDbOwner.Size = new System.Drawing.Size(137, 12);
            this.lblTargetDbOwner.TabIndex = 9;
            this.lblTargetDbOwner.Text = "Target database owner:";
            // 
            // chkGenerateSourceScripts
            // 
            this.chkGenerateSourceScripts.AutoSize = true;
            this.chkGenerateSourceScripts.Location = new System.Drawing.Point(8, 310);
            this.chkGenerateSourceScripts.Name = "chkGenerateSourceScripts";
            this.chkGenerateSourceScripts.Size = new System.Drawing.Size(270, 16);
            this.chkGenerateSourceScripts.TabIndex = 8;
            this.chkGenerateSourceScripts.Text = "Output scripts of source database as well";
            this.chkGenerateSourceScripts.UseVisualStyleBackColor = true;
            // 
            // chkExecuteOnTarget
            // 
            this.chkExecuteOnTarget.AutoSize = true;
            this.chkExecuteOnTarget.Checked = true;
            this.chkExecuteOnTarget.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExecuteOnTarget.Location = new System.Drawing.Point(8, 75);
            this.chkExecuteOnTarget.Name = "chkExecuteOnTarget";
            this.chkExecuteOnTarget.Size = new System.Drawing.Size(228, 16);
            this.chkExecuteOnTarget.TabIndex = 7;
            this.chkExecuteOnTarget.Text = "Execute scripts on target database";
            this.chkExecuteOnTarget.UseVisualStyleBackColor = true;
            // 
            // lblOutputFolder
            // 
            this.lblOutputFolder.AutoSize = true;
            this.lblOutputFolder.Location = new System.Drawing.Point(8, 339);
            this.lblOutputFolder.Name = "lblOutputFolder";
            this.lblOutputFolder.Size = new System.Drawing.Size(137, 12);
            this.lblOutputFolder.TabIndex = 6;
            this.lblOutputFolder.Text = "Scripts output folder:";
            // 
            // btnOutputFolder
            // 
            this.btnOutputFolder.Location = new System.Drawing.Point(397, 335);
            this.btnOutputFolder.Name = "btnOutputFolder";
            this.btnOutputFolder.Size = new System.Drawing.Size(36, 23);
            this.btnOutputFolder.TabIndex = 4;
            this.btnOutputFolder.Text = "...";
            this.btnOutputFolder.UseVisualStyleBackColor = true;
            this.btnOutputFolder.Click += new System.EventHandler(this.btnOutputFolder_Click);
            // 
            // txtOutputFolder
            // 
            this.txtOutputFolder.Location = new System.Drawing.Point(154, 336);
            this.txtOutputFolder.Name = "txtOutputFolder";
            this.txtOutputFolder.Size = new System.Drawing.Size(238, 21);
            this.txtOutputFolder.TabIndex = 3;
            // 
            // lblScriptsMode
            // 
            this.lblScriptsMode.AutoSize = true;
            this.lblScriptsMode.Location = new System.Drawing.Point(7, 24);
            this.lblScriptsMode.Name = "lblScriptsMode";
            this.lblScriptsMode.Size = new System.Drawing.Size(35, 12);
            this.lblScriptsMode.TabIndex = 1;
            this.lblScriptsMode.Text = "Mode:";
            // 
            // chkOutputScripts
            // 
            this.chkOutputScripts.AutoSize = true;
            this.chkOutputScripts.Location = new System.Drawing.Point(8, 281);
            this.chkOutputScripts.Name = "chkOutputScripts";
            this.chkOutputScripts.Size = new System.Drawing.Size(156, 16);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(771, 549);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
        private System.Windows.Forms.CheckBox chkPickup;
        private System.Windows.Forms.CheckBox chkScriptData;
        private System.Windows.Forms.CheckBox chkScriptSchema;
        private System.Windows.Forms.TextBox txtTargetDbOwner;
        private System.Windows.Forms.Label lblTargetDbOwner;
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
        private System.Windows.Forms.CheckBox chkSkipScriptError;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopySelection;
        private System.Windows.Forms.CheckBox chkComputeColumn;
        private System.Windows.Forms.CheckBox chkOnlyCommentComputeExpression;
        private System.Windows.Forms.CheckBox chkTreatBytesAsNull;
    }
}

