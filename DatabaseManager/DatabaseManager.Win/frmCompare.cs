using BrightIdeasSoftware;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BrightIdeasSoftware.TreeListView;

namespace DatabaseManager
{
    public partial class frmCompare : Form, IObserver<FeedbackInfo>
    {
        private DatabaseType sourceDatabaseType;
        private ConnectionInfo sourceDbConnectionInfo;
        private ConnectionInfo targetDbConnectionInfo;
        private bool useSourceConnector = true;
        private bool isRichTextBoxScrolling = false;
        private List<DbDifference> differences;
        private DbInterpreter sourceInterpreter;
        private DbInterpreter targetInterpreter;
        private DbScriptGenerator sourceScriptGenerator;
        private DbScriptGenerator targetScriptGenerator;
        private SchemaInfo sourceSchemaInfo;
        private SchemaInfo targetSchemaInfo;

        public frmCompare()
        {
            InitializeComponent();
        }

        public frmCompare(DatabaseType sourceDatabaseType, ConnectionInfo sourceConnectionInfo)
        {
            InitializeComponent();

            this.sourceDatabaseType = sourceDatabaseType;
            this.sourceDbConnectionInfo = sourceConnectionInfo;
            this.useSourceConnector = false;
        }

        private void frmCompare_Load(object sender, EventArgs e)
        {
            this.InitControls();

            if (!this.useSourceConnector)
            {
                this.targetDbProfile.DatabaseType = sourceDatabaseType;
                this.targetDbProfile.EnableDatabaseType = false;
            }
        }

        private void InitControls()
        {
            if (!this.useSourceConnector)
            {
                int increaseHeight = this.sourceDbProfile.Height;
                this.sourceDbProfile.Visible = false;
                this.btnCompare.Height = this.targetDbProfile.ClientHeight;
                this.targetDbProfile.Top -= increaseHeight;
                this.splitContainer1.Top -= increaseHeight;
                this.splitContainer1.Height += increaseHeight;
            }

            this.colType.ImageGetter = delegate (object x)
            {
                DbDifference difference = x as DbDifference;

                if (difference.DatabaseObjectType == DatabaseObjectType.None)
                {
                    return "tree_Folder.png";
                }
                else
                {
                    return $"tree_{difference.DatabaseObjectType}.png";
                }
            };

            TreeRenderer treeColumnRenderer = this.tlvDifferences.TreeColumnRenderer;

            treeColumnRenderer.IsShowGlyphs = true;
            treeColumnRenderer.UseTriangles = true;

            TreeRenderer renderer = this.tlvDifferences.TreeColumnRenderer;
            renderer.LinePen = new Pen(Color.LightGray, 0.5f);
            renderer.LinePen.DashStyle = DashStyle.Dot;

            FlagRenderer differenceTypeRenderer = new FlagRenderer();

            differenceTypeRenderer.ImageList = this.imageList2;
            differenceTypeRenderer.Add(DbDifferenceType.Added, "Add.png");
            differenceTypeRenderer.Add(DbDifferenceType.Modified, "Edit.png");
            differenceTypeRenderer.Add(DbDifferenceType.Deleted, "Remove.png");

            this.colChangeType.Renderer = differenceTypeRenderer;

            this.colChangeType.ClusteringStrategy = new FlagClusteringStrategy(typeof(DbDifferenceType));

            this.tlvDifferences.Refresh();
        }

        private async void btnCompare_Click(object sender, EventArgs e)
        {
            frmDbObjectTypeSelector selector = new frmDbObjectTypeSelector() { DatabaseType = this.sourceDbProfile.DatabaseType };

            if (selector.ShowDialog() == DialogResult.OK)
            {
                this.tlvDifferences.Items.Clear();
                this.txtSource.Clear();
                this.txtTarget.Clear();

                await Task.Run(() => this.Compare(selector.DatabaseObjectType));
            }
        }

