using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Data;
using DatabaseManager.Profile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmLockApp : Form
    {
        private bool confirmClose = false;
        private bool isLocked = false;

        public frmLockApp()
        {
            InitializeComponent();
        }

        private void frmLockApp_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private async Task InitControls()
        {
            PersonalSetting ps = await PersonalSettingManager.GetPersonalSetting();

            if (ps != null && !string.IsNullOrEmpty(ps.LockPassword))
            {
                this.txtPassword.Text = ps.LockPassword;
            }
        }

        private void frmLockApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.isLocked)
            {
                e.Cancel = false;
                return;
            }

            if (!this.confirmClose)
            {
                MessageBox.Show("Please enter password to unlock!");
                e.Cancel = true;
            }
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            this.LockOrUnlock();
        }

        private void LockOrUnlock()
        {
            string password = this.txtPassword.Text.Trim();

            bool isLock = this.btnLock.Text == "Lock";

            if (isLock)
            {
                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter the lock password!");
                    return;
                }

                DataStore.LockPassword = password;

                this.txtPassword.Text = "";

                this.btnLock.Text = "Unlock";
                this.btnExit.Text = "Exit";
                this.lblMessage.Visible = true;
                this.isLocked = true;
            }
            else
            {
                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please enter the lock password!");
                    return;
                }
                else if (password != DataStore.LockPassword)
                {
                    MessageBox.Show("The lock password is incorrect!");
                    return;
                }

                DataStore.LockPassword = null;

                this.confirmClose = true;

                this.Close();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (!this.isLocked)
            {
                this.Close();
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure to exit the application?", "Confirm", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                this.confirmClose = true;

                Application.Exit();
            }
        }      
        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.LockOrUnlock();
            }
        }
    }
}
