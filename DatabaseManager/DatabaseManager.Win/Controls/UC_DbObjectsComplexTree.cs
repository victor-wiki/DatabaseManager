using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Core;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;

namespace DatabaseManager.Controls
{
    public delegate void ShowDbObjectContentHandler(DatabaseObjectDisplayInfo content);
    public partial class UC_DbObjectsComplexTree : UserControl
    {
        private DatabaseType databaseType;
        private ConnectionInfo connectionInfo;
        private DbInterpreterOption simpleInterpreterOption = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

        public ShowDbObjectContentHandler OnShowContent;

        public UC_DbObjectsComplexTree()
        {
            InitializeComponent();
            TreeView.CheckForIllegalCrossThreadCalls = false;
        }

        public async Task LoadTree(DatabaseType dbType, ConnectionInfo connectionInfo)
        {
            this.databaseType = dbType;
            this.connectionInfo = connectionInfo;

            this.tvDbObjects.Nodes.Clear();

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(dbType, connectionInfo, simpleInterpreterOption);

            List<Database> databases = await dbInterpreter.GetDatabasesAsync();

            foreach (Database database in databases)
            {
                TreeNode node = DbObjectsTreeHelper.CreateTreeNode(database, true);

                this.tvDbObjects.Nodes.Add(node);
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
                this.tvDbObjects.SelectedNode = e.Node;

                this.SetMenuItemVisible(e.Node);

                this.contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void SetMenuItemVisible(TreeNode node)
        {
            this.tsmiRefresh.Visible = node.Level <= 2 && node.Nodes.Count > 0 && !this.IsOnlyHasFakeChild(node);
            this.tsmiGenerateScripts.Visible = node.Level == 0 || node.Level == 2 || (node.Level==4 && node.Tag is Trigger);
            this.tsmiConvert.Visible = node.Level == 0;
        }

        private ConnectionInfo GetConnectionInfo(string database)
        {
            ConnectionInfo info = ObjectHelper.CloneObject<ConnectionInfo>(this.connectionInfo);
            info.Database = database;
            return info;
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
            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.databaseType, connectionInfo, isSimpleMode ? simpleInterpreterOption : new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Details });
            return dbInterpreter;
        }

        private async Task AddDatabaseNodes(TreeNode parentNode, string database, DatabaseObjectType databaseObjectType = DatabaseObjectType.None, bool createFolderNode = true)
        {
            DbInterpreter dbInterpreter = this.GetDbInterpreter(database);

            SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(new SelectionInfo(), databaseObjectType);

            this.AddTreeNodes(parentNode, databaseObjectType, DatabaseObjectType.Table, schemaInfo.Tables, createFolderNode, true);
            this.AddTreeNodes(parentNode, databaseObjectType, DatabaseObjectType.View, schemaInfo.Views, createFolderNode);
            this.AddTreeNodes(parentNode, databaseObjectType, DatabaseObjectType.Function, schemaInfo.Functions, createFolderNode);
            this.AddTreeNodes(parentNode, databaseObjectType, DatabaseObjectType.Procedure, schemaInfo.Procedures, createFolderNode);

            parentNode.Expand();
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

        private async Task AddTableObjectNodes(TreeNode tableNode, Table table)
        {
            string database = this.GetDatabaseNode(tableNode).Name;
            DbInterpreter dbInterpreter = this.GetDbInterpreter(database, false);

            DatabaseObjectType databaseObjectType = DatabaseObjectType.Trigger | DatabaseObjectType.TableColumn | DatabaseObjectType.TablePrimaryKey | DatabaseObjectType.TableForeignKey | DatabaseObjectType.TableIndex;

            SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(new SelectionInfo() { TableNames = new string[] { table.Name } }, databaseObjectType);

            #region Columns           
            TreeNode columnRootNode = DbObjectsTreeHelper.CreateFolderNode("Columns", "Columns");

            foreach (TableColumn column in schemaInfo.TableColumns)
            {
                string text = this.GetColumnText(dbInterpreter, table, column);
                bool isPrimaryKey = schemaInfo.TablePrimaryKeys.Any(item => item.ColumnName == column.Name);
                bool isForeignKey = schemaInfo.TableForeignKeys.Any(item => item.ColumnName == column.Name);
                string imageKeyName = isPrimaryKey ? nameof(TablePrimaryKey) : (isForeignKey ? nameof(TableForeignKey) : nameof(TableColumn));

                TreeNode node = DbObjectsTreeHelper.CreateTreeNode(column.Name, text, imageKeyName);

                columnRootNode.Nodes.Add(node);
            }

            tableNode.Nodes.Add(columnRootNode);
            #endregion           

            tableNode.AddDbObjectFolderNode(schemaInfo.Triggers);

            #region Indexes
            if (schemaInfo.TableIndexes.Any())
            {
                TreeNode indexRootNode = DbObjectsTreeHelper.CreateFolderNode("Indexes", "Indexes");

                foreach (TableIndex index in schemaInfo.TableIndexes)
                {
                    bool isPrimaryKey = schemaInfo.TablePrimaryKeys.Any(item => item.ColumnName == index.ColumnName);
                    string text = $"{index.Name}{(index.IsUnique ? "(Unique)" : "")}";
                    string imageKeyName = isPrimaryKey ? nameof(TablePrimaryKey) : nameof(TableIndex);

                    TreeNode node = DbObjectsTreeHelper.CreateTreeNode(index.Name, text, imageKeyName);

                    indexRootNode.Nodes.Add(node);
                }

                tableNode.Nodes.Add(indexRootNode);
            }
            #endregion

            if (schemaInfo.TablePrimaryKeys.Any() || schemaInfo.TablePrimaryKeys.Any())
            {
                TreeNode keyRootNode = DbObjectsTreeHelper.CreateFolderNode("Keys", "Keys");
                foreach (TablePrimaryKey key in schemaInfo.TablePrimaryKeys)
                {
                    TreeNode node = DbObjectsTreeHelper.CreateTreeNode(key);
                    keyRootNode.Nodes.Add(node);
                }

                foreach (TableForeignKey key in schemaInfo.TableForeignKeys)
                {
                    TreeNode node = DbObjectsTreeHelper.CreateTreeNode(key);
                    keyRootNode.Nodes.Add(node);
                }

                tableNode.Nodes.Add(keyRootNode);
            }

            tableNode.Expand();
        }

        private string GetColumnText(DbInterpreter dbInterpreter, Table table, TableColumn column)
        {
            string text = dbInterpreter.ParseColumn(table, column).Replace(dbInterpreter.QuotationLeftChar.ToString(), "").Replace(dbInterpreter.QuotationRightChar.ToString(), "");

            return $"{column.Name} ({text.Replace(column.Name + " ", "").ToLower()})";
        }

        private async void tvDbObjects_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;

            if (!this.IsOnlyHasFakeChild(node))
            {
                return;
            }

            await this.LoadChildNodes(node);
        }

