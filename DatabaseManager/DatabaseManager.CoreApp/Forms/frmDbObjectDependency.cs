using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using View = DatabaseInterpreter.Model.View;

namespace DatabaseManager.Forms
{
    public partial class frmDbObjectDependency : Form
    {
        private DatabaseType databaseType;
        private ConnectionInfo connectionInfo;
        private DatabaseObject dbObject;
        private DbInterpreter dbInterpreter;
        private bool hasStyled = false;

        public frmDbObjectDependency()
        {
            InitializeComponent();
        }

        public frmDbObjectDependency(DatabaseType databaseType, ConnectionInfo connectionInfo, DatabaseObject dbObject)
        {
            InitializeComponent();

            this.databaseType = databaseType;
            this.connectionInfo = connectionInfo;
            this.dbObject = dbObject;

            this.dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.databaseType, this.connectionInfo, new DbInterpreterOption() { ObjectFetchMode = DatabaseObjectFetchMode.Simple });
        }

        private void frmDbObjectDependency_Load(object sender, EventArgs e)
        {
            this.InitControls();

            this.ShowDependencies();
        }

        private void InitControls()
        {
            this.rbDependOnThis.Text = this.RelaceNamePlaceHolder(this.rbDependOnThis.Text);
            this.rbThisDependOn.Text = this.RelaceNamePlaceHolder(this.rbThisDependOn.Text);
        }

        private string RelaceNamePlaceHolder(string text)
        {
            return text.Replace("$Name$", this.dbInterpreter.GetQuotedDbObjectNameWithSchema(this.dbObject));
        }

        private async void ShowDependencies()
        {
            if (this.tvDependencies.Nodes.Count == 1)
            {
                this.tvDependencies.Nodes[0].Nodes.Clear();
            }

            DepencencyFetcher fetcher = new DepencencyFetcher(this.dbInterpreter);

            var usages = await fetcher.Fetch(this.dbObject, this.rbDependOnThis.Checked);

            this.AddTreeNodes(this.dbObject, usages);
        }

        private void AddTreeNodes(DatabaseObject dbObject, List<DbObjectUsage> usages)
        {
            TreeNode rootNode = null;

            if (this.tvDependencies.Nodes.Count == 1)
            {
                rootNode = this.tvDependencies.Nodes[0];
            }
            else
            {
                rootNode = DbObjectsTreeHelper.CreateTreeNode(dbObject);
                rootNode.Tag = dbObject;

                this.tvDependencies.Nodes.Add(rootNode);
            }

            this.AddChildNodes(rootNode, usages);

            this.tvDependencies.SelectedNode = rootNode;
            rootNode.Expand();
        }

        private void AddChildNodes(TreeNode parentNode, List<DbObjectUsage> usages)
        {
            parentNode.Nodes.Clear();

            foreach (var usage in usages)
            {
                TreeNode node = null;

                if (this.rbDependOnThis.Checked)
                {
                    node = DbObjectsTreeHelper.CreateTreeNode(usage.ObjectName, usage.ObjectName, this.GetImageKey(usage));
                }
                else
                {
                    node = DbObjectsTreeHelper.CreateTreeNode(usage.RefObjectSchema, usage.RefObjectName, this.GetImageKey(usage));
                }

                node.Tag = usage;

                bool isSelfReference = this.IsSelfReferenceTable(dbObject, usage);

                if (!isSelfReference)
                {
                    if (this.rbDependOnThis.Checked)
                    {
                        node.Nodes.Add(DbObjectsTreeHelper.CreateFakeNode());
                    }
                }

                parentNode.Nodes.Add(node);
            }
        }

