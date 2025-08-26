using DatabaseManager.Helper;
using DatabaseManager.Model;
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

            this.cboFilterMode.SelectedIndex = 0;
        }

        private void txtFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if(this.Query!=null)
                {
                    this.Query(this.FilterContent, this.FilterMode);
                }
            }
        }

        public void ClearContent()
        {
            this.txtFilter.Text = string.Empty;
        }
    }
}
