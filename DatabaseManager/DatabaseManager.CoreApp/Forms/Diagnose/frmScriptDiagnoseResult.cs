using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmScriptDiagnoseResult : Form
    {
        private bool isRemovingTreeNode = false;
        private List<ScriptDiagnoseResult> results;
        public DatabaseType DatabaseType { get; set; }
        public ConnectionInfo ConnectionInfo { get; set; }
        public ScriptDiagnoseType DiagnoseType { get; set; }

        public frmScriptDiagnoseResult()
        {
            InitializeComponent();
        }

        private void frmScriptDiagnoseResult_Load(object sender, EventArgs e)
        {

        }

        public void LoadResults(List<ScriptDiagnoseResult> results)
        {
            this.results = results;

            this.LoadTree(results);
        }

        private void LoadTree(List<ScriptDiagnoseResult> results)
        {
            var views = results.Where(item => item.DbObject is DatabaseInterpreter.Model.View).Select(item => item.DbObject as DatabaseInterpreter.Model.View);
            var functions = results.Where(item => item.DbObject is Function).Select(item => item.DbObject as Function);
            var procedures = results.Where(item => item.DbObject is Procedure).Select(item => item.DbObject as Procedure);

            if (views.Count() > 0)
            {
                this.AddTreeNodes("Views", views);
            }

            if (functions.Count() > 0)
            {
                this.AddTreeNodes("Functions", functions);
            }

            if (procedures.Count() > 0)
            {
                this.AddTreeNodes("Procedures", procedures);
            }

            if (this.tvDbObjects.Nodes.Count == 1)
            {
                this.tvDbObjects.ExpandAll();

                if (this.tvDbObjects.Nodes[0].Nodes.Count == 1)
                {
                    this.tvDbObjects.SelectedNode = this.tvDbObjects.Nodes[0].Nodes[0];
                }
            }
        }

        private void AddTreeNodes(string folderName, IEnumerable<ScriptDbObject> dbObjects)
        {
            var viewFolderNode = DbObjectsTreeHelper.CreateFolderNode(folderName, folderName);

            this.tvDbObjects.Nodes.Add(viewFolderNode);

            DbObjectsTreeHelper.AddDbObjectNodes(viewFolderNode, dbObjects);
        }

        private void tvDbObjects_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this.isRemovingTreeNode)
            {
                return;
            }

            TreeNode node = this.tvDbObjects.SelectedNode;

            if (node != null && node.Tag is ScriptDbObject dbObject)
            {
                this.ShowResultDetails(dbObject);
            }
        }

        private void ShowResultDetails(ScriptDbObject dbObject)
        {
            var result = this.results.FirstOrDefault(item => item.DbObject == dbObject);

            this.dgvResultDetails.Rows.Clear();

            if (result != null)
            {
                var details = result.Details;

                foreach (var detail in details.OrderBy(item => item.Index))
                {
                    int rowIndex = this.dgvResultDetails.Rows.Add();

                    DataGridViewRow row = this.dgvResultDetails.Rows[rowIndex];

                    row.Cells[this.colObjectType.Name].Value = detail.ObjectType.ToString();
                    row.Cells[this.colName.Name].Value = detail.Name;
                    row.Cells[this.colInvalidName.Name].Value = detail.InvalidName;

                    row.Tag = detail;
                }

                this.dgvResultDetails.ClearSelection();

                this.txtDefinition.Text = dbObject.Definition;

                RichTextBoxHelper.HighlightingWord(this.txtDefinition, details.Select(item => new WordMatchInfo() { Index = item.Index, Length = item.InvalidName.Length }), Color.Yellow);
            }
        }

        private void dgvResult_SelectionChanged(object sender, EventArgs e)
        {
            DataGridViewRow row = DataGridViewHelper.GetSelectedRow(this.dgvResultDetails);

            if (row != null && row.Tag != null && this.txtDefinition.Text.Length > 0)
            {
                var detail = row.Tag as ScriptDiagnoseResultDetail;

                this.txtDefinition.SelectionStart = detail.Index;
                this.txtDefinition.SelectionLength = detail.InvalidName.Length;

                this.txtDefinition.ScrollToCaret();
            }
        }

        private void dgvResultDetails_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvResultDetails.ClearSelection();
        }

        private async void btnCorrect_Click(object sender, EventArgs e)
        {
            TreeNode node = this.tvDbObjects.SelectedNode;

            if (node == null)
            {
                MessageBox.Show("Please select a tree node.");
                return;
            }
            else if (node.Tag == null)
            {
                MessageBox.Show("Please select a valid tree node.");
                return;
            }

            this.btnCorrect.Enabled = false;

            var dbObject = node.Tag as ScriptDbObject;

            var result = this.results.FirstOrDefault(item => item.DbObject == dbObject);

            if (result != null)
            {
                await this.CorrectScripts(new ScriptDiagnoseResult[] { result });
            }

            this.btnCorrect.Enabled = true;
        }

        private async void btnCorrectAll_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure to correct all of the scripts?", "Confirm", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                this.btnCorrectAll.Enabled = false;

                await this.CorrectScripts(this.results);

                this.btnCorrectAll.Enabled = true;
            }
        }

        private async Task CorrectScripts(IEnumerable<ScriptDiagnoseResult> results)
        {
            try
            {
                DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, this.ConnectionInfo, new DbInterpreterOption());

                ScriptCorrector scriptCorrector = new ScriptCorrector(dbInterpreter);

                if (this.DiagnoseType == ScriptDiagnoseType.NameNotMatch || this.DiagnoseType == ScriptDiagnoseType.ViewColumnAliasWithoutQuotationChar)
                {
                    results = await scriptCorrector.CorrectNotMatchNames(this.DiagnoseType, results);
                }

                MessageBox.Show("Script has been corrected.");

                this.RemoveNodesAfterCorrected(results);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex));
            }
            finally
            {
                this.isRemovingTreeNode = false;
                this.btnCorrect.Enabled = true;
                this.btnCorrectAll.Enabled = true;
            }
        }

        private void RemoveNodesAfterCorrected(IEnumerable<ScriptDiagnoseResult> results)
        {
            var nodes = this.GetDbObjectTreeNodes();
            TreeNode selectedNode = this.tvDbObjects.SelectedNode;

            int count = 0;

            this.isRemovingTreeNode = true;

            foreach (var node in nodes)
            {
                var dbObject = node.Tag as ScriptDbObject;
                var result = results.FirstOrDefault(item => item.DbObject == dbObject);

                if (result != null)
                {
                    if (node == selectedNode)
                    {
                        this.txtDefinition.Clear();
                        this.dgvResultDetails.Rows.Clear();
                    }

                    node.Parent.Nodes.Remove(node);

                    count++;

                    var res = this.results.FirstOrDefault(item => item.DbObject == dbObject);

                    if (res != null)
                    {
                        this.results.Remove(res);
                    }
                }
            }

            if (count == results.Count())
            {
                TreeNode node = this.tvDbObjects.SelectedNode;

                if (node == null || node.Tag is null || this.results.Count == 0)
                {
                    this.txtDefinition.Clear();
                }
            }

            this.isRemovingTreeNode = false;
        }

        private List<TreeNode> GetDbObjectTreeNodes()
        {
            List<TreeNode> nodes = new List<TreeNode>();

            TreeNodeCollection folderNodes = this.tvDbObjects.Nodes;

            foreach (TreeNode node in folderNodes)
            {
                nodes.AddRange(node.Nodes.Cast<TreeNode>());
            }

            return nodes;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