        private async void Compare(DatabaseObjectType databaseObjectType)
        {
            DatabaseType dbType;

            if (this.useSourceConnector)
            {
                dbType = this.sourceDbProfile.DatabaseType;

                if (!this.sourceDbProfile.IsDbTypeSelected())
                {
                    MessageBox.Show("Please select a source database type.");
                    return;
                }

                if (!this.sourceDbProfile.IsProfileSelected())
                {
                    MessageBox.Show("Please select a source database profile.");
                    return;
                }

                if (!this.sourceDbConnectionInfo.IntegratedSecurity && string.IsNullOrEmpty(this.sourceDbConnectionInfo.Password))
                {
                    MessageBox.Show("Please specify password for the source database.");
                    this.sourceDbProfile.ConfigConnection(true);
                    return;
                }
            }
            else
            {
                dbType = this.sourceDatabaseType;
            }

            if (this.sourceDbConnectionInfo == null)
            {
                MessageBox.Show("Source connection is null.");
                return;
            }

            if (this.targetDbConnectionInfo == null)
            {
                MessageBox.Show("Target connection info is null.");
                return;
            }

            if (dbType != this.targetDbProfile.DatabaseType)
            {
                MessageBox.Show("Target database type must be same as source database type.");
                return;
            }

            if (this.sourceDbConnectionInfo.Server == this.targetDbConnectionInfo.Server && this.sourceDbConnectionInfo.Database == this.targetDbConnectionInfo.Database)
            {
                MessageBox.Show("Source database cannot be equal to the target database.");
                return;
            }

            this.btnCompare.Text = "...";
            this.btnCompare.Enabled = false;

            try
            {
                DbInterpreterOption sourceOption = new DbInterpreterOption();
                DbInterpreterOption targetOption = new DbInterpreterOption();
                SchemaInfoFilter sourceFilter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType };
                SchemaInfoFilter targetFilter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType };

                if (databaseObjectType.HasFlag(DatabaseObjectType.Table))
                {
                    sourceOption.GetTableAllObjects = true;
                    targetOption.GetTableAllObjects = true;
                }

