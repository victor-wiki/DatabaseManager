namespace DatabaseManager.Controls
{
    partial class UC_DbConnectionProfile
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnAddDbProfile = new System.Windows.Forms.Button();
            cboDbProfile = new System.Windows.Forms.ComboBox();
            lblTitle = new System.Windows.Forms.Label();
            cboDbType = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // btnAddDbProfile
            // 
            btnAddDbProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnAddDbProfile.Location = new System.Drawing.Point(621, 1);
            btnAddDbProfile.Margin = new System.Windows.Forms.Padding(4);
            btnAddDbProfile.Name = "btnAddDbProfile";
            btnAddDbProfile.Size = new System.Drawing.Size(38, 26);
            btnAddDbProfile.TabIndex = 38;
            btnAddDbProfile.UseVisualStyleBackColor = true;
            btnAddDbProfile.Click += btnAddDbProfile_Click;
            // 
            // cboDbProfile
            // 
            cboDbProfile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cboDbProfile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            cboDbProfile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            cboDbProfile.BackColor = System.Drawing.SystemColors.Window;
            cboDbProfile.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            cboDbProfile.FormattingEnabled = true;
            cboDbProfile.Location = new System.Drawing.Point(197, 2);
            cboDbProfile.Margin = new System.Windows.Forms.Padding(4);
            cboDbProfile.Name = "cboDbProfile";
            cboDbProfile.Size = new System.Drawing.Size(424, 24);
            cboDbProfile.TabIndex = 37;
            cboDbProfile.DrawItem += cboDbProfile_DrawItem;
            cboDbProfile.SelectedIndexChanged += cboDbProfile_SelectedIndexChanged;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new System.Drawing.Point(4, 5);
            lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(35, 17);
            lblTitle.TabIndex = 36;
            lblTitle.Text = "Title:";
            // 
            // cboDbType
            // 
            cboDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboDbType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cboDbType.FormattingEnabled = true;
            cboDbType.Location = new System.Drawing.Point(72, 1);
            cboDbType.Margin = new System.Windows.Forms.Padding(4);
            cboDbType.Name = "cboDbType";
            cboDbType.Size = new System.Drawing.Size(115, 25);
            cboDbType.TabIndex = 35;
            cboDbType.SelectedIndexChanged += cboDbType_SelectedIndexChanged;
            // 
            // UC_DbConnectionProfile
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(btnAddDbProfile);
            Controls.Add(cboDbProfile);
            Controls.Add(lblTitle);
            Controls.Add(cboDbType);
            Margin = new System.Windows.Forms.Padding(0);
            Name = "UC_DbConnectionProfile";
            Size = new System.Drawing.Size(659, 28);
            Load += UC_DbConnectionProfile_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button btnAddDbProfile;
        private System.Windows.Forms.ComboBox cboDbProfile;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.ComboBox cboDbType;
    }
}
