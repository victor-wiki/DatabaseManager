namespace DatabaseManager.Forms
{
    partial class frmTableColumnDetails
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTableColumnDetails));
            uC_TableColumnDetails1 = new DatabaseManager.Controls.UC_TableColumnDetails();
            SuspendLayout();
            // 
            // uC_TableColumnDetails1
            // 
            uC_TableColumnDetails1.Dock = System.Windows.Forms.DockStyle.Fill;
            uC_TableColumnDetails1.Location = new System.Drawing.Point(0, 0);
            uC_TableColumnDetails1.Name = "uC_TableColumnDetails1";
            uC_TableColumnDetails1.Size = new System.Drawing.Size(241, 250);
            uC_TableColumnDetails1.TabIndex = 0;
            // 
            // frmTableColumnDetails
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(241, 250);
            Controls.Add(uC_TableColumnDetails1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "frmTableColumnDetails";
            ResumeLayout(false);
        }

        #endregion

        private Controls.UC_TableColumnDetails uC_TableColumnDetails1;
    }
}