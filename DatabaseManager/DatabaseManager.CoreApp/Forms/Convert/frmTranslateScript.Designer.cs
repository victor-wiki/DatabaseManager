namespace DatabaseManager.Forms
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTranslateScript));
            cboTargetDbType = new System.Windows.Forms.ComboBox();
            cboSourceDbType = new System.Windows.Forms.ComboBox();
            lblSource = new System.Windows.Forms.Label();
            lblTarget = new System.Windows.Forms.Label();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            txtSource = new SqlCodeEditor.TextEditorControlEx();
            txtTarget = new SqlCodeEditor.TextEditorControlEx();
            btnTranlate = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            btnClear = new System.Windows.Forms.Button();
            btnExchange = new System.Windows.Forms.Button();
            chkHighlighting = new System.Windows.Forms.CheckBox();
            chkValidateScriptsAfterTranslated = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // cboTargetDbType
            // 
            cboTargetDbType.BackColor = System.Drawing.Color.White;
            cboTargetDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboTargetDbType.FormattingEnabled = true;
            cboTargetDbType.Location = new System.Drawing.Point(342, 8);
            cboTargetDbType.Margin = new System.Windows.Forms.Padding(4);
            cboTargetDbType.MaxDropDownItems = 100;
            cboTargetDbType.Name = "cboTargetDbType";
            cboTargetDbType.Size = new System.Drawing.Size(124, 25);
            cboTargetDbType.TabIndex = 42;
            cboTargetDbType.SelectedIndexChanged += cboTargetDbType_SelectedIndexChanged;
            // 
            // cboSourceDbType
            // 
            cboSourceDbType.BackColor = System.Drawing.Color.White;
            cboSourceDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboSourceDbType.FormattingEnabled = true;
            cboSourceDbType.Location = new System.Drawing.Point(69, 8);
            cboSourceDbType.Margin = new System.Windows.Forms.Padding(4);
            cboSourceDbType.Name = "cboSourceDbType";
            cboSourceDbType.Size = new System.Drawing.Size(124, 25);
            cboSourceDbType.TabIndex = 41;
            cboSourceDbType.SelectedIndexChanged += cboSourceDbType_SelectedIndexChanged;
            // 
            // lblSource
            // 
            lblSource.AutoSize = true;
            lblSource.Location = new System.Drawing.Point(11, 11);
            lblSource.Name = "lblSource";
            lblSource.Size = new System.Drawing.Size(51, 17);
            lblSource.TabIndex = 43;
            lblSource.Text = "Source:";
            // 
            // lblTarget
            // 
            lblTarget.AutoSize = true;
            lblTarget.Location = new System.Drawing.Point(287, 11);
            lblTarget.Name = "lblTarget";
            lblTarget.Size = new System.Drawing.Size(49, 17);
            lblTarget.TabIndex = 44;
            lblTarget.Text = "Target:";
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(0, 43);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(txtSource);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(txtTarget);
            splitContainer1.Size = new System.Drawing.Size(851, 396);
            splitContainer1.SplitterDistance = 400;
            splitContainer1.SplitterWidth = 10;
            splitContainer1.TabIndex = 45;
            // 
            // txtSource
            // 
            txtSource.ContextMenuEnabled = true;
            txtSource.ContextMenuShowDefaultIcons = true;
            txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
            txtSource.EnableFolding = false;
            txtSource.FoldingStrategy = null;
            txtSource.Font = new System.Drawing.Font("Courier New", 10F);
            txtSource.Location = new System.Drawing.Point(0, 0);
            txtSource.Name = "txtSource";
            txtSource.ShowVRuler = false;
            txtSource.Size = new System.Drawing.Size(400, 396);
            txtSource.SyntaxHighlighting = "\"\"";
            txtSource.TabIndex = 0;
            txtSource.KeyDown += txtSource_KeyDown;
            txtSource.KeyUp += txtSource_KeyUp;
            // 
            // txtTarget
            // 
            txtTarget.ContextMenuEnabled = true;
            txtTarget.ContextMenuShowDefaultIcons = true;
            txtTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            txtTarget.EnableFolding = false;
            txtTarget.FoldingStrategy = null;
            txtTarget.Font = new System.Drawing.Font("Courier New", 10F);
            txtTarget.Location = new System.Drawing.Point(0, 0);
            txtTarget.Name = "txtTarget";
            txtTarget.ShowVRuler = false;
            txtTarget.Size = new System.Drawing.Size(441, 396);
            txtTarget.SyntaxHighlighting = "\"\"";
            txtTarget.TabIndex = 0;
            // 
            // btnTranlate
            // 
            btnTranlate.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnTranlate.Location = new System.Drawing.Point(671, 445);
            btnTranlate.Name = "btnTranlate";
            btnTranlate.Size = new System.Drawing.Size(75, 30);
            btnTranlate.TabIndex = 46;
            btnTranlate.Text = "Translate";
            btnTranlate.UseVisualStyleBackColor = true;
            btnTranlate.Click += btnTranlate_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnClose.Location = new System.Drawing.Point(769, 445);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(75, 30);
            btnClose.TabIndex = 47;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnClear
            // 
            btnClear.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnClear.Location = new System.Drawing.Point(11, 445);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(75, 30);
            btnClear.TabIndex = 48;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            // 
            // btnExchange
            // 
            btnExchange.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            btnExchange.Location = new System.Drawing.Point(219, 8);
            btnExchange.Name = "btnExchange";
            btnExchange.Size = new System.Drawing.Size(49, 25);
            btnExchange.TabIndex = 49;
            btnExchange.UseVisualStyleBackColor = true;
            btnExchange.Click += btnExchange_Click;
            // 
            // chkHighlighting
            // 
            chkHighlighting.AutoSize = true;
            chkHighlighting.Checked = true;
            chkHighlighting.CheckState = System.Windows.Forms.CheckState.Checked;
            chkHighlighting.Location = new System.Drawing.Point(491, 12);
            chkHighlighting.Name = "chkHighlighting";
            chkHighlighting.Size = new System.Drawing.Size(122, 21);
            chkHighlighting.TabIndex = 50;
            chkHighlighting.Text = "Highlighting text";
            chkHighlighting.UseVisualStyleBackColor = true;
            chkHighlighting.CheckedChanged += chkHighlighting_CheckedChanged;
            // 
            // chkValidateScriptsAfterTranslated
            // 
            chkValidateScriptsAfterTranslated.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            chkValidateScriptsAfterTranslated.AutoSize = true;
            chkValidateScriptsAfterTranslated.Location = new System.Drawing.Point(632, 13);
            chkValidateScriptsAfterTranslated.Name = "chkValidateScriptsAfterTranslated";
            chkValidateScriptsAfterTranslated.Size = new System.Drawing.Size(209, 21);
            chkValidateScriptsAfterTranslated.TabIndex = 51;
            chkValidateScriptsAfterTranslated.Text = "Validate scripts after translated";
            chkValidateScriptsAfterTranslated.UseVisualStyleBackColor = true;
            // 
            // frmTranslateScript
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(853, 489);
            Controls.Add(chkValidateScriptsAfterTranslated);
            Controls.Add(chkHighlighting);
            Controls.Add(btnExchange);
            Controls.Add(btnClear);
            Controls.Add(btnClose);
            Controls.Add(btnTranlate);
            Controls.Add(splitContainer1);
            Controls.Add(lblTarget);
            Controls.Add(lblSource);
            Controls.Add(cboTargetDbType);
            Controls.Add(cboSourceDbType);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Name = "frmTranslateScript";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Translate Script";
            Load += frmTranslateScript_Load;
            KeyDown += frmTranslateScript_KeyDown;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox cboTargetDbType;
        private System.Windows.Forms.ComboBox cboSourceDbType;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnTranlate;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnExchange;
        private System.Windows.Forms.CheckBox chkHighlighting;
        private System.Windows.Forms.CheckBox chkValidateScriptsAfterTranslated;
        private SqlCodeEditor.TextEditorControlEx txtSource;
        private SqlCodeEditor.TextEditorControlEx txtTarget;
    }
}