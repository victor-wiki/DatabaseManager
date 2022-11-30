using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Helper;
using Newtonsoft.Json.Bson;
using Npgsql.Replication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmTableDependency : Form
    {
        private DatabaseType databaseType;
        private ConnectionInfo connectionInfo;
        private IEnumerable<TreeNode> notReferencedTreeNodes;
        private DatabaseObject dbObject;
        private bool hasStyled = false;

        public frmTableDependency()
        {
            InitializeComponent();
        }

        public frmTableDependency(DatabaseType databaseType, ConnectionInfo connectionInfo, DatabaseObject dbObject)
        {
            InitializeComponent();
            this.databaseType = databaseType;
            this.connectionInfo = connectionInfo;
            this.dbObject = dbObject;
        }

        private void frmDependency_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            this.txtName.Focus();
            this.LoadTree();
        }

        private async void LoadTree()
        {
            this.tvDbObjects.Nodes.Clear();

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

            DatabaseObjectType databaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.ForeignKey;

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.databaseType, connectionInfo, option);

            SchemaInfoFilter filter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType };

            SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

            IEnumerable<Table> tables = schemaInfo.Tables;
            var foreignKeys = schemaInfo.TableForeignKeys;

            bool isUniqueDbSchema = tables.GroupBy(item => item.Schema).Count() == 1;

            var notReferencedTables = schemaInfo.Tables.Where(item => !foreignKeys.Any(fk => fk.ReferencedTableName == item.Name));

            this.notReferencedTreeNodes = DbObjectsTreeHelper.CreateDbObjectNodes(notReferencedTables, !isUniqueDbSchema);

            IEnumerable<string> topReferencedTableNames = TableReferenceHelper.GetTopReferencedTableNames(foreignKeys);

            var topReferencedTables = tables.Where(item => topReferencedTableNames.Any(t => t == item.Name));

            var children = DbObjectsTreeHelper.CreateDbObjectNodes(topReferencedTables, !isUniqueDbSchema).ToArray();

            this.tvDbObjects.Nodes.AddRange(children);

            this.LoadChildNodes(this.tvDbObjects.Nodes.Cast<TreeNode>(), isUniqueDbSchema, tables, foreignKeys);

            if (this.dbObject != null)
            {
                this.txtName.Text = this.dbObject.Name;

                this.LocateNode(this.dbObject.Name);
            }
        }

        private void LoadChildNodes(IEnumerable<TreeNode> nodes, bool isUniqueSchema, IEnumerable<Table> tables, IEnumerable<TableForeignKey> foreignKeys)
        {
            foreach (TreeNode tn in nodes)
            {
                var table = tn.Tag as Table;

                var refTableNames = foreignKeys.Where(item => item.ReferencedTableName == table.Name).Select(item => item.TableName);

                var refTables = tables.Where(item => refTableNames.Any(t => t == item.Name));

                var children = DbObjectsTreeHelper.CreateDbObjectNodes(refTables, !isUniqueSchema).ToArray();

                tn.Nodes.AddRange(children);

                this.LoadChildNodes(children.Where(item => item.Text != tn.Text), isUniqueSchema, tables, foreignKeys);
            }
        }

        private void btnLocate_Click(object sender, EventArgs e)
        {
            string name = this.txtName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please input a name to locate.");
                return;
            }

            this.LocateNode(name);
        }

        private void LocateNode(string name)
        {
            if (this.hasStyled)
            {
                this.ClearStyles(this.tvDbObjects.Nodes);
            }

            this.LocateNode(name, this.tvDbObjects.Nodes);
        }

        private void LocateNode(string name, TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                Table table = node.Tag as Table;

                if (table.Name.ToLower() == name.ToLower())
                {
                    node.BackColor = Color.LightBlue;
                    this.hasStyled = true;

                    if (node.Nodes.Count > 0)
                    {
                        node.EnsureVisible();
                    }
                }
                else
                {
                    this.LocateNode(name, node.Nodes);
                }
            }
        }

        private void ClearStyles(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.BackColor = Color.White;

                this.ClearStyles(node.Nodes);
            }

            this.hasStyled = false;
        }

        private void SetMenuItemVisible()
        {
            this.tsmiExpandChildren.Visible = this.tvDbObjects.SelectedNode != null;
            this.tsmiClearStyles.Visible = this.hasStyled;
        }

        private void tsmiExpandAll_Click(object sender, EventArgs e)
        {
            this.tvDbObjects.ExpandAll();

            TreeNode node = this.tvDbObjects.SelectedNode;

            if (node != null)
            {
                node.EnsureVisible();
            }
        }

        private void tsmiCollapseAll_Click(object sender, EventArgs e)
        {
            this.tvDbObjects.CollapseAll();
        }

        private void chkShowNotReferenced_CheckedChanged(object sender, EventArgs e)
        {
            if (this.notReferencedTreeNodes != null)
            {
                bool show = this.chkShowNotReferenced.Checked;

                var nodes = this.tvDbObjects.Nodes.Cast<TreeNode>();

                if (show)
                {
                    int index = 0;

                    foreach (TreeNode node in notReferencedTreeNodes)
                    {
                        if (!nodes.Any(item => item.Text == node.Text))
                        {
                            this.tvDbObjects.Nodes.Insert(index, node);

                            index++;
                        }
                    }
                }
                else
                {
                    List<int> indexes = new List<int>();

                    int index = 0;

                    foreach (TreeNode node in notReferencedTreeNodes)
                    {
                        if (nodes.Any(item => item.Text == node.Text))
                        {
                            indexes.Add(index);
                        }

                        index++;
                    }

                    indexes.Reverse();

                    indexes.ForEach(item => this.tvDbObjects.Nodes.RemoveAt(item));
                }
            }
        }

        private void txtName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string name = this.txtName.Text.Trim();

                if (!string.IsNullOrEmpty(name))
                {
                    this.LocateNode(name);
                }
            }
        }

        private void tsmiExpandChildren_Click(object sender, EventArgs e)
        {
            var selectedNode = this.tvDbObjects.SelectedNode;

            if (selectedNode != null)
            {
                selectedNode.ExpandAll();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            this.SetMenuItemVisible();
        }

        private void tvDbObjects_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var isIn = e.Node.Bounds.Contains(new Point(e.X, e.Y));

                if (isIn)
                {
                    this.tvDbObjects.SelectedNode = e.Node;
                }
            }
        }

        private void tsmiClearStyles_Click(object sender, EventArgs e)
        {
            this.ClearStyles(this.tvDbObjects.Nodes);
        }
    }
}
