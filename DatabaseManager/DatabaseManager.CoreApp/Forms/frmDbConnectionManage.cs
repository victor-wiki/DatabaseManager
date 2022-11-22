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

namespace DatabaseManager
{
    public partial class frmDbConnectionManage : Form
    {
        public bool IsForSelecting { get; set; }
        public DatabaseType DatabaseType { get; set; }

        public AccountProfileInfo SelectedAccountProfileInfo { get; private set; }

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

            this.btnSelect.Visible = this.IsForSelecting;
            this.panelDbType.Visible = !this.IsForSelecting;
            this.panelOperation.Visible = !this.IsForSelecting;

            if (this.IsForSelecting)
            {
                this.Text = "Select connection";
                this.dgvDbConnection.MultiSelect = false;
                this.dgvDbConnection.Top = this.panelDbType.Top;
                this.dgvDbConnection.Height += this.panelDbType.Height;
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
                frmAccountInfo frmAccountInfo = new frmAccountInfo(dbType);
                DialogResult result = frmAccountInfo.ShowDialog();

                if (result == DialogResult.OK)
                {
                    this.LoadAccounts();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int count = this.dgvDbConnection.SelectedRows.Count;
            if (count == 0)
            {
                MessageBox.Show("Please select a row first.");
                return;
            }

            if (MessageBox.Show("Are you sure to delete selected records with their profiles?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<Guid> ids = new List<Guid>();
                List<int> rowIndexes = new List<int>();
                for (int i = count - 1; i >= 0; i--)
                {
                    int rowIndex = this.dgvDbConnection.SelectedRows[i].Index;

                    ids.Add(Guid.Parse(this.dgvDbConnection.Rows[rowIndex].Cells["Id"].Value.ToString()));

                    rowIndexes.Add(rowIndex);
                }

                this.DeleteConnections(ids);

                rowIndexes.ForEach(item => { this.dgvDbConnection.Rows.RemoveAt(item); });
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            int count = this.dgvDbConnection.Rows.Count;
            if (count == 0)
            {
                MessageBox.Show("No record.");
                return;
            }

            if (MessageBox.Show("Are you sure to delete all records with their profiles?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<Guid> ids = new List<Guid>();

                for (int i = 0; i < this.dgvDbConnection.Rows.Count; i++)
                {
                    ids.Add(Guid.Parse(this.dgvDbConnection.Rows[i].Cells["Id"].Value.ToString()));
                }

                this.DeleteConnections(ids);

                this.dgvDbConnection.Rows.Clear();
            }
        }

        private void DeleteConnections(List<Guid> ids)
        {
            AccountProfileManager.Delete(ids);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cboDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cboDbType.SelectedIndex >= 0)
            {
                this.LoadAccounts();
            }
        }

        private void LoadAccounts()
        {
            this.dgvDbConnection.Rows.Clear();

            string type = this.cboDbType.Text;

            var profiles = AccountProfileManager.GetProfiles(type);

            foreach (AccountProfileInfo profile in profiles)
            {
                this.dgvDbConnection.Rows.Add(profile.Id, profile.Server, profile.IntegratedSecurity, profile.UserId, profile.Port);
            }

            this.dgvDbConnection.Tag = profiles;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            this.Edit();
        }

        private void Edit()
        {
            AccountProfileInfo profile = this.GetSelectRecord();

            if (profile != null)
            {
                frmAccountInfo frmAccountInfo = new frmAccountInfo(ManagerUtil.GetDatabaseType(this.cboDbType.Text), true) { AccountProfileInfo = profile };

                if (frmAccountInfo.ShowDialog() == DialogResult.OK)
                {
                    this.LoadAccounts();
                }
            }
        }

        private AccountProfileInfo GetSelectRecord()
        {
            int count = this.dgvDbConnection.SelectedRows.Count;
            if (count == 0)
            {
                MessageBox.Show("Please select row by clicking row header.");
                return null;
            }

            Guid id = Guid.Parse(this.dgvDbConnection.SelectedRows[0].Cells["Id"].Value.ToString());
            IEnumerable<AccountProfileInfo> profiles = this.dgvDbConnection.Tag as IEnumerable<AccountProfileInfo>;

            AccountProfileInfo profile = profiles.FirstOrDefault(item => item.Id == id);

            return profile;
        }

        private void SelectRecord()
        {
            AccountProfileInfo profile = this.GetSelectRecord();

            if (profile != null)
            {
                this.SelectedAccountProfileInfo = profile;

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
            if(this.IsForSelecting)
            {
                this.SelectRecord();
            }    
            else
            {
                this.Edit();
            }
        }
    }
}
