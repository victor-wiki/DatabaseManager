using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Export;
using DatabaseManager.FileUtility;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmExportData : Form, IObserver<FeedbackInfo>
    {
        private DbInterpreter dbInterpreter;
        private Table table;

        public DatabaseInterpreter.Utility.FeedbackHandler OnFeedback;

        public frmExportData(DbInterpreter dbInterpreter, Table table)
        {
            InitializeComponent();

            this.dbInterpreter = dbInterpreter;
            this.table = table;
        }

        private void frmExportData_Load(object sender, EventArgs e)
        {
            this.Init();
        }

        private void Init()
        {
            this.cboFileType.SelectedIndex = 0;
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            string tableName = this.table.Name;

            if(!string.IsNullOrEmpty(tableName))
            {
                this.saveFileDialog1.FileName = tableName;
            }

            if (this.cboFileType.SelectedIndex == 0)
            {
                this.saveFileDialog1.Filter = "(*.csv)|*.csv";
            }
            else
            {
                this.saveFileDialog1.Filter = "(*.xlsx)|*.xlsx";
            }

            DialogResult result = this.saveFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.txtFilePath.Text = this.saveFileDialog1.FileName;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.ExportData();
        }

        private async void ExportData()
        {
            ExportDataOption option = new ExportDataOption();

            option.FileType = this.cboFileType.SelectedIndex == 0 ? ExportFileType.CSV : ExportFileType.EXCEL;
            option.ShowColumnNames = this.chkShowColumnName.Checked;
            option.FilePath = this.txtFilePath.Text;

            DataExporter exporter = new DataExporter();

            exporter.Subscribe(this);

            (bool Success, string FilePath) = await exporter.Export(dbInterpreter, table.Name, option);

            if (Success)
            {
                MessageBox.Show("Export successfully.");

                if (string.IsNullOrEmpty(option.FilePath))
                {
                    if (File.Exists(FilePath))
                    {
                        string cmd = "explorer.exe";
                        string arg = "/select," + FilePath;

                        var psi = new ProcessStartInfo(cmd, arg) { UseShellExecute = true };

                        Process.Start(psi);
                    }
                }
            }
            else
            {
                MessageBox.Show("Export failed.");
            }
        }

        public void OnNext(FeedbackInfo value)
        {
            this.Feedback(value);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        private void Feedback(FeedbackInfo info)
        {
            if (this.OnFeedback != null)
            {
                this.OnFeedback(info);
            }
        }

        private void Feedback(string message)
        {
            this.Feedback(new FeedbackInfo() { Message = message });
        }
    }
}
