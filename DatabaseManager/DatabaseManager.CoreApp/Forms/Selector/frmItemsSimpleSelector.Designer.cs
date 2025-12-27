namespace DatabaseManager.Forms
{
    partial class frmItemsSimpleSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmItemsSimpleSelector));
            dgvItems = new System.Windows.Forms.DataGridView();
            colItem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dgvItems).BeginInit();
            SuspendLayout();
            // 
            // dgvItems
            // 
            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dgvItems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvItems.ColumnHeadersVisible = false;
            dgvItems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colItem });
            dgvItems.Location = new System.Drawing.Point(2, 3);
            dgvItems.Margin = new System.Windows.Forms.Padding(4);
            dgvItems.Name = "dgvItems";
            dgvItems.ReadOnly = true;
            dgvItems.RowHeadersVisible = false;
            dgvItems.RowHeadersWidth = 25;
            dgvItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvItems.Size = new System.Drawing.Size(276, 423);
            dgvItems.TabIndex = 0;
            dgvItems.CellMouseDoubleClick += dgvItems_CellMouseDoubleClick;
            dgvItems.DataBindingComplete += dgvItems_DataBindingComplete;
            dgvItems.DataError += dgvItems_DataError;
            // 
            // colItem
            // 
            colItem.Frozen = true;
            colItem.HeaderText = "Item";
            colItem.Name = "colItem";
            colItem.ReadOnly = true;
            colItem.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            colItem.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            colItem.Width = 250;
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnOK.Location = new System.Drawing.Point(30, 434);
            btnOK.Margin = new System.Windows.Forms.Padding(4);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(88, 33);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnCancel.Location = new System.Drawing.Point(142, 434);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 33);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // frmItemsSimpleSelector
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(281, 470);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(dgvItems);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            Name = "frmItemsSimpleSelector";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Select Item";
            Load += frmColumSelect_Load;
            ((System.ComponentModel.ISupportInitialize)dgvItems).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvItems;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItem;
    }
}