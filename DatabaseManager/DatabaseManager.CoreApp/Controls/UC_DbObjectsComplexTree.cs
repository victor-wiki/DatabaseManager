using DatabaseConverter.Core;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using DatabaseManager.Profile;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using View = DatabaseInterpreter.Model.View;

namespace DatabaseManager.Controls
{
    public delegate void ShowDbObjectContentHandler(DatabaseObjectDisplayInfo content);

    public partial class UC_DbObjectsComplexTree : UserControl, IObserver<FeedbackInfo>
    {
        private DatabaseType databaseType;
        private ConnectionInfo connectionInfo;
        private DbInterpreterOption simpleInterpreterOption = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple, ThrowExceptionWhenErrorOccurs = true };

        public ShowDbObjectContentHandler OnShowContent;
        public DatabaseInterpreter.Utility.FeedbackHandler OnFeedback;

        public DatabaseType DatabaseType => this.databaseType;

        public UC_DbObjectsComplexTree()
        {
            InitializeComponent();

            FormEventCenter.OnRefreshNavigatorFolder += this.RefreshFolderNode;

            TreeView.CheckForIllegalCrossThreadCalls = false;
            Form.CheckForIllegalCrossThreadCalls = false;
        }

        public async Task LoadTree(DatabaseType dbType, ConnectionInfo connectionInfo)
        {
            this.databaseType = dbType;
            this.connectionInfo = connectionInfo;

            this.tvDbObjects.Nodes.Clear();

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(dbType, connectionInfo, simpleInterpreterOption);

            List<Database> databases = await dbInterpreter.GetDatabasesAsync();

            IEnumerable<DatabaseVisibilityInfo> visibilities = Enumerable.Empty<DatabaseVisibilityInfo>();

            bool isFileConnection = ManagerUtil.IsFileConnection(dbType);

            if(!isFileConnection && dbType != DatabaseType.Oracle)
            {
                AccountProfileInfo profileInfo = await AccountProfileManager.GetProfile(dbType.ToString(), connectionInfo.Server, connectionInfo.Port, connectionInfo.IntegratedSecurity, connectionInfo.UserId);

                if (profileInfo != null)
                {
                    visibilities = await DatabaseVisibilityManager.GetVisibilities(profileInfo.Id);
                }
            }            

            foreach (Database database in databases)
            {
                if (visibilities.Any(item => item.Visible == false && item.Database.ToUpper() == database.Name.ToUpper()))
                {
                    continue;
                }

                TreeNode node = DbObjectsTreeHelper.CreateTreeNode(database, true);

                if (ManagerUtil.IsFileConnection(dbType))
                {
                    FileConnectionProfileInfo profile = await FileConnectionProfileManager.GetProfileByDatabase(dbType.ToString(), connectionInfo.Database);

                    if (profile != null)
                    {
                        node.Text = profile.Name;
                    }
                }

                this.tvDbObjects.Nodes.Add(node);
            }

            if (this.tvDbObjects.Nodes.Count == 1)
            {
                this.tvDbObjects.SelectedNode = this.tvDbObjects.Nodes[0];
                this.tvDbObjects.Nodes[0].Expand();
            }
        }

        public void ClearNodes()
        {
            this.tvDbObjects.Nodes.Clear();
        }

