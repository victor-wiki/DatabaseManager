namespace DatabaseManager.Controls
{
    partial class UC_Pagination
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cboPageNum = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblPageCount = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnLast = new System.Windows.Forms.Button();
            this.btnFirst = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cboPageSize = new System.Windows.Forms.ComboBox();
            this.lblSepartor = new System.Windows.Forms.Label();
            this.lblTotalCount = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cboPageNum
            // 
            this.cboPageNum.Font = new System.Drawing.Font("宋体", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cboPageNum.FormattingEnabled = true;
            this.cboPageNum.Location = new System.Drawing.Point(70, 2);
            this.cboPageNum.Name = "cboPageNum";
            this.cboPageNum.Size = new System.Drawing.Size(51, 21);
            this.cboPageNum.TabIndex = 6;
            this.toolTip1.SetToolTip(this.cboPageNum, "Current page");
            this.cboPageNum.SelectedValueChanged += new System.EventHandler(this.cboPageNum_SelectedValueChanged);
            this.cboPageNum.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboPageNum_KeyPress);
            // 
            // lblPageCount
            // 
            this.lblPageCount.Font = new System.Drawing.Font("宋体", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPageCount.Location = new System.Drawing.Point(147, 8);
            this.lblPageCount.Name = "lblPageCount";
            this.lblPageCount.Size = new System.Drawing.Size(33, 14);
            this.lblPageCount.TabIndex = 4;
            this.lblPageCount.Text = "0";
            this.toolTip1.SetToolTip(this.lblPageCount, "总页数");
            // 
            // btnRefresh
            // 
            this.btnRefresh.FlatAppearance.BorderSize = 0;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Image = global::DatabaseManager.Properties.Resources.Refresh;
            this.btnRefresh.Location = new System.Drawing.Point(379, 2);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(37, 25);
            this.btnRefresh.TabIndex = 10;
            this.toolTip1.SetToolTip(this.btnRefresh, "刷新");
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnNext
            // 
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Image = global::DatabaseManager.Properties.Resources.Next;
            this.btnNext.Location = new System.Drawing.Point(176, 1);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(27, 23);
            this.btnNext.TabIndex = 3;
            this.toolTip1.SetToolTip(this.btnNext, "Next page");
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.FlatAppearance.BorderSize = 0;
            this.btnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrevious.Image = global::DatabaseManager.Properties.Resources.Previous;
            this.btnPrevious.Location = new System.Drawing.Point(38, 1);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(26, 23);
            this.btnPrevious.TabIndex = 2;
            this.toolTip1.SetToolTip(this.btnPrevious, "Previous page");
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnLast
            // 
            this.btnLast.FlatAppearance.BorderSize = 0;
            this.btnLast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLast.Image = global::DatabaseManager.Properties.Resources.Last;
            this.btnLast.Location = new System.Drawing.Point(209, 2);
            this.btnLast.Name = "btnLast";
            this.btnLast.Size = new System.Drawing.Size(29, 23);
            this.btnLast.TabIndex = 1;
            this.toolTip1.SetToolTip(this.btnLast, "Last page");
            this.btnLast.UseVisualStyleBackColor = true;
            this.btnLast.Click += new System.EventHandler(this.btnLast_Click);
            // 
            // btnFirst
            // 
            this.btnFirst.FlatAppearance.BorderSize = 0;
            this.btnFirst.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFirst.Image = global::DatabaseManager.Properties.Resources.First;
            this.btnFirst.Location = new System.Drawing.Point(3, 2);
            this.btnFirst.Name = "btnFirst";
            this.btnFirst.Size = new System.Drawing.Size(29, 23);
            this.btnFirst.TabIndex = 0;
            this.toolTip1.SetToolTip(this.btnFirst, "First page");
            this.btnFirst.UseVisualStyleBackColor = true;
            this.btnFirst.Click += new System.EventHandler(this.btnFirst_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(257, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "Page size:";
            // 
            // cboPageSize
            // 
            this.cboPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPageSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cboPageSize.Font = new System.Drawing.Font("宋体", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cboPageSize.FormattingEnabled = true;
            this.cboPageSize.Items.AddRange(new object[] {
            "10",
            "15",
            "20",
            "25",
            "50",
            "100",
            "200",
            "500",
            "1000"});
            this.cboPageSize.Location = new System.Drawing.Point(328, 2);
            this.cboPageSize.Name = "cboPageSize";
            this.cboPageSize.Size = new System.Drawing.Size(45, 21);
            this.cboPageSize.TabIndex = 8;
            this.cboPageSize.TextChanged += new System.EventHandler(this.cboPageSize_TextChanged);
            // 
            // lblSepartor
            // 
            this.lblSepartor.AutoSize = true;
            this.lblSepartor.Font = new System.Drawing.Font("宋体", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSepartor.Location = new System.Drawing.Point(127, 8);
            this.lblSepartor.Name = "lblSepartor";
            this.lblSepartor.Size = new System.Drawing.Size(14, 13);
            this.lblSepartor.TabIndex = 5;
            this.lblSepartor.Text = "/";
            // 
            // lblTotalCount
            // 
            this.lblTotalCount.AutoSize = true;
            this.lblTotalCount.Location = new System.Drawing.Point(443, 8);
            this.lblTotalCount.Name = "lblTotalCount";
            this.lblTotalCount.Size = new System.Drawing.Size(47, 12);
            this.lblTotalCount.TabIndex = 13;
            this.lblTotalCount.Text = "Total:0";
            this.lblTotalCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UC_Pagination
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.lblTotalCount);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.cboPageSize);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboPageNum);
            this.Controls.Add(this.lblSepartor);
            this.Controls.Add(this.lblPageCount);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.btnLast);
            this.Controls.Add(this.btnFirst);
            this.Name = "UC_Pagination";
            this.Size = new System.Drawing.Size(539, 28);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnFirst;
        private System.Windows.Forms.Button btnLast;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.ComboBox cboPageNum;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboPageSize;
        private System.Windows.Forms.Label lblSepartor;
        private System.Windows.Forms.Label lblPageCount;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblTotalCount;
    }
}
