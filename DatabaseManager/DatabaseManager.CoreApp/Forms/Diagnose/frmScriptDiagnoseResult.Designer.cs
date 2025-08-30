namespace DatabaseManager.Forms
{
    partial class frmScriptDiagnoseResult
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvDbObjects = new System.Windows.Forms.TreeView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.txtDefinition = new System.Windows.Forms.RichTextBox();
            this.btnCorrectAll = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnCorrect = new System.Windows.Forms.Button();
            this.dgvResultDetails = new System.Windows.Forms.DataGridView();
            this.colObjectType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colInvalidName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResultDetails)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvDbObjects);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(915, 524);
            this.splitContainer1.SplitterDistance = 250;
            this.splitContainer1.TabIndex = 0;
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDbObjects.HideSelection = false;
            this.tvDbObjects.Location = new System.Drawing.Point(0, 0);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(250, 524);
            this.tvDbObjects.TabIndex = 0;
            this.tvDbObjects.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvDbObjects_AfterSelect);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.txtDefinition);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnCorrectAll);
            this.splitContainer2.Panel2.Controls.Add(this.btnClose);
            this.splitContainer2.Panel2.Controls.Add(this.btnCorrect);
            this.splitContainer2.Panel2.Controls.Add(this.dgvResultDetails);
            this.splitContainer2.Size = new System.Drawing.Size(661, 524);
            this.splitContainer2.SplitterDistance = 349;
            this.splitContainer2.TabIndex = 0;
            // 
            // txtDefinition
            // 
            this.txtDefinition.BackColor = System.Drawing.Color.White;
            this.txtDefinition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDefinition.Location = new System.Drawing.Point(0, 0);
            this.txtDefinition.Name = "txtDefinition";
            this.txtDefinition.ReadOnly = true;
            this.txtDefinition.Size = new System.Drawing.Size(661, 349);
            this.txtDefinition.TabIndex = 0;
            this.txtDefinition.Text = "";
            // 
            // btnCorrectAll
            // 
            this.btnCorrectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCorrectAll.Location = new System.Drawing.Point(553, 74);
            this.btnCorrectAll.Name = "btnCorrectAll";
            this.btnCorrectAll.Size = new System.Drawing.Size(96, 28);
            this.btnCorrectAll.TabIndex = 5;
            this.btnCorrectAll.Text = "Correct all";
            this.btnCorrectAll.UseVisualStyleBackColor = true;
            this.btnCorrectAll.Click += new System.EventHandler(this.btnCorrectAll_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(553, 119);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 28);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnCorrect
            // 
            this.btnCorrect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCorrect.Location = new System.Drawing.Point(553, 29);
            this.btnCorrect.Name = "btnCorrect";
            this.btnCorrect.Size = new System.Drawing.Size(96, 28);
            this.btnCorrect.TabIndex = 3;
            this.btnCorrect.Text = "Correct this";
            this.btnCorrect.UseVisualStyleBackColor = true;
            this.btnCorrect.Click += new System.EventHandler(this.btnCorrect_Click);
            // 
            // dgvResultDetails
            // 
            this.dgvResultDetails.AllowUserToAddRows = false;
            this.dgvResultDetails.AllowUserToDeleteRows = false;
            this.dgvResultDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvResultDetails.BackgroundColor = System.Drawing.Color.White;
            this.dgvResultDetails.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvResultDetails.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvResultDetails.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResultDetails.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colObjectType,
            this.colInvalidName,
            this.colName});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvResultDetails.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvResultDetails.Location = new System.Drawing.Point(4, 4);
            this.dgvResultDetails.Margin = new System.Windows.Forms.Padding(4);
            this.dgvResultDetails.MultiSelect = false;
            this.dgvResultDetails.Name = "dgvResultDetails";
            this.dgvResultDetails.ReadOnly = true;
            this.dgvResultDetails.RowHeadersVisible = false;
            this.dgvResultDetails.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvResultDetails.Size = new System.Drawing.Size(528, 163);
            this.dgvResultDetails.TabIndex = 2;
            this.dgvResultDetails.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dgvResultDetails_DataBindingComplete);
            this.dgvResultDetails.SelectionChanged += new System.EventHandler(this.dgvResult_SelectionChanged);
            // 
            // colObjectType
            // 
            this.colObjectType.HeaderText = "Object Type";
            this.colObjectType.Name = "colObjectType";
            this.colObjectType.ReadOnly = true;
            this.colObjectType.Width = 120;
            // 
            // colInvalidName
            // 
            this.colInvalidName.DataPropertyName = "InvalidName";
            this.colInvalidName.HeaderText = "Invalid Name";
            this.colInvalidName.Name = "colInvalidName";
            this.colInvalidName.ReadOnly = true;
            this.colInvalidName.Width = 200;
            // 
            // colName
            // 
            this.colName.DataPropertyName = "Name";
            this.colName.HeaderText = "Correct Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            this.colName.Width = 200;
            // 
            // frmScriptDiagnoseResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(915, 524);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmScriptDiagnoseResult";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Script Diagnose Result";
            this.Load += new System.EventHandler(this.frmScriptDiagnoseResult_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResultDetails)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView tvDbObjects;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox txtDefinition;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnCorrect;
        private System.Windows.Forms.DataGridView dgvResultDetails;
        private System.Windows.Forms.Button btnCorrectAll;
        private System.Windows.Forms.DataGridViewTextBoxColumn colObjectType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInvalidName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
    }
}