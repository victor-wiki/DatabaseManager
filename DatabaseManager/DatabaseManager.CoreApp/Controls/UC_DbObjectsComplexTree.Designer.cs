namespace DatabaseManager.Controls
{
    partial class UC_DbObjectsComplexTree
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_DbObjectsComplexTree));
            this.tvDbObjects = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmiNewQuery = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewTable = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewFunction = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewProcedure = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiNewTrigger = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAlter = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDesign = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewData = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiConvert = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCompare = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiGenerateScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCreateScript = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiSelectScript = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiInsertScript = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiUpdateScript = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDeleteScript = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiExecuteScript = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiTranslate = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiViewDependency = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiCopyChildrenNames = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiMore = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiBackup = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiDiagnose = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiStatistic = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiClearData = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiEmptyDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tvDbObjects
            // 
            this.tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvDbObjects.Font = new System.Drawing.Font("Times New Roman", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tvDbObjects.HideSelection = false;
            this.tvDbObjects.ImageIndex = 0;
            this.tvDbObjects.ImageList = this.imageList1;
            this.tvDbObjects.Location = new System.Drawing.Point(0, 0);
            this.tvDbObjects.Margin = new System.Windows.Forms.Padding(4);
            this.tvDbObjects.Name = "tvDbObjects";
            this.tvDbObjects.SelectedImageIndex = 0;
            this.tvDbObjects.ShowLines = false;
            this.tvDbObjects.Size = new System.Drawing.Size(186, 276);
            this.tvDbObjects.TabIndex = 20;
            this.tvDbObjects.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvDbObjects_BeforeExpand);
            this.tvDbObjects.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.tvDbObjects_AfterExpand);
            this.tvDbObjects.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.tvDbObjects_ItemDrag);
            this.tvDbObjects.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvDbObjects_NodeMouseClick);
            this.tvDbObjects.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvDbObjects_KeyDown);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "tree_Fake.png");
            this.imageList1.Images.SetKeyName(1, "tree_Database.png");
            this.imageList1.Images.SetKeyName(2, "tree_Folder.png");
            this.imageList1.Images.SetKeyName(3, "tree_TableForeignKey.png");
            this.imageList1.Images.SetKeyName(4, "tree_Procedure.png");
            this.imageList1.Images.SetKeyName(5, "tree_View.png");
            this.imageList1.Images.SetKeyName(6, "tree_TableIndex.png");
            this.imageList1.Images.SetKeyName(7, "tree_TablePrimaryKey.png");
            this.imageList1.Images.SetKeyName(8, "tree_Table.png");
            this.imageList1.Images.SetKeyName(9, "tree_TableConstraint.png");
            this.imageList1.Images.SetKeyName(10, "tree_TableTrigger.png");
            this.imageList1.Images.SetKeyName(11, "Loading.gif");
            this.imageList1.Images.SetKeyName(12, "tree_Function.png");
            this.imageList1.Images.SetKeyName(13, "tree_TableColumn.png");
            this.imageList1.Images.SetKeyName(14, "tree_UserDefinedType.png");
            this.imageList1.Images.SetKeyName(15, "tree_Sequence.png");
            this.imageList1.Images.SetKeyName(16, "tree_Function_Trigger.png");
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiNewQuery,
            this.tsmiNewTable,
            this.tsmiNewView,
            this.tsmiNewFunction,
            this.tsmiNewProcedure,
            this.tsmiNewTrigger,
            this.tsmiAlter,
            this.tsmiDesign,
            this.tsmiRefresh,
            this.tsmiViewData,
            this.tsmiConvert,
            this.tsmiCompare,
            this.tsmiGenerateScripts,
            this.tsmiTranslate,
            this.tsmiCopy,
            this.tsmiDelete,
            this.tsmiViewDependency,
            this.tsmiCopyChildrenNames,
            this.tsmiMore});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(204, 422);
            // 
            // tsmiNewQuery
            // 
            this.tsmiNewQuery.Name = "tsmiNewQuery";
            this.tsmiNewQuery.Size = new System.Drawing.Size(203, 22);
            this.tsmiNewQuery.Text = "New Query";
            this.tsmiNewQuery.Click += new System.EventHandler(this.tsmiNewQuery_Click);
            // 
            // tsmiNewTable
            // 
            this.tsmiNewTable.Name = "tsmiNewTable";
            this.tsmiNewTable.Size = new System.Drawing.Size(203, 22);
            this.tsmiNewTable.Text = "New Table";
            this.tsmiNewTable.Click += new System.EventHandler(this.tsmiNewTable_Click);
            // 
            // tsmiNewView
            // 
            this.tsmiNewView.Name = "tsmiNewView";
            this.tsmiNewView.Size = new System.Drawing.Size(203, 22);
            this.tsmiNewView.Text = "New View";
            this.tsmiNewView.Click += new System.EventHandler(this.tsmiNewView_Click);
            // 
            // tsmiNewFunction
            // 
            this.tsmiNewFunction.Name = "tsmiNewFunction";
            this.tsmiNewFunction.Size = new System.Drawing.Size(203, 22);
            this.tsmiNewFunction.Text = "New Function";
            this.tsmiNewFunction.Click += new System.EventHandler(this.tsmiNewFunction_Click);
            // 
            // tsmiNewProcedure
            // 
            this.tsmiNewProcedure.Name = "tsmiNewProcedure";
            this.tsmiNewProcedure.Size = new System.Drawing.Size(203, 22);
            this.tsmiNewProcedure.Text = "New Procedure";
            this.tsmiNewProcedure.Click += new System.EventHandler(this.tsmiNewProcedure_Click);
            // 
            // tsmiNewTrigger
            // 
            this.tsmiNewTrigger.Name = "tsmiNewTrigger";
            this.tsmiNewTrigger.Size = new System.Drawing.Size(203, 22);
            this.tsmiNewTrigger.Text = "New Trigger";
            this.tsmiNewTrigger.Click += new System.EventHandler(this.tsmiNewTrigger_Click);
            // 
            // tsmiAlter
            // 
            this.tsmiAlter.Name = "tsmiAlter";
            this.tsmiAlter.Size = new System.Drawing.Size(203, 22);
            this.tsmiAlter.Text = "Modify";
            this.tsmiAlter.Click += new System.EventHandler(this.tsmiAlter_Click);
            // 
            // tsmiDesign
            // 
            this.tsmiDesign.Name = "tsmiDesign";
            this.tsmiDesign.Size = new System.Drawing.Size(203, 22);
            this.tsmiDesign.Text = "Design";
            this.tsmiDesign.Click += new System.EventHandler(this.tsmiDesign_Click);
            // 
            // tsmiRefresh
            // 
            this.tsmiRefresh.Name = "tsmiRefresh";
            this.tsmiRefresh.Size = new System.Drawing.Size(203, 22);
            this.tsmiRefresh.Text = "Refresh";
            this.tsmiRefresh.Click += new System.EventHandler(this.tsmiRefresh_Click);
            // 
            // tsmiViewData
            // 
            this.tsmiViewData.Name = "tsmiViewData";
            this.tsmiViewData.Size = new System.Drawing.Size(203, 22);
            this.tsmiViewData.Text = "View Data";
            this.tsmiViewData.Click += new System.EventHandler(this.tsmiViewData_Click);
            // 
            // tsmiConvert
            // 
            this.tsmiConvert.Name = "tsmiConvert";
            this.tsmiConvert.Size = new System.Drawing.Size(203, 22);
            this.tsmiConvert.Text = "Convert";
            this.tsmiConvert.Click += new System.EventHandler(this.tsmiConvert_Click);
            // 
            // tsmiCompare
            // 
            this.tsmiCompare.Name = "tsmiCompare";
            this.tsmiCompare.Size = new System.Drawing.Size(203, 22);
            this.tsmiCompare.Text = "Compare";
            this.tsmiCompare.Click += new System.EventHandler(this.tsmiCompare_Click);
            // 
            // tsmiGenerateScripts
            // 
            this.tsmiGenerateScripts.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiCreateScript,
            this.tsmiSelectScript,
            this.tsmiInsertScript,
            this.tsmiUpdateScript,
            this.tsmiDeleteScript,
            this.tsmiExecuteScript});
            this.tsmiGenerateScripts.Name = "tsmiGenerateScripts";
            this.tsmiGenerateScripts.Size = new System.Drawing.Size(203, 22);
            this.tsmiGenerateScripts.Text = "Scripts";
            // 
            // tsmiCreateScript
            // 
            this.tsmiCreateScript.Name = "tsmiCreateScript";
            this.tsmiCreateScript.Size = new System.Drawing.Size(157, 22);
            this.tsmiCreateScript.Text = "Create Script";
            this.tsmiCreateScript.Click += new System.EventHandler(this.tsmiCreateScript_Click);
            // 
            // tsmiSelectScript
            // 
            this.tsmiSelectScript.Name = "tsmiSelectScript";
            this.tsmiSelectScript.Size = new System.Drawing.Size(157, 22);
            this.tsmiSelectScript.Text = "Select Script";
            this.tsmiSelectScript.Click += new System.EventHandler(this.tsmiSelectScript_Click);
            // 
            // tsmiInsertScript
            // 
            this.tsmiInsertScript.Name = "tsmiInsertScript";
            this.tsmiInsertScript.Size = new System.Drawing.Size(157, 22);
            this.tsmiInsertScript.Text = "Insert Script";
            this.tsmiInsertScript.Click += new System.EventHandler(this.tsmiInsertScript_Click);
            // 
            // tsmiUpdateScript
            // 
            this.tsmiUpdateScript.Name = "tsmiUpdateScript";
            this.tsmiUpdateScript.Size = new System.Drawing.Size(157, 22);
            this.tsmiUpdateScript.Text = "Update Script";
            this.tsmiUpdateScript.Click += new System.EventHandler(this.tsmiUpdateScript_Click);
            // 
            // tsmiDeleteScript
            // 
            this.tsmiDeleteScript.Name = "tsmiDeleteScript";
            this.tsmiDeleteScript.Size = new System.Drawing.Size(157, 22);
            this.tsmiDeleteScript.Text = "Delete Script";
            this.tsmiDeleteScript.Click += new System.EventHandler(this.tsmiDeleteScript_Click);
            // 
            // tsmiExecuteScript
            // 
            this.tsmiExecuteScript.Name = "tsmiExecuteScript";
            this.tsmiExecuteScript.Size = new System.Drawing.Size(157, 22);
            this.tsmiExecuteScript.Text = "Execute Script";
            this.tsmiExecuteScript.Click += new System.EventHandler(this.tsmiExecuteScript_Click);
            // 
            // tsmiTranslate
            // 
            this.tsmiTranslate.Name = "tsmiTranslate";
            this.tsmiTranslate.Size = new System.Drawing.Size(203, 22);
            this.tsmiTranslate.Text = "Translate to";
            this.tsmiTranslate.MouseEnter += new System.EventHandler(this.tsmiTranslate_MouseEnter);
            // 
            // tsmiCopy
            // 
            this.tsmiCopy.Name = "tsmiCopy";
            this.tsmiCopy.Size = new System.Drawing.Size(203, 22);
            this.tsmiCopy.Text = "Copy";
            this.tsmiCopy.Click += new System.EventHandler(this.tsmiCopy_Click);
            // 
            // tsmiDelete
            // 
            this.tsmiDelete.Name = "tsmiDelete";
            this.tsmiDelete.Size = new System.Drawing.Size(203, 22);
            this.tsmiDelete.Text = "Delete";
            this.tsmiDelete.Click += new System.EventHandler(this.tsmiDelete_Click);
            // 
            // tsmiViewDependency
            // 
            this.tsmiViewDependency.Name = "tsmiViewDependency";
            this.tsmiViewDependency.Size = new System.Drawing.Size(203, 22);
            this.tsmiViewDependency.Text = "View Dependencies";
            this.tsmiViewDependency.Click += new System.EventHandler(this.tsmiViewDependency_Click);
            // 
            // tsmiCopyChildrenNames
            // 
            this.tsmiCopyChildrenNames.Name = "tsmiCopyChildrenNames";
            this.tsmiCopyChildrenNames.Size = new System.Drawing.Size(203, 22);
            this.tsmiCopyChildrenNames.Text = "Copy Children Names";
            this.tsmiCopyChildrenNames.Click += new System.EventHandler(this.tsmiCopyChildrenNames_Click);
            // 
            // tsmiMore
            // 
            this.tsmiMore.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiBackup,
            this.tsmiDiagnose,
            this.tsmiStatistic,
            this.tsmiClearData,
            this.tsmiEmptyDatabase});
            this.tsmiMore.Name = "tsmiMore";
            this.tsmiMore.Size = new System.Drawing.Size(203, 22);
            this.tsmiMore.Text = "More";
            // 
            // tsmiBackup
            // 
            this.tsmiBackup.Name = "tsmiBackup";
            this.tsmiBackup.Size = new System.Drawing.Size(180, 22);
            this.tsmiBackup.Text = "Backup";
            this.tsmiBackup.Click += new System.EventHandler(this.tsmiBackup_Click);
            // 
            // tsmiDiagnose
            // 
            this.tsmiDiagnose.Name = "tsmiDiagnose";
            this.tsmiDiagnose.Size = new System.Drawing.Size(180, 22);
            this.tsmiDiagnose.Text = "Diagnose";
            this.tsmiDiagnose.Click += new System.EventHandler(this.tsmiDiagnose_Click);
            // 
            // tsmiStatistic
            // 
            this.tsmiStatistic.Name = "tsmiStatistic";
            this.tsmiStatistic.Size = new System.Drawing.Size(180, 22);
            this.tsmiStatistic.Text = "Statistic";
            this.tsmiStatistic.Click += new System.EventHandler(this.tsmiStatistic_Click);
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
            // UC_DbObjectsComplexTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tvDbObjects);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "UC_DbObjectsComplexTree";
            this.Size = new System.Drawing.Size(186, 276);
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
        private System.Windows.Forms.ToolStripMenuItem tsmiDelete;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewData;
        private System.Windows.Forms.ToolStripMenuItem tsmiTranslate;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewQuery;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewView;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewFunction;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewProcedure;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewTrigger;
        private System.Windows.Forms.ToolStripMenuItem tsmiMore;
        private System.Windows.Forms.ToolStripMenuItem tsmiClearData;
        private System.Windows.Forms.ToolStripMenuItem tsmiEmptyDatabase;
        private System.Windows.Forms.ToolStripMenuItem tsmiAlter;
        private System.Windows.Forms.ToolStripMenuItem tsmiNewTable;
        private System.Windows.Forms.ToolStripMenuItem tsmiDesign;
        private System.Windows.Forms.ToolStripMenuItem tsmiBackup;
        private System.Windows.Forms.ToolStripMenuItem tsmiDiagnose;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopy;
        private System.Windows.Forms.ToolStripMenuItem tsmiCompare;
        private System.Windows.Forms.ToolStripMenuItem tsmiCreateScript;
        private System.Windows.Forms.ToolStripMenuItem tsmiSelectScript;
        private System.Windows.Forms.ToolStripMenuItem tsmiInsertScript;
        private System.Windows.Forms.ToolStripMenuItem tsmiUpdateScript;
        private System.Windows.Forms.ToolStripMenuItem tsmiDeleteScript;
        private System.Windows.Forms.ToolStripMenuItem tsmiViewDependency;
        private System.Windows.Forms.ToolStripMenuItem tsmiCopyChildrenNames;
        private System.Windows.Forms.ToolStripMenuItem tsmiExecuteScript;
        private System.Windows.Forms.ToolStripMenuItem tsmiStatistic;
    }
}
