namespace DatabaseManager.Controls
{
    partial class UC_LoadingPanel
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new System.Windows.Forms.Panel();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            btnInterrupt = new System.Windows.Forms.Button();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(progressBar1);
            panel1.Controls.Add(btnInterrupt);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(237, 108);
            panel1.TabIndex = 0;
            // 
            // progressBar1
            // 
            progressBar1.Location = new System.Drawing.Point(70, 21);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(100, 23);
            progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            progressBar1.TabIndex = 5;
            // 
            // btnInterrupt
            // 
            btnInterrupt.Location = new System.Drawing.Point(70, 68);
            btnInterrupt.Name = "btnInterrupt";
            btnInterrupt.Size = new System.Drawing.Size(100, 23);
            btnInterrupt.TabIndex = 4;
            btnInterrupt.Text = "Interrupt";
            btnInterrupt.UseVisualStyleBackColor = true;
            btnInterrupt.Click += btnInterrupt_Click;
            // 
            // UC_LoadingPanel
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            Controls.Add(panel1);
            Name = "UC_LoadingPanel";
            Size = new System.Drawing.Size(237, 108);           
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnInterrupt;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}
