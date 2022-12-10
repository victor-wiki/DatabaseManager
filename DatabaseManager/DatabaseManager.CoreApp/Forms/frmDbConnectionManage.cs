using DatabaseInterpreter.Core;
using DatabaseManager.Profile;
using DatabaseManager.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseInterpreter.Model;
using DatabaseManager.Forms;
using DatabaseManager.Model;

namespace DatabaseManager
{
    public partial class frmDbConnectionManage : Form
    {
        private string actionButtonText = "Manage";
        public bool IsForSelecting { get; set; }
        public DatabaseType DatabaseType { get; set; }

        public AccountProfileInfo SelectedAccountProfileInfo { get; private set; }
        public FileConnectionProfileInfo SelectedFileConnectionProfileInfo { get; private set; }

        public frmDbConnectionManage()
        {
            InitializeComponent();
        }

        public frmDbConnectionManage(DatabaseType databaseType)
        {
            InitializeComponent();
            this.DatabaseType = databaseType;            
        }

        private void frmDbConnectionManage_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.dgvDbConnection.AutoGenerateColumns = false;

            DataGridViewContentAlignment middleCenter = DataGridViewContentAlignment.MiddleCenter;

            this.colIntegratedSecurity.HeaderCell.Style.Alignment = middleCenter;
            this.colProfiles.HeaderCell.Style.Alignment = middleCenter;
            this.colDatabaseVisibility.HeaderCell.Style.Alignment = middleCenter;

            this.btnSelect.Visible = this.IsForSelecting;
            this.panelDbType.Visible = !this.IsForSelecting;
            this.panelOperation.Visible = !this.IsForSelecting;

            if (this.IsForSelecting)
            {
                this.Text = "Select Connection";
                this.dgvDbConnection.MultiSelect = false;
                this.dgvDbConnection.Top = this.panelDbType.Top;
                this.dgvDbConnection.Height += this.panelDbType.Height;

                this.colProfiles.Visible = false;
                this.colDatabaseVisibility.Visible = false;
                this.dgvDbConnection.AutoResizeColumnHeadersHeight();
                this.Width -= (this.colProfiles.Width + this.colDatabaseVisibility.Width -10);
            }

            this.LoadDbTypes();
        }

