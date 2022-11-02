namespace DatabaseManager
{
    partial class frmWktViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmWktViewer));
            this.panelContent = new System.Windows.Forms.Panel();
            this.picGeometry = new System.Windows.Forms.PictureBox();
            this.tbZoom = new System.Windows.Forms.TrackBar();
            this.rbGeometry = new System.Windows.Forms.RadioButton();
            this.rbGeography = new System.Windows.Forms.RadioButton();
            this.txtContent = new System.Windows.Forms.TextBox();
            this.btnView = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.panelContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picGeometry)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // panelContent
            // 
            this.panelContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelContent.BackColor = System.Drawing.Color.White;
            this.panelContent.Controls.Add(this.picGeometry);
            this.panelContent.Location = new System.Drawing.Point(1, 3);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(796, 352);
            this.panelContent.TabIndex = 0;
            // 
            // picGeometry
            // 
            this.picGeometry.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picGeometry.Location = new System.Drawing.Point(0, 0);
            this.picGeometry.Name = "picGeometry";
            this.picGeometry.Size = new System.Drawing.Size(796, 352);
            this.picGeometry.TabIndex = 0;
            this.picGeometry.TabStop = false;
            this.picGeometry.SizeChanged += new System.EventHandler(this.picGeometry_SizeChanged);
            // 
            // tbZoom
            // 
            this.tbZoom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbZoom.AutoSize = false;
            this.tbZoom.Location = new System.Drawing.Point(1, 362);
            this.tbZoom.Maximum = 100;
            this.tbZoom.Name = "tbZoom";
            this.tbZoom.Size = new System.Drawing.Size(570, 26);
            this.tbZoom.TabIndex = 1;
            this.tbZoom.ValueChanged += new System.EventHandler(this.tbZoom_ValueChanged);
            // 
            // rbGeometry
            // 
            this.rbGeometry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rbGeometry.AutoSize = true;
            this.rbGeometry.Checked = true;
            this.rbGeometry.Location = new System.Drawing.Point(595, 362);
            this.rbGeometry.Name = "rbGeometry";
            this.rbGeometry.Size = new System.Drawing.Size(83, 21);
            this.rbGeometry.TabIndex = 2;
            this.rbGeometry.TabStop = true;
            this.rbGeometry.Text = "Geometry";
            this.rbGeometry.UseVisualStyleBackColor = true;
            // 
            // rbGeography
            // 
            this.rbGeography.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.rbGeography.AutoSize = true;
            this.rbGeography.Location = new System.Drawing.Point(693, 362);
            this.rbGeography.Name = "rbGeography";
            this.rbGeography.Size = new System.Drawing.Size(91, 21);
            this.rbGeography.TabIndex = 3;
            this.rbGeography.Text = "Geography";
            this.rbGeography.UseVisualStyleBackColor = true;
            this.rbGeography.CheckedChanged += new System.EventHandler(this.rbGeography_CheckedChanged);
            // 
            // txtContent
            // 
            this.txtContent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtContent.Location = new System.Drawing.Point(1, 394);
            this.txtContent.MaxLength = 1000000;
            this.txtContent.Multiline = true;
            this.txtContent.Name = "txtContent";
            this.txtContent.Size = new System.Drawing.Size(715, 85);
            this.txtContent.TabIndex = 4;
            // 
            // btnView
            // 
            this.btnView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnView.Location = new System.Drawing.Point(722, 405);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(75, 23);
            this.btnView.TabIndex = 5;
            this.btnView.Text = "View";
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(722, 445);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // frmWktViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 482);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnView);
            this.Controls.Add(this.txtContent);
            this.Controls.Add(this.rbGeography);
            this.Controls.Add(this.rbGeometry);
            this.Controls.Add(this.tbZoom);
            this.Controls.Add(this.panelContent);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmWktViewer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WKT Viewer";
            this.Load += new System.EventHandler(this.frmGeometryViewer_Load);
            this.panelContent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picGeometry)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbZoom)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.TrackBar tbZoom;
        private System.Windows.Forms.RadioButton rbGeometry;
        private System.Windows.Forms.RadioButton rbGeography;
        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.PictureBox picGeometry;
    }
}