        private async Task LoadChildNodes(TreeNode node)
        {
            node.Nodes.Clear();

            object tag = node.Tag;

            if (tag is Database)
            {
                Database database = tag as Database;
                await this.AddDatabaseNodes(node, database.Name, DbObjectsTreeHelper.DefaultObjectType | DatabaseObjectType.Function | DatabaseObjectType.Procedure);
            }
            else if (tag is Table)
            {
                Table table = tag as Table;
                await this.AddTableObjectNodes(node, table);
            }
            else if (tag == null)
            {
                string databaseName = this.GetDatabaseNode(node).Name;
                string name = node.Name;

                DatabaseObjectType databaseObjectType = DbObjectsTreeHelper.GetDbObjectTypeByFolderName(name);

                if (databaseObjectType != DatabaseObjectType.None)
                {
                    await this.AddDatabaseNodes(node, databaseName, databaseObjectType, false);
                }
            }
        }

        private async void tsmiRefresh_Click(object sender, EventArgs e)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            await this.LoadChildNodes(this.GetSelectedNode());
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

        private async void tsmiGenerateScripts_Click(object sender, EventArgs e)
        {
            if (!this.IsValidSelectedNode())
            {
                return;
            }

            TreeNode node = this.GetSelectedNode();

            await this.GenerateScripts(node);
        }

        private async Task GenerateScripts(TreeNode node)
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

                await this.GenerateObjectScript(databaseName, tag as DatabaseObject);
            }
        }

        private async Task GenerateObjectScript(string database, DatabaseObject obj)
        {
            string typeName = obj.GetType().Name;
            DbInterpreter dbInterpreter = this.GetDbInterpreter(database, false);

            SelectionInfo selectionInfo = new SelectionInfo();
            selectionInfo.GetType().GetProperty($"{typeName}Names").SetValue(selectionInfo, new string[] { obj.Name });

            DatabaseObjectType databaseObjectType = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), typeName);          

            SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(selectionInfo, databaseObjectType);
            string script = dbInterpreter.GenerateSchemaScripts(schemaInfo);

            if (this.OnShowContent != null)
            {
                this.OnShowContent(new DatabaseObjectDisplayInfo() { Name = obj.Name, DatabaseType = this.databaseType, Content= script, ConnectionInfo = dbInterpreter.ConnectionInfo });
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
    }
}
