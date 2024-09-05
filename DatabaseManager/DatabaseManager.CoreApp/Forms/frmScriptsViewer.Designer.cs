namespace DatabaseManager
{
    partial class frmScriptsViewer
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
            btnClose = new System.Windows.Forms.Button();
            txtScripts = new SqlCodeEditor.TextEditorControlEx();
            SuspendLayout();
            // 
            // btnClose
            // 
            btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnClose.Location = new System.Drawing.Point(611, 329);
            btnClose.Margin = new System.Windows.Forms.Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(88, 33);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // txtScripts
            // 
            txtScripts.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txtScripts.EnableFolding = false;
            txtScripts.FoldingStrategy = null;
            txtScripts.Font = new System.Drawing.Font("Courier New", 10F);
            txtScripts.Location = new System.Drawing.Point(2, 1);
            txtScripts.Name = "txtScripts";
            txtScripts.ShowVRuler = false;
            txtScripts.Size = new System.Drawing.Size(708, 321);
            txtScripts.SyntaxHighlighting = null;
            txtScripts.TabIndex = 2;
            // 
            // frmScriptsViewer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(713, 366);
            Controls.Add(txtScripts);
            Controls.Add(btnClose);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "frmScriptsViewer";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Scripts Viewer";
            Load += frmScriptsViewer_Load;
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Button btnClose;
        private SqlCodeEditor.TextEditorControlEx txtScripts;
    }
}