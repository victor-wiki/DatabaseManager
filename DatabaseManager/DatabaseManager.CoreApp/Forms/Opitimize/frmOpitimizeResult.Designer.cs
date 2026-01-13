namespace DatabaseManager.Forms
{
    partial class frmOpitimizeResult
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            dgvResult = new System.Windows.Forms.DataGridView();
            colObjectType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colObjectName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colBeforeDataLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colDataLengthAfter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dgvResult).BeginInit();
            SuspendLayout();
            // 
            // dgvResult
            // 
            dgvResult.AllowUserToAddRows = false;
            dgvResult.AllowUserToDeleteRows = false;
            dgvResult.BackgroundColor = System.Drawing.Color.White;
            dgvResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colObjectType, colObjectName, colBeforeDataLength, colDataLengthAfter, colResult });
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgvResult.DefaultCellStyle = dataGridViewCellStyle5;
            dgvResult.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvResult.Location = new System.Drawing.Point(0, 0);
            dgvResult.Margin = new System.Windows.Forms.Padding(4);
            dgvResult.Name = "dgvResult";
            dgvResult.ReadOnly = true;
            dgvResult.RowHeadersVisible = false;
            dgvResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvResult.Size = new System.Drawing.Size(812, 376);
            dgvResult.TabIndex = 1;
            dgvResult.CellFormatting += dgvResult_CellFormatting;
            dgvResult.DataBindingComplete += dgvResult_DataBindingComplete;
            // 
            // colObjectType
            // 
            colObjectType.DataPropertyName = "ObjectType";
            colObjectType.HeaderText = "Object Type";
            colObjectType.Name = "colObjectType";
            colObjectType.ReadOnly = true;
            colObjectType.Width = 150;
            // 
            // colObjectName
            // 
            colObjectName.DataPropertyName = "ObjectName";
            colObjectName.HeaderText = "Object Name";
            colObjectName.Name = "colObjectName";
            colObjectName.ReadOnly = true;
            colObjectName.Width = 200;
            // 
            // colBeforeDataLength
            // 
            colBeforeDataLength.DataPropertyName = "DataLengthBeforeOptimization";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            colBeforeDataLength.DefaultCellStyle = dataGridViewCellStyle2;
            colBeforeDataLength.HeaderText = "Data Length Before(MB)";
            colBeforeDataLength.Name = "colBeforeDataLength";
            colBeforeDataLength.ReadOnly = true;
            colBeforeDataLength.Width = 180;
            // 
            // colDataLengthAfter
            // 
            colDataLengthAfter.DataPropertyName = "DataLengthAfterOptimization";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            colDataLengthAfter.DefaultCellStyle = dataGridViewCellStyle3;
            colDataLengthAfter.HeaderText = "Data Length After(MB)";
            colDataLengthAfter.Name = "colDataLengthAfter";
            colDataLengthAfter.ReadOnly = true;
            colDataLengthAfter.Width = 180;
            // 
            // colResult
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            colResult.DefaultCellStyle = dataGridViewCellStyle4;
            colResult.HeaderText = "Result";
            colResult.Name = "colResult";
            colResult.ReadOnly = true;
            // 
            // frmOpitimizeResult
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(812, 376);
            Controls.Add(dgvResult);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmOpitimizeResult";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Table Diagnose Result";
            Load += frmOpitimizeResult_Load;
            ((System.ComponentModel.ISupportInitialize)dgvResult).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DataGridView dgvResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBeforeDataLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDataLengthAfter;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResult;
    }
}