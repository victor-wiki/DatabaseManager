using DatabaseInterpreter.Model;
using DatabaseManager.Core;
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
    public partial class frmDbObjectTypeSelector : Form
    {
        private bool isChecking = false;
        public DatabaseType DatabaseType { get; set; }
        public DatabaseObjectType DatabaseObjectType { get; private set; }

        public frmDbObjectTypeSelector()
        {
            InitializeComponent();
        }

        private void frmDbObjectTypeSelector_Load(object sender, EventArgs e)
        {
            this.InitControls();

            this.chkSelectAll.Checked = true;
        }

        private void InitControls()
        {
            List<DatabaseObjectType> dbObjTypes = new List<DatabaseObjectType>()
            {
                DatabaseObjectType.Table,
                DatabaseObjectType.View,
                DatabaseObjectType.Function,
                DatabaseObjectType.Procedure
            };

            if(this.DatabaseType== DatabaseType.SqlServer)
            {
                dbObjTypes.Insert(0, DatabaseObjectType.UserDefinedType);
            }

            foreach(DatabaseObjectType type in dbObjTypes)
            {
                this.chkDbObjectTypes.Items.Add(ManagerUtil.GetPluralString(type.ToString()));
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DatabaseObjectType databaseObjectType = DatabaseObjectType.None;

            foreach (var item in this.chkDbObjectTypes.CheckedItems)
            {
                DatabaseObjectType type = (DatabaseObjectType)Enum.Parse(typeof(DatabaseObjectType), ManagerUtil.GetSingularString(item.ToString()));

                databaseObjectType = databaseObjectType | type;
            }

            if(databaseObjectType == DatabaseObjectType.None)
            {
                MessageBox.Show("Please select database object type.");
                return;
            }
            else
            {
                this.DatabaseObjectType = databaseObjectType;
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
                for (int i = 0; i < this.chkDbObjectTypes.Items.Count; i++)
                {
                    this.chkDbObjectTypes.SetItemChecked(i, @checked);
                }
            }           
        }       

        private void chkDbObjectTypes_MouseUp(object sender, MouseEventArgs e)
        {
            this.HandleItemChecked();
        }

        private void chkDbObjectTypes_KeyUp(object sender, KeyEventArgs e)
        {
            this.HandleItemChecked();
        }

        private void HandleItemChecked()
        {
            this.isChecking = true;
            this.chkSelectAll.Checked = this.chkDbObjectTypes.CheckedItems.Count == this.chkDbObjectTypes.Items.Count;
            this.isChecking = false;
        }
    }  
}
