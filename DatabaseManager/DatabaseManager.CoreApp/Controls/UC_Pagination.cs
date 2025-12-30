using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DatabaseManager.Helper;
using FontAwesome.Sharp;

namespace DatabaseManager.Controls
{
    public partial class UC_Pagination : UserControl
    {
        private const int defaultPageSize = 10;

        private int pageSize = 10;
        private long pageCount = 0;
        private long totalCount = 0;
        private long pageNumber = 1;
        private bool isSetting = false;
        private Color imageColor = IconImageHelper.DataViewerToolbarColor;

        public delegate void PageNumberChangeHandler(long pageNumber);

        public event PageNumberChangeHandler OnPageNumberChanged;

        public UC_Pagination()
        {
            InitializeComponent();

            this.Init();
        }

        private void Init()
        {
            this.PageNumber = 1;

            this.btnFirst.Image = IconImageHelper.GetImage(IconChar.BackwardStep, this.imageColor);
            this.btnPrevious.Image = IconImageHelper.GetImage(IconChar.ChevronLeft, this.imageColor, this.btnPrevious.Width);
            this.btnNext.Image = IconImageHelper.GetImage(IconChar.ChevronRight, this.imageColor, this.btnNext.Width);
            this.btnLast.Image = IconImageHelper.GetImage(IconChar.ForwardStep, this.imageColor);
            this.btnRefresh.Image = IconImageHelper.GetImage(IconChar.ArrowsRotate, this.imageColor);
        }

        public int PageSize
        {
            get
            {
                if (int.TryParse(this.cboPageSize.Text, out this.pageSize))
                {
                    this.pageSize = int.Parse(this.cboPageSize.Text);
                }
                else
                {
                    this.pageSize = defaultPageSize;
                }

                return this.pageSize;
            }
            set
            {
                this.pageSize = value;

                this.cboPageSize.Text = this.pageSize.ToString();
            }
        }

        public long PageCount
        {
            get
            {
                return this.pageCount;
            }
            set
            {
                this.pageCount = value;

                var currentPageNum = this.cboPageNumber.Text;
                isSetting = true;
                this.cboPageNumber.Items.Clear();

                for (var i = 1; i <= this.pageCount; i++)
                {
                    this.cboPageNumber.Items.Add(i.ToString());

                    if (i.ToString() == currentPageNum)
                    {
                        this.cboPageNumber.SelectedItem = i.ToString();
                    }
                }
                if (this.pageNumber > this.pageCount)
                {
                    this.PageNumber = this.pageCount;
                }

                if (this.pageCount == 0)
                {
                    this.btnFirst.Enabled = this.btnPrevious.Enabled = this.btnNext.Enabled = this.btnLast.Enabled = false;
                }

                isSetting = false;
                this.lblPageCount.Text = this.pageCount.ToString();
            }
        }

        public long PageNumber
        {
            get
            {
                if (long.TryParse(this.cboPageNumber.Text, out pageNumber))
                {
                    pageNumber = long.Parse(this.cboPageNumber.Text);
                }
                else
                {
                    pageNumber = 1;
                }

                return pageNumber;
            }
            set
            {
                this.pageNumber = value;

                if (this.pageNumber < 1)
                {
                    this.pageNumber = 1;
                }
                this.cboPageNumber.Text = this.pageNumber.ToString();
            }
        }

        public long TotalCount
        {
            get
            {
                return this.totalCount;
            }
            set
            {
                this.totalCount = value;
                this.lblTotalCount.Text = $"Total:{totalCount}";

                if (this.pageSize != 0)
                {
                    this.PageCount = this.totalCount % this.pageSize == 0 ? this.totalCount / this.pageSize : this.totalCount / this.pageSize + 1;
                }
                else
                {
                    this.PageCount = 0;
                }
            }
        }

        private void cboPageSize_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(this.cboPageSize.Text, out this.pageSize))
            {
                this.pageSize = int.Parse(this.cboPageSize.Text);

                var pCount = this.totalCount % this.pageSize == 0 ? this.totalCount / this.pageSize : this.totalCount / this.pageSize + 1;

                if (this.pageNumber > pCount)
                {
                    this.PageNumber = pCount;
                }
                else
                {
                    this.ToPage(this.pageNumber);
                }
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            this.PageNumber = 1;
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (this.pageNumber > 1)
            {
                this.PageNumber--;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (this.pageNumber < this.pageCount)
            {
                this.PageNumber++;
            }
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            this.PageNumber = this.pageCount;
        }

        private void ToPage(long pageNumber)
        {
            this.btnFirst.Enabled = this.pageNumber != 1;
            this.btnPrevious.Enabled = this.pageNumber > 1;
            this.btnNext.Enabled = this.pageNumber < this.pageCount;
            this.btnLast.Enabled = this.pageNumber != this.pageCount;

            if (this.OnPageNumberChanged != null)
            {
                if (!isSetting)
                {
                    this.OnPageNumberChanged(pageNumber);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (this.OnPageNumberChanged != null)
            {
                this.OnPageNumberChanged(this.pageNumber);
            }
        }

        private void cboPageNum_SelectedValueChanged(object sender, EventArgs e)
        {
            if (long.TryParse(this.cboPageNumber.Text, out pageNumber))
            {
                this.pageNumber = long.Parse(this.cboPageNumber.Text);

                this.ToPage(this.pageNumber);
            }
        }

        private void cboPageNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (long.TryParse(this.cboPageNumber.Text, out pageNumber))
                {
                    if (pageNumber < 1)
                    {
                        this.PageNumber = 0;
                    }
                    else if (pageNumber > this.pageCount)
                    {
                        this.PageNumber = this.pageCount;
                    }
                    else
                    {
                        this.PageNumber = pageNumber;
                    }
                }
            }
        }
    }
}
