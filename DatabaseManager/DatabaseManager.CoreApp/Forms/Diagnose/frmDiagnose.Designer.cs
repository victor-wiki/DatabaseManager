namespace DatabaseManager.Forms
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
            btnStart = new System.Windows.Forms.Button();
            btnClose = new System.Windows.Forms.Button();
            rbNameNotMatchForScript = new System.Windows.Forms.RadioButton();
            rbSelfReferenceSame = new System.Windows.Forms.RadioButton();
            rbWithLeadingOrTrailingWhitespace = new System.Windows.Forms.RadioButton();
            rbNotNullWithEmpty = new System.Windows.Forms.RadioButton();
            tabControl = new System.Windows.Forms.TabControl();
            tabForTable = new System.Windows.Forms.TabPage();
            rbEmptyValueRatherThanNull = new System.Windows.Forms.RadioButton();
            tabForScript = new System.Windows.Forms.TabPage();
            rbViewColumnAliasWithoutQuotationChar = new System.Windows.Forms.RadioButton();
            rbPrimaryKeyColumnIsNullable = new System.Windows.Forms.RadioButton();
            tabControl.SuspendLayout();
            tabForTable.SuspendLayout();
            tabForScript.SuspendLayout();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Location = new System.Drawing.Point(109, 217);
            btnStart.Margin = new System.Windows.Forms.Padding(4);
            btnStart.Name = "btnStart";
            btnStart.Size = new System.Drawing.Size(88, 33);
            btnStart.TabIndex = 3;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new System.Drawing.Point(216, 217);
            btnClose.Margin = new System.Windows.Forms.Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(88, 33);
            btnClose.TabIndex = 4;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // rbNameNotMatchForScript
            // 
            rbNameNotMatchForScript.AutoSize = true;
            rbNameNotMatchForScript.Location = new System.Drawing.Point(9, 7);
            rbNameNotMatchForScript.Name = "rbNameNotMatchForScript";
            rbNameNotMatchForScript.Size = new System.Drawing.Size(370, 21);
            rbNameNotMatchForScript.TabIndex = 0;
            rbNameNotMatchForScript.Text = "Object names in script don't matched with they are defined";
            rbNameNotMatchForScript.UseVisualStyleBackColor = true;
            // 
            // rbSelfReferenceSame
            // 
            rbSelfReferenceSame.AutoSize = true;
            rbSelfReferenceSame.Location = new System.Drawing.Point(7, 93);
            rbSelfReferenceSame.Margin = new System.Windows.Forms.Padding(4);
            rbSelfReferenceSame.Name = "rbSelfReferenceSame";
            rbSelfReferenceSame.Size = new System.Drawing.Size(196, 21);
            rbSelfReferenceSame.TabIndex = 1;
            rbSelfReferenceSame.Text = "table self refer to same value";
            rbSelfReferenceSame.UseVisualStyleBackColor = true;
            // 
            // rbWithLeadingOrTrailingWhitespace
            // 
            rbWithLeadingOrTrailingWhitespace.AutoSize = true;
            rbWithLeadingOrTrailingWhitespace.Location = new System.Drawing.Point(7, 65);
            rbWithLeadingOrTrailingWhitespace.Margin = new System.Windows.Forms.Padding(4);
            rbWithLeadingOrTrailingWhitespace.Name = "rbWithLeadingOrTrailingWhitespace";
            rbWithLeadingOrTrailingWhitespace.Size = new System.Drawing.Size(225, 21);
            rbWithLeadingOrTrailingWhitespace.TabIndex = 5;
            rbWithLeadingOrTrailingWhitespace.Text = "with leading or trailing whitespace";
            rbWithLeadingOrTrailingWhitespace.UseVisualStyleBackColor = true;
            // 
            // rbNotNullWithEmpty
            // 
            rbNotNullWithEmpty.AutoSize = true;
            rbNotNullWithEmpty.Location = new System.Drawing.Point(7, 7);
            rbNotNullWithEmpty.Margin = new System.Windows.Forms.Padding(4);
            rbNotNullWithEmpty.Name = "rbNotNullWithEmpty";
            rbNotNullWithEmpty.Size = new System.Drawing.Size(199, 21);
            rbNotNullWithEmpty.TabIndex = 0;
            rbNotNullWithEmpty.Text = "not null field with empty value";
            rbNotNullWithEmpty.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabForTable);
            tabControl.Controls.Add(tabForScript);
            tabControl.Location = new System.Drawing.Point(6, 3);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(463, 206);
            tabControl.TabIndex = 8;
            // 
            // tabForTable
            // 
            tabForTable.BackColor = System.Drawing.SystemColors.Control;
            tabForTable.Controls.Add(rbPrimaryKeyColumnIsNullable);
            tabForTable.Controls.Add(rbEmptyValueRatherThanNull);
            tabForTable.Controls.Add(rbNotNullWithEmpty);
            tabForTable.Controls.Add(rbWithLeadingOrTrailingWhitespace);
            tabForTable.Controls.Add(rbSelfReferenceSame);
            tabForTable.Location = new System.Drawing.Point(4, 26);
            tabForTable.Name = "tabForTable";
            tabForTable.Padding = new System.Windows.Forms.Padding(3);
            tabForTable.Size = new System.Drawing.Size(455, 176);
            tabForTable.TabIndex = 0;
            tabForTable.Text = "For Table";
            // 
            // rbEmptyValueRatherThanNull
            // 
            rbEmptyValueRatherThanNull.AutoSize = true;
            rbEmptyValueRatherThanNull.Location = new System.Drawing.Point(7, 36);
            rbEmptyValueRatherThanNull.Margin = new System.Windows.Forms.Padding(4);
            rbEmptyValueRatherThanNull.Name = "rbEmptyValueRatherThanNull";
            rbEmptyValueRatherThanNull.Size = new System.Drawing.Size(408, 21);
            rbEmptyValueRatherThanNull.TabIndex = 6;
            rbEmptyValueRatherThanNull.Text = "empty value rather than null (no matter the field is not null or not)";
            rbEmptyValueRatherThanNull.UseVisualStyleBackColor = true;
            // 
            // tabForScript
            // 
            tabForScript.BackColor = System.Drawing.SystemColors.Control;
            tabForScript.Controls.Add(rbViewColumnAliasWithoutQuotationChar);
            tabForScript.Controls.Add(rbNameNotMatchForScript);
            tabForScript.Location = new System.Drawing.Point(4, 26);
            tabForScript.Name = "tabForScript";
            tabForScript.Padding = new System.Windows.Forms.Padding(3);
            tabForScript.Size = new System.Drawing.Size(455, 176);
            tabForScript.TabIndex = 1;
            tabForScript.Text = "For Script";
            // 
            // rbViewColumnAliasWithoutQuotationChar
            // 
            rbViewColumnAliasWithoutQuotationChar.AutoSize = true;
            rbViewColumnAliasWithoutQuotationChar.Location = new System.Drawing.Point(9, 32);
            rbViewColumnAliasWithoutQuotationChar.Name = "rbViewColumnAliasWithoutQuotationChar";
            rbViewColumnAliasWithoutQuotationChar.Size = new System.Drawing.Size(277, 21);
            rbViewColumnAliasWithoutQuotationChar.TabIndex = 1;
            rbViewColumnAliasWithoutQuotationChar.Text = "Column alias of view has no quotation char";
            rbViewColumnAliasWithoutQuotationChar.UseVisualStyleBackColor = true;
            // 
            // rbPrimaryKeyColumnIsNullable
            // 
            rbPrimaryKeyColumnIsNullable.AutoSize = true;
            rbPrimaryKeyColumnIsNullable.Location = new System.Drawing.Point(7, 122);
            rbPrimaryKeyColumnIsNullable.Margin = new System.Windows.Forms.Padding(4);
            rbPrimaryKeyColumnIsNullable.Name = "rbPrimaryKeyColumnIsNullable";
            rbPrimaryKeyColumnIsNullable.Size = new System.Drawing.Size(203, 21);
            rbPrimaryKeyColumnIsNullable.TabIndex = 7;
            rbPrimaryKeyColumnIsNullable.Text = "primary key column is nullable";
            rbPrimaryKeyColumnIsNullable.UseVisualStyleBackColor = true;
            // 
            // frmDiagnose
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(481, 261);
            Controls.Add(tabControl);
            Controls.Add(btnClose);
            Controls.Add(btnStart);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            Name = "frmDiagnose";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Diagnose";
            Load += frmDiagnose_Load;
            tabControl.ResumeLayout(false);
            tabForTable.ResumeLayout(false);
            tabForTable.PerformLayout();
            tabForScript.ResumeLayout(false);
            tabForScript.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.RadioButton rbEmptyValueRatherThanNull;
        private System.Windows.Forms.RadioButton rbPrimaryKeyColumnIsNullable;
    }
}