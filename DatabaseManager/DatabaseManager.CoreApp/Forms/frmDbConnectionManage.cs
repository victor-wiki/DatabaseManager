using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Forms;
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

namespace DatabaseManager
{
    public partial class frmDbConnectionManage : Form
    {
        private Rectangle dragBox;
        private int rowIndexFromMouseDown;

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
            this.tsbAdd.Image = IconImageHelper.GetImageByFontType(IconChar.Plus, IconFont.Solid, null, this.tsbAdd.Width);
            this.tsbEdit.Image = IconImageHelper.GetImage(IconChar.PenToSquare, null, this.tsbEdit.Width);
            this.tsbDelete.Image = IconImageHelper.GetImage(IconChar.Times, null, this.tsbDelete.Width);
            this.tsbClear.Image = IconImageHelper.GetImage(IconChar.Trash, null, this.tsbClear.Width);
            this.tsbSave.Image = IconImageHelper.GetImage(IconChar.Save, null, this.tsbSave.Width);
            this.tsbManageProfile.Image = IconImageHelper.GetImage(IconChar.List, null, this.tsbManageProfile.Width);
            this.tsbManageVisibility.Image = IconImageHelper.GetImageByFontType(IconChar.EyeSlash, IconFont.Solid, null, this.tsbManageVisibility.Width);
            this.picInfo.Image = IconImageHelper.GetImage(IconChar.InfoCircle, IconImageHelper.TipColor);

            this.dgvDbConnection.AutoGenerateColumns = false;

            DataGridViewContentAlignment middleCenter = DataGridViewContentAlignment.MiddleCenter;

            this.colIntegratedSecurity.HeaderCell.Style.Alignment = middleCenter;    
            this.colPriority.HeaderCell.Style.Alignment = middleCenter;

            this.btnSelect.Visible = this.IsForSelecting;
            this.panelDbType.Visible = !this.IsForSelecting;
            this.toolStrip1.Visible = !this.IsForSelecting;

            if (this.IsForSelecting)
            {
                this.Text = "Select Connection";
                this.dgvDbConnection.MultiSelect = false;
                this.dgvDbConnection.Top = this.toolStrip1.Top;
                this.dgvDbConnection.Height += this.panelDbType.Height + this.toolStrip1.Height;          
                this.dgvDbConnection.AutoResizeColumnHeadersHeight();              

                this.panelInfo.Visible = false;
                this.colCheck.Visible = false;
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

        private List<DataGridViewRow> GetCheckedRows()
        {
            List<DataGridViewRow> checkedRows = new List<DataGridViewRow>();

            var rows = this.dgvDbConnection.Rows;

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

            if (MessageBox.Show("Are you sure to delete selected records with their profiles?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                List<string> ids = new List<string>();
                List<int> rowIndexes = new List<int>();

                foreach (DataGridViewRow row in checkedRows)
                {
                    int rowIndex = row.Index;

                    ids.Add(this.dgvDbConnection.Rows[rowIndex].Cells[this.colId.Name].Value.ToString());

                    rowIndexes.Add(rowIndex);
                }

                bool success = await this.DeleteConnections(ids);

                if (success)
                {
                    rowIndexes.Sort();
                    rowIndexes.Reverse();

                    rowIndexes.ForEach(item => { this.dgvDbConnection.Rows.RemoveAt(item); });
                }
            }
        }

        private async void Clear()
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

                if (success)
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

                this.SetControlStatus(isFileConnection);

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

        private void SetControlStatus(bool isFileConnection)
        {
            this.colServer.Visible = !isFileConnection;
            this.colPort.Visible = !isFileConnection;
            this.colIntegratedSecurity.Visible = !isFileConnection;
            this.colUserName.Visible = !isFileConnection;
            this.colName.Visible = isFileConnection;
            this.colDatabase.Visible = isFileConnection;
            this.tsbManageProfile.Enabled = !this.IsForSelecting && !isFileConnection;

            DatabaseType dbType = ManagerUtil.GetDatabaseType(this.cboDbType.Text);

            this.tsbManageVisibility.Enabled = !this.IsForSelecting && !(dbType == DatabaseType.Oracle || dbType == DatabaseType.Sqlite);
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
                this.dgvDbConnection.Rows.Add(false, profile.Id, profile.Server, profile.Port, profile.IntegratedSecurity, profile.UserId, null, null, profile.Priority);
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
                this.dgvDbConnection.Rows.Add(false, profile.Id, null, null, null, null, profile.Name, profile.Database, profile.Priority);
            }

            this.dgvDbConnection.Tag = profiles;
        }

        private DataGridViewRow GetSelecteRow()
        {
            var rows = this.dgvDbConnection.SelectedRows;

            if(rows.Count ==0)
            {
                return null;
            }

            return rows[0];
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var row = this.GetSelecteRow();

            if(row == null)
            {
                MessageBox.Show("Please select one row first.");
                return;
            }

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

        private void Add()
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

        private void tsbAdd_Click(object sender, EventArgs e)
        {
            this.Add();
        }

        private void tsbEdit_Click(object sender, EventArgs e)
        {
            this.Edit();
        }

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            this.dgvDbConnection.EndEdit();

            this.Delete();
        }

        private void tsbClear_Click(object sender, EventArgs e)
        {
            this.Clear();
        }

        private void dgvDbConnection_MouseDown(object sender, MouseEventArgs e)
        {
            this.rowIndexFromMouseDown = this.dgvDbConnection.HitTest(e.X, e.Y).RowIndex;

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

        private void dgvDbConnection_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (this.dragBox != Rectangle.Empty && !dragBox.Contains(e.X, e.Y))
                {
                    DragDropEffects dropEffect = this.dgvDbConnection.DoDragDrop(
                          this.dgvDbConnection.Rows[rowIndexFromMouseDown],
                          DragDropEffects.Move);
                }
            }
        }

