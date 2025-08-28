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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_DbObjectsComplexTree));
            tvDbObjects = new System.Windows.Forms.TreeView();
            imageList1 = new System.Windows.Forms.ImageList(components);
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            tsmiNewQuery = new System.Windows.Forms.ToolStripMenuItem();
            tsmiNewTable = new System.Windows.Forms.ToolStripMenuItem();
            tsmiNewView = new System.Windows.Forms.ToolStripMenuItem();
            tsmiNewFunction = new System.Windows.Forms.ToolStripMenuItem();
            tsmiNewProcedure = new System.Windows.Forms.ToolStripMenuItem();
            tsmiNewTrigger = new System.Windows.Forms.ToolStripMenuItem();
            tsmiAlter = new System.Windows.Forms.ToolStripMenuItem();
            tsmiDesign = new System.Windows.Forms.ToolStripMenuItem();
            tsmiRefresh = new System.Windows.Forms.ToolStripMenuItem();
            tsmiViewData = new System.Windows.Forms.ToolStripMenuItem();
            tsmiEditData = new System.Windows.Forms.ToolStripMenuItem();
            tsmiExportData = new System.Windows.Forms.ToolStripMenuItem();
            tsmiImportData = new System.Windows.Forms.ToolStripMenuItem();
            tsmiConvert = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCompare = new System.Windows.Forms.ToolStripMenuItem();
            tsmiGenerateScripts = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCreateScript = new System.Windows.Forms.ToolStripMenuItem();
            tsmiSelectScript = new System.Windows.Forms.ToolStripMenuItem();
            tsmiInsertScript = new System.Windows.Forms.ToolStripMenuItem();
            tsmiUpdateScript = new System.Windows.Forms.ToolStripMenuItem();
            tsmiDeleteScript = new System.Windows.Forms.ToolStripMenuItem();
            tsmiExecuteScript = new System.Windows.Forms.ToolStripMenuItem();
            tsmiTranslate = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCopy = new System.Windows.Forms.ToolStripMenuItem();
            tsmiDelete = new System.Windows.Forms.ToolStripMenuItem();
            tsmiViewDependency = new System.Windows.Forms.ToolStripMenuItem();
            tsmiCopyChildrenNames = new System.Windows.Forms.ToolStripMenuItem();
            tsmiMore = new System.Windows.Forms.ToolStripMenuItem();
            tsmiBackup = new System.Windows.Forms.ToolStripMenuItem();
            tsmiDiagnose = new System.Windows.Forms.ToolStripMenuItem();
            tsmiStatistic = new System.Windows.Forms.ToolStripMenuItem();
            tsmiClearData = new System.Windows.Forms.ToolStripMenuItem();
            tsmiEmptyDatabase = new System.Windows.Forms.ToolStripMenuItem();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tvDbObjects
            // 
            tvDbObjects.Dock = System.Windows.Forms.DockStyle.Fill;
            tvDbObjects.Font = new System.Drawing.Font("Times New Roman", 10.5F);
            tvDbObjects.HideSelection = false;
            tvDbObjects.ImageIndex = 0;
            tvDbObjects.ImageList = imageList1;
            tvDbObjects.Location = new System.Drawing.Point(0, 0);
            tvDbObjects.Margin = new System.Windows.Forms.Padding(4);
            tvDbObjects.Name = "tvDbObjects";
            tvDbObjects.SelectedImageIndex = 0;
            tvDbObjects.ShowLines = false;
            tvDbObjects.Size = new System.Drawing.Size(186, 276);
            tvDbObjects.TabIndex = 20;
            tvDbObjects.BeforeExpand += tvDbObjects_BeforeExpand;
            tvDbObjects.AfterExpand += tvDbObjects_AfterExpand;
            tvDbObjects.ItemDrag += tvDbObjects_ItemDrag;
            tvDbObjects.NodeMouseClick += tvDbObjects_NodeMouseClick;
            tvDbObjects.KeyDown += tvDbObjects_KeyDown;
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageList1.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList1.ImageStream");
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "tree_Fake.png");
            imageList1.Images.SetKeyName(1, "tree_Database.png");
            imageList1.Images.SetKeyName(2, "tree_Folder.png");
            imageList1.Images.SetKeyName(3, "tree_TableForeignKey.png");
            imageList1.Images.SetKeyName(4, "tree_Procedure.png");
            imageList1.Images.SetKeyName(5, "tree_View.png");
            imageList1.Images.SetKeyName(6, "tree_TableIndex.png");
            imageList1.Images.SetKeyName(7, "tree_TablePrimaryKey.png");
            imageList1.Images.SetKeyName(8, "tree_Table.png");
            imageList1.Images.SetKeyName(9, "tree_TableConstraint.png");
            imageList1.Images.SetKeyName(10, "tree_TableTrigger.png");
            imageList1.Images.SetKeyName(11, "Loading.gif");
            imageList1.Images.SetKeyName(12, "tree_Function.png");
            imageList1.Images.SetKeyName(13, "tree_TableColumn.png");
            imageList1.Images.SetKeyName(14, "tree_UserDefinedType.png");
            imageList1.Images.SetKeyName(15, "tree_Sequence.png");
            imageList1.Images.SetKeyName(16, "tree_Function_Trigger.png");
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiNewQuery, tsmiNewTable, tsmiNewView, tsmiNewFunction, tsmiNewProcedure, tsmiNewTrigger, tsmiAlter, tsmiDesign, tsmiRefresh, tsmiViewData, tsmiEditData, tsmiExportData, tsmiImportData, tsmiConvert, tsmiCompare, tsmiGenerateScripts, tsmiTranslate, tsmiCopy, tsmiDelete, tsmiViewDependency, tsmiCopyChildrenNames, tsmiMore });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(204, 510);
            // 
            // tsmiNewQuery
            // 
            tsmiNewQuery.Name = "tsmiNewQuery";
            tsmiNewQuery.Size = new System.Drawing.Size(203, 22);
            tsmiNewQuery.Text = "New Query";
            tsmiNewQuery.Click += tsmiNewQuery_Click;
            // 
            // tsmiNewTable
            // 
            tsmiNewTable.Name = "tsmiNewTable";
            tsmiNewTable.Size = new System.Drawing.Size(203, 22);
            tsmiNewTable.Text = "New Table";
            tsmiNewTable.Click += tsmiNewTable_Click;
            // 
            // tsmiNewView
            // 
            tsmiNewView.Name = "tsmiNewView";
            tsmiNewView.Size = new System.Drawing.Size(203, 22);
            tsmiNewView.Text = "New View";
            tsmiNewView.Click += tsmiNewView_Click;
            // 
            // tsmiNewFunction
            // 
            tsmiNewFunction.Name = "tsmiNewFunction";
            tsmiNewFunction.Size = new System.Drawing.Size(203, 22);
            tsmiNewFunction.Text = "New Function";
            tsmiNewFunction.Click += tsmiNewFunction_Click;
            // 
            // tsmiNewProcedure
            // 
            tsmiNewProcedure.Name = "tsmiNewProcedure";
            tsmiNewProcedure.Size = new System.Drawing.Size(203, 22);
            tsmiNewProcedure.Text = "New Procedure";
            tsmiNewProcedure.Click += tsmiNewProcedure_Click;
            // 
            // tsmiNewTrigger
            // 
            tsmiNewTrigger.Name = "tsmiNewTrigger";
            tsmiNewTrigger.Size = new System.Drawing.Size(203, 22);
            tsmiNewTrigger.Text = "New Trigger";
            tsmiNewTrigger.Click += tsmiNewTrigger_Click;
            // 
            // tsmiAlter
            // 
            tsmiAlter.Name = "tsmiAlter";
            tsmiAlter.Size = new System.Drawing.Size(203, 22);
            tsmiAlter.Text = "Modify";
            tsmiAlter.Click += tsmiAlter_Click;
            // 
            // tsmiDesign
            // 
            tsmiDesign.Name = "tsmiDesign";
            tsmiDesign.Size = new System.Drawing.Size(203, 22);
            tsmiDesign.Text = "Design";
            tsmiDesign.Click += tsmiDesign_Click;
            // 
            // tsmiRefresh
            // 
            tsmiRefresh.Name = "tsmiRefresh";
            tsmiRefresh.Size = new System.Drawing.Size(203, 22);
            tsmiRefresh.Text = "Refresh";
            tsmiRefresh.Click += tsmiRefresh_Click;
            // 
            // tsmiViewData
            // 
            tsmiViewData.Name = "tsmiViewData";
            tsmiViewData.Size = new System.Drawing.Size(203, 22);
            tsmiViewData.Text = "View Data";
            tsmiViewData.Click += tsmiViewData_Click;
            // 
            // tsmiEditData
            // 
            tsmiEditData.Name = "tsmiEditData";
            tsmiEditData.Size = new System.Drawing.Size(203, 22);
            tsmiEditData.Text = "Edit Data";
            tsmiEditData.Click += tsmiEditData_Click;
            // 
            // tsmiExportData
            // 
            tsmiExportData.Name = "tsmiExportData";
            tsmiExportData.Size = new System.Drawing.Size(203, 22);
            tsmiExportData.Text = "Export Data";
            tsmiExportData.Click += tsmiExportData_Click;
            // 
            // tsmiImportData
            // 
            tsmiImportData.Name = "tsmiImportData";
            tsmiImportData.Size = new System.Drawing.Size(203, 22);
            tsmiImportData.Text = "Import Data";
            tsmiImportData.Click += tsmiImportData_Click;
            // 
            // tsmiConvert
            // 
            tsmiConvert.Name = "tsmiConvert";
            tsmiConvert.Size = new System.Drawing.Size(203, 22);
            tsmiConvert.Text = "Convert";
            tsmiConvert.Click += tsmiConvert_Click;
            // 
            // tsmiCompare
            // 
            tsmiCompare.Name = "tsmiCompare";
            tsmiCompare.Size = new System.Drawing.Size(203, 22);
            tsmiCompare.Text = "Compare";
            tsmiCompare.Click += tsmiCompare_Click;
            // 
            // tsmiGenerateScripts
            // 
            tsmiGenerateScripts.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiCreateScript, tsmiSelectScript, tsmiInsertScript, tsmiUpdateScript, tsmiDeleteScript, tsmiExecuteScript });
            tsmiGenerateScripts.Name = "tsmiGenerateScripts";
            tsmiGenerateScripts.Size = new System.Drawing.Size(203, 22);
            tsmiGenerateScripts.Text = "Scripts";
            // 
            // tsmiCreateScript
            // 
            tsmiCreateScript.Name = "tsmiCreateScript";
            tsmiCreateScript.Size = new System.Drawing.Size(157, 22);
            tsmiCreateScript.Text = "Create Script";
            tsmiCreateScript.Click += tsmiCreateScript_Click;
            // 
            // tsmiSelectScript
            // 
            tsmiSelectScript.Name = "tsmiSelectScript";
            tsmiSelectScript.Size = new System.Drawing.Size(157, 22);
            tsmiSelectScript.Text = "Select Script";
            tsmiSelectScript.Click += tsmiSelectScript_Click;
            // 
            // tsmiInsertScript
            // 
            tsmiInsertScript.Name = "tsmiInsertScript";
            tsmiInsertScript.Size = new System.Drawing.Size(157, 22);
            tsmiInsertScript.Text = "Insert Script";
            tsmiInsertScript.Click += tsmiInsertScript_Click;
            // 
            // tsmiUpdateScript
            // 
            tsmiUpdateScript.Name = "tsmiUpdateScript";
            tsmiUpdateScript.Size = new System.Drawing.Size(157, 22);
            tsmiUpdateScript.Text = "Update Script";
            tsmiUpdateScript.Click += tsmiUpdateScript_Click;
            // 
            // tsmiDeleteScript
            // 
            tsmiDeleteScript.Name = "tsmiDeleteScript";
            tsmiDeleteScript.Size = new System.Drawing.Size(157, 22);
            tsmiDeleteScript.Text = "Delete Script";
            tsmiDeleteScript.Click += tsmiDeleteScript_Click;
            // 
            // tsmiExecuteScript
            // 
            tsmiExecuteScript.Name = "tsmiExecuteScript";
            tsmiExecuteScript.Size = new System.Drawing.Size(157, 22);
            tsmiExecuteScript.Text = "Execute Script";
            tsmiExecuteScript.Click += tsmiExecuteScript_Click;
            // 
            // tsmiTranslate
            // 
            tsmiTranslate.Name = "tsmiTranslate";
            tsmiTranslate.Size = new System.Drawing.Size(203, 22);
            tsmiTranslate.Text = "Translate to";
            tsmiTranslate.MouseEnter += tsmiTranslate_MouseEnter;
            // 
            // tsmiCopy
            // 
            tsmiCopy.Name = "tsmiCopy";
            tsmiCopy.Size = new System.Drawing.Size(203, 22);
            tsmiCopy.Text = "Copy";
            tsmiCopy.Click += tsmiCopy_Click;
            // 
            // tsmiDelete
            // 
            tsmiDelete.Name = "tsmiDelete";
            tsmiDelete.Size = new System.Drawing.Size(203, 22);
            tsmiDelete.Text = "Delete";
            tsmiDelete.Click += tsmiDelete_Click;
            // 
            // tsmiViewDependency
            // 
            tsmiViewDependency.Name = "tsmiViewDependency";
            tsmiViewDependency.Size = new System.Drawing.Size(203, 22);
            tsmiViewDependency.Text = "View Dependencies";
            tsmiViewDependency.Click += tsmiViewDependency_Click;
            // 
            // tsmiCopyChildrenNames
            // 
            tsmiCopyChildrenNames.Name = "tsmiCopyChildrenNames";
            tsmiCopyChildrenNames.Size = new System.Drawing.Size(203, 22);
            tsmiCopyChildrenNames.Text = "Copy Children Names";
            tsmiCopyChildrenNames.Click += tsmiCopyChildrenNames_Click;
            // 
            // tsmiMore
            // 
            tsmiMore.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmiBackup, tsmiDiagnose, tsmiStatistic, tsmiClearData, tsmiEmptyDatabase });
            tsmiMore.Name = "tsmiMore";
            tsmiMore.Size = new System.Drawing.Size(203, 22);
            tsmiMore.Text = "More";
            // 
            // tsmiBackup
            // 
            tsmiBackup.Name = "tsmiBackup";
            tsmiBackup.Size = new System.Drawing.Size(137, 22);
            tsmiBackup.Text = "Backup";
            tsmiBackup.Click += tsmiBackup_Click;
            // 
            // tsmiDiagnose
            // 
            tsmiDiagnose.Name = "tsmiDiagnose";
            tsmiDiagnose.Size = new System.Drawing.Size(137, 22);
            tsmiDiagnose.Text = "Diagnose";
            tsmiDiagnose.Click += tsmiDiagnose_Click;
            // 
            // tsmiStatistic
            // 
            tsmiStatistic.Name = "tsmiStatistic";
            tsmiStatistic.Size = new System.Drawing.Size(137, 22);
            tsmiStatistic.Text = "Statistic";
            tsmiStatistic.Click += tsmiStatistic_Click;
            // 
            // tsmiClearData
            // 
            tsmiClearData.Name = "tsmiClearData";
            tsmiClearData.Size = new System.Drawing.Size(137, 22);
            tsmiClearData.Text = "Clear Data";
            tsmiClearData.Click += tsmiClearData_Click;
            // 
            // tsmiEmptyDatabase
            // 
            tsmiEmptyDatabase.Name = "tsmiEmptyDatabase";
            tsmiEmptyDatabase.Size = new System.Drawing.Size(137, 22);
            tsmiEmptyDatabase.Text = "Empty";
            tsmiEmptyDatabase.Click += tsmiEmptyDatabase_Click;
            // 
            // UC_DbObjectsComplexTree
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tvDbObjects);
            Margin = new System.Windows.Forms.Padding(4);
            Name = "UC_DbObjectsComplexTree";
            Size = new System.Drawing.Size(186, 276);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem tsmiEditData;
        private System.Windows.Forms.ToolStripMenuItem tsmiExportData;
        private System.Windows.Forms.ToolStripMenuItem tsmiImportData;
    }
}
