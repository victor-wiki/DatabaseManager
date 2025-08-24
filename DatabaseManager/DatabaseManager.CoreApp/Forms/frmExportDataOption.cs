using DatabaseInterpreter.Utility;
using DatabaseManager.FileUtility;
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
    public partial class frmExportDataOption : Form
    {
        private ExportDataOption option;

        public ExportDataOption Option => this.option;

        public frmExportDataOption()
        {
            InitializeComponent();
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

        private void btnCancel_Click(object sender, EventArgs e)
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

            this.option = option;

            this.DialogResult = DialogResult.OK;

            this.Close();
        }
    }
}
