using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.Core.Model;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmStatistic : Form
    {
        private DatabaseType databaseType { get; set; }
        private ConnectionInfo connectionInfo { get; set; }
        private string schema;
        private DbStatistic statistic;

        public frmStatistic(DatabaseType databaseType, ConnectionInfo connectionInfo, string schema)
        {
            InitializeComponent();

            this.databaseType = databaseType;
            this.connectionInfo = connectionInfo;
            this.schema = schema;

            this.statistic = new DbStatistic(this.databaseType, this.connectionInfo);
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.statistic.Subscribe(observer);
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.rbTableRowCount.Checked)
                {
                    IEnumerable<TableRecordCount> records = await this.statistic.CountTableRecords();

                    frmTableRecordCount form = new frmTableRecordCount();

                    form.LoadData(records);

                    form.ShowDialog();
                }
                else if(this.rbColumnContentLength.Checked)
                {
                    IEnumerable<TableColumnContentMaxLength> records = await this.statistic.GetTableColumnContentLengths();

                    frmTableColumnContentMaxLength form = new frmTableColumnContentMaxLength();

                    form.LoadData(records);

                    form.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Please select an item.");
                }
            }
            catch (Exception ex)
            {
                string errMsg = ExceptionHelper.GetExceptionDetails(ex);

                LogHelper.LogError(errMsg);

                MessageBox.Show(errMsg);               
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
