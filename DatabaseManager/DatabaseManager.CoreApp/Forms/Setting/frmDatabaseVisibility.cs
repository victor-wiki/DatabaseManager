using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Data;
using DatabaseManager.Helper;
using DatabaseManager.Profile.Manager;
using DatabaseManager.Profile.Model;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmDatabaseVisibility : Form
    {
        private string accountId;
        public DatabaseType DatabaseType { get; set; }
        public AccountProfileInfo AccountProfileInfo { get; set; }

        public frmDatabaseVisibility(string accountId)
        {
            InitializeComponent();

            this.accountId = accountId;

            this.Init();
        }

        private void frmDatabaseVisibility_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void Init()
        {
            this.tsbInvisible.Image = IconImageHelper.GetImageByFontType(IconChar.EyeSlash, IconFont.Solid, null, this.tsbInvisible.Width);
            this.tsbVisible.Image = IconImageHelper.GetImageByFontType(IconChar.Eye, IconFont.Solid, null, this.tsbVisible.Width);
            this.tsbDelete.Image = IconImageHelper.GetImage(IconChar.Times, null, this.tsbDelete.Width);
            this.tsbClear.Image = IconImageHelper.GetImage(IconChar.Trash, null, this.tsbClear.Width);
            this.tsbRefresh.Image = IconImageHelper.GetImage(IconChar.Refresh, null, this.tsbRefresh.Width);
        }

        private void InitControls()
        {
            this.dgvDatabases.AutoGenerateColumns = false;
            this.colVisible.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.LoadData();
        }

        private async void LoadData()
        {
            this.dgvDatabases.Rows.Clear();

            IEnumerable<Database> databases = Enumerable.Empty<Database>();

            if (this.AccountProfileInfo != null)
            {
                if (!this.AccountProfileInfo.IntegratedSecurity && string.IsNullOrEmpty(this.AccountProfileInfo.Password))
                {
                    var storedInfo = DataStore.GetAccountProfileInfo(this.AccountProfileInfo.Id);

                    if (storedInfo != null && !string.IsNullOrEmpty(storedInfo.Password))
                    {
                        this.AccountProfileInfo.Password = storedInfo.Password;
                    }
                    else
                    {
                        MessageBox.Show("Please specify password for the database.");

                        if (!this.SetConnectionInfo(this.AccountProfileInfo))
                        {
                            return;
                        }
                    }
                }

                ConnectionInfo connectionInfo = new ConnectionInfo();

                ObjectHelper.CopyProperties(this.AccountProfileInfo, connectionInfo);

                databases = await this.GetDatabases(connectionInfo);
            }

            this.LoadRecords(databases);
        }

        private async void LoadRecords(IEnumerable<Database> databases)
        {
            var databaseNames = databases.Select(item => item.Name).OrderBy(item=>item.ToLower());

            var visibilities = await DatabaseVisibilityManager.GetVisibilities(this.accountId);

            foreach (var visibility in visibilities)
            {
                int rowIndex = this.dgvDatabases.Rows.Add(visibility.Id, visibility.Database, visibility.Visible);

                DataGridViewRow row = this.dgvDatabases.Rows[rowIndex];

                if (databaseNames.Count() > 0 && !databaseNames.Any(item => item.ToUpper() == visibility.Database.ToUpper()))
                {
                    row.DefaultCellStyle.BackColor = Color.Red;
                }
            }

            foreach (var dbName in databaseNames)
            {
                if (!visibilities.Any(item => item.Database.ToUpper() == dbName.ToUpper()))
                {
                    this.dgvDatabases.Rows.Add(Guid.NewGuid().ToString(), dbName, true);
                }
            }

            this.dgvDatabases.ClearSelection();
        }
        private async Task<IEnumerable<Database>> GetDatabases(ConnectionInfo connectionInfo)
        {
            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.DatabaseType, connectionInfo, new DbInterpreterOption());

            try
            {
                var databases = await dbInterpreter.GetDatabasesAsync();

                return databases;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionHelper.GetExceptionDetails(ex));

                return Enumerable.Empty<Database>();
            }
        }

        private bool SetConnectionInfo(AccountProfileInfo accountProfileInfo)
        {
            frmAccountInfo frmAccountInfo = new frmAccountInfo(this.DatabaseType, true) { AccountProfileInfo = accountProfileInfo };

            DialogResult dialogResult = frmAccountInfo.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                AccountProfileInfo profileInfo = frmAccountInfo.AccountProfileInfo;

                this.AccountProfileInfo = profileInfo;

                return true;
            }

            return false;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.LoadData();
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
           
        }

        private async void Delete()
        {
            int count = this.dgvDatabases.SelectedRows.Count;

            if (count == 0)
            {
                MessageBox.Show("No any row selected.");
                return;
            }

            if (MessageBox.Show("Are you sure to delete the selected records?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<string> ids = new List<string>();
                List<int> rowIndexes = new List<int>();

                for (int i = count - 1; i >= 0; i--)
                {
                    int rowIndex = this.dgvDatabases.SelectedRows[i].Index;

                    ids.Add(this.dgvDatabases.Rows[rowIndex].Cells[this.colId.Name].Value.ToString());

                    rowIndexes.Add(rowIndex);
                }

                bool success = await this.DeleteRecords(ids);

                if (success)
                {
                    rowIndexes.ForEach(item => { this.dgvDatabases.Rows.RemoveAt(item); });
                }
            }
        }

        private async Task<bool> DeleteRecords(List<string> ids)
        {
            return await DatabaseVisibilityManager.Delete(ids);
        }

        private async void btnClear_Click(object sender, EventArgs e)
        {
           
        }

        private async void Clear()
        {
            int count = this.dgvDatabases.Rows.Count;

            if (count == 0)
            {
                MessageBox.Show("No record.");
                return;
            }

            if (MessageBox.Show("Are you sure to delete all records of this account?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<string> ids = new List<string>();

                for (int i = 0; i < this.dgvDatabases.Rows.Count; i++)
                {
                    ids.Add(this.dgvDatabases.Rows[i].Cells[this.colId.Name].Value.ToString());
                }

                bool success = await this.DeleteRecords(ids);

                if (success)
                {
                    this.dgvDatabases.Rows.Clear();
                }
            }
        }

        private async void btnVisible_Click(object sender, EventArgs e)
        {
            await this.SetVisible(true);
        }

        private async void btnInVisible_Click(object sender, EventArgs e)
        {
            await this.SetVisible(false);
        }

        private async Task<bool> SetVisible(bool visible)
        {
            int count = this.dgvDatabases.SelectedRows.Count;

            if (count == 0)
            {
                MessageBox.Show("Please select rows first.");
                return false;
            }

            List<DatabaseVisibilityInfo> visibilities = new List<DatabaseVisibilityInfo>();

            foreach (DataGridViewRow row in this.dgvDatabases.SelectedRows)
            {
                string id = row.Cells[this.colId.Name].Value.ToString();
                string database = row.Cells[this.colDatabase.Name].Value.ToString();

                visibilities.Add(new DatabaseVisibilityInfo() { Id = id, AccountId = this.accountId, Database = database, Visible = visible });
            }

            bool success = await DatabaseVisibilityManager.Save(this.accountId, visibilities);

            if (success)
            {
                MessageBox.Show("Operate succeeded.");

                this.LoadData();
            }

            return success;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void tsbInvisible_Click(object sender, EventArgs e)
        {
            await this.SetVisible(false);
        }

        private async void tsbVisible_Click(object sender, EventArgs e)
        {
            await this.SetVisible(true);
        }

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            this.Delete();
        }

        private void tsbClear_Click(object sender, EventArgs e)
        {
            this.Clear();
        }

        private void tsbRefresh_Click(object sender, EventArgs e)
        {
            this.LoadData();
        }
    }
}
