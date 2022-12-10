namespace DatabaseManager.Forms
{
    partial class frmFileConnection
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
            this.ucFileConnection = new DatabaseManager.Controls.UC_FileConnectionInfo();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.txtDisplayName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panelContent = new System.Windows.Forms.Panel();
            this.panelChoose = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.rbChoose = new System.Windows.Forms.RadioButton();
            this.rbInput = new System.Windows.Forms.RadioButton();
            this.panelContent.SuspendLayout();
            this.panelChoose.SuspendLayout();
            this.SuspendLayout();
            // 
            // ucFileConnection
            // 
            this.ucFileConnection.DatabaseType = DatabaseInterpreter.Model.DatabaseType.Unknown;
            this.ucFileConnection.Location = new System.Drawing.Point(5, 4);
            this.ucFileConnection.Name = "ucFileConnection";
            this.ucFileConnection.Size = new System.Drawing.Size(358, 136);
            this.ucFileConnection.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(205, 189);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 33);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(105, 189);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(4);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(88, 33);
            this.btnConfirm.TabIndex = 13;
            this.btnConfirm.Text = "OK";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // txtDisplayName
            // 
            this.txtDisplayName.Location = new System.Drawing.Point(106, 148);
            this.txtDisplayName.Name = "txtDisplayName";
            this.txtDisplayName.Size = new System.Drawing.Size(187, 23);
            this.txtDisplayName.TabIndex = 16;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 149);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 17);
            this.label3.TabIndex = 15;
            this.label3.Text = "Display name:";
            // 
            // panelContent
            // 
            this.panelContent.Controls.Add(this.ucFileConnection);
            this.panelContent.Controls.Add(this.txtDisplayName);
            this.panelContent.Controls.Add(this.btnCancel);
            this.panelContent.Controls.Add(this.label3);
            this.panelContent.Controls.Add(this.btnConfirm);
            this.panelContent.Location = new System.Drawing.Point(3, 36);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(364, 233);
            this.panelContent.TabIndex = 17;
            // 
            // panelChoose
            // 
            this.panelChoose.Controls.Add(this.label1);
            this.panelChoose.Controls.Add(this.rbChoose);
            this.panelChoose.Controls.Add(this.rbInput);
            this.panelChoose.Location = new System.Drawing.Point(3, 5);
            this.panelChoose.Name = "panelChoose";
            this.panelChoose.Size = new System.Drawing.Size(363, 27);
            this.panelChoose.TabIndex = 18;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Mode:";
            // 
            // rbChoose
            // 
            this.rbChoose.AutoSize = true;
            this.rbChoose.Location = new System.Drawing.Point(215, 3);
            this.rbChoose.Name = "rbChoose";
            this.rbChoose.Size = new System.Drawing.Size(70, 21);
            this.rbChoose.TabIndex = 1;
            this.rbChoose.Text = "Choose";
            this.rbChoose.UseVisualStyleBackColor = true;
            this.rbChoose.CheckedChanged += new System.EventHandler(this.rbChoose_CheckedChanged);
            // 
            // rbInput
            // 
            this.rbInput.AutoSize = true;
            this.rbInput.Checked = true;
            this.rbInput.Location = new System.Drawing.Point(105, 3);
            this.rbInput.Name = "rbInput";
            this.rbInput.Size = new System.Drawing.Size(56, 21);
            this.rbInput.TabIndex = 0;
            this.rbInput.TabStop = true;
            this.rbInput.Text = "Input";
            this.rbInput.UseVisualStyleBackColor = true;
            // 
            // frmFileConnection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(370, 273);
            this.Controls.Add(this.panelChoose);
            this.Controls.Add(this.panelContent);
            this.MaximizeBox = false;
            this.Name = "frmFileConnection";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "File Connection";
            this.Load += new System.EventHandler(this.frmFileConnection_Load);
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.panelChoose.ResumeLayout(false);
            this.panelChoose.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.UC_FileConnectionInfo ucFileConnection;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.TextBox txtDisplayName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Panel panelChoose;
        private System.Windows.Forms.RadioButton rbChoose;
        private System.Windows.Forms.RadioButton rbInput;
        private System.Windows.Forms.Label label1;
    }
}