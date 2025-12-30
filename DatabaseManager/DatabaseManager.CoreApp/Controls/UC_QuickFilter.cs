using DatabaseManager.Core.Model;
using DatabaseManager.Helper;
using FontAwesome.Sharp;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public delegate void QuickQueryHandler(string content, FilterMode mode);

    public partial class UC_QuickFilter : UserControl
    {
        public event QuickQueryHandler Query;

        public string FilterContent => this.txtFilter.Text.Trim();

        public FilterMode FilterMode
        {
            get
            {
                FilterMode mode = FilterMode.Contains;

                int selectedIndex = this.cboFilterMode.SelectedIndex;

                switch (selectedIndex)
                {
                    case 0:
                        mode = FilterMode.Contains;
                        break;
                    case 1:
                        mode = FilterMode.Equals;
                        break;
                    case 2:
                        mode = FilterMode.SQL;
                        break;
                }

                return mode;
            }
        }

        public UC_QuickFilter()
        {
            InitializeComponent();

            this.Init();
        }

        private void Init()
        {
            this.btnFilter.Image = IconImageHelper.GetImageByFontType(IconChar.Filter, IconFont.Solid);
            this.pbClearContent.Image = IconImageHelper.GetImageByFontType(IconChar.Close, IconFont.Auto, Color.Gray, 16);         

            this.cboFilterMode.SelectedIndex = 0;
        }

        private void txtFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (this.Query != null)
                {
                    this.Query(this.FilterContent, this.FilterMode);
                }
            }
        }

        public void ClearContent()
        {
            this.txtFilter.Text = string.Empty;
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            this.pbClearContent.Visible = this.txtFilter.Text.Length > 0;
        }

        private void pbClearContent_Click(object sender, EventArgs e)
        {
            this.txtFilter.Text = string.Empty;
            this.pbClearContent.Visible = false;

            if (this.Query != null)
            {
                this.Query(this.FilterContent, this.FilterMode);
            }
        }
    }
}
