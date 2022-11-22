namespace DatabaseManager
{
    partial class frmSchemaMapping
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.gbSourceSchema = new System.Windows.Forms.GroupBox();
            this.panelSourceSchema = new System.Windows.Forms.Panel();
            this.gbTargetSchema = new System.Windows.Forms.GroupBox();
            this.panelTargetSchema = new System.Windows.Forms.Panel();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnAutoMap = new System.Windows.Forms.Button();
            this.gbSourceSchema.SuspendLayout();
            this.gbTargetSchema.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(426, 415);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 33);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(314, 415);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 33);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // gbSourceSchema
            // 
            this.gbSourceSchema.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gbSourceSchema.Controls.Add(this.panelSourceSchema);
            this.gbSourceSchema.Location = new System.Drawing.Point(7, 17);
            this.gbSourceSchema.Margin = new System.Windows.Forms.Padding(4);
            this.gbSourceSchema.Name = "gbSourceSchema";
            this.gbSourceSchema.Padding = new System.Windows.Forms.Padding(4);
            this.gbSourceSchema.Size = new System.Drawing.Size(251, 381);
            this.gbSourceSchema.TabIndex = 5;
            this.gbSourceSchema.TabStop = false;
            this.gbSourceSchema.Text = "Source database schema";
            // 
            // panelSourceSchema
            // 
            this.panelSourceSchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSourceSchema.Location = new System.Drawing.Point(4, 20);
            this.panelSourceSchema.Margin = new System.Windows.Forms.Padding(4);
            this.panelSourceSchema.Name = "panelSourceSchema";
            this.panelSourceSchema.Size = new System.Drawing.Size(243, 357);
            this.panelSourceSchema.TabIndex = 0;
            // 
            // gbTargetSchema
            // 
            this.gbTargetSchema.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbTargetSchema.Controls.Add(this.panelTargetSchema);
            this.gbTargetSchema.Location = new System.Drawing.Point(265, 17);
            this.gbTargetSchema.Margin = new System.Windows.Forms.Padding(4);
            this.gbTargetSchema.Name = "gbTargetSchema";
            this.gbTargetSchema.Padding = new System.Windows.Forms.Padding(4);
            this.gbTargetSchema.Size = new System.Drawing.Size(254, 381);
            this.gbTargetSchema.TabIndex = 6;
            this.gbTargetSchema.TabStop = false;
            this.gbTargetSchema.Text = "Target database schema";
            // 
            // panelTargetSchema
            // 
            this.panelTargetSchema.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTargetSchema.Location = new System.Drawing.Point(4, 20);
            this.panelTargetSchema.Margin = new System.Windows.Forms.Padding(4);
            this.panelTargetSchema.Name = "panelTargetSchema";
            this.panelTargetSchema.Size = new System.Drawing.Size(246, 357);
            this.panelTargetSchema.TabIndex = 1;
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.Location = new System.Drawing.Point(125, 414);
            this.btnReset.Margin = new System.Windows.Forms.Padding(4);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(88, 33);
            this.btnReset.TabIndex = 8;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnAutoMap
            // 
            this.btnAutoMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAutoMap.Location = new System.Drawing.Point(13, 414);
            this.btnAutoMap.Margin = new System.Windows.Forms.Padding(4);
            this.btnAutoMap.Name = "btnAutoMap";
            this.btnAutoMap.Size = new System.Drawing.Size(88, 33);
            this.btnAutoMap.TabIndex = 7;
            this.btnAutoMap.Text = "Auto Map";
            this.btnAutoMap.UseVisualStyleBackColor = true;
            this.btnAutoMap.Click += new System.EventHandler(this.btnAutoMap_Click);
            // 
            // frmSchemaMapping
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(527, 460);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnAutoMap);
            this.Controls.Add(this.gbTargetSchema);
            this.Controls.Add(this.gbSourceSchema);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "frmSchemaMapping";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Schema Mapping";
            this.Load += new System.EventHandler(this.frmColumnMapping_Load);
            this.gbSourceSchema.ResumeLayout(false);
            this.gbTargetSchema.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox gbSourceSchema;
        private System.Windows.Forms.GroupBox gbTargetSchema;
        private System.Windows.Forms.Panel panelSourceSchema;
        private System.Windows.Forms.Panel panelTargetSchema;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnAutoMap;
    }
}