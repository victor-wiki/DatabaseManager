namespace DatabaseManager.Controls
{
    partial class UC_QueryResultGrid
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            dgvData = new CustomDataGridView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCopyWithHeader = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCopyContent = new System.Windows.Forms.ToolStripMenuItem();
            tsmiShowContent = new System.Windows.Forms.ToolStripMenuItem();
            tsmiViewGeometry = new System.Windows.Forms.ToolStripMenuItem();
            tsmiSave = new System.Windows.Forms.ToolStripMenuItem();
            tsmiAutoColumnWidth = new System.Windows.Forms.ToolStripMenuItem();
            tsmiSetColumnWidthByDefault = new System.Windows.Forms.ToolStripMenuItem();
            tsmiFindText = new System.Windows.Forms.ToolStripMenuItem();
            tsmiClearHighlighting = new System.Windows.Forms.ToolStripMenuItem();
            tsmiHideRowHeader = new System.Windows.Forms.ToolStripMenuItem();
            tsmiShowRowHeader = new System.Windows.Forms.ToolStripMenuItem();
            dlgSave = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // dgvData
            // 
            dgvData.AllowUserToAddRows = false;
            dgvData.AllowUserToDeleteRows = false;
            dgvData.BackgroundColor = System.Drawing.Color.White;
            dgvData.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvData.Location = new System.Drawing.Point(0, 0);
            dgvData.Margin = new System.Windows.Forms.Padding(4);
            dgvData.Name = "dgvData";
            dgvData.ReadOnly = true;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvData.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgvData.Size = new System.Drawing.Size(658, 329);
            dgvData.TabIndex = 6;
            dgvData.CellFormatting += dgvData_CellFormatting;
            dgvData.CellPainting += dgvData_CellPainting;
            dgvData.DataBindingComplete += dgvData_DataBindingComplete;
            dgvData.DataError += dgvData_DataError;
            dgvData.RowPostPaint += dgvData_RowPostPaint;
            dgvData.KeyDown += dgvData_KeyDown;
            dgvData.MouseUp += dgvData_MouseUp;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiCopy, tsmiCopyWithHeader, tsmiCopyContent, tsmiShowContent, tsmiViewGeometry, tsmiSave, tsmiAutoColumnWidth, tsmiSetColumnWidthByDefault, tsmiFindText, tsmiClearHighlighting, tsmiHideRowHeader, tsmiShowRowHeader });
            contextMenuStrip1.Name = "contextMenuStrip2";
            contextMenuStrip1.Size = new System.Drawing.Size(250, 290);
            // 
            // tsmiCopy
            // 
            tsmiCopy.Name = "tsmiCopy";
            tsmiCopy.Size = new System.Drawing.Size(249, 22);
            tsmiCopy.Text = "Copy";
            tsmiCopy.Click += tsmiCopy_Click;
            // 
            // tsmiCopyWithHeader
            // 
            tsmiCopyWithHeader.Name = "tsmiCopyWithHeader";
            tsmiCopyWithHeader.Size = new System.Drawing.Size(249, 22);
            tsmiCopyWithHeader.Text = "Copy with header";
            tsmiCopyWithHeader.Click += tsmiCopyWithHeader_Click;
            // 
            // tsmiCopyContent
            // 
            tsmiCopyContent.Name = "tsmiCopyContent";
            tsmiCopyContent.Size = new System.Drawing.Size(249, 22);
            tsmiCopyContent.Text = "Copy content";
            tsmiCopyContent.Click += tsmiCopyContent_Click;
            // 
            // tsmiShowContent
            // 
            tsmiShowContent.Name = "tsmiShowContent";
            tsmiShowContent.Size = new System.Drawing.Size(249, 22);
            tsmiShowContent.Text = "Show content";
            tsmiShowContent.Click += tsmiShowContent_Click;
            // 
            // tsmiViewGeometry
            // 
            tsmiViewGeometry.Name = "tsmiViewGeometry";
            tsmiViewGeometry.Size = new System.Drawing.Size(249, 22);
            tsmiViewGeometry.Text = "View geometry";
            tsmiViewGeometry.Click += tsmiViewGeometry_Click;
            // 
            // tsmiSave
            // 
            tsmiSave.Name = "tsmiSave";
            tsmiSave.Size = new System.Drawing.Size(249, 22);
            tsmiSave.Text = "Save";
            tsmiSave.Click += tsmiSave_Click;
            // 
            // tsmiAutoColumnWidth
            // 
            tsmiAutoColumnWidth.Name = "tsmiAutoColumnWidth";
            tsmiAutoColumnWidth.Size = new System.Drawing.Size(249, 22);
            tsmiAutoColumnWidth.Text = "Auto column width by content";
            tsmiAutoColumnWidth.Click += tsmiAutoColumnWidth_Click;
            // 
            // tsmiSetColumnWidthByDefault
            // 
            tsmiSetColumnWidthByDefault.Name = "tsmiSetColumnWidthByDefault";
            tsmiSetColumnWidthByDefault.Size = new System.Drawing.Size(249, 22);
            tsmiSetColumnWidthByDefault.Text = "Set column width by default";
            tsmiSetColumnWidthByDefault.Click += tsmiSetColumnWidthByDefault_Click;
            // 
            // tsmiFindText
            // 
            tsmiFindText.Name = "tsmiFindText";
            tsmiFindText.Size = new System.Drawing.Size(249, 22);
            tsmiFindText.Text = "Find text";
            tsmiFindText.Click += tsmiFindText_Click;
            // 
            // tsmiClearHighlighting
            // 
            tsmiClearHighlighting.Name = "tsmiClearHighlighting";
            tsmiClearHighlighting.Size = new System.Drawing.Size(249, 22);
            tsmiClearHighlighting.Text = "Clear highlighting";
            tsmiClearHighlighting.Click += tsmiClearHighlighting_Click;
            // 
            // tsmiHideRowHeader
            // 
            tsmiHideRowHeader.Name = "tsmiHideRowHeader";
            tsmiHideRowHeader.Size = new System.Drawing.Size(249, 22);
            tsmiHideRowHeader.Text = "Hide row header";
            tsmiHideRowHeader.Click += tsmiHideRowHeader_Click;
            // 
            // tsmiShowRowHeader
            // 
            tsmiShowRowHeader.Name = "tsmiShowRowHeader";
            tsmiShowRowHeader.Size = new System.Drawing.Size(249, 22);
            tsmiShowRowHeader.Text = "Show row header";
            tsmiShowRowHeader.Visible = false;
            tsmiShowRowHeader.Click += tsmiShowRowHeader_Click;
            // 
            // dlgSave
            // 
            dlgSave.Filter = "\"csv file|*.csv|txt file|*.txt\"";
            // 
            // UC_QueryResultGrid
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(dgvData);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "UC_QueryResultGrid";
            Size = new System.Drawing.Size(658, 329);
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private CustomDataGridView dgvData;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiSave;
        private System.Windows.Forms.SaveFileDialog dlgSave;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyWithHeader;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewGeometry;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyContent;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowContent;
        private System.Windows.Forms.ToolStripMenuItem tsmiAutoColumnWidth;
        private System.Windows.Forms.ToolStripMenuItem tsmiSetColumnWidthByDefault;
        private System.Windows.Forms.ToolStripMenuItem tsmiClearHighlighting;
        private System.Windows.Forms.ToolStripMenuItem tsmiFindText;
        private System.Windows.Forms.ToolStripMenuItem tsmiHideRowHeader;
        private System.Windows.Forms.ToolStripMenuItem tsmiShowRowHeader;
    }
}
