namespace DatabaseManager.Controls
{
    partial class UC_TablePartition_Postgres
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
            txtDefinition = new SqlCodeEditor.TextEditorControlEx();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            txtInheritedTables = new SqlCodeEditor.TextEditorControlEx();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // txtDefinition
            // 
            txtDefinition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtDefinition.Dock = System.Windows.Forms.DockStyle.Fill;
            txtDefinition.EnableFolding = false;
            txtDefinition.FoldingStrategy = null;
            txtDefinition.Font = new System.Drawing.Font("Courier New", 10F);
            txtDefinition.IsReadOnly = true;
            txtDefinition.Location = new System.Drawing.Point(3, 19);
            txtDefinition.Name = "txtDefinition";
            txtDefinition.ShowLineNumbers = false;
            txtDefinition.ShowVRuler = false;
            txtDefinition.Size = new System.Drawing.Size(626, 97);
            txtDefinition.SyntaxHighlighting = "PlSql";
            txtDefinition.TabIndex = 9;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox1.Controls.Add(txtDefinition);
            groupBox1.Location = new System.Drawing.Point(0, 0);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(632, 119);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "Definition";
            // 
            // groupBox2
            // 
            groupBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox2.Controls.Add(txtInheritedTables);
            groupBox2.Location = new System.Drawing.Point(0, 125);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(632, 333);
            groupBox2.TabIndex = 11;
            groupBox2.TabStop = false;
            groupBox2.Text = "Inherited Tables";
            // 
            // txtInheritedTables
            // 
            txtInheritedTables.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtInheritedTables.Dock = System.Windows.Forms.DockStyle.Fill;
            txtInheritedTables.EnableFolding = false;
            txtInheritedTables.FoldingStrategy = null;
            txtInheritedTables.Font = new System.Drawing.Font("Courier New", 10F);
            txtInheritedTables.IsReadOnly = true;
            txtInheritedTables.Location = new System.Drawing.Point(3, 19);
            txtInheritedTables.Name = "txtInheritedTables";
            txtInheritedTables.ShowLineNumbers = false;
            txtInheritedTables.ShowVRuler = false;
            txtInheritedTables.Size = new System.Drawing.Size(626, 311);
            txtInheritedTables.SyntaxHighlighting = "PlSql";
            txtInheritedTables.TabIndex = 10;
            // 
            // UC_TablePartition_Postgres
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "UC_TablePartition_Postgres";
            Size = new System.Drawing.Size(632, 449);
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion   

        private SqlCodeEditor.TextEditorControlEx txtDefinition;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private SqlCodeEditor.TextEditorControlEx txtInheritedTables;
    }
}
