namespace DatabaseManager.Forms
{
    partial class frmDataTypeMappingSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDataTypeMappingSetting));
            btnExchange = new System.Windows.Forms.Button();
            lblTarget = new System.Windows.Forms.Label();
            lblSource = new System.Windows.Forms.Label();
            cboTargetDbType = new System.Windows.Forms.ComboBox();
            cboSourceDbType = new System.Windows.Forms.ComboBox();
            dgvData = new System.Windows.Forms.DataGridView();
            colCheck = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colSourceType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colIsExp = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            colTargetType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colPrecision = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colScale = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colSubstitute = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colArgs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            colSpecials = new System.Windows.Forms.DataGridViewButtonColumn();
            btnLoadDefault = new System.Windows.Forms.Button();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbSave = new System.Windows.Forms.ToolStripButton();
            tssSave = new System.Windows.Forms.ToolStripSeparator();
            tsbSaveAs = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            tsbAdd = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tsbDelete = new System.Windows.Forms.ToolStripButton();
            btnAdd = new System.Windows.Forms.Button();
            cboCustom = new System.Windows.Forms.ComboBox();
            panelOperation = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            toolStrip1.SuspendLayout();
            panelOperation.SuspendLayout();
            SuspendLayout();
            // 
            // btnExchange
            // 
            btnExchange.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            btnExchange.Location = new System.Drawing.Point(212, 7);
            btnExchange.Name = "btnExchange";
            btnExchange.Size = new System.Drawing.Size(49, 25);
            btnExchange.TabIndex = 54;
            btnExchange.UseVisualStyleBackColor = true;
            btnExchange.Click += btnExchange_Click;
            // 
            // lblTarget
            // 
            lblTarget.AutoSize = true;
            lblTarget.Location = new System.Drawing.Point(280, 10);
            lblTarget.Name = "lblTarget";
            lblTarget.Size = new System.Drawing.Size(49, 17);
            lblTarget.TabIndex = 53;
            lblTarget.Text = "Target:";
            // 
            // lblSource
            // 
            lblSource.AutoSize = true;
            lblSource.Location = new System.Drawing.Point(4, 10);
            lblSource.Name = "lblSource";
            lblSource.Size = new System.Drawing.Size(51, 17);
            lblSource.TabIndex = 52;
            lblSource.Text = "Source:";
            // 
            // cboTargetDbType
            // 
            cboTargetDbType.BackColor = System.Drawing.Color.White;
            cboTargetDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboTargetDbType.FormattingEnabled = true;
            cboTargetDbType.Location = new System.Drawing.Point(335, 7);
            cboTargetDbType.Margin = new System.Windows.Forms.Padding(4);
            cboTargetDbType.MaxDropDownItems = 100;
            cboTargetDbType.Name = "cboTargetDbType";
            cboTargetDbType.Size = new System.Drawing.Size(124, 25);
            cboTargetDbType.TabIndex = 51;
            cboTargetDbType.SelectedIndexChanged += cboTargetDbType_SelectedIndexChanged;
            // 
            // cboSourceDbType
            // 
            cboSourceDbType.BackColor = System.Drawing.Color.White;
            cboSourceDbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboSourceDbType.FormattingEnabled = true;
            cboSourceDbType.Location = new System.Drawing.Point(62, 7);
            cboSourceDbType.Margin = new System.Windows.Forms.Padding(4);
            cboSourceDbType.Name = "cboSourceDbType";
            cboSourceDbType.Size = new System.Drawing.Size(124, 25);
            cboSourceDbType.TabIndex = 50;
            cboSourceDbType.SelectedIndexChanged += cboSourceDbType_SelectedIndexChanged;
            // 
            // dgvData
            // 
            dgvData.AllowUserToAddRows = false;
            dgvData.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvData.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colCheck, colSourceType, colIsExp, colTargetType, colLength, colPrecision, colScale, colSubstitute, colArgs, colSpecials });
            dgvData.Location = new System.Drawing.Point(0, 72);
            dgvData.Margin = new System.Windows.Forms.Padding(4);
            dgvData.Name = "dgvData";
            dgvData.RowHeadersVisible = false;
            dgvData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            dgvData.Size = new System.Drawing.Size(1135, 517);
            dgvData.TabIndex = 55;
            dgvData.CellContentClick += dgvData_CellContentClick;
            dgvData.CellValidating += dgvData_CellValidating;
            dgvData.DataError += dgvData_DataError;
            // 
            // colCheck
            // 
            colCheck.HeaderText = "";
            colCheck.Name = "colCheck";
            colCheck.Width = 40;
            // 
            // colSourceType
            // 
            colSourceType.HeaderText = "Source Type";
            colSourceType.Name = "colSourceType";
            colSourceType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colSourceType.Width = 150;
            // 
            // colIsExp
            // 
            colIsExp.HeaderText = "Is Expression";
            colIsExp.Name = "colIsExp";
            // 
            // colTargetType
            // 
            colTargetType.HeaderText = "Target Type";
            colTargetType.Name = "colTargetType";
            colTargetType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colTargetType.Width = 150;
            // 
            // colLength
            // 
            colLength.HeaderText = "Length";
            colLength.Name = "colLength";
            // 
            // colPrecision
            // 
            colPrecision.HeaderText = "Precision";
            colPrecision.Name = "colPrecision";
            // 
            // colScale
            // 
            colScale.HeaderText = "Scale";
            colScale.Name = "colScale";
            // 
            // colSubstitute
            // 
            colSubstitute.HeaderText = "Substitute";
            colSubstitute.Name = "colSubstitute";
            colSubstitute.Width = 180;
            // 
            // colArgs
            // 
            colArgs.HeaderText = "Args";
            colArgs.Name = "colArgs";
            // 
            // colSpecials
            // 
            colSpecials.HeaderText = "Specials";
            colSpecials.Name = "colSpecials";
            colSpecials.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colSpecials.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // btnLoadDefault
            // 
            btnLoadDefault.Location = new System.Drawing.Point(493, 8);
            btnLoadDefault.Name = "btnLoadDefault";
            btnLoadDefault.Size = new System.Drawing.Size(101, 25);
            btnLoadDefault.TabIndex = 59;
            btnLoadDefault.Text = "Load default";
            btnLoadDefault.UseVisualStyleBackColor = true;
            btnLoadDefault.Click += btnLoadDefault_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbSave, tssSave, tsbSaveAs, toolStripSeparator3, tsbAdd, toolStripSeparator1, tsbDelete });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1135, 28);
            toolStrip1.TabIndex = 60;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbSave
            // 
            tsbSave.AutoSize = false;
            tsbSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbSave.Image = (System.Drawing.Image)resources.GetObject("tsbSave.Image");
            tsbSave.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbSave.Name = "tsbSave";
            tsbSave.Size = new System.Drawing.Size(23, 25);
            tsbSave.Text = "Save";
            tsbSave.Click += tsbSave_Click;
            // 
            // tssSave
            // 
            tssSave.Name = "tssSave";
            tssSave.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbSaveAs
            // 
            tsbSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbSaveAs.Image = (System.Drawing.Image)resources.GetObject("tsbSaveAs.Image");
            tsbSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbSaveAs.Name = "tsbSaveAs";
            tsbSaveAs.Size = new System.Drawing.Size(23, 25);
            tsbSaveAs.Text = "Save it as custom config";
            tsbSaveAs.Click += tsbSaveAs_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbAdd
            // 
            tsbAdd.AutoSize = false;
            tsbAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbAdd.Image = (System.Drawing.Image)resources.GetObject("tsbAdd.Image");
            tsbAdd.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbAdd.Name = "tsbAdd";
            tsbAdd.Size = new System.Drawing.Size(25, 25);
            tsbAdd.Text = "Add";
            tsbAdd.Click += tsbAdd_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // tsbDelete
            // 
            tsbDelete.AutoSize = false;
            tsbDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbDelete.Image = (System.Drawing.Image)resources.GetObject("tsbDelete.Image");
            tsbDelete.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            tsbDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbDelete.Name = "tsbDelete";
            tsbDelete.Size = new System.Drawing.Size(23, 25);
            tsbDelete.Text = "Delete";
            tsbDelete.Click += tsbDelete_Click;
            // 
            // btnAdd
            // 
            btnAdd.Location = new System.Drawing.Point(623, 7);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(96, 27);
            btnAdd.TabIndex = 62;
            btnAdd.Text = "Load custom";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // cboCustom
            // 
            cboCustom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboCustom.DropDownWidth = 180;
            cboCustom.FormattingEnabled = true;
            cboCustom.Location = new System.Drawing.Point(625, 8);
            cboCustom.Name = "cboCustom";
            cboCustom.Size = new System.Drawing.Size(107, 25);
            cboCustom.TabIndex = 61;
            cboCustom.SelectedIndexChanged += cboCustom_SelectedIndexChanged;
            // 
            // panelOperation
            // 
            panelOperation.Controls.Add(lblSource);
            panelOperation.Controls.Add(btnAdd);
            panelOperation.Controls.Add(cboSourceDbType);
            panelOperation.Controls.Add(cboCustom);
            panelOperation.Controls.Add(cboTargetDbType);
            panelOperation.Controls.Add(lblTarget);
            panelOperation.Controls.Add(btnLoadDefault);
            panelOperation.Controls.Add(btnExchange);
            panelOperation.Location = new System.Drawing.Point(0, 31);
            panelOperation.Name = "panelOperation";
            panelOperation.Size = new System.Drawing.Size(865, 40);
            panelOperation.TabIndex = 63;
            // 
            // frmDataTypeMappingSetting
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1135, 589);
            Controls.Add(panelOperation);
            Controls.Add(toolStrip1);
            Controls.Add(dgvData);
            Name = "frmDataTypeMappingSetting";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Data Type Mapping Setting";
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panelOperation.ResumeLayout(false);
            panelOperation.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnExchange;
        private System.Windows.Forms.Label lblTarget;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.ComboBox cboTargetDbType;
        private System.Windows.Forms.ComboBox cboSourceDbType;
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.Button btnLoadDefault;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripSeparator tssSave;
        private System.Windows.Forms.ToolStripButton tsbAdd;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCheck;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSourceType;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsExp;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTargetType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrecision;
        private System.Windows.Forms.DataGridViewTextBoxColumn colScale;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSubstitute;
        private System.Windows.Forms.DataGridViewTextBoxColumn colArgs;
        private System.Windows.Forms.DataGridViewButtonColumn colSpecials;
        private System.Windows.Forms.ToolStripButton tsbSaveAs;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ComboBox cboCustom;
        private System.Windows.Forms.Panel panelOperation;
    }
}