        private string GetImageKey(DbObjectUsage usage)
        {
            string objectType = this.rbDependOnThis.Checked ? usage.ObjectType : usage.RefObjectType;

            return objectType;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tvDependencies_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.txtObjectType.Text = "";
            this.txtObjectName.Text = "";

            TreeNode node = this.tvDependencies.SelectedNode;

            if (node == null || node.Tag == null)
            {
                return;
            }

            object tag = node.Tag;

            if (tag is DatabaseObject dbObj)
            {
                this.txtObjectType.Text = DbObjectHelper.GetDatabaseObjectType(dbObj).ToString();
                this.txtObjectName.Text = this.dbInterpreter.GetQuotedDbObjectNameWithSchema(dbObj);
            }
            else if (tag is DbObjectUsage usage)
            {
                if (this.rbDependOnThis.Checked)
                {
                    this.txtObjectType.Text = usage.ObjectType;

                    this.txtObjectName.Text = this.GetDbObjectFullName(usage.ObjectSchema, usage.ObjectName);
                }
                else if (this.rbThisDependOn.Checked)
                {
                    this.txtObjectType.Text = usage.RefObjectType;

                    this.txtObjectName.Text = this.GetDbObjectFullName(usage.RefObjectSchema, usage.RefObjectName);
                }
            }
        }

        private string GetDbObjectFullName(string schema, string name)
        {
            return this.dbInterpreter.GetQuotedDbObjectNameWithSchema(schema, name);
        }

        private void rbDependOnThis_CheckedChanged(object sender, EventArgs e)
        {
            this.ShowDependencies();
        }
        private bool IsSelfReferenceTable(DatabaseObject dbObject, DbObjectUsage usage)
        {
            if (dbObject is Table)
            {
                if (this.rbDependOnThis.Checked)
                {
                    return usage.ObjectSchema == dbObject.Schema && usage.ObjectName == dbObject.Name;
                }
                else
                {
                    return usage.RefObjectSchema == dbObject.Schema && usage.RefObjectName == dbObject.Name;
                }
            }

            return false;
        }

        private void tvDependencies_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;

            if (!this.IsOnlyHasFakeChild(node))
            {
                return;
            }

            this.tvDependencies.BeginInvoke(new Action(async () => await this.LoadChildNodes(node)));
        }

        private async Task LoadChildNodes(TreeNode node)
        {
            this.ShowLoading(node);

            object tag = node.Tag;

            if (tag is DbObjectUsage usage)
            {
                string objectType = usage.ObjectType;

                DatabaseObject dboObj = new DatabaseObject() { Schema = usage.ObjectSchema, Name = usage.ObjectName };

                switch (objectType)
                {
                    case nameof(Table):
                        dboObj = ObjectHelper.CloneObject<Table>(dboObj);
                        break;
                    case nameof(View):
                        dboObj = ObjectHelper.CloneObject<View>(dboObj);
                        break;
                    case nameof(Function):
                        dboObj = ObjectHelper.CloneObject<Function>(dboObj);
                        break;
                    case nameof(Procedure):
                        dboObj = ObjectHelper.CloneObject<Procedure>(dboObj);
                        break;
                    default:
                        return;
                }

                DepencencyFetcher fetcher = new DepencencyFetcher(this.dbInterpreter);

                var usages = await fetcher.Fetch(dboObj, this.rbDependOnThis.Checked);

                this.AddChildNodes(node, usages);
            }
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

        private bool IsOnlyHasFakeChild(TreeNode node)
        {
            if (node.Nodes.Count == 1 && node.Nodes[0].Name == DbObjectsTreeHelper.FakeNodeName)
            {
                return true;
            }

            return false;
        }

        private void tvDependencies_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                this.FindChildren();
            }
        }

        private void FindChildren()
        {
            frmFindBox findBox = new frmFindBox();

            DialogResult result = findBox.ShowDialog();

            if (result == DialogResult.OK)
            {
                string word = findBox.FindWord;

                this.ClearStyles(this.tvDependencies.Nodes);

                this.FindTreeNode(word, this.tvDependencies.Nodes);
            }
        }

        private void FindTreeNode(string word, TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                string text = node.Text.Split('.').LastOrDefault();

                if (text.ToLower() == word.ToLower())
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
                    this.FindTreeNode(word, node.Nodes);
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
    }
}
