namespace DatabaseManager.Controls
{
    partial class UC_DbObjectsSimpleTree
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
            this.tvDbObjects = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.CheckBoxes = true;
            this.tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDbObjects.Location = new System.Drawing.Point(0, 0);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.Size = new System.Drawing.Size(249, 317);
            this.tvDbObjects.TabIndex = 20;
            this.tvDbObjects.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tvDbObjects_AfterCheck);
            // 
            // UC_DbObjectsTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvDbObjects);
            this.Name = "UC_DbObjectsTree";
            this.Size = new System.Drawing.Size(249, 317);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvDbObjects;
    }
}
