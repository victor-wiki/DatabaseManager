using DatabaseConverter.Core;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Core;
using DatabaseManager.Helper;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmDataTypeMappingSetting : Form
    {
        private string currentFilePath = null;
        private bool isReadonly;
        private DatabaseType initSourceDbType;
        private DatabaseType initTargetDbType;
        private bool isDefault;
        private string customFileName;

        public frmDataTypeMappingSetting(bool isReadonly = false, DatabaseType sourceDbType = DatabaseType.Unknown, DatabaseType targetDbType = DatabaseType.Unknown, bool isDefault = false, string customFileName = null)
        {
            InitializeComponent();

            this.isReadonly = isReadonly;
            this.initSourceDbType = sourceDbType;
            this.initTargetDbType = targetDbType;
            this.isDefault = isDefault;
            this.customFileName = customFileName;

            this.Init();
        }

        private void Init()
        {
            this.LoadDbTypes();

            if (!this.isReadonly)
            {
                this.btnExchange.Image = IconImageHelper.GetImageByFontType(IconChar.Exchange, IconFont.Solid, Color.DodgerBlue);
                this.tsbAdd.Image = IconImageHelper.GetImageByFontType(IconChar.Plus, IconFont.Solid, null, this.tsbAdd.Width);
                this.tsbDelete.Image = IconImageHelper.GetImage(IconChar.Times, null, this.tsbDelete.Width);
                this.tsbSave.Image = IconImageHelper.GetImage(IconChar.Save, null, this.tsbSave.Width);
                this.tsbSaveAs.Image = IconImageHelper.GetImage(IconChar.Clone, null, this.tsbSaveAs.Width);
            }
            else
            {
                this.Text = "Data Type Mappings";
                this.toolStrip1.Visible = false;
                this.panelOperation.Visible = false;
                this.dgvData.Top = 0;
                this.dgvData.Height += (this.toolStrip1.Height + this.panelOperation.Height);
                this.colCheck.Visible = false;

                foreach(DataGridViewColumn col in this.dgvData.Columns)
                {
                    col.ReadOnly = true;
                }               

                this.cboSourceDbType.Text = this.initSourceDbType.ToString();
                this.cboTargetDbType.Text = this.initTargetDbType.ToString();

                this.cboCustom.Text = this.customFileName;

                this.LoadMappings(this.isDefault);
            }           
        }

        private void LoadDbTypes()
        {
            var databaseTypes = DbInterpreterHelper.GetDisplayDatabaseTypes();

            foreach (var value in databaseTypes)
            {
                this.cboSourceDbType.Items.Add(value.ToString());
                this.cboTargetDbType.Items.Add(value.ToString());
            }
        }

        private void LoadCustomFileNames(string defaultItem = null)
        {
            this.cboCustom.Items.Clear();

            string sourceDbType = this.cboSourceDbType.Text;
            string targetDbType = this.cboTargetDbType.Text;

            if (!string.IsNullOrEmpty(sourceDbType) && !string.IsNullOrEmpty(targetDbType))
            {
                var sourceDatabaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), sourceDbType);
                var targetDatabaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), targetDbType);

                string customMappingFolder = SettingManager.Setting.CustomMappingFolder;

                string customMappingSubFolder = DataTypeMappingManager.GetDataTypeMappingCustomSubFolder(sourceDatabaseType, targetDatabaseType, customMappingFolder);

                DirectoryInfo di = new DirectoryInfo(customMappingSubFolder);

                if (di.Exists)
                {
                    var fileNames = di.GetFiles().Select(item => Path.GetFileNameWithoutExtension(item.Name)).ToArray();

                    this.cboCustom.Items.AddRange(fileNames);

                    if (!string.IsNullOrEmpty(defaultItem))
                    {
                        this.cboCustom.SelectedItem = defaultItem;
                    }
                }
            }
        }

        private void btnExchange_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.cboSourceDbType.Text) && !string.IsNullOrEmpty(this.cboTargetDbType.Text))
            {
                string temp = this.cboSourceDbType.Text;

                this.cboSourceDbType.Text = this.cboTargetDbType.Text;
                this.cboTargetDbType.Text = temp;
            }
        }

        private void btnLoadDefault_Click(object sender, EventArgs e)
        {
            this.cboCustom.SelectedItem = null;

            this.LoadMappings();
        }

        private void LoadMappings(bool isDefault = true)
        {
            string sourceDbTypeName = this.cboSourceDbType.Text;
            string targetDbTypeName = this.cboTargetDbType.Text;

            if (string.IsNullOrEmpty(sourceDbTypeName) || string.IsNullOrEmpty(targetDbTypeName))
            {
                MessageBox.Show("Please select database type");
                return;
            }

            var sourceDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), sourceDbTypeName);
            var targetDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), targetDbTypeName);

            string filePath = null;

            if (!isDefault)
            {
                string customMappingFolder = SettingManager.Setting.CustomMappingFolder;

                string customMappingSubFolder = DataTypeMappingManager.GetDataTypeMappingCustomSubFolder(sourceDbType, targetDbType, customMappingFolder);

                filePath = Path.Combine(customMappingSubFolder, this.cboCustom.Text + ".xml");
            }

            this.currentFilePath = filePath;

            var mappings = DataTypeMappingManager.GetDataTypeMappings(sourceDbType, targetDbType, filePath);

            this.LoadData(mappings);
        }

        private void LoadData(List<DataTypeMapping> mappings)
        {
            this.dgvData.Rows.Clear();

            foreach (var mapping in mappings)
            {
                var source = mapping.Source;
                var tartet = mapping.Target;

                int rowIndex = this.dgvData.Rows.Add(false, source.Type, source.IsExpression, tartet.Type, tartet.Length, tartet.Precision, tartet.Scale, tartet.Substitute, tartet.Args, this.isReadonly? "View": "Config");

                this.dgvData.Rows[rowIndex].Tag = mapping;
            }

            this.dgvData.ClearSelection();
        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var row = DataGridViewHelper.GetCurrentRow(this.dgvData);

            if (row == null)
            {
                return;
            }

            if (e.ColumnIndex == this.colSpecials.Index)
            {
                var targetDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), this.cboTargetDbType.Text);

                var specials = (row.Tag as DataTypeMapping).Specials;

                frmDataTypeMappingSpecial form = new frmDataTypeMappingSpecial(targetDbType, specials, this.isReadonly);

                DialogResult result = form.ShowDialog();

                if (result == DialogResult.OK)
                {
                    (row.Tag as DataTypeMapping).Specials = form.Specials;
                }
            }
        }

        private void tsbAdd_Click(object sender, EventArgs e)
        {
            this.AddRow();
        }

        private void AddRow()
        {
            DataGridViewRow row = new DataGridViewRow();

            int cellCount = this.dgvData.ColumnCount;

            for (int i = 0; i < cellCount; i++)
            {
                if (i == this.colCheck.Index || i == this.colIsExp.Index)
                {
                    var cell = new DataGridViewCheckBoxCell();

                    row.Cells.Add(cell);
                }
                else
                {
                    var cell = new DataGridViewTextBoxCell();

                    row.Cells.Add(cell);
                }
            }

            this.dgvData.Rows.Add(row);

            this.dgvData.CurrentCell = this.dgvData.Rows[this.dgvData.Rows.Count - 1].Cells[1];
            this.dgvData.Focus();
        }

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            this.dgvData.EndEdit();

            List<DataGridViewRow> checkedRows = this.GetCheckedRows();

            if (checkedRows.Count == 0)
            {
                MessageBox.Show("No any row selected.");
                return;
            }

            List<int> rowIndexes = new List<int>();

            foreach (DataGridViewRow row in checkedRows)
            {
                int rowIndex = row.Index;

                rowIndexes.Add(rowIndex);
            }

            rowIndexes.Sort();
            rowIndexes.Reverse();

            rowIndexes.ForEach(item => { this.dgvData.Rows.RemoveAt(item); });
        }

        private List<DataGridViewRow> GetCheckedRows()
        {
            List<DataGridViewRow> checkedRows = new List<DataGridViewRow>();

            var rows = this.dgvData.Rows;

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

        private void tsbSave_Click(object sender, EventArgs e)
        {
            this.Save();
        }

        private void Save(bool isSaveAs = false, bool validated = false, string defaultFileName = null)
        {
            try
            {
                if (!validated)
                {
                    this.dgvData.EndEdit();

                    this.dgvData.Invalidate();

                    bool isValid = this.ValidateGrid();

                    if (!isValid)
                    {
                        MessageBox.Show("The grid has invalid data!");
                        return;
                    }
                }

                var sourceDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), this.cboSourceDbType.Text);
                var targetDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), this.cboTargetDbType.Text);

                List<DataTypeMapping> mappings = new List<DataTypeMapping>();

                foreach (DataGridViewRow row in this.dgvData.Rows)
                {
                    if (this.IsFullRowEmpty(row))
                    {
                        continue;
                    }

                    DataTypeMapping mapping = new DataTypeMapping();

                    DataTypeMappingSource source = new DataTypeMappingSource();
                    DataTypeMappingTarget target = new DataTypeMappingTarget();

                    source.Type = row.Cells[this.colSourceType.Name].Value.ToString();
                    source.IsExpression = Convert.ToBoolean(row.Cells[this.colIsExp.Name].Value.ToString());

                    target.Type = row.Cells[this.colTargetType.Name].Value.ToString();
                    target.Length = row.Cells[this.colLength.Name].Value?.ToString();
                    target.Precision = row.Cells[this.colPrecision.Name].Value?.ToString();
                    target.Scale = row.Cells[this.colScale.Name].Value?.ToString();
                    target.Substitute = row.Cells[this.colSubstitute.Name].Value?.ToString();

                    mapping.Source = source;
                    mapping.Target = target;
                    mapping.Specials = (row.Tag as DataTypeMapping).Specials;

                    mappings.Add(mapping);
                }

                string filePath = this.currentFilePath;

                if (isSaveAs)
                {
                    frmInput form = new frmInput("Please input file name", defaultFileName);

                    var result = form.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        string content = form.Content;

                        string customMappingFolder = SettingManager.Setting.CustomMappingFolder;

                        string customMappingSubFolder = DataTypeMappingManager.GetDataTypeMappingCustomSubFolder(sourceDbType, targetDbType, customMappingFolder);

                        var fileNames = DataTypeMappingManager.GetCustomDataTypeMappingFileNames(customMappingSubFolder);

                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(content);

                        if (fileNames.Any(item => Path.GetFileNameWithoutExtension(item).ToLower() == fileNameWithoutExt.ToLower()))
                        {
                            var res = MessageBox.Show("The name is existing, are you sure to overwrite it?", "Question", MessageBoxButtons.YesNo);

                            if (res != DialogResult.Yes)
                            {
                                this.Save(true, true, content);
                                return;
                            }
                        }

                        filePath = Path.Combine(customMappingSubFolder, fileNameWithoutExt + ".xml");
                    }
                    else
                    {
                        return;
                    }
                }

                DataTypeMappingManager.SaveDataTypeMappings(sourceDbType, targetDbType, mappings, filePath);

                MessageBox.Show("Saved.");

                this.LoadCustomFileNames(this.cboCustom.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool ValidateGrid()
        {
            List<string> sourceTypes = new List<string>();

            var sourceDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), this.cboSourceDbType.Text);
            var targetDbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), this.cboTargetDbType.Text);

            for (int i = 0; i < this.dgvData.Rows.Count; i++)
            {
                var row = this.dgvData.Rows[i];

                if (this.IsFullRowEmpty(row))
                {
                    continue;
                }

                for (int j = 0; j < this.dgvData.ColumnCount; j++)
                {
                    var cell = row.Cells[j];

                    if (!this.IsCellValid(cell))
                    {
                        return false;
                    }

                    if (j == this.colSourceType.Index)
                    {
                        string sourceType = cell.Value.ToString();
                        bool isExpression = Convert.ToBoolean(row.Cells[this.colIsExp.Name].Value.ToString());

                        if (!isExpression)
                        {
                            var specifications = DataTypeManager.GetDataTypeSpecifications(sourceDbType);

                            if (!specifications.Any(item => item.Name.ToLower() == sourceType.ToLower()))
                            {
                                cell.ErrorText = $"{sourceType} is invalid!";
                                return false;
                            }
                        }

                        if (sourceTypes.Contains(sourceType))
                        {
                            cell.ErrorText = $"{sourceType} is duplicate!";

                            return false;
                        }

                        sourceTypes.Add(sourceType);
                    }
                    else if (j == this.colTargetType.Index)
                    {
                        string targetType = cell.Value.ToString();

                        var specifications = DataTypeManager.GetDataTypeSpecifications(targetDbType);

                        if (!specifications.Any(item => item.Name.ToLower() == targetType.ToLower()))
                        {
                            cell.ErrorText = $"{targetType} is invalid!";
                        }
                    }
                }
            }

            return true;
        }

        private bool IsFullRowEmpty(DataGridViewRow row)
        {
            int emptyValueCount = 0;

            for (int i = 0; i < this.dgvData.ColumnCount; i++)
            {
                string columnName = this.dgvData.Columns[i].Name;

                var value = row.Cells[i].Value;

                if (value == null || value == DBNull.Value || value?.ToString() == string.Empty)
                {
                    emptyValueCount++;
                }
            }

            return emptyValueCount == this.dgvData.ColumnCount - 1;
        }

        private void dgvData_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            int columnIndex = e.ColumnIndex;
            int rowIndex = e.RowIndex;

            if (columnIndex >= 0 && rowIndex >= 0)
            {
                if (this.dgvData.Columns[columnIndex].ReadOnly)
                {
                    return;
                }

                var cell = this.dgvData.Rows[rowIndex].Cells[columnIndex];

                string value = cell.Value?.ToString();
                string editValue = cell.EditedFormattedValue?.ToString();

                if (value != editValue)
                {
                    cell.Value = cell.EditedFormattedValue;
                }

                if (!this.IsCellValid(cell))
                {
                    return;
                }
                else
                {
                    cell.ErrorText = null;
                }
            }

            e.Cancel = false;
        }

        private bool IsCellValid(DataGridViewCell cell)
        {
            int columnIndex = cell.ColumnIndex;

            if (columnIndex == this.colSourceType.Index || columnIndex == this.colTargetType.Index)
            {
                if (this.IsNullValue(cell.Value))
                {
                    cell.ErrorText = "value is required!";
                    return false;
                }
            }
            else if (columnIndex == this.colLength.Index || columnIndex == this.colPrecision.Index || columnIndex == this.colScale.Index)
            {
                if (!this.IsNullValue(cell.Value) && !this.IsIntegerValue(cell.Value))
                {
                    cell.ErrorText = "value must be integer!";
                    return false;
                }
            }

            return true;
        }

        private bool IsNullValue(object value)
        {
            return value == null || value == DBNull.Value || value.ToString() == string.Empty;
        }

        private bool IsIntegerValue(object value)
        {
            return !this.IsNullValue(value) && int.TryParse(value.ToString(), out _);
        }

        private void dgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }

        private void tsbSaveAs_Click(object sender, EventArgs e)
        {
            this.Save(true, false);
        }

        private void cboSourceDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadCustomFileNames();
        }

        private void cboTargetDbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadCustomFileNames();
        }

        private void cboCustom_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = this.cboCustom.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedItem))
            {
                this.LoadMappings(false);
            }
        }
    }
}