        public void LoadDbTypes()
        {
            var databaseTypes = DbInterpreterHelper.GetDisplayDatabaseTypes();

            foreach (var value in databaseTypes)
            {
                this.cboDbType.Items.Add(value.ToString());
            }

            if (this.cboDbType.Items.Count > 0)
            {
                if (!this.IsForSelecting)
                {
                    this.cboDbType.SelectedIndex = 0;
                }
                else
                {
                    this.cboDbType.Text = this.DatabaseType.ToString();
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string databaseType = this.cboDbType.Text;

            if (string.IsNullOrEmpty(databaseType))
            {
                MessageBox.Show("Please select a database type first.");
            }
            else
            {
                DatabaseType dbType = ManagerUtil.GetDatabaseType(databaseType);

                bool isFileConnection = this.IsFileConnection();

                if (!isFileConnection)
                {
                    frmAccountInfo form = new frmAccountInfo(dbType);
                    DialogResult result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        this.LoadAccounts();
                    }
                }
                else
                {
                    frmFileConnection form = new frmFileConnection(dbType);
                    DialogResult result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        this.LoadFileConnections();
                    }
                }
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            int count = this.dgvDbConnection.SelectedRows.Count;

            if (count == 0)
            {
                MessageBox.Show("No any row selected.");
                return;
            }

            if (MessageBox.Show("Are you sure to delete selected records with their profiles?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<string> ids = new List<string>();
                List<int> rowIndexes = new List<int>();

                for (int i = count - 1; i >= 0; i--)
                {
                    int rowIndex = this.dgvDbConnection.SelectedRows[i].Index;

                    ids.Add(this.dgvDbConnection.Rows[rowIndex].Cells[this.colId.Name].Value.ToString());

                    rowIndexes.Add(rowIndex);
                }

                bool success = await this.DeleteConnections(ids);

                if(success)
                {
                    rowIndexes.ForEach(item => { this.dgvDbConnection.Rows.RemoveAt(item); });
                }                
            }
        }

        private async void btnClear_Click(object sender, EventArgs e)
        {
            int count = this.dgvDbConnection.Rows.Count;

            if (count == 0)
            {
                MessageBox.Show("No record.");
                return;
            }

            if (MessageBox.Show("Are you sure to delete all records with their profiles?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<string> ids = new List<string>();

                for (int i = 0; i < this.dgvDbConnection.Rows.Count; i++)
                {
                    ids.Add(this.dgvDbConnection.Rows[i].Cells[this.colId.Name].Value.ToString());
                }

                bool success = await this.DeleteConnections(ids);

                if(success)
                {
                    this.dgvDbConnection.Rows.Clear();
                }               
            }
        }

        private async Task<bool> DeleteConnections(List<string> ids)
        {
            if (!this.IsFileConnection())
            {
                return await AccountProfileManager.Delete(ids);
            }
            else
            {
                return await FileConnectionProfileManager.Delete(ids);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cboDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cboDbType.SelectedIndex >= 0)
            {
                bool isFileConnection = this.IsFileConnection();

                this.SetColumnsVisible(isFileConnection);

                if (!isFileConnection)
                {
                    this.LoadAccounts();
                }
                else
                {
                    this.LoadFileConnections();
                }

                this.dgvDbConnection.ClearSelection();
            }
        }

        private void SetColumnsVisible(bool isFileConnection)
        {
            this.colServer.Visible = !isFileConnection;
            this.colPort.Visible = !isFileConnection;
            this.colIntegratedSecurity.Visible = !isFileConnection;
            this.colUserName.Visible = !isFileConnection;
            this.colName.Visible = isFileConnection;
            this.colDatabase.Visible = isFileConnection;
            this.colProfiles.Visible =!this.IsForSelecting && !isFileConnection;

            DatabaseType dbType = ManagerUtil.GetDatabaseType(this.cboDbType.Text);

            this.colDatabaseVisibility.Visible = !this.IsForSelecting && !(dbType == DatabaseType.Oracle || dbType == DatabaseType.Sqlite);
        }

        private bool IsFileConnection()
        {
            string dbType = this.cboDbType.Text;

            if (!string.IsNullOrEmpty(dbType))
            {
                return ManagerUtil.IsFileConnection(ManagerUtil.GetDatabaseType(dbType));
            }

            return false;
        }

        private async void LoadAccounts()
        {
            this.dgvDbConnection.Rows.Clear();

            string type = this.cboDbType.Text;

            var profiles = await AccountProfileManager.GetProfiles(type);

            foreach (AccountProfileInfo profile in profiles)
            {
                this.dgvDbConnection.Rows.Add(profile.Id, profile.Server, profile.Port, profile.IntegratedSecurity, profile.UserId,  null, null, this.actionButtonText, this.actionButtonText);
            }

            this.dgvDbConnection.Tag = profiles;
        }

        private async void LoadFileConnections()
        {
            this.dgvDbConnection.Rows.Clear();

            string type = this.cboDbType.Text;

            var profiles = await FileConnectionProfileManager.GetProfiles(type);

            foreach (var profile in profiles)
            {
                this.dgvDbConnection.Rows.Add(profile.Id, null, null, null, null, profile.Name, profile.Database, this.actionButtonText, this.actionButtonText);
            }

            this.dgvDbConnection.Tag = profiles;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            this.Edit();
        }

        private void Edit()
        {
            bool isFileConnection = this.IsFileConnection();

            if (!isFileConnection)
            {
                AccountProfileInfo profile = this.GetSelectedAccountProfile();

                if (profile != null)
                {
                    frmAccountInfo form = new frmAccountInfo(ManagerUtil.GetDatabaseType(this.cboDbType.Text), true) { AccountProfileInfo = profile };

                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        this.LoadAccounts();
                    }
                }
            }
            else
            {
                FileConnectionProfileInfo profile = this.GetSelectedFileConnectionProfile();

                if (profile != null)
                {
                    frmFileConnection frmAccountInfo = new frmFileConnection(ManagerUtil.GetDatabaseType(this.cboDbType.Text), true) { FileConnectionProfileInfo = profile };

                    if (frmAccountInfo.ShowDialog() == DialogResult.OK)
                    {
                        this.LoadFileConnections();
                    }
                }
            }
        }

        private string GetSelectedId()
        {
            int count = this.dgvDbConnection.SelectedRows.Count;

            if (count == 0)
            {
                MessageBox.Show("Please select row by clicking row header.");
                return null;
            }

            string id = this.dgvDbConnection.SelectedRows[0].Cells[this.colId.Name].Value.ToString();

            return id;
        }

        private AccountProfileInfo GetSelectedAccountProfile()
        {
            string id = this.GetSelectedId();

            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var profiles = this.dgvDbConnection.Tag as IEnumerable<AccountProfileInfo>;

            var profile = profiles.FirstOrDefault(item => item.Id == id);

            return profile;
        }

        private FileConnectionProfileInfo GetSelectedFileConnectionProfile()
        {
            string id = this.GetSelectedId();

            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var profiles = this.dgvDbConnection.Tag as IEnumerable<FileConnectionProfileInfo>;

            var profile = profiles.FirstOrDefault(item => item.Id == id);

            return profile;
        }

        private void SelectRecord()
        {
            bool isFileConnection = this.IsFileConnection();

            bool selected = false;

            if (!isFileConnection)
            {
                AccountProfileInfo profile = this.GetSelectedAccountProfile();

                if (profile != null)
                {
                    this.SelectedAccountProfileInfo = profile;

                    selected = true;
                }
            }
            else
            {
                FileConnectionProfileInfo profile = this.GetSelectedFileConnectionProfile();

                if (profile != null)
                {
                    this.SelectedFileConnectionProfileInfo = profile;

                    selected = true;
                }
            }

            if (selected)
            {
                this.DialogResult = DialogResult.OK;

                this.Close();
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            this.SelectRecord();
        }

        private void dgvDbConnection_DoubleClick(object sender, EventArgs e)
        {
            if (this.IsForSelecting)
            {
                this.SelectRecord();
            }
            else
            {
                this.Edit();
            }
        }

        private void dgvDbConnection_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvDbConnection.ClearSelection();
        }

        private void dgvDbConnection_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var row = DataGridViewHelper.GetCurrentRow(this.dgvDbConnection);

            if(row == null)
            {
                return;
            }

            string id = row.Cells[this.colId.Name].Value.ToString();

            if (e.ColumnIndex == this.colProfiles.Index)
            {
                frmDbConnectionProfileManage form = new frmDbConnectionProfileManage(id);

                form.ShowDialog();
            }
            else if(e.ColumnIndex == this.colDatabaseVisibility.Index)
            {
                frmDatabaseVisibility form = new frmDatabaseVisibility(id);

                var profile = (this.dgvDbConnection.Tag as IEnumerable<AccountProfileInfo>).FirstOrDefault(item=>item.Id == id);

                form.AccountProfileInfo = profile;
                form.DatabaseType = ManagerUtil.GetDatabaseType(this.cboDbType.Text);

                form.ShowDialog();
            }
        }
    }
}
