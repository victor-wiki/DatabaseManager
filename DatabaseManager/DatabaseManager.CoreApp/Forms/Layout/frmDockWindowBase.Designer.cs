namespace DatabaseManager.Forms
{
    partial class frmDockWindowBase
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
            components = new System.ComponentModel.Container();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiClose = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCloseAll = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCloseOthers = new System.Windows.Forms.ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiClose, tsmiCloseAll, tsmiCloseOthers });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(152, 70);
            // 
            // tsmiClose
            // 
            tsmiClose.Name = "tsmiClose";
            tsmiClose.Size = new System.Drawing.Size(151, 22);
            tsmiClose.Text = "Close";
            tsmiClose.Click += tsmiClose_Click;
            // 
            // tsmiCloseAll
            // 
            tsmiCloseAll.Name = "tsmiCloseAll";
            tsmiCloseAll.Size = new System.Drawing.Size(151, 22);
            tsmiCloseAll.Text = "Close All";
            tsmiCloseAll.Click += tsmiCloseAll_Click;
            // 
            // tsmiCloseOthers
            // 
            tsmiCloseOthers.Name = "tsmiCloseOthers";
            tsmiCloseOthers.Size = new System.Drawing.Size(151, 22);
            tsmiCloseOthers.Text = "Close Others";
            tsmiCloseOthers.Click += tsmiCloseOthers_Click;
            // 
            // frmDockWindowBase
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Name = "frmDockWindowBase";
            Load += frmDockWindowBase_Load;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiClose;
        private System.Windows.Forms.ToolStripMenuItem tsmiCloseAll;
        private System.Windows.Forms.ToolStripMenuItem tsmiCloseOthers;
    }
}