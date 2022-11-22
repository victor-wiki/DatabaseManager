namespace DatabaseManager
{
    partial class frmTranslateScript
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTranslateScript));
            this.cboTargetDbType = new System.Windows.Forms.ComboBox();
            this.cboSourceDbType = new System.Windows.Forms.ComboBox();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblTarget = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.txtSource = new System.Windows.Forms.RichTextBox();
            this.txtTarget = new System.Windows.Forms.RichTextBox();
            this.btnTranlate = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.sourceContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.targetContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiValidateScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExchange = new System.Windows.Forms.Button();
            this.chkHighlighting = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.chkValidateScriptsAfterTranslated = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.sourceContextMenuStrip.SuspendLayout();
            this.targetContextMenuStrip.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboTargetDbType
            // 
            this.cboTargetDbType.BackColor = System.Drawing.Color.White;
            this.cboTargetDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTargetDbType.FormattingEnabled = true;
            this.cboTargetDbType.Location = new System.Drawing.Point(342, 8);
            this.cboTargetDbType.Margin = new System.Windows.Forms.Padding(4);
            this.cboTargetDbType.MaxDropDownItems = 100;
            this.cboTargetDbType.Name = "cboTargetDbType";
            this.cboTargetDbType.Size = new System.Drawing.Size(124, 25);
            this.cboTargetDbType.TabIndex = 42;
            // 
            // cboSourceDbType
            // 
            this.cboSourceDbType.BackColor = System.Drawing.Color.White;
            this.cboSourceDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSourceDbType.FormattingEnabled = true;
            this.cboSourceDbType.Location = new System.Drawing.Point(69, 8);
            this.cboSourceDbType.Margin = new System.Windows.Forms.Padding(4);
            this.cboSourceDbType.Name = "cboSourceDbType";
            this.cboSourceDbType.Size = new System.Drawing.Size(124, 25);
            this.cboSourceDbType.TabIndex = 41;
            // 
            // lblSource
            // 
            this.lblSource.AutoSize = true;
            this.lblSource.Location = new System.Drawing.Point(11, 11);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(51, 17);
            this.lblSource.TabIndex = 43;
            this.lblSource.Text = "Source:";
            // 
            // lblTarget
            // 
            this.lblTarget.AutoSize = true;
            this.lblTarget.Location = new System.Drawing.Point(287, 11);
            this.lblTarget.Name = "lblTarget";
            this.lblTarget.Size = new System.Drawing.Size(49, 17);
            this.lblTarget.TabIndex = 44;
            this.lblTarget.Text = "Target:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 43);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtSource);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtTarget);
            this.splitContainer1.Size = new System.Drawing.Size(851, 396);
            this.splitContainer1.SplitterDistance = 400;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 45;
            // 
            // txtSource
            // 
            this.txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSource.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtSource.Location = new System.Drawing.Point(0, 0);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(400, 396);
            this.txtSource.TabIndex = 0;
            this.txtSource.Text = "";
            this.txtSource.SelectionChanged += new System.EventHandler(this.txtSource_SelectionChanged);
            this.txtSource.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSource_KeyDown);
            this.txtSource.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSource_KeyUp);
            this.txtSource.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtSource_MouseUp);
            // 
            // txtTarget
            // 
            this.txtTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtTarget.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtTarget.Location = new System.Drawing.Point(0, 0);
            this.txtTarget.Name = "txtTarget";
            this.txtTarget.Size = new System.Drawing.Size(450, 396);
            this.txtTarget.TabIndex = 1;
            this.txtTarget.Text = "";
            this.txtTarget.MouseUp += new System.Windows.Forms.MouseEventHandler(this.txtTarget_MouseUp);
            // 
            // btnTranlate
            // 
            this.btnTranlate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTranlate.Location = new System.Drawing.Point(671, 445);
            this.btnTranlate.Name = "btnTranlate";
            this.btnTranlate.Size = new System.Drawing.Size(75, 30);
            this.btnTranlate.TabIndex = 46;
            this.btnTranlate.Text = "Translate";
            this.toolTip1.SetToolTip(this.btnTranlate, "F5");
            this.btnTranlate.UseVisualStyleBackColor = true;
            this.btnTranlate.Click += new System.EventHandler(this.btnTranlate_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(769, 445);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 30);
            this.btnClose.TabIndex = 47;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(11, 445);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 30);
            this.btnClear.TabIndex = 48;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // sourceContextMenuStrip
            // 
            this.sourceContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiPaste});
            this.sourceContextMenuStrip.Name = "sourceContextMenuStrip";
            this.sourceContextMenuStrip.Size = new System.Drawing.Size(108, 26);
            // 
            // tsmiPaste
            // 
            this.tsmiPaste.Name = "tsmiPaste";
            this.tsmiPaste.Size = new System.Drawing.Size(107, 22);
            this.tsmiPaste.Text = "Paste";
            this.tsmiPaste.Click += new System.EventHandler(this.tsmiPaste_Click);
            // 
            // targetContextMenuStrip
            // 
            this.targetContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCopy,
            this.tsmiValidateScripts});
            this.targetContextMenuStrip.Name = "sourceContextMenuStrip";
            this.targetContextMenuStrip.Size = new System.Drawing.Size(167, 48);
            this.targetContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.targetContextMenuStrip_Opening);
            // 
            // tsmiCopy
            // 
            this.tsmiCopy.Name = "tsmiCopy";
            this.tsmiCopy.Size = new System.Drawing.Size(166, 22);
            this.tsmiCopy.Text = "Copy";
            this.tsmiCopy.Click += new System.EventHandler(this.tsmiCopy_Click);
            // 
            // tsmiValidateScripts
            // 
            this.tsmiValidateScripts.Name = "tsmiValidateScripts";
            this.tsmiValidateScripts.Size = new System.Drawing.Size(166, 22);
            this.tsmiValidateScripts.Text = "Validate Scripts";
            this.tsmiValidateScripts.Click += new System.EventHandler(this.tsmiValidateScripts_Click);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2});
            this.contextMenuStrip2.Name = "sourceContextMenuStrip";
            this.contextMenuStrip2.Size = new System.Drawing.Size(108, 26);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(107, 22);
            this.toolStripMenuItem2.Text = "Paste";
            // 
            // btnExchange
            // 
            this.btnExchange.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnExchange.Image = global::DatabaseManager.Resources.Exchange;
            this.btnExchange.Location = new System.Drawing.Point(219, 8);
            this.btnExchange.Name = "btnExchange";
            this.btnExchange.Size = new System.Drawing.Size(49, 25);
            this.btnExchange.TabIndex = 49;
            this.btnExchange.UseVisualStyleBackColor = true;
            this.btnExchange.Click += new System.EventHandler(this.btnExchange_Click);
            // 
            // chkHighlighting
            // 
            this.chkHighlighting.AutoSize = true;
            this.chkHighlighting.Checked = true;
            this.chkHighlighting.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHighlighting.Location = new System.Drawing.Point(491, 12);
            this.chkHighlighting.Name = "chkHighlighting";
            this.chkHighlighting.Size = new System.Drawing.Size(122, 21);
            this.chkHighlighting.TabIndex = 50;
            this.chkHighlighting.Text = "Highlighting text";
            this.chkHighlighting.UseVisualStyleBackColor = true;
            this.chkHighlighting.CheckedChanged += new System.EventHandler(this.chkHighlighting_CheckedChanged);
            // 
            // chkValidateScriptsAfterTranslated
            // 
            this.chkValidateScriptsAfterTranslated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkValidateScriptsAfterTranslated.AutoSize = true;
            this.chkValidateScriptsAfterTranslated.Location = new System.Drawing.Point(632, 13);
            this.chkValidateScriptsAfterTranslated.Name = "chkValidateScriptsAfterTranslated";
            this.chkValidateScriptsAfterTranslated.Size = new System.Drawing.Size(209, 21);
            this.chkValidateScriptsAfterTranslated.TabIndex = 51;
            this.chkValidateScriptsAfterTranslated.Text = "Validate scripts after translated";
            this.chkValidateScriptsAfterTranslated.UseVisualStyleBackColor = true;
            // 
            // frmTranslateScript
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(853, 489);
            this.Controls.Add(this.chkValidateScriptsAfterTranslated);
            this.Controls.Add(this.chkHighlighting);
            this.Controls.Add(this.btnExchange);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnTranlate);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.lblTarget);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.cboTargetDbType);
            this.Controls.Add(this.cboSourceDbType);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "frmTranslateScript";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Translate Script";
            this.Load += new System.EventHandler(this.frmTranslateScript_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmTranslateScript_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.sourceContextMenuStrip.ResumeLayout(false);
            this.targetContextMenuStrip.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboTargetDbType;
        private System.Windows.Forms.ComboBox cboSourceDbType;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox txtSource;
        private System.Windows.Forms.RichTextBox txtTarget;
        private System.Windows.Forms.Button btnTranlate;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.ContextMenuStrip sourceContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem tsmiPaste;
        private System.Windows.Forms.ContextMenuStrip targetContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.Button btnExchange;
        private System.Windows.Forms.CheckBox chkHighlighting;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiValidateScripts;
        private System.Windows.Forms.CheckBox chkValidateScriptsAfterTranslated;
    }
}