        private void dgvDbConnection_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dgvDbConnection_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = this.dgvDbConnection.PointToClient(new Point(e.X, e.Y));

            int index = this.dgvDbConnection.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            if (index < 0)
            {
                index = this.dgvDbConnection.RowCount - 1;
            }

            if (this.rowIndexFromMouseDown != index)
            {
                if (e.Effect == DragDropEffects.Move)
                {
                    DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;

                    this.dgvDbConnection.Rows.RemoveAt(this.rowIndexFromMouseDown);
                    this.dgvDbConnection.Rows.Insert(index, rowToMove);

                    this.UpdatePriorities();
                }
            }
        }

        private async void UpdatePriorities()
        {
            bool hasChanged = false;

            for (int i = 0; i < this.dgvDbConnection.RowCount; i++)
            {
                int rowNumber = i + 1;

                int oldPriority = Convert.ToInt32(this.dgvDbConnection.Rows[i].Cells[this.colPriority.Name].Value);

                this.dgvDbConnection.Rows[i].Cells[this.colPriority.Name].Value = rowNumber;

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

        private void tsbSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private async void Save()
        {
            bool isFileConnection = this.IsFileConnection();

            IEnumerable<AccountProfileInfo> accountProfiles = null;
            IEnumerable<FileConnectionProfileInfo> fileConnectionProfiles = null;

            if (!isFileConnection)
            {
                accountProfiles = this.dgvDbConnection.Tag as IEnumerable<AccountProfileInfo>;
            }
            else
            {
                fileConnectionProfiles = this.dgvDbConnection.Tag as IEnumerable<FileConnectionProfileInfo>;
            }

            Dictionary<string, int> dictPriorities = new Dictionary<string, int>();

            for (int i = 0; i < this.dgvDbConnection.RowCount; i++)
            {
                string id = this.dgvDbConnection.Rows[i].Cells[this.colId.Name].Value.ToString();

                int priority = Convert.ToInt32(this.dgvDbConnection.Rows[i].Cells[this.colPriority.Name].Value);

                int oldPriority = isFileConnection ? fileConnectionProfiles.FirstOrDefault(item => item.Id == id).Priority :
                    accountProfiles.FirstOrDefault(item => item.Id == id).Priority;

                if (priority != oldPriority)
                {
                    dictPriorities.Add(id, priority);
                }
            }

            int affectedRows = await ProfileBaseManager.UpdatePriorities(isFileConnection ? ProfileType.FileConnection : ProfileType.Account, dictPriorities);

            if (affectedRows > 0)
            {
                if (isFileConnection)
                {
                    foreach (var profile in fileConnectionProfiles)
                    {
                        string id = profile.Id;

                        if (dictPriorities.ContainsKey(id))
                        {
                            profile.Priority = dictPriorities[id];
                        }
                    }
                }
                else
                {
                    foreach (var profile in accountProfiles)
                    {
                        string id = profile.Id;

                        if (dictPriorities.ContainsKey(id))
                        {
                            profile.Priority = dictPriorities[id];
                        }
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

        private void tsbManageProfile_Click(object sender, EventArgs e)
        {
            var row = this.GetSelecteRow();

            if (row == null)
            {
                MessageBox.Show("Please select one row first.");
                return;
            }

            frmDbConnectionProfileManage form = new frmDbConnectionProfileManage(this.GetSelectedId()) { DatabaseType = ManagerUtil.GetDatabaseType(this.cboDbType.Text) };

            form.ShowDialog();
        }

        private void tsbManageVisibility_Click(object sender, EventArgs e)
        {
            var row = this.GetSelecteRow();

            if (row == null)
            {
                MessageBox.Show("Please select one row first.");
                return;
            }

            string id = this.GetSelectedId();

            frmDatabaseVisibility form = new frmDatabaseVisibility(id);

            var profile = (this.dgvDbConnection.Tag as IEnumerable<AccountProfileInfo>).FirstOrDefault(item => item.Id == id);

            form.AccountProfileInfo = profile;
            form.DatabaseType = ManagerUtil.GetDatabaseType(this.cboDbType.Text);

            form.ShowDialog();
        }
    }
}
