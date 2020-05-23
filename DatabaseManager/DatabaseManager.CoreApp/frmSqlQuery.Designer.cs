namespace DatabaseManager
{
    partial class frmSqlQuery
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSqlQuery));
            this.btnClose = new System.Windows.Forms.Button();
            this.ucSqlQuery = new DatabaseManager.Controls.UC_SqlQuery();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(615, 387);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 33);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // ucSqlQuery
            // 
            this.ucSqlQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ucSqlQuery.Location = new System.Drawing.Point(0, 0);
            this.ucSqlQuery.Margin = new System.Windows.Forms.Padding(0);
            this.ucSqlQuery.Name = "ucSqlQuery";
            this.ucSqlQuery.ReadOnly = false;
            this.ucSqlQuery.ShowEditorMessage = true;
            this.ucSqlQuery.Size = new System.Drawing.Size(714, 371);
            this.ucSqlQuery.SplitterDistance = 250;
            this.ucSqlQuery.TabIndex = 0;
            // 
            // frmSqlQuery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 429);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.ucSqlQuery);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmSqlQuery";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sql Query";
            this.Load += new System.EventHandler(this.frmSqlQuery_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.UC_SqlQuery ucSqlQuery;
        private System.Windows.Forms.Button btnClose;
    }
}