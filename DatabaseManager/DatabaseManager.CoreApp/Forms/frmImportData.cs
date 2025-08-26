using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core;
using DatabaseManager.FileUtility;
using DatabaseManager.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public partial class frmImportData : Form, IObserver<FeedbackInfo>
    {
        private DbInterpreter dbInterpreter;
        private Table table;

        public DatabaseInterpreter.Utility.FeedbackHandler OnFeedback;

        public frmImportData(DbInterpreter dbInterpreter, Table table)
        {
            InitializeComponent();

            this.dbInterpreter = dbInterpreter;
            this.table = table;
        }   

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.FileName = "";

            DialogResult result = this.openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                string filePath = this.openFileDialog1.FileName;

                this.txtFilePath.Text = filePath;
            }
        }

        private async void btnOK_Click(object sender, EventArgs e)
        {
            string filePath = this.txtFilePath.Text;

            if(string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("File Path can't be empty!");
                return;
            }

            ImportDataInfo info = new ImportDataInfo();

            info.FilePath = filePath;
            info.FirstRowIsColumnName = this.chkFirstRowIsColumnName.Checked;

            DataImporter importer = new DataImporter();

            importer.Subscribe(this);

            (bool Success, DataValidateResult ValidateResult) = await importer.Import(this.dbInterpreter, table, info);

            if (Success)
            {
                MessageBox.Show("Import successfully.");
            }
            else
            {
                MessageBox.Show("Import failed!");

                if (ValidateResult != null && ValidateResult.IsValid == false)
                {
                    string resultFilePath = importer.WriteValidateResultToExcel(ValidateResult, table, info.FirstRowIsColumnName);

                    if (File.Exists(resultFilePath))
                    {
                        var psi = new ProcessStartInfo(resultFilePath) { UseShellExecute = true };

                        Process.Start(psi);
                    }
                }               
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
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
