using DatabaseInterpreter.Model;
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

namespace DatabaseManager
{
    public partial class frmItemsSelector : Form
    {
        private bool isChecking = false;
        private List<CheckItemInfo> items;
        public bool Required { get; set; } = true;
   
        public List<CheckItemInfo> CheckedItem { get; set; } = new List<CheckItemInfo>();

        public frmItemsSelector(List<CheckItemInfo> items)
        {
            InitializeComponent();
            this.items = items;
        }

        public frmItemsSelector(string title, List<CheckItemInfo> items)
        {
            InitializeComponent();

            this.Text = title;
            this.items = items;
        }

        private void frmItemsSelector_Load(object sender, EventArgs e)
        {
            this.InitControls();           
        }

        private void InitControls()
        {
            foreach(CheckItemInfo item in this.items)
            {
                this.chkItems.Items.Add(item.Name, item.Checked);
            }

            if(this.items.All(item=> item.Checked))
            {
                this.chkSelectAll.Checked = true;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.Required && this.chkItems.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select a item.");
                return;
            }       

            foreach (var item in this.chkItems.CheckedItems)
            {
                this.CheckedItem.Add(new CheckItemInfo() { Name = item.ToString(), Checked = true });
            }                     

            this.DialogResult = DialogResult.OK;

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            this.CheckItems(this.chkSelectAll.Checked);
        }

        private void CheckItems(bool @checked)
        {
            if(!this.isChecking)
            {
                for (int i = 0; i < this.chkItems.Items.Count; i++)
                {
                    this.chkItems.SetItemChecked(i, @checked);
                }
            }           
        }       

        private void chkItems_MouseUp(object sender, MouseEventArgs e)
        {
            this.HandleItemChecked();
        }

        private void chkItems_KeyUp(object sender, KeyEventArgs e)
        {
            this.HandleItemChecked();
        }

        private void HandleItemChecked()
        {
            this.isChecking = true;
            this.chkSelectAll.Checked = this.chkItems.CheckedItems.Count == this.chkItems.Items.Count;
            this.isChecking = false;
        }
    }  

  
}
