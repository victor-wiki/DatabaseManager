using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseManager.Model;
using DatabaseManager.Core;
using System.IO;
using DatabaseManager.Helper;
using DatabaseInterpreter.Model;

namespace DatabaseManager
{
    public partial class frmBackupSetting : Form
    {
        public frmBackupSetting()
        {
            InitializeComponent();

            this.dgvSettings.AutoGenerateColumns = false;
        }

        private void frmBackupSetting_Load(object sender, EventArgs e)
        {
            this.LoadSettings();
        }

        private void LoadSettings()
        {
            List<BackupSetting> settings = BackupSettingManager.GetSettings();

            var dbTypes = Enum.GetNames(typeof(DatabaseType));

            foreach (string dbType in dbTypes)
            {
                if (dbType != DatabaseType.Unknown.ToString())
                {
                    BackupSetting setting = settings.FirstOrDefault(item => item.DatabaseType == dbType);

                    if (setting == null)
                    {
                        settings.Add(new BackupSetting() { DatabaseType = dbType });
                    }
                }
            }

            this.dgvSettings.DataSource = settings;
        }

        private void dgvSettings_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewCell cell = this.dgvSettings.Rows[e.RowIndex].Cells[e.ColumnIndex];

            if(cell.ReadOnly)
            {
                return;
            }

            string value = DataGridViewHelper.GetCellStringValue(cell);

            if (e.ColumnIndex == this.colClientToolFilePath.Index)
            {
                if (this.openFileDialog1 == null)
                {
                    this.openFileDialog1 = new OpenFileDialog();
                }               

                if (!string.IsNullOrEmpty(value) && File.Exists(value))
                {
                    this.openFileDialog1.FileName = value;
                }
                else
                {
                    this.openFileDialog1.FileName = "";
                }

                DialogResult result = this.openFileDialog1.ShowDialog();

                if (result == DialogResult.OK)
                {
                    this.SetCellValue(cell, this.openFileDialog1.FileName);
                }
            }
            else if (e.ColumnIndex == this.colSaveFolder.Index)
            {
                if(this.folderBrowserDialog1==null)
                {
                    this.folderBrowserDialog1 = new FolderBrowserDialog();
                }            

                if (!string.IsNullOrEmpty(value) && File.Exists(value))
                {
                    this.folderBrowserDialog1.SelectedPath = value;
                }
                else
                {
                    this.folderBrowserDialog1.SelectedPath = "";
                }

                DialogResult result = this.folderBrowserDialog1.ShowDialog();

                if (result == DialogResult.OK)
                {
                    this.SetCellValue(cell, this.folderBrowserDialog1.SelectedPath);
                }
            }
        }

        private void SetCellValue(DataGridViewCell cell, string value)
        {
            cell.Value = value;

            this.dgvSettings.EndEdit();
            this.dgvSettings.CurrentCell = null;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Save();

            MessageBox.Show("Saved successfully.");

            this.DialogResult = DialogResult.OK;

            this.Close();
        }

        private List<BackupSetting> GetSettings()
        {
            List<BackupSetting> settings = new List<BackupSetting>();

            foreach (DataGridViewRow row in this.dgvSettings.Rows)
            {
                BackupSetting setting = new BackupSetting();
                setting.DatabaseType = DataGridViewHelper.GetCellStringValue(row, this.colDatabaseType.Name);
                setting.ClientToolFilePath = DataGridViewHelper.GetCellStringValue(row, this.colClientToolFilePath.Name);
                setting.SaveFolder = DataGridViewHelper.GetCellStringValue(row, this.colSaveFolder.Name);
                setting.ZipFile = DataGridViewHelper.GetCellBoolValue(row, this.colZipBackupFile.Name);

                settings.Add(setting);
            }

            return settings;
        }

        private void Save()
        {
            BackupSettingManager.SaveConfig(this.GetSettings());
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvSettings_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach(DataGridViewRow row in this.dgvSettings.Rows)
            {
                string dbType = DataGridViewHelper.GetCellStringValue(row, this.colDatabaseType.Name);

                if(dbType == DatabaseType.SqlServer.ToString())
                {
                    row.Cells[this.colClientToolFilePath.Name].ReadOnly = true;
                    row.Cells[this.colZipBackupFile.Name].ReadOnly = true;
                }
                else if(dbType == DatabaseType.Postgres.ToString())
                {
                    row.Cells[this.colZipBackupFile.Name].ReadOnly = true;
                }
            }

            this.dgvSettings.ClearSelection();
        }
    }
}
