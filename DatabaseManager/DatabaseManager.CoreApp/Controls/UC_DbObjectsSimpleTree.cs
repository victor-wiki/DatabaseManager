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
using DatabaseManager.Forms;
using System.IO.Packaging;

namespace DatabaseManager.Controls
{
    public partial class UC_DbObjectsSimpleTree : UserControl
    {
        public UC_DbObjectsSimpleTree()
        {
            InitializeComponent();
            TreeView.CheckForIllegalCrossThreadCalls = false;
        }

        public async Task LoadTree(DatabaseType dbType, ConnectionInfo connectionInfo)
        {
            this.tvDbObjects.Nodes.Clear();

            DbInterpreterOption option = new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple };

            DatabaseObjectType databaseObjectType = DbObjectsTreeHelper.DefaultObjectType;

            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(dbType, connectionInfo, option);
            SchemaInfoFilter filter = new SchemaInfoFilter() { DatabaseObjectType = databaseObjectType };

            SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

            this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(UserDefinedType), "User Defined Types", schemaInfo.UserDefinedTypes);
            this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(Sequence), "Sequences", schemaInfo.Sequences);
            this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(Function), "Functions", schemaInfo.Functions);
            this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(Table), "Tables", schemaInfo.Tables);
            this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(DatabaseInterpreter.Model.View), "Views", schemaInfo.Views);
            this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(Procedure), "Procedures", schemaInfo.Procedures);
            this.tvDbObjects.Nodes.AddDbObjectFolderNode(nameof(TableTrigger), "Triggers", schemaInfo.TableTriggers);

            if (this.tvDbObjects.Nodes.Count == 1)
            {
                this.tvDbObjects.ExpandAll();
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
    }
}
