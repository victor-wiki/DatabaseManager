﻿namespace DatabaseManager.Controls
{
    partial class UC_DbObjectContent
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
            dlgSave = new System.Windows.Forms.SaveFileDialog();
            SuspendLayout();
            // 
            // dlgSave
            // 
            dlgSave.Filter = "sql file|*.sql|txt file|*.txt";
            // 
            // UC_DbObjectContent
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "UC_DbObjectContent";
            Size = new System.Drawing.Size(579, 653);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.SaveFileDialog dlgSave;
    }
}
