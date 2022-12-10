using DatabaseInterpreter.Model;
using DatabaseManager.Profile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmDbConnectionProfileManage : Form
    {
        private string accountId;
        public DatabaseType DatabaseType { get; set; }

        public frmDbConnectionProfileManage()
        {
            InitializeComponent();
        }

        public frmDbConnectionProfileManage(string accountId)
        {
            InitializeComponent();

            this.accountId = accountId;
        }

        private void frmDbConnectionProfile_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.dgvDbConnectionProfile.AutoGenerateColumns = false;          

            this.LoadProfiles();
        }

        private async void LoadProfiles()
        {
            this.dgvDbConnectionProfile.Rows.Clear();

            var profiles = await ConnectionProfileManager.GetProfilesByAccountId(this.accountId);

            foreach (ConnectionProfileInfo profile in profiles)
            {
                this.dgvDbConnectionProfile.Rows.Add(profile.Id, profile.Name, profile.Server, profile.Port, profile.Database);
            }

            this.dgvDbConnectionProfile.Tag = profiles;
        }        

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            int count = this.dgvDbConnectionProfile.SelectedRows.Count;

            if (count == 0)
            {
                MessageBox.Show("No any row selected.");
                return;
            }

            if (MessageBox.Show("Are you sure to delete the selected profiles?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<string> ids = new List<string>();
                List<int> rowIndexes = new List<int>();

                for (int i = count - 1; i >= 0; i--)
                {
                    int rowIndex = this.dgvDbConnectionProfile.SelectedRows[i].Index;

                    ids.Add(this.dgvDbConnectionProfile.Rows[rowIndex].Cells[this.colId.Name].Value.ToString());

                    rowIndexes.Add(rowIndex);
                }

                bool success = await this.DeleteConnections(ids);

                if(success)
                {
                    rowIndexes.ForEach(item => { this.dgvDbConnectionProfile.Rows.RemoveAt(item); });
                }               
            }
        }

        private async Task<bool> DeleteConnections(List<string> ids)
        {
            return await ConnectionProfileManager.Delete(ids);
        }

        private async void btnClear_Click(object sender, EventArgs e)
        {
            int count = this.dgvDbConnectionProfile.Rows.Count;

            if (count == 0)
            {
                MessageBox.Show("No record.");
                return;
            }

            if (MessageBox.Show("Are you sure to delete all profiles of this account?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<string> ids = new List<string>();

                for (int i = 0; i < this.dgvDbConnectionProfile.Rows.Count; i++)
                {
                    ids.Add(this.dgvDbConnectionProfile.Rows[i].Cells[this.colId.Name].Value.ToString());
                }

                bool success = await this.DeleteConnections(ids);

                if(success)
                {
                    this.dgvDbConnectionProfile.Rows.Clear();
                }                
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
