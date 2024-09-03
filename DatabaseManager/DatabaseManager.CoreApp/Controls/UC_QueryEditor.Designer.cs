namespace DatabaseManager.Controls
{
    partial class UC_QueryEditor
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
            txtToolTip = new System.Windows.Forms.TextBox();
            txtEditor = new SqlCodeEditor.TextEditorControlEx();
            SuspendLayout();
            // 
            // txtToolTip
            // 
            txtToolTip.BackColor = System.Drawing.Color.FromArgb(255, 255, 225);
            txtToolTip.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtToolTip.Location = new System.Drawing.Point(172, 411);
            txtToolTip.Margin = new System.Windows.Forms.Padding(4);
            txtToolTip.Name = "txtToolTip";
            txtToolTip.ReadOnly = true;
            txtToolTip.Size = new System.Drawing.Size(186, 23);
            txtToolTip.TabIndex = 6;
            txtToolTip.Visible = false;
            // 
            // txtEditor
            // 
            txtEditor.ContextMenuEnabled = true;
            txtEditor.ContextMenuShowDefaultIcons = true;
            txtEditor.ContextMenuShowShortCutKeys = true;
            txtEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            txtEditor.EnableFolding = false;
            txtEditor.FoldingStrategy = "XML";
            txtEditor.Font = new System.Drawing.Font("Courier New", 10F);
            txtEditor.Location = new System.Drawing.Point(0, 0);
            txtEditor.Name = "txtEditor";
            txtEditor.ShowVRuler = false;
            txtEditor.Size = new System.Drawing.Size(384, 482);
            txtEditor.SyntaxHighlighting = "TSql";
            txtEditor.TabIndex = 8;
            // 
            // UC_QueryEditor
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(txtEditor);
            Controls.Add(txtToolTip);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "UC_QueryEditor";
            Size = new System.Drawing.Size(384, 482);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TextBox txtToolTip;
        private SqlCodeEditor.TextEditorControlEx txtEditor;
    }
}
