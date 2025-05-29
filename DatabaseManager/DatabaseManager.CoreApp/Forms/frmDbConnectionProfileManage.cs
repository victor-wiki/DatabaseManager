using DatabaseInterpreter.Model;
using DatabaseManager.Helper;
using DatabaseManager.Profile.Manager;
using DatabaseManager.Profile.Model;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmDbConnectionProfileManage : Form
    {
        private string accountId;
        private Rectangle dragBox;
        private int rowIndexFromMouseDown;

        public DatabaseType DatabaseType { get; set; }       

        public frmDbConnectionProfileManage(string accountId)
        {
            InitializeComponent();

            this.accountId = accountId;

            this.Init();
        }

        private void Init()
        {
            this.tsbEdit.Image = IconImageHelper.GetImage(IconChar.PenToSquare, null, this.tsbEdit.Width);
            this.tsbDelete.Image = IconImageHelper.GetImage(IconChar.Times, null, this.tsbDelete.Width);
            this.tsbClear.Image = IconImageHelper.GetImage(IconChar.Trash, null, this.tsbClear.Width);
            this.tsbSave.Image = IconImageHelper.GetImage(IconChar.Save, null, this.tsbSave.Width);
            this.picInfo.Image = IconImageHelper.GetImage(IconChar.InfoCircle, IconImageHelper.TipColor);

            this.colPriority.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
                this.dgvDbConnectionProfile.Rows.Add(false, profile.Id, profile.Name, profile.Server, profile.Port, profile.Database, profile.Priority);
            }

            this.dgvDbConnectionProfile.Tag = profiles;
        }

        private List<DataGridViewRow> GetCheckedRows()
        {
            List<DataGridViewRow> checkedRows = new List<DataGridViewRow>();

            var rows = this.dgvDbConnectionProfile.Rows;

            foreach (DataGridViewRow row in rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells[this.colCheck.Name].Value.ToString());

                if (isChecked)
                {
                    checkedRows.Add(row);
                }
            }

            return checkedRows;
        }

        private async void Delete()
        {
            List<DataGridViewRow> checkedRows = this.GetCheckedRows();

            if (checkedRows.Count == 0)
            {
                MessageBox.Show("No any row selected.");
                return;
            }

            if (MessageBox.Show("Are you sure to delete the selected profiles?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<string> ids = new List<string>();
                List<int> rowIndexes = new List<int>();

                foreach (DataGridViewRow row in checkedRows)
                {
                    int rowIndex = row.Index;

                    ids.Add(this.dgvDbConnectionProfile.Rows[rowIndex].Cells[this.colId.Name].Value.ToString());

                    rowIndexes.Add(rowIndex);
                }

                bool success = await this.DeleteConnections(ids);

                if (success)
                {
                    rowIndexes.Sort();
                    rowIndexes.Reverse();

                    rowIndexes.ForEach(item => { this.dgvDbConnectionProfile.Rows.RemoveAt(item); });
                }
            }
        }

        private async Task<bool> DeleteConnections(List<string> ids)
        {
            return await ConnectionProfileManager.Delete(ids);
        }

        private async void Clear()
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

                if (success)
                {
                    this.dgvDbConnectionProfile.Rows.Clear();
                }
            }
        }

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            this.dgvDbConnectionProfile.EndEdit();

            this.Delete();
        }

        private void tsbClear_Click(object sender, EventArgs e)
        {
            this.Clear();
        }

        private void dgvDbConnectionProfile_MouseDown(object sender, MouseEventArgs e)
        {
            this.rowIndexFromMouseDown = this.dgvDbConnectionProfile.HitTest(e.X, e.Y).RowIndex;

            if (rowIndexFromMouseDown != -1)
            {
                Size dragSize = SystemInformation.DragSize;

                this.dragBox = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
            }
            else
            {
                this.dragBox = Rectangle.Empty;
            }
        }

        private void dgvDbConnectionProfile_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (this.dragBox != Rectangle.Empty && !dragBox.Contains(e.X, e.Y))
                {
                    DragDropEffects dropEffect = this.dgvDbConnectionProfile.DoDragDrop(
                          this.dgvDbConnectionProfile.Rows[rowIndexFromMouseDown],
                          DragDropEffects.Move);
                }
            }
        }

        private void dgvDbConnectionProfile_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dgvDbConnectionProfile_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = this.dgvDbConnectionProfile.PointToClient(new Point(e.X, e.Y));

            int index = this.dgvDbConnectionProfile.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            if (index < 0)
            {
                index = this.dgvDbConnectionProfile.RowCount - 1;
            }

            if (this.rowIndexFromMouseDown != index)
            {
                if (e.Effect == DragDropEffects.Move)
                {
                    DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;

                    this.dgvDbConnectionProfile.Rows.RemoveAt(this.rowIndexFromMouseDown);
                    this.dgvDbConnectionProfile.Rows.Insert(index, rowToMove);

                    this.UpdatePriorities();
                }
            }
        }

        private async void UpdatePriorities()
        {
            bool hasChanged = false;

            for (int i = 0; i < this.dgvDbConnectionProfile.RowCount; i++)
            {
                int rowNumber = i + 1;

                int oldPriority = Convert.ToInt32(this.dgvDbConnectionProfile.Rows[i].Cells[this.colPriority.Name].Value);

                this.dgvDbConnectionProfile.Rows[i].Cells[this.colPriority.Name].Value = rowNumber;

                if (oldPriority != rowNumber)
                {
                    hasChanged = true;
                }
            }

            if (hasChanged)
            {
                this.tsbSave.Enabled = true;
            }
        }

        private async void tsbSave_Click(object sender, EventArgs e)
        {
            IEnumerable<ConnectionProfileInfo> profiles = this.dgvDbConnectionProfile.Tag as IEnumerable<ConnectionProfileInfo>;

            Dictionary<string, int> dictPriorities = new Dictionary<string, int>();

            for (int i = 0; i < this.dgvDbConnectionProfile.RowCount; i++)
            {
                string id = this.dgvDbConnectionProfile.Rows[i].Cells[this.colId.Name].Value.ToString();

                int priority = Convert.ToInt32(this.dgvDbConnectionProfile.Rows[i].Cells[this.colPriority.Name].Value);

                int oldPriority = profiles.FirstOrDefault(item => item.Id == id).Priority;

                if (priority != oldPriority)
                {
                    dictPriorities.Add(id, priority);
                }
            }

            int affectedRows = await ProfileBaseManager.UpdatePriorities(ProfileType.Connection, dictPriorities);

            if (affectedRows > 0)
            {
                foreach (var profile in profiles)
                {
                    string id = profile.Id;

                    if (dictPriorities.ContainsKey(id))
                    {
                        profile.Priority = dictPriorities[id];
                    }
                }

                this.tsbSave.Enabled = false;

                MessageBox.Show("Saved.");
            }
            else
            {
                MessageBox.Show("No rows affected.");
            }
        }

        private void tsbEdit_Click(object sender, EventArgs e)
        {
            var rows = this.dgvDbConnectionProfile.SelectedRows;

            if(rows.Count ==0)
            {
                MessageBox.Show("Please select one row first.");
                return;
            }

            this.Edit();
        }

        public void Edit()
        {
            var row = this.dgvDbConnectionProfile.SelectedRows[0];
            var profiles = this.dgvDbConnectionProfile.Tag as IEnumerable<ConnectionProfileInfo>;

            string id = row.Cells[this.colId.Name].Value.ToString();

            ConnectionProfileInfo profile = profiles.FirstOrDefault(item=>item.Id == id);

            frmDbConnect frm = new frmDbConnect(this.DatabaseType, id, false, true);
            frm.ConnectionInfo = this.GetConnectionInfo(profile);

            this.SetConnectionInfo(frm, profile);
        }

        private async void SetConnectionInfo(frmDbConnect frmDbConnect, ConnectionProfileInfo profile)
        {
            DialogResult dialogResult = frmDbConnect.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                ConnectionInfo connectionInfo = frmDbConnect.ConnectionInfo;

                var result = await ConnectionProfileManager.GetProfileById(profile.Id);
     
                profile.Database = connectionInfo.Database;
                profile.Name = result.Name;

                var row = this.dgvDbConnectionProfile.SelectedRows[0];

                row.Cells[this.colName.Name].Value = profile.Name;
                row.Cells[this.colDatabase.Name].Value = profile.Database;
            }            
        }

        private ConnectionInfo GetConnectionInfo(ConnectionProfileInfo profile)
        {
            if (profile != null)
            {
                return new ConnectionInfo() { Server = profile.Server, Port = profile.Port, Database = profile.Database, IntegratedSecurity = profile.IntegratedSecurity, UserId = profile.UserId, Password = profile.Password, IsDba = profile.IsDba, UseSsl = profile.UseSsl };
            }

            return null;
        }

        private void SetProfileConnectionInfo(ConnectionProfileInfo profile, ConnectionInfo connectionInfo)
        {
            if (connectionInfo != null)
            {
                profile.Server = connectionInfo.Server;
                profile.Port = connectionInfo.Port;
                profile.Database = connectionInfo.Database;
                profile.IntegratedSecurity = connectionInfo.IntegratedSecurity;
                profile.UserId = connectionInfo.UserId;
                profile.Password = connectionInfo.Password;
                profile.IsDba = connectionInfo.IsDba;
                profile.UseSsl = connectionInfo.UseSsl;
            }
        }
    }
}
