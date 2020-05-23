using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_Pagination : UserControl
    {
        private const int defaultPageSize = 10;

        private int pageSize = 10;
        private long pageCount = 0;
        private long totalCount = 0;
        private long pageNum = 1;
        private bool isSetting = false;

        public delegate void PageNumberChangeHandler(long pageNum);

        public event PageNumberChangeHandler OnPageNumberChanged;

        public UC_Pagination()
        {
            InitializeComponent();
            this.PageNum = 1;
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

                var currentPageNum = this.cboPageNum.Text;
                isSetting = true;
                this.cboPageNum.Items.Clear();

                for (var i = 1; i <= this.pageCount; i++)
                {
                    this.cboPageNum.Items.Add(i.ToString());

                    if (i.ToString() == currentPageNum)
                    {
                        this.cboPageNum.SelectedItem = i.ToString();
                    }
                }
                if (this.pageNum > this.pageCount)
                {
                    this.PageNum = this.pageCount;
                }

                if (this.pageCount == 0)
                {
                    this.btnFirst.Enabled = this.btnPrevious.Enabled = this.btnNext.Enabled = this.btnLast.Enabled = false;
                }

                isSetting = false;
                this.lblPageCount.Text = this.pageCount.ToString();
            }
        }

        public long PageNum
        {
            get
            {
                if (long.TryParse(this.cboPageNum.Text, out pageNum))
                {
                    pageNum = long.Parse(this.cboPageNum.Text);
                }
                else
                {
                    pageNum = 1;
                }

                return pageNum;
            }
            set
            {
                this.pageNum = value;

                if (this.pageNum < 1)
                {
                    this.pageNum = 1;
                }
                this.cboPageNum.Text = this.pageNum.ToString();
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

                if (this.pageNum > pCount)
                {
                    this.PageNum = pCount;
                }
                else
                {
                    this.ToPage(this.pageNum);
                }
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            this.PageNum = 1;
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (this.pageNum > 1)
            {
                this.PageNum--;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (this.pageNum < this.pageCount)
            {
                this.PageNum++;
            }
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            this.PageNum = this.pageCount;
        }

        private void ToPage(long pageNum)
        {
            this.btnFirst.Enabled = pageNum != 1;
            this.btnPrevious.Enabled = pageNum > 1;
            this.btnNext.Enabled = pageNum < this.pageCount;
            this.btnLast.Enabled = pageNum != this.pageCount;

            if (this.OnPageNumberChanged != null)
            {
                if (!isSetting)
                {
                    this.OnPageNumberChanged(pageNum);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (this.OnPageNumberChanged != null)
            {
                this.OnPageNumberChanged(this.pageNum);
            }
        }

        private void cboPageNum_SelectedValueChanged(object sender, EventArgs e)
        {
            if (long.TryParse(this.cboPageNum.Text, out pageNum))
            {
                this.pageNum = long.Parse(this.cboPageNum.Text);

                this.ToPage(this.pageNum);
            }
        }

        private void cboPageNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (long.TryParse(this.cboPageNum.Text, out pageNum))
                {
                    if (pageNum < 1)
                    {
                        this.PageNum = 0;
                    }
                    else if (pageNum > this.pageCount)
                    {
                        this.PageNum = this.pageCount;
                    }
                    else
                    {
                        this.PageNum = pageNum;
                    }
                }
            }
        }
    }
}
