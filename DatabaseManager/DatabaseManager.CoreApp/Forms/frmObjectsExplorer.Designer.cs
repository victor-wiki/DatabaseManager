namespace DatabaseManager.Forms
{
    partial class frmObjectsExplorer
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
            ucExplorer = new Controls.UC_DbObjectsExplorer();
            SuspendLayout();
            // 
            // ucNavigator
            // 
            ucExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            ucExplorer.Location = new System.Drawing.Point(0, 0);
            ucExplorer.Margin = new System.Windows.Forms.Padding(4);
            ucExplorer.Name = "ucNavigator";
            ucExplorer.Size = new System.Drawing.Size(224, 450);
            ucExplorer.TabIndex = 0;
            // 
            // frmObjectsExplorer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(224, 450);
            Controls.Add(ucExplorer);
            DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight;
            Name = "frmObjectsExplorer";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Objects Explorer";          
            ResumeLayout(false);
        }

        #endregion

        private Controls.UC_DbObjectsExplorer ucExplorer;
    }
}