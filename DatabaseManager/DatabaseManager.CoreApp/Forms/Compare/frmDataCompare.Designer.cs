namespace DatabaseManager.Forms
{
    partial class frmDataCompare
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDataCompare));
            targetDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            sourceDbProfile = new DatabaseManager.Controls.UC_DbConnectionProfile();
            groupBox1 = new System.Windows.Forms.GroupBox();
            chkIdenticalRecords = new System.Windows.Forms.CheckBox();
            chkOnlyInTargetRecords = new System.Windows.Forms.CheckBox();
            chkOnlyInSourceRecords = new System.Windows.Forms.CheckBox();
            chkDifferentRecords = new System.Windows.Forms.CheckBox();
            btnCompare = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            txtMessage = new System.Windows.Forms.TextBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            tvDbObjects = new DatabaseManager.Controls.UC_DbObjectsSimpleTree();
            btnFetch = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // targetDbProfile
            // 
            targetDbProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            targetDbProfile.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            targetDbProfile.EnableDatabaseType = true;
            targetDbProfile.Location = new System.Drawing.Point(9, 40);
            targetDbProfile.Margin = new System.Windows.Forms.Padding(0);
            targetDbProfile.Name = "targetDbProfile";
            targetDbProfile.Size = new System.Drawing.Size(768, 26);
            targetDbProfile.TabIndex = 42;
            targetDbProfile.Title = "Target:";
            targetDbProfile.ProfileSelectedChanged += targetDbProfile_ProfileSelectedChanged;
            // 
            // sourceDbProfile
            // 
            sourceDbProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            sourceDbProfile.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            sourceDbProfile.EnableDatabaseType = true;
            sourceDbProfile.Location = new System.Drawing.Point(9, 9);
            sourceDbProfile.Margin = new System.Windows.Forms.Padding(0);
            sourceDbProfile.Name = "sourceDbProfile";
            sourceDbProfile.Size = new System.Drawing.Size(768, 29);
            sourceDbProfile.TabIndex = 41;
            sourceDbProfile.Title = "Source:";
            sourceDbProfile.ProfileSelectedChanged += sourceDbProfile_ProfileSelectedChanged;
            sourceDbProfile.DatabaseTypeSelectedChanged += sourceDbProfile_DatabaseTypeSelectedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox1.Controls.Add(chkIdenticalRecords);
            groupBox1.Controls.Add(chkOnlyInTargetRecords);
            groupBox1.Controls.Add(chkOnlyInSourceRecords);
            groupBox1.Controls.Add(chkDifferentRecords);
            groupBox1.Location = new System.Drawing.Point(12, 301);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(839, 191);
            groupBox1.TabIndex = 43;
            groupBox1.TabStop = false;
            groupBox1.Text = "Data to display";
            // 
            // chkIdenticalRecords
            // 
            chkIdenticalRecords.AutoSize = true;
            chkIdenticalRecords.Location = new System.Drawing.Point(20, 144);
            chkIdenticalRecords.Name = "chkIdenticalRecords";
            chkIdenticalRecords.Size = new System.Drawing.Size(132, 21);
            chkIdenticalRecords.TabIndex = 3;
            chkIdenticalRecords.Text = "Indentical records";
            chkIdenticalRecords.UseVisualStyleBackColor = true;
            // 
            // chkOnlyInTargetRecords
            // 
            chkOnlyInTargetRecords.AutoSize = true;
            chkOnlyInTargetRecords.Checked = true;
            chkOnlyInTargetRecords.CheckState = System.Windows.Forms.CheckState.Checked;
            chkOnlyInTargetRecords.Location = new System.Drawing.Point(20, 108);
            chkOnlyInTargetRecords.Name = "chkOnlyInTargetRecords";
            chkOnlyInTargetRecords.Size = new System.Drawing.Size(155, 21);
            chkOnlyInTargetRecords.TabIndex = 2;
            chkOnlyInTargetRecords.Text = "Only in target records";
            chkOnlyInTargetRecords.UseVisualStyleBackColor = true;
            // 
            // chkOnlyInSourceRecords
            // 
            chkOnlyInSourceRecords.AutoSize = true;
            chkOnlyInSourceRecords.Checked = true;
            chkOnlyInSourceRecords.CheckState = System.Windows.Forms.CheckState.Checked;
            chkOnlyInSourceRecords.Location = new System.Drawing.Point(20, 72);
            chkOnlyInSourceRecords.Name = "chkOnlyInSourceRecords";
            chkOnlyInSourceRecords.Size = new System.Drawing.Size(159, 21);
            chkOnlyInSourceRecords.TabIndex = 1;
            chkOnlyInSourceRecords.Text = "Only in source records";
            chkOnlyInSourceRecords.UseVisualStyleBackColor = true;
            // 
            // chkDifferentRecords
            // 
            chkDifferentRecords.AutoSize = true;
            chkDifferentRecords.Checked = true;
            chkDifferentRecords.CheckState = System.Windows.Forms.CheckState.Checked;
            chkDifferentRecords.Location = new System.Drawing.Point(20, 36);
            chkDifferentRecords.Name = "chkDifferentRecords";
            chkDifferentRecords.Size = new System.Drawing.Size(126, 21);
            chkDifferentRecords.TabIndex = 0;
            chkDifferentRecords.Text = "Different records";
            chkDifferentRecords.UseVisualStyleBackColor = true;
            // 
            // btnCompare
            // 
            btnCompare.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnCompare.Enabled = false;
            btnCompare.Location = new System.Drawing.Point(334, 506);
            btnCompare.Name = "btnCompare";
            btnCompare.Size = new System.Drawing.Size(75, 23);
            btnCompare.TabIndex = 44;
            btnCompare.Text = "Compare";
            btnCompare.UseVisualStyleBackColor = true;
            btnCompare.Click += btnCompare_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            btnCancel.Enabled = false;
            btnCancel.Location = new System.Drawing.Point(458, 506);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 45;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtMessage
            // 
            txtMessage.BackColor = System.Drawing.Color.White;
            txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            txtMessage.Location = new System.Drawing.Point(0, 536);
            txtMessage.Margin = new System.Windows.Forms.Padding(4);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new System.Drawing.Size(858, 60);
            txtMessage.TabIndex = 46;
            // 
            // tvDbObjects
            // 
            tvDbObjects.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tvDbObjects.Location = new System.Drawing.Point(13, 70);
            tvDbObjects.Margin = new System.Windows.Forms.Padding(4);
            tvDbObjects.Name = "tvDbObjects";
            tvDbObjects.ShowCheckBox = true;
            tvDbObjects.Size = new System.Drawing.Size(838, 224);
            tvDbObjects.TabIndex = 47;
            // 
            // btnFetch
            // 
            btnFetch.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnFetch.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F);
            btnFetch.Location = new System.Drawing.Point(781, 9);
            btnFetch.Margin = new System.Windows.Forms.Padding(4);
            btnFetch.Name = "btnFetch";
            btnFetch.Size = new System.Drawing.Size(70, 57);
            btnFetch.TabIndex = 48;
            btnFetch.Text = "Fetch";
            btnFetch.UseVisualStyleBackColor = true;
            btnFetch.Click += btnFetch_Click;
            // 
            // frmDataCompare
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(858, 596);
            Controls.Add(btnFetch);
            Controls.Add(tvDbObjects);
            Controls.Add(txtMessage);
            Controls.Add(btnCancel);
            Controls.Add(btnCompare);
            Controls.Add(groupBox1);
            Controls.Add(targetDbProfile);
            Controls.Add(sourceDbProfile);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "frmDataCompare";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Data Compare";
            Load += frmDataCompare_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Controls.UC_DbConnectionProfile targetDbProfile;
        private Controls.UC_DbConnectionProfile sourceDbProfile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkIdenticalRecords;
        private System.Windows.Forms.CheckBox chkOnlyInTargetRecords;
        private System.Windows.Forms.CheckBox chkOnlyInSourceRecords;
        private System.Windows.Forms.CheckBox chkDifferentRecords;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.ToolTip toolTip1;
        private Controls.UC_DbObjectsSimpleTree tvDbObjects;
        private System.Windows.Forms.Button btnFetch;
    }
}