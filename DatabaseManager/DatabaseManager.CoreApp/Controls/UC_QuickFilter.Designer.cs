namespace DatabaseManager.Controls
{
    partial class UC_QuickFilter
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
            btnFilter = new System.Windows.Forms.Button();
            cboFilterMode = new System.Windows.Forms.ComboBox();
            txtFilter = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // btnFilter
            // 
            btnFilter.Location = new System.Drawing.Point(195, 1);
            btnFilter.Name = "btnFilter";
            btnFilter.Size = new System.Drawing.Size(38, 27);
            btnFilter.TabIndex = 21;
            btnFilter.UseVisualStyleBackColor = true;
            // 
            // cboFilterMode
            // 
            cboFilterMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboFilterMode.DropDownWidth = 180;
            cboFilterMode.FormattingEnabled = true;
            cboFilterMode.Items.AddRange(new object[] { "Filter by text (if contains)", "Filter strictly by text (if equals)", "Filter by SQL expression" });
            cboFilterMode.Location = new System.Drawing.Point(196, 2);
            cboFilterMode.Name = "cboFilterMode";
            cboFilterMode.Size = new System.Drawing.Size(50, 25);
            cboFilterMode.TabIndex = 20;
            // 
            // txtFilter
            // 
            txtFilter.Location = new System.Drawing.Point(3, 3);
            txtFilter.Name = "txtFilter";
            txtFilter.PlaceholderText = "Filter data";
            txtFilter.Size = new System.Drawing.Size(189, 23);
            txtFilter.TabIndex = 19;
            txtFilter.KeyUp += txtFilter_KeyUp;
            // 
            // UC_QuickFilter
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(btnFilter);
            Controls.Add(cboFilterMode);
            Controls.Add(txtFilter);
            Name = "UC_QuickFilter";
            Size = new System.Drawing.Size(250, 30);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnFilter;
        private System.Windows.Forms.ComboBox cboFilterMode;
        private System.Windows.Forms.TextBox txtFilter;
    }
}
