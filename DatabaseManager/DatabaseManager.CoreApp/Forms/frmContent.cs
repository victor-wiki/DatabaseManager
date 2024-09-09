using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseManager.Controls;
using DatabaseManager.Model;
using DockSample;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseManager.Forms
{
    public partial class frmContent : frmDockWindowBase
    {
        public UC_DbObjectContent ContentControl => this.ucContent;

        public frmContent()
        {
            InitializeComponent();
        }

        private async void frmContent_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool canClose = await this.CloseForm();

            e.Cancel = !canClose;
        }

        private async Task<bool> CloseForm()
        {
            bool canClose = true;            

            DatabaseObjectDisplayInfo info = this.Tag as DatabaseObjectDisplayInfo;

            if (info != null)
            {
                bool isNew = info.IsNew;

                IDbObjContentDisplayer control = this.ucContent.Controls[0] as IDbObjContentDisplayer;

                bool saveRequired = false;

                if (control is UC_SqlQuery sqlQuery)
                {
                    if (isNew)
                    {
                        if (sqlQuery.Editor.Text.Trim().Length > 0)
                        {
                            saveRequired = true;
                        }
                    }
                    else
                    {
                        if (sqlQuery.IsTextChanged())
                        {
                            saveRequired = true;
                        }
                    }
                }
                else if(control is UC_DataEditor dataEditor)
                {
                    if(dataEditor.IsDataChanged())
                    {
                        saveRequired = true;
                    }
                }
                else if (control is UC_TableDesigner tableDesigner)
                {
                    if (await tableDesigner.IsChanged())
                    {
                        saveRequired = true;
                    }
                }

                if (saveRequired)
                {
                    DialogResult result = MessageBox.Show($"Do you want to save {info.Name}?", "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        bool success =  this.ucContent.Save();

                        canClose = success;
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        canClose = false;
                    }
                }
            }

            if (canClose)
            {
                if (info.DisplayType == DatabaseObjectDisplayType.Script)
                {
                    var sqlQueryControl = this.ucContent.Controls[0] as UC_SqlQuery;

                    sqlQueryControl.DisposeResources();
                }                
            }

            return canClose;
        }
    }
}
