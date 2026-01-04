namespace DatabaseManager.Controls
{
    partial class UC_TableColumnDetails
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_TableColumnDetails));
            imageList1 = new System.Windows.Forms.ImageList(components);
            lvTableColumns = new System.Windows.Forms.ListView();
            colFlag = new System.Windows.Forms.ColumnHeader();
            colColumnName = new System.Windows.Forms.ColumnHeader();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            SuspendLayout();
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "tree_TablePrimaryKey.png");
            imageList1.Images.SetKeyName(1, "tree_TableForeignKey.png");
            // 
            // lvTableColumns
            // 
            lvTableColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colFlag, colColumnName });
            lvTableColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            lvTableColumns.FullRowSelect = true;
            lvTableColumns.GridLines = true;
            lvTableColumns.Location = new System.Drawing.Point(0, 0);
            lvTableColumns.MultiSelect = false;
            lvTableColumns.Name = "lvTableColumns";
            lvTableColumns.ShowItemToolTips = true;
            lvTableColumns.Size = new System.Drawing.Size(230, 260);
            lvTableColumns.SmallImageList = imageList1;
            lvTableColumns.TabIndex = 2;
            lvTableColumns.UseCompatibleStateImageBehavior = false;
            lvTableColumns.View = System.Windows.Forms.View.Details;
            lvTableColumns.MouseLeave += lvTableColumns_MouseLeave;
            lvTableColumns.MouseMove += lvTableColumns_MouseMove;
            // 
            // colFlag
            // 
            colFlag.Text = "";
            colFlag.Width = 20;
            // 
            // colColumnName
            // 
            colColumnName.Text = "Colum Name";
            colColumnName.Width = 200;
            // 
            // UC_TableColumnDetails
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(lvTableColumns);
            Name = "UC_TableColumnDetails";
            Size = new System.Drawing.Size(230, 260);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ListView lvTableColumns;
        private System.Windows.Forms.ColumnHeader colFlag;
        private System.Windows.Forms.ColumnHeader colColumnName;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
