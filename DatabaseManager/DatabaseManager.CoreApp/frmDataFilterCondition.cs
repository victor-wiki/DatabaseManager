using DatabaseInterpreter.Core;
using DatabaseInterpreter.Utility;
using DatabaseManager.Helper;
using DatabaseManager.Model;
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
    public partial class frmDataFilterCondition : Form
    {
        public DataGridViewColumn Column { get; set; }

        public QueryConditionItem Condition { get; set; }

        public frmDataFilterCondition()
        {
            InitializeComponent();
        }

        private void frmDataFilterCondition_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.cboOperator.SelectedIndex = -1;
            this.txtValue.Text = "";
            this.txtFrom.Text = "";
            this.txtTo.Text = "";
            this.txtValues.Text = "";
        }

        private void InitControls()
        {
            this.rbSingle.Checked = true;

            this.SetValue();
        }

        public void SetValue()
        {
            QueryConditionItem condition = this.Condition;

            if (condition == null)
            {
                return;
            }

            if (condition.Mode == QueryConditionMode.Single)
            {
                this.rbSingle.Checked = true;
                this.cboOperator.Text = condition.Operator;
                this.txtValue.Text = condition.Value;
            }
            else if (condition.Mode == QueryConditionMode.Range)
            {
                this.rbRange.Checked = true;
                this.txtFrom.Text = condition.From;
                this.txtTo.Text = condition.To;
            }
            else if (condition.Mode == QueryConditionMode.Series)
            {
                this.rbSeries.Checked = true;
                this.txtValues.Text = string.Join(",", condition.Values);
            }

            this.SetControlEnabled();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            QueryConditionItem condition;

            if (!this.BuildCondition(out condition))
            {
                return;
            }

            this.Condition = condition;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private bool BuildCondition(out QueryConditionItem condition)
        {
            condition = new QueryConditionItem() { ColumnName = this.Column.Name, DataType = this.Column.ValueType };

            if (this.rbSingle.Checked)
            {
                if (this.cboOperator.SelectedIndex >= 0)
                {
                    if (string.IsNullOrEmpty(this.cboOperator.Text))
                    {
                        MessageBox.Show("Value can't be empty.");
                        return false;
                    }

                    condition.Mode = QueryConditionMode.Single;
                    condition.Operator = this.cboOperator.Text;
                    condition.Value = this.txtValue.Text;
                }
                else
                {
                    MessageBox.Show("Please select operator.");
                    return false;
                }
            }
            else if (this.rbRange.Checked)
            {
                string from = this.txtFrom.Text;
                string to = this.txtTo.Text;

                if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                {
                    MessageBox.Show("From and To value can't be empty.");
                    return false;
                }

                condition.Mode = QueryConditionMode.Range;
                condition.From = from.Trim();
                condition.To = to.Trim();
            }
            else if (this.rbSeries.Checked)
            {
                string values = this.txtValues.Text;

                if (string.IsNullOrWhiteSpace(values))
                {
                    MessageBox.Show("Value can't be empty.");
                    return false;
                }

                string[] items = values.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (items.Length == 0)
                {
                    MessageBox.Show("Has no any valid value.");
                    return false;
                }

                condition.Mode = QueryConditionMode.Series;
                condition.Values = items.ToList();
            }

            return true;
        }

        private string GetValue(string value)
        {
            bool needQuoted = FrontQueryHelper.NeedQuotedForSql(this.Column.ValueType);

            return needQuoted ? $"'{value}'" : value;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rbSingle_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlEnabled();           
        }

        private void rbRange_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlEnabled();            
        }

        private void rbSeries_CheckedChanged(object sender, EventArgs e)
        {
            this.SetControlEnabled();            
        }

        private void SetControlEnabled()
        {
            this.panelSingle.Enabled = this.rbSingle.Checked;
            this.panelRange.Enabled = this.rbRange.Checked;
            this.panelSeries.Enabled = this.rbSeries.Checked;
        }
    }
}
