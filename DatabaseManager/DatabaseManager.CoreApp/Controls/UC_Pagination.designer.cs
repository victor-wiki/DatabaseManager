namespace DatabaseManager.Controls
{
    partial class UC_Pagination
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
            components = new System.ComponentModel.Container();
            cboPageNumber = new System.Windows.Forms.ComboBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            lblPageCount = new System.Windows.Forms.Label();
            btnRefresh = new System.Windows.Forms.Button();
            btnNext = new System.Windows.Forms.Button();
            btnPrevious = new System.Windows.Forms.Button();
            btnLast = new System.Windows.Forms.Button();
            btnFirst = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            cboPageSize = new System.Windows.Forms.ComboBox();
            lblSepartor = new System.Windows.Forms.Label();
            lblTotalCount = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // cboPageNum
            // 
            cboPageNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cboPageNumber.FormattingEnabled = true;
            cboPageNumber.Location = new System.Drawing.Point(79, 5);
            cboPageNumber.Margin = new System.Windows.Forms.Padding(4);
            cboPageNumber.Name = "cboPageNum";
            cboPageNumber.Size = new System.Drawing.Size(59, 24);
            cboPageNumber.TabIndex = 6;
            toolTip1.SetToolTip(cboPageNumber, "Current page");
            cboPageNumber.SelectedValueChanged += cboPageNum_SelectedValueChanged;
            cboPageNumber.KeyPress += cboPageNum_KeyPress;
            // 
            // lblPageCount
            // 
            lblPageCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            lblPageCount.Location = new System.Drawing.Point(172, 8);
            lblPageCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPageCount.Name = "lblPageCount";
            lblPageCount.Size = new System.Drawing.Size(38, 20);
            lblPageCount.TabIndex = 4;
            lblPageCount.Text = "0";
            toolTip1.SetToolTip(lblPageCount, "Total pages");
            // 
            // btnRefresh
            // 
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRefresh.Location = new System.Drawing.Point(442, 3);
            btnRefresh.Margin = new System.Windows.Forms.Padding(4);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new System.Drawing.Size(25, 25);
            btnRefresh.TabIndex = 10;
            toolTip1.SetToolTip(btnRefresh, "Refresh");
            btnRefresh.UseVisualStyleBackColor = true;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnNext
            // 
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnNext.Location = new System.Drawing.Point(215, 4);
            btnNext.Margin = new System.Windows.Forms.Padding(4);
            btnNext.Name = "btnNext";
            btnNext.Size = new System.Drawing.Size(25, 25);
            btnNext.TabIndex = 3;
            toolTip1.SetToolTip(btnNext, "Next page");
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // btnPrevious
            // 
            btnPrevious.FlatAppearance.BorderSize = 0;
            btnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnPrevious.Location = new System.Drawing.Point(39, 4);
            btnPrevious.Margin = new System.Windows.Forms.Padding(4);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new System.Drawing.Size(25, 25);
            btnPrevious.TabIndex = 2;
            toolTip1.SetToolTip(btnPrevious, "Previous page");
            btnPrevious.UseVisualStyleBackColor = true;
            btnPrevious.Click += btnPrevious_Click;
            // 
            // btnLast
            // 
            btnLast.FlatAppearance.BorderSize = 0;
            btnLast.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnLast.Location = new System.Drawing.Point(252, 4);
            btnLast.Margin = new System.Windows.Forms.Padding(4);
            btnLast.Name = "btnLast";
            btnLast.Size = new System.Drawing.Size(25, 25);
            btnLast.TabIndex = 1;
            toolTip1.SetToolTip(btnLast, "Last page");
            btnLast.UseVisualStyleBackColor = true;
            btnLast.Click += btnLast_Click;
            // 
            // btnFirst
            // 
            btnFirst.FlatAppearance.BorderSize = 0;
            btnFirst.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFirst.Location = new System.Drawing.Point(4, 4);
            btnFirst.Margin = new System.Windows.Forms.Padding(4);
            btnFirst.Name = "btnFirst";
            btnFirst.Size = new System.Drawing.Size(25, 25);
            btnFirst.TabIndex = 0;
            toolTip1.SetToolTip(btnFirst, "First page");
            btnFirst.UseVisualStyleBackColor = true;
            btnFirst.Click += btnFirst_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(300, 8);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(67, 17);
            label1.TabIndex = 7;
            label1.Text = "Page Size:";
            // 
            // cboPageSize
            // 
            cboPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboPageSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            cboPageSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            cboPageSize.FormattingEnabled = true;
            cboPageSize.Items.AddRange(new object[] { "10", "15", "20", "25", "50", "100", "200", "500", "1000" });
            cboPageSize.Location = new System.Drawing.Point(383, 5);
            cboPageSize.Margin = new System.Windows.Forms.Padding(4);
            cboPageSize.Name = "cboPageSize";
            cboPageSize.Size = new System.Drawing.Size(52, 24);
            cboPageSize.TabIndex = 8;
            cboPageSize.TextChanged += cboPageSize_TextChanged;
            // 
            // lblSepartor
            // 
            lblSepartor.AutoSize = true;
            lblSepartor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            lblSepartor.Location = new System.Drawing.Point(148, 7);
            lblSepartor.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblSepartor.Name = "lblSepartor";
            lblSepartor.Size = new System.Drawing.Size(11, 16);
            lblSepartor.TabIndex = 5;
            lblSepartor.Text = "/";
            // 
            // lblTotalCount
            // 
            lblTotalCount.AutoSize = true;
            lblTotalCount.Location = new System.Drawing.Point(517, 7);
            lblTotalCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblTotalCount.Name = "lblTotalCount";
            lblTotalCount.Size = new System.Drawing.Size(47, 17);
            lblTotalCount.TabIndex = 13;
            lblTotalCount.Text = "Total:0";
            lblTotalCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UC_Pagination
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Transparent;
            Controls.Add(lblTotalCount);
            Controls.Add(btnRefresh);
            Controls.Add(cboPageSize);
            Controls.Add(label1);
            Controls.Add(cboPageNumber);
            Controls.Add(lblSepartor);
            Controls.Add(lblPageCount);
            Controls.Add(btnNext);
            Controls.Add(btnPrevious);
            Controls.Add(btnLast);
            Controls.Add(btnFirst);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "UC_Pagination";
            Size = new System.Drawing.Size(629, 31);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnFirst;
        private System.Windows.Forms.Button btnLast;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.ComboBox cboPageNumber;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cboPageSize;
        private System.Windows.Forms.Label lblSepartor;
        private System.Windows.Forms.Label lblPageCount;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblTotalCount;
    }
}
