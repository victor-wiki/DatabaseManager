using DatabaseManager.Controls;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using DatabaseManager.Helper;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                bool isScriptFile = false;
                string filePath = null;

                if (control is UC_SqlQuery sqlQuery)
                {
                    isScriptFile = true;

                    if (isNew)
                    {
                        if (sqlQuery.Editor.Text.Trim().Length > 0)
                        {
                            saveRequired = true;
                        }
                    }
                    else
                    {
                        filePath = sqlQuery.DisplayInfo.FilePath;

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
                        ContentSaveResult saveResult =  this.ucContent.Save();

                        canClose = saveResult.IsOK;

                        if(isScriptFile && saveResult.IsOK)
                        {
                            filePath = saveResult.ResultData?.ToString();
                        }
                    }
                    else if (result == DialogResult.No)
                    {
                        canClose = true;
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        canClose = false;
                    }
                }


                if (isScriptFile && File.Exists(filePath))
                {
                    if(frmMain.IsClosing && SettingManager.Setting.RememberApplicationLayoutInformation)
                    {
                        this.RememberRecentFilePath(filePath);
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

        private void RememberRecentFilePath(string filePath)
        {
            File.AppendAllLines(ProfileFileHelper.RecentFilePath, new string[] { filePath });
        }
    }
}
