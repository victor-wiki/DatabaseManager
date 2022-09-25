using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseManager.Profile;
using DatabaseManager.Helper;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;

namespace DatabaseManager.Controls
{
    public delegate void SelectedChangeHandler(object sender, ConnectionInfo connectionInfo);

    public partial class UC_DbConnectionProfile : UserControl
    {
        private ConnectionInfo connectionInfo;
        public ConnectionInfo ConnectionInfo => this.connectionInfo;

        public event SelectedChangeHandler OnSelectedChanged;

        [Category("Title")]
        public string Title
        {
            get
            {
                return this.lblTitle.Text;
            }
            set
            {
                this.lblTitle.Text = value;
            }
        }

        public int ClientHeight => this.btnAddDbProfile.Height;

        public DatabaseType DatabaseType
        {
            get { return ManagerUtil.GetDatabaseType(this.cboDbType.Text); }
            set { this.cboDbType.Text = value.ToString(); }
        }

        public bool EnableDatabaseType
        {
            get { return this.cboDbType.Enabled; }
            set { this.cboDbType.Enabled = value; }
        }

        public UC_DbConnectionProfile()
        {
            InitializeComponent();           
        }

        private void UC_DbConnectionProfile_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.LoadDbTypes();
        }

        public void LoadDbTypes()
        {
            var databaseTypes = DbInterpreterHelper.GetDisplayDatabaseTypes();

            foreach (var value in databaseTypes)
            {
                this.cboDbType.Items.Add(value.ToString());
            }
        }   
      

        public bool IsDbTypeSelected()
        {
            return !string.IsNullOrEmpty(this.cboDbType.Text);
        }

        public bool IsProfileSelected()
        {
            return !string.IsNullOrEmpty(this.cboDbProfile.Text);
        }

        private void cboDbProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.GetConnectionInfoByProfile();
        }

        private void GetConnectionInfoByProfile()
        {
            DatabaseType dbType = this.DatabaseType;
            string profileName = (this.cboDbProfile.SelectedItem as ConnectionProfileInfo)?.Name;
            ConnectionInfo connectionInfo = ConnectionProfileManager.GetConnectionInfo(dbType.ToString(), profileName);

            if (connectionInfo != null)
            {
                if (this.OnSelectedChanged != null)
                {
                    this.OnSelectedChanged(this, connectionInfo);
                }
            }
        }

        private void LoadProfileNames(string defaultValue = null)
        {
            string type = this.cboDbType.Text;

            if (type != "")
            {
                IEnumerable<ConnectionProfileInfo> profiles = ConnectionProfileManager.GetProfiles(type);

                List<string> names = profiles.Select(item => item.Name).ToList();

                this.cboDbProfile.DataSource = profiles.ToList();
                this.cboDbProfile.DisplayMember = nameof(ConnectionProfileInfo.Description);
                this.cboDbProfile.ValueMember = nameof(ConnectionProfileInfo.Name);

                if (string.IsNullOrEmpty(defaultValue))
                {
                    if (profiles.Count() > 0)
                    {
                        this.cboDbProfile.SelectedIndex = 0;
                    }
                }
                else
                {
                    if (names.Contains(defaultValue))
                    {
                        this.cboDbProfile.Text = profiles.FirstOrDefault(item => item.Name == defaultValue)?.Description;
                    }
                }

                bool selected = this.cboDbProfile.Text.Length > 0;

                this.btnConfigDbProfile.Visible = this.btnDeleteDbProfile.Visible = selected;
            }
        }

        private void cboDbProfile_DrawItem(object sender, DrawItemEventArgs e)
        {
            ComboBox combobox = sender as ComboBox;
            if (combobox.DroppedDown)
            {
                e.DrawBackground();
            }

            e.DrawFocusRectangle();

            var items = combobox.Items;

            if (e.Index < 0)
            {
                e.Graphics.DrawString(combobox.Text, e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left, e.Bounds.Y);
            }
            else
            {
                if (items.Count > 0 && e.Index < items.Count)
                {
                    ConnectionProfileInfo model = items[e.Index] as ConnectionProfileInfo;
                    e.Graphics.DrawString(model.Description, e.Font, new SolidBrush(combobox.DroppedDown ? e.ForeColor : Color.Black), e.Bounds.Left, e.Bounds.Y);
                }
            }
        }

        private void btnAddDbProfile_Click(object sender, EventArgs e)
        {
            this.AddConnection(true, this.cboDbType.Text);
        }

        private void AddConnection(bool isSource, string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Please select database type.");
                return;
            }

            DatabaseType dbType = this.DatabaseType;
            frmDbConnect frmDbConnect = new frmDbConnect(dbType);
            if (this.SetConnectionInfo(frmDbConnect))
            {
                this.LoadProfileNames(frmDbConnect.ProflieName);
            }
        }

        private bool SetConnectionInfo(frmDbConnect frmDbConnect)
        {
            DialogResult dialogResult = frmDbConnect.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                ConnectionInfo connectionInfo = frmDbConnect.ConnectionInfo;
                this.connectionInfo = connectionInfo;

                if (this.OnSelectedChanged != null)
                {
                    this.OnSelectedChanged(this, connectionInfo);
                }

                return true;
            }
            return false;
        }

        private void btnConfigDbProfile_Click(object sender, EventArgs e)
        {
            this.ConfigConnection();
        }

        public void ConfigConnection(bool requriePassword = false)
        {
            string type = this.cboDbType.Text;
            object selectedItem = this.cboDbProfile.SelectedItem;
            ConnectionProfileInfo profile = selectedItem as ConnectionProfileInfo;
            string profileName = selectedItem == null ? string.Empty : profile?.Name;

            if (string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Please select database type.");
                return;
            }

            if (string.IsNullOrEmpty(profileName))
            {
                MessageBox.Show("Please select a profile.");
                return;
            }

            DatabaseType dbType = this.DatabaseType;
            frmDbConnect from = new frmDbConnect(dbType, profileName, requriePassword);
            from.ConnectionInfo = profile?.ConnectionInfo;

            this.SetConnectionInfo(from);

            if (profileName != from.ProflieName)
            {
                this.LoadProfileNames(from.ProflieName);                
            }

            if (this.cboDbProfile.SelectedItem != null)
            {
                (this.cboDbProfile.SelectedItem as ConnectionProfileInfo).ConnectionInfo = from.ConnectionInfo;
            }
        }

        private void btnDeleteDbProfile_Click(object sender, EventArgs e)
        {
            this.DeleteProfile();
        }

        private void DeleteProfile()
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure to delete the profile?", "Confirm", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                string profileName = (this.cboDbProfile.SelectedItem as ConnectionProfileInfo).Name;
                if (ConnectionProfileManager.Delete(this.DatabaseType, profileName))
                {
                    this.LoadProfileNames();
                }
            }
        }

        private void cboDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadProfileNames();
        }        
    }
}