                this.sourceInterpreter = DbInterpreterHelper.GetDbInterpreter(dbType, this.sourceDbConnectionInfo, sourceOption);
                this.targetInterpreter = DbInterpreterHelper.GetDbInterpreter(this.targetDbProfile.DatabaseType, this.targetDbConnectionInfo, targetOption);
                this.sourceScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.sourceInterpreter);
                this.targetScriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.targetInterpreter);

                this.sourceInterpreter.Subscribe(this);
                this.targetInterpreter.Subscribe(this);

                this.sourceSchemaInfo = await this.sourceInterpreter.GetSchemaInfoAsync(sourceFilter);
                this.targetSchemaInfo = await this.targetInterpreter.GetSchemaInfoAsync(targetFilter);

                DbCompare dbCompare = new DbCompare(sourceSchemaInfo, targetSchemaInfo);

                this.Feedback("Begin to compare...");

                this.differences = dbCompare.Compare();

                this.Feedback("End compare.");

                this.LoadData();
            }
            catch (Exception ex)
            {
                string message = ExceptionHelper.GetExceptionDetails(ex);

                LogHelper.LogError(message);

                MessageBox.Show("Error:" + message);
            }
            finally
            {
                this.btnCompare.Text = "Compare";
                this.btnCompare.Enabled = true;
            }
        }

        private void LoadData()
        {
            this.DecorateData(this.differences);

            this.tlvDifferences.CanExpandGetter = delegate (object obj)
            {
                DbDifference difference = obj as DbDifference;
                return this.CanExpand(difference);
            };

            this.tlvDifferences.ChildrenGetter = delegate (object obj)
            {
                DbDifference difference = obj as DbDifference;
                return this.GetChildren(difference);
            };

            var roots = this.differences.Where(item => item.DatabaseObjectType == DatabaseObjectType.None);

            this.tlvDifferences.Roots = roots;
        }

        private IEnumerable<DbDifference> GetChildren(DbDifference difference)
        {
            if (this.differences == null)
            {
                return Enumerable.Empty<DbDifference>();
            }

            var children = this.differences.Where(item => item.ParentType == difference.Type
            && ((item.ParentName == null || (item.ParentName == difference.Source?.Name || item.ParentName == difference.Target?.Name)) ||
                (item.ParentName == null || (item.ParentName == difference.Parent?.Source?.Name || item.ParentName == difference?.Parent.Target?.Name))
            ));

            if (difference.DatabaseObjectType == DatabaseObjectType.Table)
            {
                return this.GetTableChildrenFolders(difference.Source as Table, difference.Target as Table, difference);
            }
            else if (difference.Type == DbObjectTreeFolderType.Columns.ToString())
            {
                return difference.Parent.SubDifferences.Where(item => item.DatabaseObjectType == DatabaseObjectType.TableColumn);
            }
            else if (difference.Type == "Primary Keys")
            {
                return difference.Parent.SubDifferences.Where(item => item.DatabaseObjectType == DatabaseObjectType.TablePrimaryKey);
            }
            else if (difference.Type == "Foreign Keys")
            {
                return difference.Parent.SubDifferences.Where(item => item.DatabaseObjectType == DatabaseObjectType.TableForeignKey);
            }
            else if (difference.Type == DbObjectTreeFolderType.Indexes.ToString())
            {
                return difference.Parent.SubDifferences.Where(item => item.DatabaseObjectType == DatabaseObjectType.TableIndex);
            }
            else if (difference.Type == DbObjectTreeFolderType.Constraints.ToString())
            {
                return difference.Parent.SubDifferences.Where(item => item.DatabaseObjectType == DatabaseObjectType.TableConstraint);
            }
            else if (difference.Type == DbObjectTreeFolderType.Triggers.ToString())
            {
                return difference.Parent.SubDifferences.Where(item => item.DatabaseObjectType == DatabaseObjectType.TableTrigger);
            }
            else
            {
                return children;
            }
        }

        private bool CanExpand(DbDifference difference)
        {
            DatabaseObjectType databaseObjectType = difference.DatabaseObjectType;

            return (databaseObjectType == DatabaseObjectType.None || difference.DatabaseObjectType == DatabaseObjectType.Table);
        }

        private IEnumerable<DbDifference> GetTableChildrenFolders(Table source, Table target, DbDifference difference)
        {
            string tableName = source == null ? target.Name : source.Name;

            List<DbDifference> differences = new List<DbDifference>();

            Action<DatabaseObjectType, string> addFolder = (databaseObjectType, folderName) =>
            {
                if (difference.SubDifferences.Any(item => item.DatabaseObjectType == databaseObjectType))
                {
                    differences.Add(new DbDifference() { Type = folderName, ParentType = nameof(Table), ParentName = tableName, Parent = difference, DifferenceType = this.GetTableSubFolderDiffType(difference, databaseObjectType) });
                }
            };

            addFolder(DatabaseObjectType.TableColumn, DbObjectTreeFolderType.Columns.ToString());

            addFolder(DatabaseObjectType.TablePrimaryKey, "Primary Keys");

            addFolder(DatabaseObjectType.TableForeignKey, "Foreign Keys");

            addFolder(DatabaseObjectType.TableIndex, DbObjectTreeFolderType.Indexes.ToString());

            addFolder(DatabaseObjectType.TableConstraint, DbObjectTreeFolderType.Constraints.ToString());

            addFolder(DatabaseObjectType.TableTrigger, DbObjectTreeFolderType.Triggers.ToString());

            return differences;
        }

        private DbDifferenceType GetTableSubFolderDiffType(DbDifference difference, DatabaseObjectType databaseObjectType)
        {
            return difference.SubDifferences.Any(item => item.DatabaseObjectType == databaseObjectType && item.DifferenceType != DbDifferenceType.None) ? DbDifferenceType.Modified : DbDifferenceType.None;
        }

        private void DecorateData(List<DbDifference> differences)
        {
            Dictionary<DbObjectTreeFolderType, DbDifferenceType> folderTypes = new Dictionary<DbObjectTreeFolderType, DbDifferenceType>();

            Action<DbObjectTreeFolderType, DbDifferenceType> addFolderType = (folderType, diffType) =>
            {
                if (!folderTypes.ContainsKey(folderType))
                {
                    folderTypes.Add(folderType, diffType);
                }
                else if (diffType != DbDifferenceType.None)
                {
                    folderTypes[folderType] = DbDifferenceType.Modified;
                }
            };

            foreach (DbDifference difference in differences)
            {
                DatabaseObjectType databaseObjectType = difference.DatabaseObjectType;

                switch (databaseObjectType)
                {
                    case DatabaseObjectType.UserDefinedType:
                        difference.ParentType = DbObjectTreeFolderType.Types.ToString();
                        addFolderType(DbObjectTreeFolderType.Types, difference.DifferenceType);
                        break;
                    case DatabaseObjectType.Table:
                        difference.ParentType = DbObjectTreeFolderType.Tables.ToString();
                        addFolderType(DbObjectTreeFolderType.Tables, difference.DifferenceType);
                        break;
                    case DatabaseObjectType.View:
                        difference.ParentType = DbObjectTreeFolderType.Views.ToString();
                        addFolderType(DbObjectTreeFolderType.Views, difference.DifferenceType);
                        break;
                    case DatabaseObjectType.Function:
                        difference.ParentType = DbObjectTreeFolderType.Functions.ToString();
                        addFolderType(DbObjectTreeFolderType.Functions, difference.DifferenceType);
                        break;
                    case DatabaseObjectType.Procedure:
                        difference.ParentType = DbObjectTreeFolderType.Procedures.ToString();
                        addFolderType(DbObjectTreeFolderType.Procedures, difference.DifferenceType);
                        break;
                }
            }

            int i = 0;
            foreach (var kp in folderTypes)
            {
                differences.Insert(i++, new DbDifference() { Type = kp.Key.ToString(), DifferenceType = kp.Value });
            }
        }

        private async void btnSync_Click(object sender, EventArgs e)
        {
            await this.GenerateOrSync(true);
        }

        private async void btnGenerate_Click(object sender, EventArgs e)
        {
            await this.GenerateOrSync(false);
        }

        private async Task GenerateOrSync(bool isSync, DbDifference difference = null)
        {
            if (this.sourceInterpreter == null || this.targetInterpreter == null || this.differences == null)
            {
                MessageBox.Show("Please compare first.");
                return;
            }

            try
            {
                DbSynchro dbSynchro = new DbSynchro(this.sourceInterpreter, this.targetInterpreter);

                if (!isSync)
                {
                    List<Script> scripts = null;

                    string targetDbOwner = this.GetTargetDbOwner();

                    if (difference == null)
                    {
                        scripts = await dbSynchro.GenerateChangedScripts(this.sourceSchemaInfo, targetDbOwner, this.differences);
                    }
                    else if (difference.Source is ScriptDbObject || difference.Target is ScriptDbObject)
                    {
                        scripts = dbSynchro.GenereateUserDefinedTypeChangedScripts(difference, targetDbOwner);
                    }
                    else if (difference.DatabaseObjectType == DatabaseObjectType.Table)
                    {
                        scripts = await dbSynchro.GenerateTableChangedScripts(this.sourceSchemaInfo, difference, targetDbOwner);
                    }
                    else if (difference.Source is TableChild || difference.Target is TableChild)
                    {
                        scripts = await dbSynchro.GenerateTableChildChangedScripts(difference);
                    }
                    else if (difference.Source is UserDefinedType || difference.Target is UserDefinedType)
                    {
                        scripts = dbSynchro.GenereateUserDefinedTypeChangedScripts(difference, targetDbOwner);
                    }

                    if (scripts != null)
                    {
                        string strScripts = string.Join(Environment.NewLine, scripts.Select(item => item.Content));

                        frmScriptsViewer scriptsViewer = new frmScriptsViewer() { DatabaseType = this.targetInterpreter.DatabaseType };
                        scriptsViewer.LoadScripts(StringHelper.ToSingleEmptyLine(strScripts).Trim());

                        scriptsViewer.ShowDialog();
                    }
                }
                else
                {
                    if (MessageBox.Show("Are you sure to sync changes to target database?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    {
                        ContentSaveResult result = await dbSynchro.Sync(this.sourceSchemaInfo, this.GetTargetDbOwner(), this.differences);

                        if (result.IsOK)
                        {
                            MessageBox.Show("sync successfully.");
                        }
                        else
                        {
                            MessageBox.Show(result.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            string errMsg = ExceptionHelper.GetExceptionDetails(ex);

            LogHelper.LogError(errMsg);

            this.Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = errMsg });

            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private string GetTargetDbOwner()
        {
            if (this.targetInterpreter.DatabaseType == DatabaseType.Oracle)
            {
                return (this.targetInterpreter as OracleInterpreter).GetDbOwner();
            }
            else if (this.targetInterpreter.DatabaseType == DatabaseType.MySql)
            {
                return this.targetInterpreter.ConnectionInfo.Database;
            }
            else if (this.targetInterpreter.DatabaseType == DatabaseType.SqlServer)
            {
                return "dbo";
            }

            return null;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void sourceDbProfile_OnSelectedChanged(object sender, ConnectionInfo connectionInfo)
        {
            this.sourceDbConnectionInfo = connectionInfo;

            this.targetDbProfile.DatabaseType = this.sourceDbProfile.DatabaseType;
        }

        private void targetDbProfile_OnSelectedChanged(object sender, ConnectionInfo connectionInfo)
        {
            this.targetDbConnectionInfo = connectionInfo;
        }

        private void frmCompare_SizeChanged(object sender, EventArgs e)
        {
            this.splitContainer2.SplitterDistance = (int)((this.splitContainer2.Width - this.splitContainer2.SplitterWidth) * 1.0 / 2);
        }

        private void tlvDifferences_SelectedIndexChanged(object sender, EventArgs e)
        {
            DbDifference difference = this.tlvDifferences.SelectedObject as DbDifference;

            if (difference != null)
            {
                this.ShowScripts(difference);

                this.HighlightingDifferences(this.txtSource, this.txtTarget);
            }
        }

        private void ShowScripts(DbDifference difference)
        {
            DatabaseObject source = difference.Source;
            DatabaseObject target = difference.Target;

            this.ShowSourceScripts(this.GetDatabaseObjectScripts(source, true));
            this.ShowTargetScripts(this.GetDatabaseObjectScripts(target, false));
        }

        private string GetDatabaseObjectScripts(DatabaseObject dbObj, bool isSource)
        {
            if (dbObj == null)
            {
                return string.Empty;
            }

            DbScriptGenerator scriptGenerator = isSource ? this.sourceScriptGenerator : this.targetScriptGenerator;

            if (dbObj is Table table)
            {
                SchemaInfo schemaInfo = isSource ? this.sourceSchemaInfo : this.targetSchemaInfo;

                IEnumerable<TableColumn> columns = schemaInfo.TableColumns.Where(item => item.Owner == table.Owner && item.TableName == table.Name).OrderBy(item => item.Name);
                TablePrimaryKey tablePrimaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault(item => item.Owner == table.Owner && item.TableName == table.Name);
                IEnumerable<TableForeignKey> foreignKeys = schemaInfo.TableForeignKeys.Where(item => item.Owner == table.Owner && item.TableName == table.Name);
                IEnumerable<TableIndex> indexes = schemaInfo.TableIndexes.Where(item => item.Owner == table.Owner && item.TableName == table.Name).OrderBy(item => item.Name);
                IEnumerable<TableConstraint> constraints = schemaInfo.TableConstraints.Where(item => item.Owner == table.Owner && item.TableName == table.Name);

                return scriptGenerator.AddTable(table, columns, tablePrimaryKey, foreignKeys, indexes, constraints).ToString();
            }
            else
            {
                return scriptGenerator.Add(dbObj).Content;
            }
        }

        private void ShowSourceScripts(string scripts)
        {
            this.ShowScripts(this.txtSource, scripts);
        }

        private void ShowTargetScripts(string scripts)
        {
            this.ShowScripts(this.txtTarget, scripts);
        }

        private void ShowScripts(RichTextBox textBox, string scripts)
        {
            textBox.Clear();

            if (!string.IsNullOrEmpty(scripts))
            {
                textBox.AppendText(scripts.Trim());

                RichTextBoxHelper.Highlighting(textBox, this.targetDbProfile.DatabaseType, false);
            }
        }

        private void HighlightingDifferences(RichTextBox sourceTextBox, RichTextBox targetTextBox)
        {
            if (sourceTextBox.TextLength > 0 && targetTextBox.TextLength > 0)
            {
                SideBySideDiffModel model = SideBySideDiffBuilder.Instance.BuildDiffModel(targetTextBox.Text, sourceTextBox.Text);

                this.HighlightingChanges(targetTextBox, model.OldText.Lines);
                this.HighlightingChanges(sourceTextBox, model.NewText.Lines);

                sourceTextBox.SelectionStart = 0;
                targetTextBox.SelectionStart = 0;
            }
        }

        private void HighlightingChanges(RichTextBox richTextBox, List<DiffPiece> lines)
        {
            int lineIndex = 0;

            foreach (DiffPiece line in lines)
            {
                if (line.Position.HasValue && line.Type != ChangeType.Unchanged)
                {
                    int lineFirstCharIndex = richTextBox.GetFirstCharIndexFromLine(lineIndex);

                    if (line.Type == ChangeType.Inserted || line.Type == ChangeType.Deleted)
                    {
                        this.HighlightingText(richTextBox, lineFirstCharIndex, line.Text.Length, line.Type);
                    }
                    else if (line.Type == ChangeType.Modified)
                    {
                        int subLength = 0;

                        foreach (DiffPiece subPiece in line.SubPieces)
                        {
                            if (subPiece.Text != null)
                            {
                                if (subPiece.Type != ChangeType.Unchanged)
                                {
                                    int startIndex = lineFirstCharIndex + subLength;

                                    this.HighlightingText(richTextBox, startIndex, subPiece.Text.Length, ChangeType.Modified);
                                }

                                subLength += subPiece.Text.Length;
                            }
                        }
                    }
                }

                if (line.Type != ChangeType.Imaginary)
                {
                    lineIndex++;
                }
            }
        }

        private void HighlightingText(RichTextBox richTextBox, int startIndex, int length, ChangeType changeType)
        {
            if (startIndex < 0)
            {
                return;
            }

            Color color = Color.White;

            if (changeType == ChangeType.Modified)
            {
                color = Color.Yellow;
            }
            else if (changeType == ChangeType.Inserted)
            {
                color = ColorTranslator.FromHtml("#B7EB9B");
            }
            else if (changeType == ChangeType.Deleted)
            {
                color = ColorTranslator.FromHtml("#F2C4C4");
            }

            richTextBox.Select(startIndex, length);
            richTextBox.SelectionBackColor = color;
        }

        private void txtSource_VScroll(object sender, EventArgs e)
        {
            this.SyncScrollBar(sender as RichTextBox);
        }

        private void txtTarget_VScroll(object sender, EventArgs e)
        {
            this.SyncScrollBar(sender as RichTextBox);
        }

        private void SyncScrollBar(RichTextBox richTextBox)
        {
            if (this.isRichTextBoxScrolling)
            {
                return;
            }

            this.isRichTextBoxScrolling = true;

            this.SyncScrollBarLocation(richTextBox);

            this.isRichTextBoxScrolling = false;
        }

        private void SyncScrollBarLocation(RichTextBox richTextBox)
        {
            Point point = richTextBox.Location;
            int charIndex = richTextBox.GetCharIndexFromPosition(point);
            int lineIndex = richTextBox.GetLineFromCharIndex(charIndex);

            RichTextBox anotherTextbox = richTextBox.Name == this.txtSource.Name ? this.txtTarget : this.txtSource;

            int firstCharIndexOfLine = anotherTextbox.GetFirstCharIndexFromLine(lineIndex);

            if (firstCharIndexOfLine >= 0)
            {
                anotherTextbox.SelectionStart = firstCharIndexOfLine;
                anotherTextbox.SelectionLength = 0;
                anotherTextbox.ScrollToCaret();
            }
        }

        private void tsmiExpandAll_Click(object sender, EventArgs e)
        {
            DbDifference difference = this.tlvDifferences.SelectedObject as DbDifference;

            if (difference != null)
            {
                this.ExpandCollapseAllChildren(difference, true);
            }
            else
            {
                this.tlvDifferences.ExpandAll();
            }
        }

        private void ExpandCollapseAllChildren(DbDifference difference, bool isExpand)
        {
            if (this.tlvDifferences.CanExpand(difference))
            {
                if (isExpand)
                {
                    this.tlvDifferences.Expand(difference);
                }
                else
                {
                    this.tlvDifferences.Collapse(difference);
                }
            }

            IEnumerable<DbDifference> children = this.tlvDifferences.GetChildren(difference).OfType<DbDifference>();

            foreach (DbDifference child in children)
            {
                this.ExpandCollapseAllChildren(child, isExpand);
            }
        }

        private void tsmiCollapseAll_Click(object sender, EventArgs e)
        {
            DbDifference difference = this.tlvDifferences.SelectedObject as DbDifference;

            if (difference != null)
            {
                this.ExpandCollapseAllChildren(difference, false);
            }
            else
            {
                this.tlvDifferences.CollapseAll();
            }
        }

        private void Feedback(string message)
        {
            this.Feedback(new FeedbackInfo() { Message = message });
        }

        private void Feedback(FeedbackInfo info)
        {
            this.Invoke(new Action(() =>
            {
                this.txtMessage.Text = info.Message;

                if (info.InfoType == FeedbackInfoType.Error)
                {
                    this.txtMessage.ForeColor = Color.Red;
                    this.toolTip1.SetToolTip(this.txtMessage, info.Message);
                }
                else
                {
                    this.txtMessage.ForeColor = Color.Black;
                }
            }));
        }

        #region IObserver<FeedbackInfo>
        void IObserver<FeedbackInfo>.OnCompleted()
        {
        }
        void IObserver<FeedbackInfo>.OnError(Exception error)
        {
        }
        void IObserver<FeedbackInfo>.OnNext(FeedbackInfo info)
        {
            this.Feedback(info);
        }
        #endregion

        private void tlvDifferences_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (this.tlvDifferences.Items.Count == 0)
                {
                    return;
                }

                DbDifference difference = this.tlvDifferences.SelectedObject as DbDifference;

                if (difference != null)
                {
                    this.tsmiCollapseAll.Visible = this.tsmiExpandAll.Visible = this.CanExpand(difference);

                    this.tsmiGenerateChangedScripts.Visible = (difference.DifferenceType != DbDifferenceType.None && difference.DatabaseObjectType != DatabaseObjectType.None);
                }
                else
                {
                    this.tsmiCollapseAll.Visible = this.tsmiExpandAll.Visible = true;
                }

                this.contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private async void tsmiGenerateChangedScripts_Click(object sender, EventArgs e)
        {
            DbDifference difference = this.tlvDifferences.SelectedObject as DbDifference;

            await this.GenerateOrSync(false, difference);
        }

        private void tlvDifferences_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
            if (this.tlvDifferences.Roots != null)
            {
                var roots = this.tlvDifferences.Roots.OfType<DbDifference>();
                var firstRecord = roots.FirstOrDefault();

                if (roots.Count() == 1 && this.CanExpand(firstRecord) && !this.tlvDifferences.IsExpanded(firstRecord))
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(1000);

                        try
                        {
                            this.tlvDifferences.Expand(firstRecord);
                        }
                        catch (Exception ex)
                        {
                        }
                    });                   
                }
            }
        }
    }
}
