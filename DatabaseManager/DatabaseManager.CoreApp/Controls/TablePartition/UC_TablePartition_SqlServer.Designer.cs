namespace DatabaseManager.Controls
{
    partial class UC_TablePartition_SqlServer
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            gbScheme = new System.Windows.Forms.GroupBox();
            txtSchemeDefinition = new SqlCodeEditor.TextEditorControlEx();
            txtFilegroups = new System.Windows.Forms.TextBox();
            label6 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            txtColumnName = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            txtSchemeName = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            gbFunction = new System.Windows.Forms.GroupBox();
            txtFunctionDefinition = new SqlCodeEditor.TextEditorControlEx();
            label5 = new System.Windows.Forms.Label();
            txtFunctionName = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            gbScheme.SuspendLayout();
            gbFunction.SuspendLayout();
            SuspendLayout();
            // 
            // gbScheme
            // 
            gbScheme.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbScheme.Controls.Add(txtSchemeDefinition);
            gbScheme.Controls.Add(txtFilegroups);
            gbScheme.Controls.Add(label6);
            gbScheme.Controls.Add(label3);
            gbScheme.Controls.Add(txtColumnName);
            gbScheme.Controls.Add(label2);
            gbScheme.Controls.Add(txtSchemeName);
            gbScheme.Controls.Add(label1);
            gbScheme.Location = new System.Drawing.Point(5, 5);
            gbScheme.Name = "gbScheme";
            gbScheme.Size = new System.Drawing.Size(624, 247);
            gbScheme.TabIndex = 0;
            gbScheme.TabStop = false;
            gbScheme.Text = "Partition Scheme";
            // 
            // txtSchemeDefinition
            // 
            txtSchemeDefinition.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtSchemeDefinition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtSchemeDefinition.EnableFolding = false;
            txtSchemeDefinition.FoldingStrategy = null;
            txtSchemeDefinition.Font = new System.Drawing.Font("Courier New", 10F);
            txtSchemeDefinition.IsReadOnly = true;
            txtSchemeDefinition.Location = new System.Drawing.Point(111, 173);
            txtSchemeDefinition.Name = "txtSchemeDefinition";
            txtSchemeDefinition.ShowLineNumbers = false;
            txtSchemeDefinition.ShowVRuler = false;
            txtSchemeDefinition.Size = new System.Drawing.Size(507, 60);
            txtSchemeDefinition.SyntaxHighlighting = "TSql";
            txtSchemeDefinition.TabIndex = 9;
            // 
            // txtFilegroups
            // 
            txtFilegroups.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            txtFilegroups.BackColor = System.Drawing.Color.White;
            txtFilegroups.Location = new System.Drawing.Point(111, 89);
            txtFilegroups.Multiline = true;
            txtFilegroups.Name = "txtFilegroups";
            txtFilegroups.ReadOnly = true;
            txtFilegroups.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtFilegroups.Size = new System.Drawing.Size(367, 71);
            txtFilegroups.TabIndex = 8;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(12, 173);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(66, 17);
            label6.TabIndex = 6;
            label6.Text = "Definition:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(12, 89);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(76, 17);
            label3.TabIndex = 4;
            label3.Text = "File groups:";
            // 
            // txtColumnName
            // 
            txtColumnName.BackColor = System.Drawing.Color.White;
            txtColumnName.Location = new System.Drawing.Point(111, 56);
            txtColumnName.Name = "txtColumnName";
            txtColumnName.ReadOnly = true;
            txtColumnName.Size = new System.Drawing.Size(367, 23);
            txtColumnName.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 59);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(94, 17);
            label2.TabIndex = 2;
            label2.Text = "Column Name:";
            // 
            // txtSchemeName
            // 
            txtSchemeName.BackColor = System.Drawing.Color.White;
            txtSchemeName.Location = new System.Drawing.Point(111, 27);
            txtSchemeName.Name = "txtSchemeName";
            txtSchemeName.ReadOnly = true;
            txtSchemeName.Size = new System.Drawing.Size(367, 23);
            txtSchemeName.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 30);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(95, 17);
            label1.TabIndex = 0;
            label1.Text = "Scheme Name:";
            // 
            // gbFunction
            // 
            gbFunction.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbFunction.Controls.Add(txtFunctionDefinition);
            gbFunction.Controls.Add(label5);
            gbFunction.Controls.Add(txtFunctionName);
            gbFunction.Controls.Add(label4);
            gbFunction.Location = new System.Drawing.Point(5, 258);
            gbFunction.Name = "gbFunction";
            gbFunction.Size = new System.Drawing.Size(624, 188);
            gbFunction.TabIndex = 1;
            gbFunction.TabStop = false;
            gbFunction.Text = "Partition Function";
            // 
            // txtFunctionDefinition
            // 
            txtFunctionDefinition.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtFunctionDefinition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtFunctionDefinition.EnableFolding = false;
            txtFunctionDefinition.FoldingStrategy = null;
            txtFunctionDefinition.Font = new System.Drawing.Font("Courier New", 10F);
            txtFunctionDefinition.IsReadOnly = true;
            txtFunctionDefinition.Location = new System.Drawing.Point(111, 71);
            txtFunctionDefinition.Name = "txtFunctionDefinition";
            txtFunctionDefinition.ShowLineNumbers = false;
            txtFunctionDefinition.ShowVRuler = false;
            txtFunctionDefinition.Size = new System.Drawing.Size(507, 109);
            txtFunctionDefinition.SyntaxHighlighting = "TSql";
            txtFunctionDefinition.TabIndex = 10;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(11, 73);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(66, 17);
            label5.TabIndex = 4;
            label5.Text = "Definition:";
            // 
            // txtFunctionName
            // 
            txtFunctionName.BackColor = System.Drawing.Color.White;
            txtFunctionName.Location = new System.Drawing.Point(111, 32);
            txtFunctionName.Name = "txtFunctionName";
            txtFunctionName.ReadOnly = true;
            txtFunctionName.Size = new System.Drawing.Size(367, 23);
            txtFunctionName.TabIndex = 3;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(10, 35);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(98, 17);
            label4.TabIndex = 2;
            label4.Text = "Function Name:";
            // 
            // UC_TablePartition_SqlServer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(gbFunction);
            Controls.Add(gbScheme);
            Name = "UC_TablePartition_SqlServer";
            Size = new System.Drawing.Size(632, 449);         
            gbScheme.ResumeLayout(false);
            gbScheme.PerformLayout();
            gbFunction.ResumeLayout(false);
            gbFunction.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox gbScheme;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtColumnName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSchemeName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbFunction;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtFunctionName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtFilegroups;
        private SqlCodeEditor.TextEditorControlEx txtSchemeDefinition;
        private SqlCodeEditor.TextEditorControlEx txtFunctionDefinition;
    }
}