        private void tvDbObjects_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.Node.Name == DbObjectsTreeHelper.FakeNodeName)
                {
                    return;
                }

                this.tvDbObjects.SelectedNode = e.Node;

                this.SetMenuItemVisible(e.Node);

                this.contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private bool CanRefresh(TreeNode node)
        {
            return (node.Level <= 3) && !this.IsOnlyHasFakeChild(node)
               && !(node.Tag is ScriptDbObject && !(node.Tag is View))
               && !(node.Tag is UserDefinedType)
               && !(node.Tag is Sequence);
        }

        private bool CanDelete(TreeNode node)
        {
            return node.Level == 2 || (node.Level == 4 && !(node.Tag is TableColumn));
        }

        private void SetMenuItemVisible(TreeNode node)
        {
            bool isDatabase = node.Level == 0;
            bool isView = node.Tag is View;
            bool isTable = node.Tag is Table;
            bool isScriptObject = node.Tag is ScriptDbObject;
            bool isUserDefinedType = node.Tag is UserDefinedType;
            bool isSequence = node.Tag is Sequence;
            bool isFunction = node.Tag is Function;
            bool isTriggerFunction = isFunction && (node.Tag as Function).IsTriggerFunction;
            bool isProcedure = node.Tag is Procedure;

            this.tsmiNewQuery.Visible = isDatabase;
            this.tsmiNewTable.Visible = node.Name == nameof(DbObjectTreeFolderType.Tables) || isTable;
            this.tsmiNewView.Visible = node.Name == nameof(DbObjectTreeFolderType.Views) || isView;
            this.tsmiNewFunction.Visible = node.Name == nameof(DbObjectTreeFolderType.Functions) || node.Tag is Function;
            this.tsmiNewProcedure.Visible = node.Name == nameof(DbObjectTreeFolderType.Procedures) || node.Tag is Procedure;
            this.tsmiNewTrigger.Visible = node.Name == nameof(DbObjectTreeFolderType.Triggers) || node.Tag is TableTrigger;
            this.tsmiAlter.Visible = isScriptObject;
            this.tsmiDesign.Visible = isTable;
            this.tsmiCopy.Visible = isTable;
            this.tsmiRefresh.Visible = this.CanRefresh(node);
            this.tsmiGenerateScripts.Visible = isDatabase || isTable || isScriptObject || isUserDefinedType || isSequence;
            this.tsmiConvert.Visible = isDatabase;
            this.tsmiEmptyDatabase.Visible = isDatabase;
            this.tsmiDelete.Visible = this.CanDelete(node);
            this.tsmiViewData.Visible = isTable || isView;
            this.tsmiTranslate.Visible = isTable || isUserDefinedType || isSequence || isScriptObject;
            this.tsmiMore.Visible = isDatabase;
            this.tsmiBackup.Visible = isDatabase;
            this.tsmiDiagnose.Visible = isDatabase;
            this.tsmiCompare.Visible = isDatabase;

            this.tsmiSelectScript.Visible = isTable || isView || (isFunction && !isTriggerFunction);
            this.tsmiInsertScript.Visible = isTable;
            this.tsmiUpdateScript.Visible = isTable;
            this.tsmiDeleteScript.Visible = isTable;
            this.tsmiViewDependency.Visible = isDatabase || ((isTable || isView || isFunction || isProcedure) && this.databaseType != DatabaseType.Sqlite);
            this.tsmiExecuteScript.Visible = isProcedure;

            this.tsmiCopyChildrenNames.Visible = node.Level == 1 && node.Nodes.Count > 0 && (node.Nodes[0].Tag != null);
        }

        private ConnectionInfo GetConnectionInfo(string database)
        {
            ConnectionInfo info = ObjectHelper.CloneObject<ConnectionInfo>(this.connectionInfo);
            info.Database = database;
            return info;
        }

        public ConnectionInfo GetCurrentConnectionInfo()
        {
            TreeNode node = this.tvDbObjects.SelectedNode;

            if (node != null)
            {
                TreeNode dbNode = this.GetDatabaseNode(node);
                ConnectionInfo connectionInfo = this.GetConnectionInfo(dbNode.Name);

                return connectionInfo;
            }

            return null;
        }

        private bool IsOnlyHasFakeChild(TreeNode node)
        {
            if (node.Nodes.Count == 1 && node.Nodes[0].Name == DbObjectsTreeHelper.FakeNodeName)
            {
                return true;
            }

            return false;
        }

        private TreeNode GetDatabaseNode(TreeNode node)
        {
            while (!(node.Tag is Database))
            {
                return this.GetDatabaseNode(node.Parent);
            }
            return node;
        }

        private DbInterpreter GetDbInterpreter(string database, bool isSimpleMode = true)
        {
            ConnectionInfo connectionInfo = this.GetConnectionInfo(database);

            DbInterpreterOption option = isSimpleMode ? simpleInterpreterOption : new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Details };

            option.ThrowExceptionWhenErrorOccurs = false;

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.databaseType, connectionInfo, option);

            return dbInterpreter;
        }

        private async Task AddDbObjectNodes(TreeNode parentNode, string database, DatabaseObjectType databaseObjectType = DatabaseObjectType.None, bool createFolderNode = true)
        {
            DbInterpreter dbInterpreter = this.GetDbInterpreter(database);

            SchemaInfoFilter filter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType };

            if (this.databaseType == DatabaseType.Oracle)
            {
                filter.Schema = database;
            }

            SchemaInfo schemaInfo = databaseObjectType == DatabaseObjectType.None ? new SchemaInfo() :
                                    await dbInterpreter.GetSchemaInfoAsync(filter);

            this.ClearNodes(parentNode);

            if (databaseObjectType == DatabaseObjectType.Table)
            {
                this.AddTreeNodes(parentNode, databaseObjectType, DatabaseObjectType.Table, schemaInfo.Tables, createFolderNode, true);
            }

            if (databaseObjectType == DatabaseObjectType.View)
            {
                this.AddTreeNodes(parentNode, databaseObjectType, DatabaseObjectType.View, schemaInfo.Views, createFolderNode, true);
            }

            if (databaseObjectType == DatabaseObjectType.Function)
            {
                this.AddTreeNodes(parentNode, databaseObjectType, DatabaseObjectType.Function, schemaInfo.Functions, createFolderNode);
            }

            if (databaseObjectType == DatabaseObjectType.Procedure)
            {
                this.AddTreeNodes(parentNode, databaseObjectType, DatabaseObjectType.Procedure, schemaInfo.Procedures, createFolderNode);
            }

            if (databaseObjectType == DatabaseObjectType.Type)
            {
                foreach (UserDefinedType userDefinedType in schemaInfo.UserDefinedTypes)
                {
                    string dataType = userDefinedType.Attributes.Count > 1 ? "" : userDefinedType.Attributes.First().DataType;
                    string strDataType = string.IsNullOrEmpty(dataType) ? "" : $"({dataType})";

                    string text = $"{userDefinedType.Name}{strDataType}";

                    string imageKeyName = nameof(userDefinedType);

                    TreeNode node = DbObjectsTreeHelper.CreateTreeNode(userDefinedType.Name, text, imageKeyName);
                    node.Tag = userDefinedType;

                    parentNode.Nodes.Add(node);
                }
            }

            if (databaseObjectType == DatabaseObjectType.Sequence)
            {
                this.AddTreeNodes(parentNode, databaseObjectType, DatabaseObjectType.Sequence, schemaInfo.Sequences, createFolderNode);
            }
        }

        private TreeNodeCollection AddTreeNodes<T>(TreeNode node, DatabaseObjectType types, DatabaseObjectType type, List<T> dbObjects, bool createFolderNode = true, bool createFakeNode = false)
            where T : DatabaseObject
        {
            TreeNode targetNode = node;

            if (types.HasFlag(type))
            {
                if (createFolderNode)
                {
                    targetNode = node.AddDbObjectFolderNode(dbObjects);
                }
                else
                {
                    targetNode = node.AddDbObjectNodes(dbObjects);
                }
            }

            if (createFakeNode && targetNode != null)
            {
                foreach (TreeNode child in targetNode.Nodes)
                {
                    child.Nodes.Add(DbObjectsTreeHelper.CreateFakeNode());
                }
            }

            return node.Nodes;
        }

        private void AddTableFakeNodes(TreeNode tableNode, Table table)
        {
            this.ClearNodes(tableNode);

            tableNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Columns), nameof(DbObjectTreeFolderType.Columns), true));
            tableNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Triggers), nameof(DbObjectTreeFolderType.Triggers), true));
            tableNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Indexes), nameof(DbObjectTreeFolderType.Indexes), true));
            tableNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Keys), nameof(DbObjectTreeFolderType.Keys), true));
            tableNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Constraints), nameof(DbObjectTreeFolderType.Constraints), true));
        }

        private void AddViewFakeNodes(TreeNode viewNode, View view)
        {
            this.ClearNodes(viewNode);

            viewNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Columns), nameof(DbObjectTreeFolderType.Columns), true));

            if (this.databaseType == DatabaseType.SqlServer || this.databaseType == DatabaseType.Sqlite)
            {
                viewNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Triggers), nameof(DbObjectTreeFolderType.Triggers), true));
            }

            if (this.databaseType == DatabaseType.SqlServer)
            {
                viewNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Indexes), nameof(DbObjectTreeFolderType.Indexes), true));
            }
        }

        private void AddDatabaseFakeNodes(TreeNode databaseNode, Database database)
        {
            this.ClearNodes(databaseNode);

            DbInterpreter dbInterpreter = this.GetDbInterpreter(database.Name, true);

            DatabaseObjectType supportDbObjectType = dbInterpreter.SupportDbObjectType;

            databaseNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Tables), nameof(DbObjectTreeFolderType.Tables), true));
            databaseNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Views), nameof(DbObjectTreeFolderType.Views), true));

            if (supportDbObjectType.HasFlag(DatabaseObjectType.Function))
            {
                databaseNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Functions), nameof(DbObjectTreeFolderType.Functions), true));
            }

            if (supportDbObjectType.HasFlag(DatabaseObjectType.Procedure))
            {
                databaseNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Procedures), nameof(DbObjectTreeFolderType.Procedures), true));
            }

            if (supportDbObjectType.HasFlag(DatabaseObjectType.Type))
            {
                databaseNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Types), nameof(DbObjectTreeFolderType.Types), true));
            }

            if (supportDbObjectType.HasFlag(DatabaseObjectType.Sequence))
            {
                databaseNode.Nodes.Add(DbObjectsTreeHelper.CreateFolderNode(nameof(DbObjectTreeFolderType.Sequences), nameof(DbObjectTreeFolderType.Sequences), true));
            }
        }

        private async Task AddTableObjectNodes(TreeNode treeNode, Table table, DatabaseObjectType databaseObjectType, bool isForView = false)
        {
            string nodeName = treeNode.Name;
            string database = this.GetDatabaseNode(treeNode).Name;
            DbInterpreter dbInterpreter = this.GetDbInterpreter(database, false);

            dbInterpreter.Subscribe(this);

            SchemaInfoFilter filter = new SchemaInfoFilter() { Strict = true, DatabaseObjectType = databaseObjectType, Schema = table.Schema, TableNames = new string[] { table.Name } };

            if (isForView)
            {
                filter.ColumnType = ColumnType.ViewColumn;
                filter.IsForView = true;
            }

            SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

            this.ClearNodes(treeNode);

            #region Columns           
            if (nodeName == nameof(DbObjectTreeFolderType.Columns))
            {
                foreach (TableColumn column in schemaInfo.TableColumns)
                {
                    string text = this.GetColumnText(dbInterpreter, table, column);
                    bool isPrimaryKey = schemaInfo.TablePrimaryKeys.Any(item => item.Columns.Any(t => t.ColumnName == column.Name));
                    bool isForeignKey = schemaInfo.TableForeignKeys.Any(item => item.Columns.Any(t => t.ColumnName == column.Name));
                    string imageKeyName = isPrimaryKey ? nameof(TablePrimaryKey) : (isForeignKey ? nameof(TableForeignKey) : nameof(TableColumn));

                    TreeNode node = DbObjectsTreeHelper.CreateTreeNode(column.Name, text, imageKeyName);
                    node.Tag = column;

                    treeNode.Nodes.Add(node);
                }
            }
            #endregion

            if (nodeName == nameof(DbObjectTreeFolderType.Triggers))
            {
                treeNode.AddDbObjectNodes(schemaInfo.TableTriggers);
            }

            if (!isForView)
            {
                #region Indexes
                if (nodeName == nameof(DbObjectTreeFolderType.Indexes) && schemaInfo.TableIndexes.Any())
                {
                    foreach (TableIndex index in schemaInfo.TableIndexes)
                    {
                        bool isUnique = index.IsUnique;
                        string strColumns = string.Join(",", index.Columns.OrderBy(item => item.Order).Select(item => item.ColumnName));

                        string content = index.Columns.Count > 0 ? (isUnique ? $"(Unique, {strColumns})" : $"({strColumns})")
                                        : (isUnique ? "(Unique)" : "");

                        string text = $"{index.Name}{content}";
                        string imageKeyName = nameof(TableIndex);

                        TreeNode node = DbObjectsTreeHelper.CreateTreeNode(index.Name, text, imageKeyName);
                        node.Tag = index;

                        treeNode.Nodes.Add(node);
                    }
                }
                #endregion

                if (nodeName == nameof(DbObjectTreeFolderType.Keys))
                {
                    foreach (TablePrimaryKey key in schemaInfo.TablePrimaryKeys)
                    {
                        TreeNode node = DbObjectsTreeHelper.CreateTreeNode(key);

                        if (string.IsNullOrEmpty(node.Text))
                        {
                            node.Text = $"PK_{key.TableName}(unnamed)";
                        }

                        treeNode.Nodes.Add(node);
                    }

                    foreach (TableForeignKey key in schemaInfo.TableForeignKeys)
                    {
                        TreeNode node = DbObjectsTreeHelper.CreateTreeNode(key);

                        if (string.IsNullOrEmpty(node.Text))
                        {
                            node.Text = $"FK_{key.TableName}(unnamed)";
                        }

                        treeNode.Nodes.Add(node);
                    }
                }

                #region Constraints
                if (nodeName == nameof(DbObjectTreeFolderType.Constraints) && schemaInfo.TableConstraints.Any())
                {
                    foreach (TableConstraint constraint in schemaInfo.TableConstraints)
                    {
                        TreeNode node = DbObjectsTreeHelper.CreateTreeNode(constraint);
                        treeNode.Nodes.Add(node);
                    }
                }
                #endregion
            }

            this.Feedback("");
        }

        private string GetColumnText(DbInterpreter dbInterpreter, Table table, TableColumn column)
        {
            string text = dbInterpreter.ParseColumn(table, column).Replace(dbInterpreter.QuotationLeftChar.ToString(), "").Replace(dbInterpreter.QuotationRightChar.ToString(), "");

            int index = text.IndexOf(column.Name);

            string displayText = text.Substring(index + column.Name.Length);

            return $"{column.Name} ({displayText.ToLower().Trim()})";
        }

        private async void tvDbObjects_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;

            if (!this.IsOnlyHasFakeChild(node))
            {
                return;
            }

            this.tvDbObjects.BeginInvoke(new Action(async () => await this.LoadChildNodes(node)));
        }

        private void ClearNodes(TreeNode node)
        {
            node.Nodes.Clear();
        }

        private void ShowLoading(TreeNode node)
        {
            string loadingImageKey = "Loading.gif";
            string loadingText = "loading...";

            if (this.IsOnlyHasFakeChild(node))
            {
                node.Nodes[0].ImageKey = loadingImageKey;
                node.Nodes[0].Text = loadingText;
            }
            else
            {
                node.Nodes.Add(DbObjectsTreeHelper.CreateTreeNode("Loading", loadingText, loadingImageKey));
            }
        }

        private async Task LoadChildNodes(TreeNode node)
        {
            this.ShowLoading(node);

            object tag = node.Tag;

            if (tag is Database)
            {
                Database database = tag as Database;

                this.AddDatabaseFakeNodes(node, database);
            }
            else if (tag is Table)
            {
                Table table = tag as Table;
                this.AddTableFakeNodes(node, table);
            }
            else if (tag is View)
            {
                View view = tag as View;
                this.AddViewFakeNodes(node, view);
            }
            else if (tag == null)
            {
                string name = node.Name;

                TreeNode parentNode = node.Parent;

                if (parentNode.Tag is Database)
                {
                    string databaseName = parentNode.Name;

                    DatabaseObjectType databaseObjectType = DbObjectsTreeHelper.GetDbObjectTypeByFolderName(name);

                    if (databaseObjectType != DatabaseObjectType.None)
                    {
                        await this.AddDbObjectNodes(node, databaseName, databaseObjectType, false);

                        this.ShowChildrenCount(node);
                    }
                }
                else if (parentNode.Tag is Table)
                {
                    DatabaseObjectType databaseObjectType = this.GetDatabaseObjectTypeByFolderNames(name);

                    await this.AddTableObjectNodes(node, parentNode.Tag as Table, databaseObjectType);
                }
                else if (parentNode.Tag is View)
                {
                    Table table = ObjectHelper.CloneObject<Table>(parentNode.Tag as View);

                    DatabaseObjectType databaseObjectType = this.GetDatabaseObjectTypeByFolderNames(name);

                    await this.AddTableObjectNodes(node, table, databaseObjectType, true);
                }
            }
        }

        private DatabaseObjectType GetDatabaseObjectTypeByFolderNames(string folderName)
        {
            DatabaseObjectType databaseObjectType = DatabaseObjectType.None;

            switch (folderName)
            {
                case nameof(DbObjectTreeFolderType.Columns):
                    databaseObjectType = DatabaseObjectType.Column | DatabaseObjectType.PrimaryKey | DatabaseObjectType.ForeignKey;
                    break;
                case nameof(DbObjectTreeFolderType.Triggers):
                    databaseObjectType = DatabaseObjectType.Trigger;
                    break;
                case nameof(DbObjectTreeFolderType.Indexes):
                    databaseObjectType = DatabaseObjectType.Index;
                    break;
                case nameof(DbObjectTreeFolderType.Keys):
                    databaseObjectType = DatabaseObjectType.PrimaryKey | DatabaseObjectType.ForeignKey;
                    break;
                case nameof(DbObjectTreeFolderType.Constraints):
                    databaseObjectType = DatabaseObjectType.Constraint;
                    break;
            }

            return databaseObjectType;
        }

        private void ShowChildrenCount(TreeNode node)
        {
            node.Text = $"{node.Name} ({node.Nodes.Count})";
        }

        private async void tsmiRefresh_Click(object sender, EventArgs e)
        {
            await this.RefreshNode();
        }

        private async Task RefreshNode()
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            if (this.CanRefresh(node))
            {
                await this.LoadChildNodes(node);
            }
        }

        private bool IsValidSelectedNode()
        {
            TreeNode node = this.GetSelectedNode();

            return node != null;
        }

        private TreeNode GetSelectedNode()
        {
            return this.tvDbObjects.SelectedNode;
        }

        private async void GenerateScripts(ScriptAction scriptAction)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            await this.GenerateScripts(node, scriptAction);
        }

        private async Task GenerateScripts(TreeNode node, ScriptAction scriptAction)
        {
            object tag = node.Tag;

            if (tag is Database)
            {
                Database database = tag as Database;

                frmGenerateScripts frmGenerateScripts = new frmGenerateScripts(this.databaseType, this.GetConnectionInfo(database.Name));
                frmGenerateScripts.ShowDialog();
            }
            else if (tag is DatabaseObject)
            {
                string databaseName = this.GetDatabaseNode(node).Name;

                await this.GenerateObjectScript(databaseName, tag as DatabaseObject, scriptAction);
            }
        }

        private async Task GenerateObjectScript(string database, DatabaseObject dbObj, ScriptAction scriptAction)
        {
            try
            {
                DbInterpreter dbInterpreter = this.GetDbInterpreter(database, false);

                dbInterpreter.Option.ThrowExceptionWhenErrorOccurs = true;

                ScriptGenerator scriptGenerator = new ScriptGenerator(dbInterpreter);

                ScriptGenerateResult result = await scriptGenerator.Generate(dbObj, scriptAction);

                this.ShowContent(new DatabaseObjectDisplayInfo()
                {
                    Name = dbObj.Name,
                    DatabaseType = this.databaseType,
                    DatabaseObject = dbObj,
                    ConnectionInfo = dbInterpreter.ConnectionInfo,
                    Content = result.Script,
                    ScriptAction = scriptAction,
                    ScriptParameters = result.Parameters
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex));
            }
        }

        private void tsmiConvert_Click(object sender, EventArgs e)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            this.ConvertDatabase(node);
        }

        private void ConvertDatabase(TreeNode node)
        {
            Database database = node.Tag as Database;

            frmConvert frmConvert = new frmConvert(this.databaseType, this.GetConnectionInfo(database.Name));
            frmConvert.ShowDialog();
        }

        private async void tsmiClearData_Click(object sender, EventArgs e)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            if (MessageBox.Show($"Are you sure to clear all data of the database?{Environment.NewLine}Please handle this operation carefully!", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                TreeNode node = this.GetSelectedNode();

                await this.ClearData(node.Name);
            }
        }

        private async Task ClearData(string database)
        {
            DbInterpreter dbInterpreter = this.GetDbInterpreter(database);
            DbManager dbManager = new DbManager(dbInterpreter);

            dbManager.Subscribe(this);
            dbInterpreter.Subscribe(this);

            await dbManager.ClearData();

            if (!dbInterpreter.HasError)
            {
                MessageBox.Show("Data has been cleared.");
            }
        }

        private void Feedback(FeedbackInfo info)
        {
            if (this.OnFeedback != null)
            {
                this.OnFeedback(info);
            }
        }

        private void Feedback(string message)
        {
            this.Feedback(new FeedbackInfo() { Message = message });
        }

        private async void tsmiEmptyDatabase_Click(object sender, EventArgs e)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            if (MessageBox.Show($"Are you sure to delete all objects of the database?{Environment.NewLine}Please handle this operation carefully!", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var dbInterpreter = this.GetDbInterpreter((this.GetSelectedNode().Tag as Database).Name, true);

                frmItemsSelector selector = new frmItemsSelector("Select Database Object Types", ItemsSelectorHelper.GetDatabaseObjectTypeItems(this.databaseType, dbInterpreter.SupportDbObjectType));

                if (selector.ShowDialog() == DialogResult.OK)
                {
                    TreeNode node = this.GetSelectedNode();

                    await this.EmptyDatabase(node.Name, ItemsSelectorHelper.GetDatabaseObjectTypeByCheckItems(selector.CheckedItem));

                    await this.LoadChildNodes(node);

                    this.Feedback("");
                }
            }
        }

        private async Task EmptyDatabase(string database, DatabaseObjectType databaseObjectType)
        {
            DbInterpreter dbInterpreter = this.GetDbInterpreter(database);
            dbInterpreter.Option.ThrowExceptionWhenErrorOccurs = false;
            DbManager dbManager = new DbManager(dbInterpreter);

            dbInterpreter.Subscribe(this);
            dbManager.Subscribe(this);

            await dbManager.EmptyDatabase(databaseObjectType);

            if (!dbInterpreter.HasError)
            {
                MessageBox.Show("Seleted database objects have been deleted.");
            }
        }

        private async void tvDbObjects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                await this.RefreshNode();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                this.DeleteNode();
            }

            if (e.Control)
            {
                if (e.KeyCode == Keys.F)
                {
                    this.FindChild();
                }
                else if (e.KeyCode == Keys.C)
                {
                    this.CopyNodeText();
                }
            }
        }

        private void CopyNodeText()
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            Clipboard.SetDataObject(node.Text);
        }

        private bool IsEmptyTreeNode(TreeNode node)
        {
            return node.Nodes.Count == 0 || (node.Nodes.Count == 1 && node.Nodes[0].Tag == null);
        }

        private bool IsTreeNodeHasDbObjectChildren(TreeNode node)
        {
            return !(this.IsEmptyTreeNode(node) || node.Nodes.Cast<TreeNode>().All(item => item.Tag == null && this.IsEmptyTreeNode(item)));
        }

        private void FindChild()
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            if (node.Level >= 1)
            {
                var targetNodes = this.IsTreeNodeHasDbObjectChildren(node) ? node.Nodes : node.Parent.Nodes;

                if (targetNodes.Count <= 1)
                {
                    return;
                }

                frmFindBox findBox = new frmFindBox();

                findBox.StartPosition = FormStartPosition.Manual;
                findBox.Location = new System.Drawing.Point(0, 90);

                DialogResult result = findBox.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string word = findBox.FindWord;

                    TreeNode foundNode = this.FindTreeNode(targetNodes, word);

                    if (foundNode != null)
                    {
                        this.tvDbObjects.SelectedNode = foundNode;
                        foundNode.EnsureVisible();
                    }
                    else
                    {
                        MessageBox.Show("Not found.");
                    }
                }
            }
        }

        private TreeNode FindTreeNode(TreeNodeCollection nodes, string word)
        {
            foreach (TreeNode node in nodes)
            {
                object tag = node.Tag;

                if (node.Tag != null)
                {
                    string text = node.Text.Split('.').LastOrDefault()?.Split('(')?.FirstOrDefault()?.Trim();

                    if (text.ToUpper() == word.ToUpper())
                    {
                        return node;
                    }
                }
                else if (node.Nodes.Count >= 1)
                {
                    return this.FindTreeNode(node.Nodes, word);
                }
            }

            return null;
        }

        private void DeleteNode()
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            if (this.CanDelete(node))
            {
                if (MessageBox.Show("Are you sure to delete this object?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    this.DropDbObject(node);
                }
            }
        }

        private async void DropDbObject(TreeNode node)
        {
            string database = this.GetDatabaseNode(node).Name;
            DatabaseObject dbObject = node.Tag as DatabaseObject;

            DbInterpreter dbInterpreter = this.GetDbInterpreter(database);
            dbInterpreter.Subscribe(this);

            DbManager dbManager = new DbManager(dbInterpreter);

            dbInterpreter.Subscribe(this);
            dbManager.Subscribe(this);

            bool success = await dbManager.DropDbObject(dbObject);

            if (!dbInterpreter.HasError && success)
            {
                bool parentIsChildFolderOfDatabase = node.Parent?.Parent?.Tag is Database;
                TreeNode parentNode = node.Parent;

                node.Parent.Nodes.Remove(node);

                if (parentIsChildFolderOfDatabase)
                {
                    this.ShowChildrenCount(parentNode);
                }
            }
            else
            {
                MessageBox.Show("Not drop the database object.");
            }
        }

        private void tsmiDelete_Click(object sender, EventArgs e)
        {
            this.DeleteNode();
        }

        private void tsmiViewData_Click(object sender, EventArgs e)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            this.ViewData(node);
        }

        private void ViewData(TreeNode node)
        {
            string database = this.GetDatabaseNode(node).Name;
            DatabaseObject dbObject = node.Tag as DatabaseObject;

            this.ShowContent(new DatabaseObjectDisplayInfo() { Name = dbObject.Name, DatabaseType = this.databaseType, DatabaseObject = dbObject, DisplayType = DatabaseObjectDisplayType.Data, ConnectionInfo = this.GetConnectionInfo(database) });
        }

        public void OnNext(FeedbackInfo value)
        {
            this.Feedback(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        private void tvDbObjects_AfterExpand(object sender, TreeViewEventArgs e)
        {
            this.Feedback("");
        }

        private void tsmiTranslate_MouseEnter(object sender, EventArgs e)
        {
            this.tsmiTranslate.DropDownItems.Clear();

            var node = this.GetSelectedNode();

            if(node == null || node.Tag == null)
            {
                return;
            }

            DatabaseObjectType dbObjectType = DbObjectHelper.GetDatabaseObjectType(node.Tag as DatabaseObject);

            var dbTypes = DbInterpreterHelper.GetDisplayDatabaseTypes();

            foreach (var dbType in dbTypes)
            {
                if ((int)dbType != (int)this.databaseType)
                {
                    var dbInterpreter = DbInterpreterHelper.GetDbInterpreter(dbType, new ConnectionInfo(), new DbInterpreterOption());

                    if(dbInterpreter.SupportDbObjectType.HasFlag(dbObjectType))
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem(dbType.ToString());
                        item.Click += TranslateItem_Click;

                        this.tsmiTranslate.DropDownItems.Add(item);
                    }                    
                }
            }
        }

        private async void TranslateItem_Click(object sender, EventArgs e)
        {
            DatabaseType dbType = ManagerUtil.GetDatabaseType((sender as ToolStripMenuItem).Text);

            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();
            this.tvDbObjects.SelectedNode = node;

            await this.Translate(node, dbType);
        }

        private async Task Translate(TreeNode node, DatabaseType targetDbType)
        {
            object tag = node.Tag;

            ConnectionInfo connectionInfo = this.GetConnectionInfo((this.GetDatabaseNode(node).Tag as Database).Name);

            if (tag is DatabaseObject)
            {
                TranslateManager translateManager = new TranslateManager();
                translateManager.Subscribe(this);

                var dbObject = tag as DatabaseObject;

                try
                {
                    TranslateResult result = await translateManager.Translate(this.databaseType, targetDbType, dbObject, connectionInfo, true);

                    if (result != null)
                    {
                        DatabaseObjectDisplayInfo info = new DatabaseObjectDisplayInfo()
                        {
                            Schema = result.DbObjectSchema ?? dbObject.Schema,
                            Name = dbObject.Name,
                            DatabaseType = targetDbType,
                            DatabaseObject = dbObject,
                            Content = result.Data?.ToString(),
                            ConnectionInfo = null,
                            Error = result.Error,
                            IsTranlatedScript = true
                        };

                        this.ShowContent(info);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex));
                }
            }
        }

        private void tvDbObjects_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeNode treeNode = e.Item as TreeNode;

                if (treeNode != null && treeNode.Tag is DatabaseObject)
                {
                    string text = treeNode.Text;
                    int index = text.IndexOf('(');

                    if (index > 0)
                    {
                        text = text.Substring(0, index);
                    }

                    DbInterpreter dbInterpreter = this.GetDbInterpreter(this.GetDatabaseNode(treeNode).Name);

                    var items = text.Trim().Split('.').Select(item => dbInterpreter.GetQuotedString(item));

                    DoDragDrop(string.Join(".", items), DragDropEffects.Move);
                }
            }
        }

        public DatabaseObjectDisplayInfo GetDisplayInfo()
        {
            TreeNode node = this.tvDbObjects.SelectedNode;

            DatabaseObjectDisplayInfo info = new DatabaseObjectDisplayInfo() { DatabaseType = this.DatabaseType };

            if (node != null)
            {
                if (node.Tag is DatabaseObject dbObject)
                {
                    info.Name = dbObject.Name;
                    info.DatabaseObject = dbObject;
                }

                TreeNode databaseNode = this.GetDatabaseNode(node);

                if (databaseNode != null)
                {
                    info.ConnectionInfo = this.GetConnectionInfo(databaseNode.Name);
                }
                else
                {
                    info.ConnectionInfo = this.connectionInfo;
                }
            }

            return info;
        }

        private void tsmiNewQuery_Click(object sender, EventArgs e)
        {
            this.ShowContent(DatabaseObjectDisplayType.Script);
        }

        private void ShowContent(DatabaseObjectDisplayInfo info)
        {
            if (this.OnShowContent != null)
            {
                this.OnShowContent(info);
            }
        }

        private void ShowContent(DatabaseObjectDisplayType displayType, bool isNew = true)
        {
            DatabaseObjectDisplayInfo info = new DatabaseObjectDisplayInfo() { IsNew = isNew, DisplayType = displayType, DatabaseType = this.DatabaseType };

            if (!isNew)
            {
                DatabaseObject dbObject = this.tvDbObjects.SelectedNode.Tag as DatabaseObject;

                if (dbObject != null)
                {
                    info.DatabaseObject = dbObject;
                    info.Schema = dbObject.Schema;
                    info.Name = dbObject.Name;
                }
            }

            info.ConnectionInfo = this.GetCurrentConnectionInfo();

            this.ShowContent(info);
        }

        private void tsmiNewView_Click(object sender, EventArgs e)
        {
            this.DoScript(DatabaseObjectType.View, ScriptAction.CREATE);
        }

        private void tsmiNewFunction_Click(object sender, EventArgs e)
        {
            this.DoScript(DatabaseObjectType.Function, ScriptAction.CREATE);
        }

        private void tsmiNewProcedure_Click(object sender, EventArgs e)
        {
            this.DoScript(DatabaseObjectType.Procedure, ScriptAction.CREATE);
        }

        private void tsmiNewTrigger_Click(object sender, EventArgs e)
        {
            this.DoScript(DatabaseObjectType.Trigger, ScriptAction.CREATE);
        }

        private void DoScript(DatabaseObjectType databaseObjectType, ScriptAction scriptAction)
        {
            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.databaseType, new ConnectionInfo(), new DbInterpreterOption());

            ScriptTemplate scriptTemplate = new ScriptTemplate(dbInterpreter);

            DatabaseObjectDisplayInfo displayInfo = this.GetDisplayInfo();
            displayInfo.IsNew = true;

            DatabaseObject dbObj = null;

            if (databaseObjectType == DatabaseObjectType.Trigger)
            {
                dbObj = this.GetSelectedNode().Parent?.Tag as Table;
            }

            displayInfo.Content = scriptTemplate.GetTemplateContent(databaseObjectType, scriptAction, dbObj);
            displayInfo.ScriptAction = scriptAction;

            this.ShowContent(displayInfo);
        }

        private void tsmiAlter_Click(object sender, EventArgs e)
        {
            this.GenerateScripts(ScriptAction.ALTER);
        }

        private void tsmiNewTable_Click(object sender, EventArgs e)
        {
            this.ShowContent(DatabaseObjectDisplayType.TableDesigner);
        }

        private void tsmiDesign_Click(object sender, EventArgs e)
        {
            this.ShowContent(DatabaseObjectDisplayType.TableDesigner, false);
        }

        private async void RefreshFolderNode()
        {
            TreeNode node = this.tvDbObjects.SelectedNode;

            if (node == null)
            {
                return;
            }

            if (node.Tag is DatabaseObject && node.Parent != null && this.CanRefresh(node.Parent))
            {
                string selectedName = node.Name;

                TreeNode parentNode = node.Parent;

                await this.LoadChildNodes(parentNode);

                foreach (TreeNode child in parentNode.Nodes)
                {
                    if (child.Name == selectedName)
                    {
                        this.tvDbObjects.SelectedNode = child;
                        break;
                    }
                }

                this.tvDbObjects.SelectedNode = parentNode;
            }
            else if (!(node.Tag is DatabaseObject) && this.CanRefresh(node))
            {
                this.tvDbObjects.SelectedNode = node;

                await this.LoadChildNodes(node);
            }
        }

        private async void tsmiBackup_Click(object sender, EventArgs e)
        {
            ConnectionInfo connectionInfo = this.GetCurrentConnectionInfo();

            DbManager dbManager = new DbManager();

            dbManager.Subscribe(this);

            Action<BackupSetting> backup = (setting) =>
            {
                bool success = dbManager.Backup(setting, connectionInfo);

                if (success)
                {
                    MessageBox.Show("Backup finished.");
                }
            };

            frmBackupSettingRedefine form = new frmBackupSettingRedefine() { DatabaseType = this.databaseType };

            if (form.ShowDialog() == DialogResult.OK)
            {
                await Task.Run(() => backup(form.Setting));
            }
        }

        private void tsmiDiagnose_Click(object sender, EventArgs e)
        {
            ConnectionInfo connectionInfo = this.GetCurrentConnectionInfo();

            frmDiagnose form = new frmDiagnose();
            form.DatabaseType = this.databaseType;
            form.ConnectionInfo = connectionInfo;

            if (this.databaseType == DatabaseType.Oracle)
            {
                form.Schema = this.GetDatabaseNode(this.GetSelectedNode()).Name;
            }

            form.Init(this);
            form.ShowDialog();
        }

        private void tsmiCopy_Click(object sender, EventArgs e)
        {
            frmTableCopy form = new frmTableCopy()
            {
                DatabaseType = this.databaseType,
                ConnectionInfo = this.GetCurrentConnectionInfo(),
                Table = this.tvDbObjects.SelectedNode.Tag as Table
            };

            form.OnFeedback += this.Feedback;

            form.ShowDialog();
        }

        private void tsmiCompare_Click(object sender, EventArgs e)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            this.CompareDatabase(node);
        }

        private void CompareDatabase(TreeNode node)
        {
            Database database = node.Tag as Database;

            frmCompare frmCompare = new frmCompare(this.databaseType, this.GetConnectionInfo(database.Name));
            frmCompare.ShowDialog();
        }

        private void tsmiCreateScript_Click(object sender, EventArgs e)
        {
            this.GenerateScripts(ScriptAction.CREATE);
        }

        private void tsmiSelectScript_Click(object sender, EventArgs e)
        {
            this.GenerateScripts(ScriptAction.SELECT);
        }

        private void tsmiInsertScript_Click(object sender, EventArgs e)
        {
            this.GenerateScripts(ScriptAction.INSERT);
        }

        private void tsmiUpdateScript_Click(object sender, EventArgs e)
        {
            this.GenerateScripts(ScriptAction.UPDATE);
        }

        private void tsmiDeleteScript_Click(object sender, EventArgs e)
        {
            this.GenerateScripts(ScriptAction.DELETE);
        }

        private void tsmiViewDependency_Click(object sender, EventArgs e)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            var tag = node.Tag;
            Database database = null;

            if (tag is Database)
            {
                database = tag as Database;

                frmTableDependency tableDependency = new frmTableDependency(this.databaseType, this.GetConnectionInfo(database.Name), null);

                tableDependency.Show();
            }
            else if (tag is DatabaseObject dbObj)
            {
                database = this.GetDatabaseNode(node).Tag as Database;

                frmDbObjectDependency dbOjectDependency = new frmDbObjectDependency(this.databaseType, this.GetConnectionInfo(database.Name), dbObj);

                dbOjectDependency.Show();
            }
        }

        private void tsmiCopyChildrenNames_Click(object sender, EventArgs e)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            if (node != null)
            {
                var dbObjects = node.Nodes.Cast<TreeNode>().Select(item => item.Tag as DatabaseObject);

                bool isUniqueSchema = dbObjects.GroupBy(item => item.Schema).Count() == 1;

                var names = dbObjects.Select(item => (isUniqueSchema ? item.Name : $"{item.Schema}.{item.Name}"));

                string content = string.Join(Environment.NewLine, names);

                frmTextContent frm = new frmTextContent(content);
                frm.Show();
            }
        }

        private void tsmiExecuteScript_Click(object sender, EventArgs e)
        {
            this.GenerateScripts(ScriptAction.EXECUTE);
        }

        private async void tsmiStatistic_Click(object sender, EventArgs e)
        {
            ConnectionInfo connectionInfo = this.GetCurrentConnectionInfo();

            DbStatistic statistic = new DbStatistic(this.databaseType, connectionInfo);

            statistic.OnFeedback += this.OnFeedback;

            IEnumerable<TableRecordCount> records = await statistic.CountTableRecords();

            frmTableRecordCount form = new frmTableRecordCount();            

            form.LoadData(records);

            form.ShowDialog();
        }
    }
}