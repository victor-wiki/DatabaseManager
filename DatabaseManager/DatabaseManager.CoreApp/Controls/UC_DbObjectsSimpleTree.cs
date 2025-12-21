using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Forms;
using DatabaseManager.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public delegate void TreeNodeSelectedHandler(object sender, TreeViewEventArgs e);

    public partial class UC_DbObjectsSimpleTree : UserControl
    {
        private DbInterpreter dbInterpreter;

        public bool ShowCheckBox
        {
            get
            {
                return this.tvDbObjects.CheckBoxes;
            }

            set
            {
                this.tvDbObjects.CheckBoxes = value;
            }
        }

        public TreeNodeSelectedHandler TreeNodeSelected;

        public UC_DbObjectsSimpleTree()
        {
            InitializeComponent();

            TreeView.CheckForIllegalCrossThreadCalls = false;
        }

        public async Task LoadTree(DatabaseType dbType, ConnectionInfo connectionInfo, bool onlyShowTables = false)
        {
            this.tvDbObjects.Nodes.Clear();

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

            DatabaseObjectType databaseObjectType = DbObjectsTreeHelper.DefaultObjectType;

            if (onlyShowTables)
            {
                databaseObjectType = DatabaseObjectType.Table;
            }

            this.dbInterpreter = DbInterpreterHelper.GetDbInterpreter(dbType, connectionInfo, option);
            SchemaInfoFilter filter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType };

            SchemaInfo schemaInfo = await this.dbInterpreter.GetSchemaInfoAsync(filter);

            this.LoadTree(schemaInfo, onlyShowTables);
        }

        public void LoadTree(SchemaInfo schemaInfo, bool onlyShowTables = false)
        {
            if (!onlyShowTables)
            {
                this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(UserDefinedType), "User Defined Types", schemaInfo.UserDefinedTypes, this.NeedShowSchema(schemaInfo.UserDefinedTypes));
                this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(Sequence), "Sequences", schemaInfo.Sequences, this.NeedShowSchema(schemaInfo.Sequences));
                this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(Function), "Functions", schemaInfo.Functions, this.NeedShowSchema(schemaInfo.Functions));
            }

            this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(Table), "Tables", schemaInfo.Tables, this.NeedShowSchema(schemaInfo.Tables));

            if (!onlyShowTables)
            {
                this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(DatabaseInterpreter.Model.View), "Views", schemaInfo.Views, this.NeedShowSchema(schemaInfo.Views));
                this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(Procedure), "Procedures", schemaInfo.Procedures, this.NeedShowSchema(schemaInfo.Procedures));
                this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(TableTrigger), "Triggers", schemaInfo.TableTriggers, this.NeedShowSchema(schemaInfo.TableTriggers));
            }

            if (this.tvDbObjects.Nodes.Count == 1)
            {
                this.tvDbObjects.ExpandAll();

                if (onlyShowTables && this.tvDbObjects.Nodes[0].Nodes.Count > 0 && this.ShowCheckBox == false)
                {
                    this.tvDbObjects.SelectedNode = this.tvDbObjects.Nodes[0].Nodes[0];
                }
            }
        }

        private bool NeedShowSchema(IEnumerable<DatabaseObject> dbObjects)
        {
            if(this.dbInterpreter == null)
            {
                bool isUniqueSchema = dbObjects.GroupBy(item => item.Schema).Count() == 1;

                return !isUniqueSchema;
            }
            else
            {
                return DbObjectsTreeHelper.NeedShowSchema(this.dbInterpreter, dbObjects);
            }
        }

        public void ClearNodes()
        {
            this.tvDbObjects.Nodes.Clear();
        }

        private void tvDbObjects_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count > 0)
            {
                foreach (TreeNode node in e.Node.Nodes)
                {
                    node.Checked = e.Node.Checked;
                }                
            }           
        }

        public SchemaInfo GetSchemaInfo()
        {
            SchemaInfo schemaInfo = new SchemaInfo();
            foreach (TreeNode node in this.tvDbObjects.Nodes)
            {
                foreach (TreeNode item in node.Nodes)
                {
                    if (item.Checked)
                    {
                        switch (node.Name)
                        {
                            case nameof(UserDefinedType):
                                schemaInfo.UserDefinedTypes.Add(item.Tag as UserDefinedType);
                                break;
                            case nameof(Sequence):
                                schemaInfo.Sequences.Add(item.Tag as Sequence);
                                break;
                            case nameof(Table):
                                schemaInfo.Tables.Add(item.Tag as Table);
                                break;
                            case nameof(DatabaseInterpreter.Model.View):
                                schemaInfo.Views.Add(item.Tag as DatabaseInterpreter.Model.View);
                                break;
                            case nameof(Function):
                                schemaInfo.Functions.Add(item.Tag as Function);
                                break;
                            case nameof(Procedure):
                                schemaInfo.Procedures.Add(item.Tag as Procedure);
                                break;
                            case nameof(TableTrigger):
                                schemaInfo.TableTriggers.Add(item.Tag as TableTrigger);
                                break;
                        }
                    }
                }
            }

            return schemaInfo;
        }

        public bool HasDbObjectNodeSelected()
        {
            foreach (TreeNode node in this.tvDbObjects.Nodes)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    if (child.Checked)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void tsmiShowSortedNames_Click(object sender, EventArgs e)
        {
            TreeNode node = this.contextMenuStrip1.Tag as TreeNode;

            if (node != null)
            {
                var dbObjects = node.Nodes.Cast<TreeNode>().Select(item => item.Tag as DatabaseObject).OrderBy(item => item.Order);

                bool isUniqueSchema = dbObjects.GroupBy(item => item.Schema).Count() == 1;

                var names = dbObjects.Select(item => (isUniqueSchema ? item.Name : $"{item.Schema}.{item.Name}"));

                string content = string.Join(Environment.NewLine, names);

                frmTextContent frm = new frmTextContent(content);
                frm.Show();
            }
        }

        private void tvDbObjects_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var isIn = e.Node.Bounds.Contains(new Point(e.X, e.Y));

                if (isIn)
                {
                    if (e.Node.Parent == null)
                    {
                        this.contextMenuStrip1.Show(Cursor.Position);

                        this.tvDbObjects.SelectedNode = e.Node;
                        this.contextMenuStrip1.Tag = e.Node;
                    }
                }
            }
        }

        private void tvDbObjects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.F)
                {
                    if (this.tvDbObjects.SelectedNode != null)
                    {
                        this.FindChild();
                    }
                }
            }
        }

        private void FindChild()
        {
            var node = this.tvDbObjects.SelectedNode;

            frmFindBox findBox = new frmFindBox();

            DialogResult result = findBox.ShowDialog();

            if (result == DialogResult.OK)
            {
                string word = findBox.FindWord;

                var nodes = node.Nodes.Count == 0 ? node.Parent.Nodes : node.Nodes;

                TreeNode foundNode = this.FindTreeNode(nodes, word);

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

        private TreeNode FindTreeNode(TreeNodeCollection nodes, string word)
        {
            foreach (TreeNode node in nodes)
            {
                object tag = node.Tag;

                if (node.Tag != null)
                {
                    string text = node.Text.Split('.').LastOrDefault()?.Trim();

                    if (text.ToUpper() == word.ToUpper())
                    {
                        return node;
                    }
                }
            }

            return null;
        }

        private void tvDbObjects_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.TreeNodeSelected != null)
            {
                this.TreeNodeSelected(sender, e);
            }
        }

        public void CheckRootNodes()
        {
            var rootNodes = this.tvDbObjects.Nodes;

            foreach (TreeNode node in rootNodes)
            {
                this.CheckNode(node);
            }
        }


        public void CheckNode(TreeNode node)
        {
            if (node != null)
            {
                node.Checked = true;
            }
        }       
    }
}
