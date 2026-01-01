using System.Windows.Forms;

namespace DatabaseManager.Forms.Compare
{
    partial class frmDataCompareResult
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
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDataCompareResult));
            tlvDifferences = new BrightIdeasSoftware.TreeListView();
            colType = new BrightIdeasSoftware.OLVColumn();
            colSourceRecordCount = new BrightIdeasSoftware.OLVColumn();
            colTargetRecordCount = new BrightIdeasSoftware.OLVColumn();
            colDifferentCount = new BrightIdeasSoftware.OLVColumn();
            colOnlyInSourceCount = new BrightIdeasSoftware.OLVColumn();
            colOnlyInTargetCount = new BrightIdeasSoftware.OLVColumn();
            colIdenticalCount = new BrightIdeasSoftware.OLVColumn();
            splitContainer1 = new SplitContainer();
            tabControl1 = new TabControl();
            tabPageDifferent = new TabPage();
            btnExportDifferent = new Button();
            paginationDifferent = new DatabaseManager.Controls.UC_Pagination();
            dgvDifferent = new DataGridView();
            tabPageOnlyInsource = new TabPage();
            btnExportOnlyInSource = new Button();
            paginationOnlyInSource = new DatabaseManager.Controls.UC_Pagination();
            dgvOnlyInSource = new DataGridView();
            tabPageOnlyInTarget = new TabPage();
            btnExportOnlyInTarget = new Button();
            paginationOnlyInTarget = new DatabaseManager.Controls.UC_Pagination();
            dgvOnlyInTarget = new DataGridView();
            tabPageIdentical = new TabPage();
            btnExportIdentical = new Button();
            paginationIdentical = new DatabaseManager.Controls.UC_Pagination();
            dgvIdentical = new DataGridView();
            contextMenuStrip1 = new ContextMenuStrip(components);
            tbsiSelectAll = new ToolStripMenuItem();
            tsmiUnselectAll = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            tsbGenerateScript = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            tsbSynchronize = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            tsbCancel = new ToolStripButton();
            saveFileDialog1 = new SaveFileDialog();
            txtMessage = new TextBox();
            toolTip1 = new ToolTip(components);
            saveFileDialog2 = new SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)tlvDifferences).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPageDifferent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDifferent).BeginInit();
            tabPageOnlyInsource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOnlyInSource).BeginInit();
            tabPageOnlyInTarget.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOnlyInTarget).BeginInit();
            tabPageIdentical.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvIdentical).BeginInit();
            contextMenuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // tlvDifferences
            // 
            tlvDifferences.AllColumns.Add(colType);
            tlvDifferences.AllColumns.Add(colSourceRecordCount);
            tlvDifferences.AllColumns.Add(colTargetRecordCount);
            tlvDifferences.AllColumns.Add(colDifferentCount);
            tlvDifferences.AllColumns.Add(colOnlyInSourceCount);
            tlvDifferences.AllColumns.Add(colOnlyInTargetCount);
            tlvDifferences.AllColumns.Add(colIdenticalCount);
            tlvDifferences.BackColor = System.Drawing.SystemColors.Window;
            tlvDifferences.CellEditUseWholeCell = false;
            tlvDifferences.CheckBoxes = true;
            tlvDifferences.Columns.AddRange(new ColumnHeader[] { colType, colSourceRecordCount, colTargetRecordCount, colDifferentCount, colOnlyInSourceCount, colOnlyInTargetCount, colIdenticalCount });
            tlvDifferences.Dock = DockStyle.Fill;
            tlvDifferences.FullRowSelect = true;
            tlvDifferences.GridLines = true;
            tlvDifferences.Location = new System.Drawing.Point(0, 0);
            tlvDifferences.Margin = new Padding(4);
            tlvDifferences.MultiSelect = false;
            tlvDifferences.Name = "tlvDifferences";
            tlvDifferences.SelectColumnsOnRightClick = false;
            tlvDifferences.SelectColumnsOnRightClickBehaviour = BrightIdeasSoftware.ObjectListView.ColumnSelectBehaviour.None;
            tlvDifferences.SelectedBackColor = System.Drawing.SystemColors.ButtonFace;
            tlvDifferences.SelectedForeColor = System.Drawing.Color.Black;
            tlvDifferences.ShowFilterMenuOnRightClick = false;
            tlvDifferences.ShowGroups = false;
            tlvDifferences.ShowImagesOnSubItems = true;
            tlvDifferences.Size = new System.Drawing.Size(1018, 215);
            tlvDifferences.TabIndex = 2;
            tlvDifferences.UseCompatibleStateImageBehavior = false;
            tlvDifferences.View = View.Details;
            tlvDifferences.VirtualMode = true;
            tlvDifferences.FormatRow += tlvDifferences_FormatRow;
            tlvDifferences.ItemCheck += tlvDifferences_ItemCheck;
            tlvDifferences.SelectedIndexChanged += tlvDifferences_SelectedIndexChanged;
            tlvDifferences.MouseClick += tlvDifferences_MouseClick;
            // 
            // colType
            // 
            colType.AspectName = "Type";
            colType.Sortable = false;
            colType.Text = "Type";
            colType.Width = 250;
            // 
            // colSourceRecordCount
            // 
            colSourceRecordCount.AspectName = "SourceRecordCount";
            colSourceRecordCount.Sortable = false;
            colSourceRecordCount.Text = "Source Record Count";
            colSourceRecordCount.TextAlign = HorizontalAlignment.Center;
            colSourceRecordCount.Width = 140;
            // 
            // colTargetRecordCount
            // 
            colTargetRecordCount.AspectName = "TargetRecordCount";
            colTargetRecordCount.Sortable = false;
            colTargetRecordCount.Text = "Target Record Count";
            colTargetRecordCount.TextAlign = HorizontalAlignment.Center;
            colTargetRecordCount.Width = 140;
            // 
            // colDifferentCount
            // 
            colDifferentCount.AspectName = "DifferentCount";
            colDifferentCount.Sortable = false;
            colDifferentCount.Text = "Different Count";
            colDifferentCount.TextAlign = HorizontalAlignment.Center;
            colDifferentCount.Width = 100;
            // 
            // colOnlyInSourceCount
            // 
            colOnlyInSourceCount.AspectName = "OnlyInSourceCount";
            colOnlyInSourceCount.Text = "Only in Source Count";
            colOnlyInSourceCount.TextAlign = HorizontalAlignment.Center;
            colOnlyInSourceCount.Width = 140;
            // 
            // colOnlyInTargetCount
            // 
            colOnlyInTargetCount.AspectName = "OnlyInTargetCount";
            colOnlyInTargetCount.Text = "Only in Target Count";
            colOnlyInTargetCount.TextAlign = HorizontalAlignment.Center;
            colOnlyInTargetCount.Width = 140;
            // 
            // colIdenticalCount
            // 
            colIdenticalCount.AspectName = "IdenticalCount";
            colIdenticalCount.Text = "Identical Count";
            colIdenticalCount.TextAlign = HorizontalAlignment.Center;
            colIdenticalCount.Width = 100;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(0, 28);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tlvDifferences);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tabControl1);
            splitContainer1.Size = new System.Drawing.Size(1018, 545);
            splitContainer1.SplitterDistance = 215;
            splitContainer1.TabIndex = 1;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPageDifferent);
            tabControl1.Controls.Add(tabPageOnlyInsource);
            tabControl1.Controls.Add(tabPageOnlyInTarget);
            tabControl1.Controls.Add(tabPageIdentical);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(1018, 326);
            tabControl1.TabIndex = 0;
            // 
            // tabPageDifferent
            // 
            tabPageDifferent.Controls.Add(btnExportDifferent);
            tabPageDifferent.Controls.Add(paginationDifferent);
            tabPageDifferent.Controls.Add(dgvDifferent);
            tabPageDifferent.Location = new System.Drawing.Point(4, 26);
            tabPageDifferent.Name = "tabPageDifferent";
            tabPageDifferent.Padding = new Padding(3);
            tabPageDifferent.Size = new System.Drawing.Size(1010, 296);
            tabPageDifferent.TabIndex = 0;
            tabPageDifferent.Text = "Different Records";
            tabPageDifferent.UseVisualStyleBackColor = true;
            // 
            // btnExportDifferent
            // 
            btnExportDifferent.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportDifferent.FlatAppearance.BorderSize = 0;
            btnExportDifferent.FlatStyle = FlatStyle.Flat;
            btnExportDifferent.Location = new System.Drawing.Point(979, 268);
            btnExportDifferent.Margin = new Padding(4);
            btnExportDifferent.Name = "btnExportDifferent";
            btnExportDifferent.Size = new System.Drawing.Size(22, 22);
            btnExportDifferent.TabIndex = 20;
            toolTip1.SetToolTip(btnExportDifferent, "Export");
            btnExportDifferent.UseVisualStyleBackColor = true;
            btnExportDifferent.Click += btnExportDifferent_Click;
            // 
            // paginationDifferent
            // 
            paginationDifferent.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            paginationDifferent.BackColor = System.Drawing.Color.Transparent;
            paginationDifferent.Location = new System.Drawing.Point(3, 266);
            paginationDifferent.Margin = new Padding(4);
            paginationDifferent.Name = "paginationDifferent";
            paginationDifferent.PageCount = 0L;
            paginationDifferent.PageNumber = 1L;
            paginationDifferent.PageSize = 100;
            paginationDifferent.Size = new System.Drawing.Size(963, 26);
            paginationDifferent.TabIndex = 1;
            paginationDifferent.TotalCount = 0L;
            paginationDifferent.OnPageNumberChanged += paginationDifferent_OnPageNumberChanged;
            // 
            // dgvDifferent
            // 
            dgvDifferent.AllowUserToAddRows = false;
            dgvDifferent.AllowUserToDeleteRows = false;
            dgvDifferent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDifferent.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDifferent.Location = new System.Drawing.Point(3, 3);
            dgvDifferent.Name = "dgvDifferent";
            dgvDifferent.ReadOnly = true;
            dgvDifferent.Size = new System.Drawing.Size(1004, 260);
            dgvDifferent.TabIndex = 0;
            dgvDifferent.CellFormatting += dgvDifferent_CellFormatting;
            dgvDifferent.DataError += dgvDifferent_DataError;
            // 
            // tabPageOnlyInsource
            // 
            tabPageOnlyInsource.Controls.Add(btnExportOnlyInSource);
            tabPageOnlyInsource.Controls.Add(paginationOnlyInSource);
            tabPageOnlyInsource.Controls.Add(dgvOnlyInSource);
            tabPageOnlyInsource.Location = new System.Drawing.Point(4, 26);
            tabPageOnlyInsource.Name = "tabPageOnlyInsource";
            tabPageOnlyInsource.Padding = new Padding(3);
            tabPageOnlyInsource.Size = new System.Drawing.Size(1010, 296);
            tabPageOnlyInsource.TabIndex = 1;
            tabPageOnlyInsource.Text = "Only in Source";
            tabPageOnlyInsource.UseVisualStyleBackColor = true;
            // 
            // btnExportOnlyInSource
            // 
            btnExportOnlyInSource.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportOnlyInSource.FlatAppearance.BorderSize = 0;
            btnExportOnlyInSource.FlatStyle = FlatStyle.Flat;
            btnExportOnlyInSource.Location = new System.Drawing.Point(981, 268);
            btnExportOnlyInSource.Margin = new Padding(4);
            btnExportOnlyInSource.Name = "btnExportOnlyInSource";
            btnExportOnlyInSource.Size = new System.Drawing.Size(22, 22);
            btnExportOnlyInSource.TabIndex = 21;
            toolTip1.SetToolTip(btnExportOnlyInSource, "Export");
            btnExportOnlyInSource.UseVisualStyleBackColor = true;
            btnExportOnlyInSource.Click += btnExportOnlyInSource_Click;
            // 
            // paginationOnlyInSource
            // 
            paginationOnlyInSource.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            paginationOnlyInSource.BackColor = System.Drawing.Color.Transparent;
            paginationOnlyInSource.Location = new System.Drawing.Point(3, 266);
            paginationOnlyInSource.Margin = new Padding(4);
            paginationOnlyInSource.Name = "paginationOnlyInSource";
            paginationOnlyInSource.PageCount = 0L;
            paginationOnlyInSource.PageNumber = 1L;
            paginationOnlyInSource.PageSize = 100;
            paginationOnlyInSource.Size = new System.Drawing.Size(966, 26);
            paginationOnlyInSource.TabIndex = 7;
            paginationOnlyInSource.TotalCount = 0L;
            paginationOnlyInSource.OnPageNumberChanged += paginationOnlyInSource_OnPageNumberChanged;
            // 
            // dgvOnlyInSource
            // 
            dgvOnlyInSource.AllowUserToAddRows = false;
            dgvOnlyInSource.AllowUserToDeleteRows = false;
            dgvOnlyInSource.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvOnlyInSource.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvOnlyInSource.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOnlyInSource.Location = new System.Drawing.Point(3, 3);
            dgvOnlyInSource.Margin = new Padding(4);
            dgvOnlyInSource.Name = "dgvOnlyInSource";
            dgvOnlyInSource.ReadOnly = true;
            dgvOnlyInSource.Size = new System.Drawing.Size(1004, 260);
            dgvOnlyInSource.TabIndex = 6;
            dgvOnlyInSource.CellFormatting += dgvOnlyInSource_CellFormatting;
            dgvOnlyInSource.DataBindingComplete += dgvOnlyInSource_DataBindingComplete;
            dgvOnlyInSource.DataError += dgvOnlyInSource_DataError;
            // 
            // tabPageOnlyInTarget
            // 
            tabPageOnlyInTarget.Controls.Add(btnExportOnlyInTarget);
            tabPageOnlyInTarget.Controls.Add(paginationOnlyInTarget);
            tabPageOnlyInTarget.Controls.Add(dgvOnlyInTarget);
            tabPageOnlyInTarget.Location = new System.Drawing.Point(4, 26);
            tabPageOnlyInTarget.Name = "tabPageOnlyInTarget";
            tabPageOnlyInTarget.Padding = new Padding(3);
            tabPageOnlyInTarget.Size = new System.Drawing.Size(1010, 296);
            tabPageOnlyInTarget.TabIndex = 2;
            tabPageOnlyInTarget.Text = "Only in Target";
            tabPageOnlyInTarget.UseVisualStyleBackColor = true;
            // 
            // btnExportOnlyInTarget
            // 
            btnExportOnlyInTarget.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportOnlyInTarget.FlatAppearance.BorderSize = 0;
            btnExportOnlyInTarget.FlatStyle = FlatStyle.Flat;
            btnExportOnlyInTarget.Location = new System.Drawing.Point(977, 269);
            btnExportOnlyInTarget.Margin = new Padding(4);
            btnExportOnlyInTarget.Name = "btnExportOnlyInTarget";
            btnExportOnlyInTarget.Size = new System.Drawing.Size(22, 22);
            btnExportOnlyInTarget.TabIndex = 22;
            toolTip1.SetToolTip(btnExportOnlyInTarget, "Export");
            btnExportOnlyInTarget.UseVisualStyleBackColor = true;
            btnExportOnlyInTarget.Click += btnExportOnlyInTarget_Click;
            // 
            // paginationOnlyInTarget
            // 
            paginationOnlyInTarget.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            paginationOnlyInTarget.BackColor = System.Drawing.Color.Transparent;
            paginationOnlyInTarget.Location = new System.Drawing.Point(3, 265);
            paginationOnlyInTarget.Margin = new Padding(4);
            paginationOnlyInTarget.Name = "paginationOnlyInTarget";
            paginationOnlyInTarget.PageCount = 0L;
            paginationOnlyInTarget.PageNumber = 1L;
            paginationOnlyInTarget.PageSize = 100;
            paginationOnlyInTarget.Size = new System.Drawing.Size(965, 26);
            paginationOnlyInTarget.TabIndex = 9;
            paginationOnlyInTarget.TotalCount = 0L;
            paginationOnlyInTarget.OnPageNumberChanged += paginationOnlyInTarget_OnPageNumberChanged;
            // 
            // dgvOnlyInTarget
            // 
            dgvOnlyInTarget.AllowUserToAddRows = false;
            dgvOnlyInTarget.AllowUserToDeleteRows = false;
            dgvOnlyInTarget.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvOnlyInTarget.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvOnlyInTarget.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOnlyInTarget.Location = new System.Drawing.Point(3, 3);
            dgvOnlyInTarget.Margin = new Padding(4);
            dgvOnlyInTarget.Name = "dgvOnlyInTarget";
            dgvOnlyInTarget.ReadOnly = true;
            dgvOnlyInTarget.Size = new System.Drawing.Size(1004, 260);
            dgvOnlyInTarget.TabIndex = 8;
            dgvOnlyInTarget.CellFormatting += dgvOnlyInTarget_CellFormatting;
            dgvOnlyInTarget.DataBindingComplete += dgvOnlyInTarget_DataBindingComplete;
            dgvOnlyInTarget.DataError += dgvOnlyInTarget_DataError;
            // 
            // tabPageIdentical
            // 
            tabPageIdentical.Controls.Add(btnExportIdentical);
            tabPageIdentical.Controls.Add(paginationIdentical);
            tabPageIdentical.Controls.Add(dgvIdentical);
            tabPageIdentical.Location = new System.Drawing.Point(4, 26);
            tabPageIdentical.Name = "tabPageIdentical";
            tabPageIdentical.Padding = new Padding(3);
            tabPageIdentical.Size = new System.Drawing.Size(1010, 296);
            tabPageIdentical.TabIndex = 3;
            tabPageIdentical.Text = "Identical Records";
            tabPageIdentical.UseVisualStyleBackColor = true;
            // 
            // btnExportIdentical
            // 
            btnExportIdentical.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnExportIdentical.FlatAppearance.BorderSize = 0;
            btnExportIdentical.FlatStyle = FlatStyle.Flat;
            btnExportIdentical.Location = new System.Drawing.Point(979, 268);
            btnExportIdentical.Margin = new Padding(4);
            btnExportIdentical.Name = "btnExportIdentical";
            btnExportIdentical.Size = new System.Drawing.Size(22, 22);
            btnExportIdentical.TabIndex = 23;
            toolTip1.SetToolTip(btnExportIdentical, "Export");
            btnExportIdentical.UseVisualStyleBackColor = true;
            btnExportIdentical.Click += btnExportIdentical_Click;
            // 
            // paginationIdentical
            // 
            paginationIdentical.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            paginationIdentical.BackColor = System.Drawing.Color.Transparent;
            paginationIdentical.Location = new System.Drawing.Point(3, 266);
            paginationIdentical.Margin = new Padding(4);
            paginationIdentical.Name = "paginationIdentical";
            paginationIdentical.PageCount = 0L;
            paginationIdentical.PageNumber = 1L;
            paginationIdentical.PageSize = 100;
            paginationIdentical.Size = new System.Drawing.Size(970, 26);
            paginationIdentical.TabIndex = 11;
            paginationIdentical.TotalCount = 0L;
            paginationIdentical.OnPageNumberChanged += paginationIdentical_OnPageNumberChanged;
            // 
            // dgvIdentical
            // 
            dgvIdentical.AllowUserToAddRows = false;
            dgvIdentical.AllowUserToDeleteRows = false;
            dgvIdentical.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dgvIdentical.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvIdentical.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvIdentical.Location = new System.Drawing.Point(3, 3);
            dgvIdentical.Margin = new Padding(4);
            dgvIdentical.Name = "dgvIdentical";
            dgvIdentical.ReadOnly = true;
            dgvIdentical.Size = new System.Drawing.Size(1004, 260);
            dgvIdentical.TabIndex = 10;
            dgvIdentical.CellFormatting += dgvIdentical_CellFormatting;
            dgvIdentical.DataBindingComplete += dgvIdentical_DataBindingComplete;
            dgvIdentical.DataError += dgvIdentical_DataError;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { tbsiSelectAll, tsmiUnselectAll });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(143, 48);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // tbsiSelectAll
            // 
            tbsiSelectAll.Name = "tbsiSelectAll";
            tbsiSelectAll.Size = new System.Drawing.Size(142, 22);
            tbsiSelectAll.Text = "Check all";
            tbsiSelectAll.Click += tbsiSelectAll_Click;
            // 
            // tsmiUnselectAll
            // 
            tsmiUnselectAll.Name = "tsmiUnselectAll";
            tsmiUnselectAll.Size = new System.Drawing.Size(142, 22);
            tsmiUnselectAll.Text = "Uncheck all";
            tsmiUnselectAll.Click += tsmiUnselectAll_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new ToolStripItem[] { tsbGenerateScript, toolStripSeparator1, tsbSynchronize, toolStripSeparator2, tsbCancel });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1018, 33);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // tsbGenerateScript
            // 
            tsbGenerateScript.AutoSize = false;
            tsbGenerateScript.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbGenerateScript.Image = Resources.DbScripts;
            tsbGenerateScript.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbGenerateScript.Name = "tsbGenerateScript";
            tsbGenerateScript.Size = new System.Drawing.Size(30, 30);
            tsbGenerateScript.Text = "Generate scripts";
            tsbGenerateScript.Click += tsbGenerateScript_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 33);
            // 
            // tsbSynchronize
            // 
            tsbSynchronize.AutoSize = false;
            tsbSynchronize.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbSynchronize.Image = Resources.Run;
            tsbSynchronize.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbSynchronize.Name = "tsbSynchronize";
            tsbSynchronize.Size = new System.Drawing.Size(30, 30);
            tsbSynchronize.Text = "Synchronize";
            tsbSynchronize.Click += tsbSynchronize_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 33);
            // 
            // tsbCancel
            // 
            tsbCancel.AutoSize = false;
            tsbCancel.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbCancel.Enabled = false;
            tsbCancel.Image = (System.Drawing.Image)resources.GetObject("tsbCancel.Image");
            tsbCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbCancel.Name = "tsbCancel";
            tsbCancel.Size = new System.Drawing.Size(30, 30);
            tsbCancel.Text = "Cancel";
            tsbCancel.Click += tsbCancel_Click;
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.Filter = "SQL file(*.sql)|All file(*.*)";
            // 
            // txtMessage
            // 
            txtMessage.BackColor = System.Drawing.SystemColors.Control;
            txtMessage.Dock = DockStyle.Bottom;
            txtMessage.Location = new System.Drawing.Point(0, 573);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new System.Drawing.Size(1018, 23);
            txtMessage.TabIndex = 3;
            // 
            // frmDataCompareResult
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1018, 596);
            Controls.Add(txtMessage);
            Controls.Add(toolStrip1);
            Controls.Add(splitContainer1);
            Name = "frmDataCompareResult";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Data Compare Result";
            Load += frmDataCompareResult_Load;
            ((System.ComponentModel.ISupportInitialize)tlvDifferences).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPageDifferent.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDifferent).EndInit();
            tabPageOnlyInsource.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOnlyInSource).EndInit();
            tabPageOnlyInTarget.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOnlyInTarget).EndInit();
            tabPageIdentical.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvIdentical).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageDifferent;
        private System.Windows.Forms.TabPage tabPageOnlyInsource;
        private System.Windows.Forms.TabPage tabPageOnlyInTarget;
        private System.Windows.Forms.TabPage tabPageIdentical;
        private System.Windows.Forms.DataGridView dgvDifferent;
        private Controls.UC_Pagination paginationDifferent;
        private System.Windows.Forms.DataGridView dgvOnlyInSource;
        private Controls.UC_Pagination paginationOnlyInSource;
        private Controls.UC_Pagination paginationOnlyInTarget;
        private System.Windows.Forms.DataGridView dgvOnlyInTarget;
        private Controls.UC_Pagination paginationIdentical;
        private System.Windows.Forms.DataGridView dgvIdentical;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem tbsiSelectAll;
        private System.Windows.Forms.ToolStripMenuItem tsmiUnselectAll;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbGenerateScript;
        private System.Windows.Forms.ToolStripButton tsbSynchronize;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.ToolTip toolTip1;
        private BrightIdeasSoftware.TreeListView tlvDifferences;
        private BrightIdeasSoftware.OLVColumn colType;
        private BrightIdeasSoftware.OLVColumn colSourceRecordCount;
        private BrightIdeasSoftware.OLVColumn colTargetRecordCount;
        private BrightIdeasSoftware.OLVColumn colDifferentCount;
        private BrightIdeasSoftware.OLVColumn colOnlyInSourceCount;
        private BrightIdeasSoftware.OLVColumn colOnlyInTargetCount;
        private BrightIdeasSoftware.OLVColumn colIdenticalCount;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton tsbCancel;
        private Button btnExportDifferent;
        private Button btnExportOnlyInSource;
        private Button btnExportOnlyInTarget;
        private Button btnExportIdentical;
        private SaveFileDialog saveFileDialog2;
    }
}