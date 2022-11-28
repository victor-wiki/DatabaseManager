namespace DatabaseManager
{
    partial class frmDiagnose
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDiagnose));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.rbNameNotMatchForScript = new System.Windows.Forms.RadioButton();
            this.rbSelfReferenceSame = new System.Windows.Forms.RadioButton();
            this.rbWithLeadingOrTrailingWhitespace = new System.Windows.Forms.RadioButton();
            this.rbNotNullWithEmpty = new System.Windows.Forms.RadioButton();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabForTable = new System.Windows.Forms.TabPage();
            this.tabForScript = new System.Windows.Forms.TabPage();
            this.rbViewColumnAliasWithoutQuotationChar = new System.Windows.Forms.RadioButton();
            this.tabControl.SuspendLayout();
            this.tabForTable.SuspendLayout();
            this.tabForScript.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(109, 217);
            this.btnStart.Margin = new System.Windows.Forms.Padding(4);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(88, 33);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(216, 217);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 33);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // rbNameNotMatchForScript
            // 
            this.rbNameNotMatchForScript.AutoSize = true;
            this.rbNameNotMatchForScript.Location = new System.Drawing.Point(9, 7);
            this.rbNameNotMatchForScript.Name = "rbNameNotMatchForScript";
            this.rbNameNotMatchForScript.Size = new System.Drawing.Size(370, 21);
            this.rbNameNotMatchForScript.TabIndex = 0;
            this.rbNameNotMatchForScript.Text = "Object names in script don\'t matched with they are defined";
            this.rbNameNotMatchForScript.UseVisualStyleBackColor = true;
            // 
            // rbSelfReferenceSame
            // 
            this.rbSelfReferenceSame.AutoSize = true;
            this.rbSelfReferenceSame.Location = new System.Drawing.Point(7, 64);
            this.rbSelfReferenceSame.Margin = new System.Windows.Forms.Padding(4);
            this.rbSelfReferenceSame.Name = "rbSelfReferenceSame";
            this.rbSelfReferenceSame.Size = new System.Drawing.Size(196, 21);
            this.rbSelfReferenceSame.TabIndex = 1;
            this.rbSelfReferenceSame.Text = "table self refer to same value";
            this.rbSelfReferenceSame.UseVisualStyleBackColor = true;
            // 
            // rbWithLeadingOrTrailingWhitespace
            // 
            this.rbWithLeadingOrTrailingWhitespace.AutoSize = true;
            this.rbWithLeadingOrTrailingWhitespace.Location = new System.Drawing.Point(7, 36);
            this.rbWithLeadingOrTrailingWhitespace.Margin = new System.Windows.Forms.Padding(4);
            this.rbWithLeadingOrTrailingWhitespace.Name = "rbWithLeadingOrTrailingWhitespace";
            this.rbWithLeadingOrTrailingWhitespace.Size = new System.Drawing.Size(225, 21);
            this.rbWithLeadingOrTrailingWhitespace.TabIndex = 5;
            this.rbWithLeadingOrTrailingWhitespace.Text = "with leading or trailing whitespace";
            this.rbWithLeadingOrTrailingWhitespace.UseVisualStyleBackColor = true;
            // 
            // rbNotNullWithEmpty
            // 
            this.rbNotNullWithEmpty.AutoSize = true;
            this.rbNotNullWithEmpty.Location = new System.Drawing.Point(7, 7);
            this.rbNotNullWithEmpty.Margin = new System.Windows.Forms.Padding(4);
            this.rbNotNullWithEmpty.Name = "rbNotNullWithEmpty";
            this.rbNotNullWithEmpty.Size = new System.Drawing.Size(199, 21);
            this.rbNotNullWithEmpty.TabIndex = 0;
            this.rbNotNullWithEmpty.Text = "not null field with empty value";
            this.rbNotNullWithEmpty.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabForTable);
            this.tabControl.Controls.Add(this.tabForScript);
            this.tabControl.Location = new System.Drawing.Point(6, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(396, 206);
            this.tabControl.TabIndex = 8;
            // 
            // tabForTable
            // 
            this.tabForTable.BackColor = System.Drawing.SystemColors.Control;
            this.tabForTable.Controls.Add(this.rbNotNullWithEmpty);
            this.tabForTable.Controls.Add(this.rbWithLeadingOrTrailingWhitespace);
            this.tabForTable.Controls.Add(this.rbSelfReferenceSame);
            this.tabForTable.Location = new System.Drawing.Point(4, 26);
            this.tabForTable.Name = "tabForTable";
            this.tabForTable.Padding = new System.Windows.Forms.Padding(3);
            this.tabForTable.Size = new System.Drawing.Size(388, 176);
            this.tabForTable.TabIndex = 0;
            this.tabForTable.Text = "For Table";
            // 
            // tabForScript
            // 
            this.tabForScript.BackColor = System.Drawing.SystemColors.Control;
            this.tabForScript.Controls.Add(this.rbViewColumnAliasWithoutQuotationChar);
            this.tabForScript.Controls.Add(this.rbNameNotMatchForScript);
            this.tabForScript.Location = new System.Drawing.Point(4, 26);
            this.tabForScript.Name = "tabForScript";
            this.tabForScript.Padding = new System.Windows.Forms.Padding(3);
            this.tabForScript.Size = new System.Drawing.Size(388, 176);
            this.tabForScript.TabIndex = 1;
            this.tabForScript.Text = "For Script";
            // 
            // rbViewColumnAliasWithoutQuotationChar
            // 
            this.rbViewColumnAliasWithoutQuotationChar.AutoSize = true;
            this.rbViewColumnAliasWithoutQuotationChar.Location = new System.Drawing.Point(9, 32);
            this.rbViewColumnAliasWithoutQuotationChar.Name = "rbViewColumnAliasWithoutQuotationChar";
            this.rbViewColumnAliasWithoutQuotationChar.Size = new System.Drawing.Size(277, 21);
            this.rbViewColumnAliasWithoutQuotationChar.TabIndex = 1;
            this.rbViewColumnAliasWithoutQuotationChar.Text = "Column alias of view has no quotation char";
            this.rbViewColumnAliasWithoutQuotationChar.UseVisualStyleBackColor = true;
            // 
            // frmDiagnose
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 261);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnStart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmDiagnose";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Diagnose";
            this.Load += new System.EventHandler(this.frmDiagnose_Load);
            this.tabControl.ResumeLayout(false);
            this.tabForTable.ResumeLayout(false);
            this.tabForTable.PerformLayout();
            this.tabForScript.ResumeLayout(false);
            this.tabForScript.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.RadioButton rbNameNotMatchForScript;
        private System.Windows.Forms.RadioButton rbSelfReferenceSame;
        private System.Windows.Forms.RadioButton rbWithLeadingOrTrailingWhitespace;
        private System.Windows.Forms.RadioButton rbNotNullWithEmpty;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabForTable;
        private System.Windows.Forms.TabPage tabForScript;
        private System.Windows.Forms.RadioButton rbViewColumnAliasWithoutQuotationChar;
    }
}