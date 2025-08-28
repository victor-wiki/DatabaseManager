using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Helper;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmDataTypeMappingSpecial : Form
    {
        private DatabaseType targetDatabaseType;
        private List<DataTypeMappingSpecial> specials;
        private bool isReadonly;

        public List<DataTypeMappingSpecial> Specials => this.specials;

        public frmDataTypeMappingSpecial(DatabaseType targetDatabaseType, List<DataTypeMappingSpecial> specials, bool isReadony = false)
        {
            InitializeComponent();

            this.targetDatabaseType = targetDatabaseType;
            this.specials = specials;
            this.isReadonly = isReadony;

            this.Init();
        }

        private void Init()
        {
            if(!this.isReadonly)
            {
                this.tsbAdd.Image = IconImageHelper.GetImageByFontType(IconChar.Plus, IconFont.Solid, null, this.tsbAdd.Width);
                this.tsbDelete.Image = IconImageHelper.GetImage(IconChar.Times, null, this.tsbDelete.Width);
                this.tsbCommit.Image = IconImageHelper.GetImage(IconChar.Check, null, this.tsbCommit.Width);
            }
            else
            {
                this.toolStrip1.Visible = false;
                this.dgvData.Top = 0;
                this.dgvData.Height += this.toolStrip1.Height;

                this.colCheck.Visible = false;

                foreach (DataGridViewColumn col in this.dgvData.Columns)
                {
                    col.ReadOnly = true;
                }
            }            
        }

        private void frmDataTypeMappingSpecial_Load(object sender, EventArgs e)
        {
            this.LoadData();
        }

        private void LoadData()
        {
            this.dgvData.Rows.Clear();

            if (this.specials != null)
            {
                foreach (var special in this.specials)
                {
                    int rowIndex = this.dgvData.Rows.Add(false, special.Name, special.Value, special.Precision, special.Scale, special.Type, special.TargetMaxLength, special.NoLength, special.Substitute);

                    this.dgvData.Rows[rowIndex].Tag = special;
                }
            }

            this.dgvData.ClearSelection();
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
                if (i == this.colCheck.Index || i == this.colNoLength.Index)
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

        private bool ValidateGrid()
        {
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

                    if (j == this.colTargetType.Index)
                    {
                        string targetType = cell.Value.ToString();

                        var specifications = DataTypeManager.GetDataTypeSpecifications(this.targetDatabaseType);

                        if (!specifications.Any(item => item.Name.ToLower() == targetType.ToLower()))
                        {
                            cell.ErrorText = $"{targetType} is invalid!";
                            return false;
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

        private bool IsCellValid(DataGridViewCell cell)
        {
            int columnIndex = cell.ColumnIndex;

            if (columnIndex == this.colName.Index)
            {
                if (this.IsNullValue(cell.Value))
                {
                    cell.ErrorText = "value is requried!";
                    return false;
                }
            }
            else if (columnIndex == this.colTargetMaxLength.Index || columnIndex == this.colPrecision.Index || columnIndex == this.colScale.Index)
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

        private void tsbCommit_Click(object sender, EventArgs e)
        {
            this.dgvData.Invalidate();

            bool isValid = this.ValidateGrid();

            if (!isValid)
            {
                MessageBox.Show("The grid has invalid data!");
                return;
            }

            List<DataTypeMappingSpecial> list = new List<DataTypeMappingSpecial>();

            foreach (DataGridViewRow row in this.dgvData.Rows)
            {
                if (this.IsFullRowEmpty(row))
                {
                    continue;
                }

                DataTypeMappingSpecial special = new DataTypeMappingSpecial();

                special.Name = row.Cells[this.colName.Name].Value.ToString();
                special.Value = row.Cells[this.colValue.Name].Value?.ToString();
                special.Precision = row.Cells[this.colPrecision.Name].Value?.ToString();
                special.Scale = row.Cells[this.colScale.Name].Value?.ToString();
                special.Type = row.Cells[this.colTargetType.Name].Value?.ToString();
                special.TargetMaxLength = row.Cells[this.colTargetMaxLength.Name].Value?.ToString();
                special.NoLength = Convert.ToBoolean(row.Cells[this.colNoLength.Name].Value.ToString());
                special.Substitute = row.Cells[this.colSubstitute.Name].Value?.ToString();

                list.Add(special);
            }

            this.specials = list;

            this.DialogResult = DialogResult.OK;
        }

        private void dgvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvData.ClearSelection();
        }

        private void dgvData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }
    }
}
