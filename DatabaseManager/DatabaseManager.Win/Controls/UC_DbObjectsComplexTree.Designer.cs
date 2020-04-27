namespace DatabaseManager.Controls
{
    partial class UC_DbObjectsComplexTree
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_DbObjectsComplexTree));
            this.tvDbObjects = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiNewQuery = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewData = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiConvert = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiTranslate = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiClearData = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiEmptyDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewFunction = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewProcedure = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewTrigger = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDbObjects.Font = new System.Drawing.Font("Times New Roman", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvDbObjects.HideSelection = false;
            this.tvDbObjects.ImageIndex = 0;
            this.tvDbObjects.ImageList = this.imageList1;
            this.tvDbObjects.Location = new System.Drawing.Point(0, 0);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.SelectedImageIndex = 0;
            this.tvDbObjects.ShowLines = false;
            this.tvDbObjects.Size = new System.Drawing.Size(159, 195);
            this.tvDbObjects.TabIndex = 20;
            this.tvDbObjects.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvDbObjects_BeforeExpand);
            this.tvDbObjects.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.tvDbObjects_AfterExpand);
            this.tvDbObjects.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvDbObjects_ItemDrag);
            this.tvDbObjects.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvDbObjects_NodeMouseClick);
            this.tvDbObjects.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvDbObjects_KeyDown);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "tree_Fake.png");
            this.imageList1.Images.SetKeyName(1, "tree_Database.png");
            this.imageList1.Images.SetKeyName(2, "tree_Folder.png");
            this.imageList1.Images.SetKeyName(3, "tree_TableForeignKey.png");
            this.imageList1.Images.SetKeyName(4, "tree_Procedure.png");
            this.imageList1.Images.SetKeyName(5, "tree_View.png");
            this.imageList1.Images.SetKeyName(6, "tree_TableIndex.png");
            this.imageList1.Images.SetKeyName(7, "tree_TableColumn.png");
            this.imageList1.Images.SetKeyName(8, "tree_TablePrimaryKey.png");
            this.imageList1.Images.SetKeyName(9, "tree_Table.png");
            this.imageList1.Images.SetKeyName(10, "tree_TableConstraint.png");
            this.imageList1.Images.SetKeyName(11, "tree_TableTrigger.png");
            this.imageList1.Images.SetKeyName(12, "Loading.gif");
            this.imageList1.Images.SetKeyName(13, "tree_Function.png");
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiNewQuery,
            this.tsmiNewView,
            this.tsmiNewFunction,
            this.tsmiNewProcedure,
            this.tsmiNewTrigger,
            this.tsmiRefresh,
            this.tsmiViewData,
            this.tsmiConvert,
            this.tsmiTranslate,
            this.tsmiGenerateScripts,
            this.tsmiClearData,
            this.tsmiEmptyDatabase,
            this.tsmiDelete});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 312);
            // 
            // tsmiNewQuery
            // 
            this.tsmiNewQuery.Name = "tsmiNewQuery";
            this.tsmiNewQuery.Size = new System.Drawing.Size(180, 22);
            this.tsmiNewQuery.Text = "New Query";
            this.tsmiNewQuery.Click += new System.EventHandler(this.tsmiNewQuery_Click);
            // 
            // tsmiRefresh
            // 
            this.tsmiRefresh.Name = "tsmiRefresh";
            this.tsmiRefresh.Size = new System.Drawing.Size(180, 22);
            this.tsmiRefresh.Text = "Refresh";
            this.tsmiRefresh.Click += new System.EventHandler(this.tsmiRefresh_Click);
            // 
            // tsmiViewData
            // 
            this.tsmiViewData.Name = "tsmiViewData";
            this.tsmiViewData.Size = new System.Drawing.Size(180, 22);
            this.tsmiViewData.Text = "View Data";
            this.tsmiViewData.Click += new System.EventHandler(this.tsmiViewData_Click);
            // 
            // tsmiConvert
            // 
            this.tsmiConvert.Name = "tsmiConvert";
            this.tsmiConvert.Size = new System.Drawing.Size(180, 22);
            this.tsmiConvert.Text = "Convert";
            this.tsmiConvert.Click += new System.EventHandler(this.tsmiConvert_Click);
            // 
            // tsmiTranslate
            // 
            this.tsmiTranslate.Name = "tsmiTranslate";
            this.tsmiTranslate.Size = new System.Drawing.Size(180, 22);
            this.tsmiTranslate.Text = "Translate";
            this.tsmiTranslate.MouseEnter += new System.EventHandler(this.tsmiTranslate_MouseEnter);
            // 
            // tsmiGenerateScripts
            // 
            this.tsmiGenerateScripts.Name = "tsmiGenerateScripts";
            this.tsmiGenerateScripts.Size = new System.Drawing.Size(180, 22);
            this.tsmiGenerateScripts.Text = "Generate Scripts";
            this.tsmiGenerateScripts.Click += new System.EventHandler(this.tsmiGenerateScripts_Click);
            // 
            // tsmiClearData
            // 
            this.tsmiClearData.Name = "tsmiClearData";
            this.tsmiClearData.Size = new System.Drawing.Size(180, 22);
            this.tsmiClearData.Text = "Clear Data";
            this.tsmiClearData.Click += new System.EventHandler(this.tsmiClearData_Click);
            // 
            // tsmiEmptyDatabase
            // 
            this.tsmiEmptyDatabase.Name = "tsmiEmptyDatabase";
            this.tsmiEmptyDatabase.Size = new System.Drawing.Size(180, 22);
            this.tsmiEmptyDatabase.Text = "Empty";
            this.tsmiEmptyDatabase.Click += new System.EventHandler(this.tsmiEmptyDatabase_Click);
            // 
            // tsmiDelete
            // 
            this.tsmiDelete.Name = "tsmiDelete";
            this.tsmiDelete.Size = new System.Drawing.Size(180, 22);
            this.tsmiDelete.Text = "Delete";
            this.tsmiDelete.Click += new System.EventHandler(this.tsmiDelete_Click);
            // 
            // tsmiNewView
            // 
            this.tsmiNewView.Name = "tsmiNewView";
            this.tsmiNewView.Size = new System.Drawing.Size(180, 22);
            this.tsmiNewView.Text = "New View";
            this.tsmiNewView.Click += new System.EventHandler(this.tsmiNewView_Click);
            // 
            // tsmiNewFunction
            // 
            this.tsmiNewFunction.Name = "tsmiNewFunction";
            this.tsmiNewFunction.Size = new System.Drawing.Size(180, 22);
            this.tsmiNewFunction.Text = "New Function";
            this.tsmiNewFunction.Click += new System.EventHandler(this.tsmiNewFunction_Click);
            // 
            // tsmiNewProcedure
            // 
            this.tsmiNewProcedure.Name = "tsmiNewProcedure";
            this.tsmiNewProcedure.Size = new System.Drawing.Size(180, 22);
            this.tsmiNewProcedure.Text = "New Procedure";
            this.tsmiNewProcedure.Click += new System.EventHandler(this.tsmiNewProcedure_Click);
            // 
            // tsmiNewTrigger
            // 
            this.tsmiNewTrigger.Name = "tsmiNewTrigger";
            this.tsmiNewTrigger.Size = new System.Drawing.Size(180, 22);
            this.tsmiNewTrigger.Text = "New Trigger";
            this.tsmiNewTrigger.Click += new System.EventHandler(this.tsmiNewTrigger_Click);
            // 
            // UC_DbObjectsComplexTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvDbObjects);
            this.Name = "UC_DbObjectsComplexTree";
            this.Size = new System.Drawing.Size(159, 195);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView tvDbObjects;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tsmiGenerateScripts;
        private System.Windows.Forms.ToolStripMenuItem tsmiRefresh;
        private System.Windows.Forms.ToolStripMenuItem tsmiConvert;
        private System.Windows.Forms.ToolStripMenuItem tsmiClearData;
        private System.Windows.Forms.ToolStripMenuItem tsmiEmptyDatabase;
        private System.Windows.Forms.ToolStripMenuItem tsmiDelete;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewData;
        private System.Windows.Forms.ToolStripMenuItem tsmiTranslate;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewQuery;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewView;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewFunction;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewProcedure;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewTrigger;
    }
}
