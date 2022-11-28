using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DatabaseManager.Forms
{
    public delegate void FindBoxHandler();
    public delegate void FindBoxClosedHandler();

    public partial class frmFindBox : Form
    {
        private bool showOptions = false;

        public string FindWord { get; private set; }
        public bool MatchCase { get; private set; }
        public bool MatchWholeWord { get; private set; }
        public event FindBoxHandler OnFind;
        public event FindBoxClosedHandler OnEndFind;

        public frmFindBox(bool showOptions = false)
        {
            InitializeComponent();

            this.showOptions = showOptions;
        }

        private void frmFindBox_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            if (!this.showOptions)
            {
                this.optionsPanel.Visible = false;
                this.Height -= this.optionsPanel.Height;
            }
            else
            {
                this.chkMatchWholeWord.Checked = true;
            }

            this.txtWord.Focus();
        }

        private void txtWord_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.Find();
            }
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            this.Find();
        }

        private void Find()
        {
            string word = this.txtWord.Text.Trim();

            if (string.IsNullOrEmpty(word))
            {
                MessageBox.Show("Please enter a word.");
                return;
            }

            this.FindWord = word;
            this.MatchCase = this.chkMatchCase.Checked;
            this.MatchWholeWord = this.chkMatchWholeWord.Checked;

            if (this.OnFind != null)
            {
                this.OnFind();

                this.btnFind.Focus();
            }
            else
            {
                this.Close();

                this.DialogResult = DialogResult.OK;
            }
        }

        private void frmFindBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.OnEndFind != null)
            {
                this.OnEndFind();
            }
        }
    }
}
