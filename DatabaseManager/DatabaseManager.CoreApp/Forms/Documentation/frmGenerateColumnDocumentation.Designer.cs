using DatabaseManager.Controls;

namespace DatabaseManager.Forms
{
    partial class frmGenerateColumnDocumentation
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            btnSelectPath = new System.Windows.Forms.Button();
            txtFilePath = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            chkShowTableComment = new System.Windows.Forms.CheckBox();
            gbProperties = new System.Windows.Forms.GroupBox();
            dgvProperties = new DraggableDataGridView();
            colSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colPropertyName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDisplayName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            txtMessage = new System.Windows.Forms.TextBox();
            btnCancel = new System.Windows.Forms.Button();
            btnOK = new System.Windows.Forms.Button();
            colorDialog1 = new System.Windows.Forms.ColorDialog();
            label1 = new System.Windows.Forms.Label();
            txtBackColor = new System.Windows.Forms.TextBox();
            btnSelectBackColor = new System.Windows.Forms.Button();
            gbTableStyle = new System.Windows.Forms.GroupBox();
            chkColumnHeaderIsBold = new System.Windows.Forms.CheckBox();
            label3 = new System.Windows.Forms.Label();
            btnSelectForeColor = new System.Windows.Forms.Button();
            txtForeColor = new System.Windows.Forms.TextBox();
            gbProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProperties).BeginInit();
            gbTableStyle.SuspendLayout();
            SuspendLayout();
            // 
            // btnSelectPath
            // 
            btnSelectPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnSelectPath.Location = new System.Drawing.Point(498, 11);
            btnSelectPath.Name = "btnSelectPath";
            btnSelectPath.Size = new System.Drawing.Size(40, 25);
            btnSelectPath.TabIndex = 13;
            btnSelectPath.Text = "...";
            btnSelectPath.UseVisualStyleBackColor = true;
            btnSelectPath.Click += btnSelectPath_Click;
            // 
            // txtFilePath
            // 
            txtFilePath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtFilePath.Location = new System.Drawing.Point(76, 12);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new System.Drawing.Size(416, 23);
            txtFilePath.TabIndex = 12;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, 15);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(59, 17);
            label2.TabIndex = 11;
            label2.Text = "File Path:";
            // 
            // chkShowTableComment
            // 
            chkShowTableComment.AutoSize = true;
            chkShowTableComment.Checked = true;
            chkShowTableComment.CheckState = System.Windows.Forms.CheckState.Checked;
            chkShowTableComment.Location = new System.Drawing.Point(16, 421);
            chkShowTableComment.Name = "chkShowTableComment";
            chkShowTableComment.Size = new System.Drawing.Size(149, 21);
            chkShowTableComment.TabIndex = 14;
            chkShowTableComment.Text = "Show table comment";
            chkShowTableComment.UseVisualStyleBackColor = true;
            // 
            // gbProperties
            // 
            gbProperties.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbProperties.Controls.Add(dgvProperties);
            gbProperties.Location = new System.Drawing.Point(5, 42);
            gbProperties.Name = "gbProperties";
            gbProperties.Size = new System.Drawing.Size(535, 229);
            gbProperties.TabIndex = 15;
            gbProperties.TabStop = false;
            gbProperties.Text = "Properties to generate";
            // 
            // dgvProperties
            // 
            dgvProperties.AllowDrop = true;
            dgvProperties.AllowUserToAddRows = false;
            dgvProperties.AllowUserToDeleteRows = false;
            dgvProperties.AllowUserToOrderColumns = true;
            dgvProperties.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightGray;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvProperties.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colSelect, colPropertyName, colDisplayName });
            dgvProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvProperties.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            dgvProperties.Location = new System.Drawing.Point(3, 19);
            dgvProperties.Name = "dgvProperties";
            dgvProperties.RowHeadersWidth = 25;
            dgvProperties.Size = new System.Drawing.Size(529, 207);
            dgvProperties.TabIndex = 0;
            // 
            // colSelect
            // 
            colSelect.HeaderText = "";
            colSelect.Name = "colSelect";
            colSelect.Width = 40;
            // 
            // colPropertyName
            // 
            colPropertyName.HeaderText = "Property Name";
            colPropertyName.Name = "colPropertyName";
            colPropertyName.ReadOnly = true;
            colPropertyName.Width = 220;
            // 
            // colDisplayName
            // 
            colDisplayName.HeaderText = "Display Name";
            colDisplayName.Name = "colDisplayName";
            colDisplayName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colDisplayName.Width = 220;
            // 
            // txtMessage
            // 
            txtMessage.BackColor = System.Drawing.Color.White;
            txtMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            txtMessage.Location = new System.Drawing.Point(0, 514);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.ReadOnly = true;
            txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtMessage.Size = new System.Drawing.Size(540, 58);
            txtMessage.TabIndex = 18;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnCancel.Enabled = false;
            btnCancel.Location = new System.Drawing.Point(267, 480);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 17;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnOK.Location = new System.Drawing.Point(156, 480);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 16;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 30);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(209, 17);
            label1.TabIndex = 19;
            label1.Text = "Column header background color:";
            // 
            // txtBackColor
            // 
            txtBackColor.BackColor = System.Drawing.Color.White;
            txtBackColor.Location = new System.Drawing.Point(222, 27);
            txtBackColor.Name = "txtBackColor";
            txtBackColor.Size = new System.Drawing.Size(100, 23);
            txtBackColor.TabIndex = 20;
            // 
            // btnSelectBackColor
            // 
            btnSelectBackColor.Location = new System.Drawing.Point(328, 27);
            btnSelectBackColor.Name = "btnSelectBackColor";
            btnSelectBackColor.Size = new System.Drawing.Size(75, 23);
            btnSelectBackColor.TabIndex = 21;
            btnSelectBackColor.Text = "Select";
            btnSelectBackColor.UseVisualStyleBackColor = true;
            btnSelectBackColor.Click += btnSelectBackColor_Click;
            // 
            // gbTableStyle
            // 
            gbTableStyle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gbTableStyle.Controls.Add(chkColumnHeaderIsBold);
            gbTableStyle.Controls.Add(label3);
            gbTableStyle.Controls.Add(btnSelectForeColor);
            gbTableStyle.Controls.Add(txtForeColor);
            gbTableStyle.Controls.Add(label1);
            gbTableStyle.Controls.Add(btnSelectBackColor);
            gbTableStyle.Controls.Add(txtBackColor);
            gbTableStyle.Location = new System.Drawing.Point(6, 280);
            gbTableStyle.Name = "gbTableStyle";
            gbTableStyle.Size = new System.Drawing.Size(531, 129);
            gbTableStyle.TabIndex = 22;
            gbTableStyle.TabStop = false;
            gbTableStyle.Text = "Table style";
            // 
            // chkColumnHeaderIsBold
            // 
            chkColumnHeaderIsBold.AutoSize = true;
            chkColumnHeaderIsBold.Checked = true;
            chkColumnHeaderIsBold.CheckState = System.Windows.Forms.CheckState.Checked;
            chkColumnHeaderIsBold.Location = new System.Drawing.Point(11, 96);
            chkColumnHeaderIsBold.Name = "chkColumnHeaderIsBold";
            chkColumnHeaderIsBold.Size = new System.Drawing.Size(187, 21);
            chkColumnHeaderIsBold.TabIndex = 25;
            chkColumnHeaderIsBold.Text = "Column header font is bold";
            chkColumnHeaderIsBold.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(7, 66);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(159, 17);
            label3.TabIndex = 22;
            label3.Text = "Column header text color:";
            // 
            // btnSelectForeColor
            // 
            btnSelectForeColor.Location = new System.Drawing.Point(328, 63);
            btnSelectForeColor.Name = "btnSelectForeColor";
            btnSelectForeColor.Size = new System.Drawing.Size(75, 23);
            btnSelectForeColor.TabIndex = 24;
            btnSelectForeColor.Text = "Select";
            btnSelectForeColor.UseVisualStyleBackColor = true;
            btnSelectForeColor.Click += btnSelectForeColor_Click;
            // 
            // txtForeColor
            // 
            txtForeColor.BackColor = System.Drawing.Color.White;
            txtForeColor.Location = new System.Drawing.Point(222, 63);
            txtForeColor.Name = "txtForeColor";
            txtForeColor.Size = new System.Drawing.Size(100, 23);
            txtForeColor.TabIndex = 23;
            // 
            // frmGenerateColumnDocumentation
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(540, 572);
            Controls.Add(gbTableStyle);
            Controls.Add(txtMessage);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(gbProperties);
            Controls.Add(chkShowTableComment);
            Controls.Add(btnSelectPath);
            Controls.Add(txtFilePath);
            Controls.Add(label2);
            Name = "frmGenerateColumnDocumentation";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Generate Column Documentation";
            Load += frmGenerateColumnDocumentation_Load;
            gbProperties.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvProperties).EndInit();
            gbTableStyle.ResumeLayout(false);
            gbTableStyle.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnSelectPath;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.CheckBox chkShowTableComment;
        private System.Windows.Forms.GroupBox gbProperties;
        private DraggableDataGridView dgvProperties;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPropertyName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisplayName;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBackColor;
        private System.Windows.Forms.Button btnSelectBackColor;
        private System.Windows.Forms.GroupBox gbTableStyle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSelectForeColor;
        private System.Windows.Forms.TextBox txtForeColor;
        private System.Windows.Forms.CheckBox chkColumnHeaderIsBold;
    }
}