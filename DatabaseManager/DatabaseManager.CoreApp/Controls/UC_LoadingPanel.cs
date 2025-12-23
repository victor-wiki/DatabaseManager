using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager.Controls
{
    public partial class UC_LoadingPanel : UserControl
    {
        private bool isShowed = false;
        private int buttonMargin = 20;
        private int buttonWidth = 100;
        private int buttonHeight = 23;
        private Control control = null;
        private Color backgroundColor = System.Drawing.SystemColors.ControlDark; // Color.FromArgb(120, Color.Black);

        public Color BackgroundColor
        {
            get
            {
                return this.backgroundColor;
            }
            set
            {
                this.backgroundColor = value;
            }
        }

        public bool InterruptButtonVisible { get; set; } = true;

        public CancellationTokenSource CancellationTokenSource { get; set; }


        public UC_LoadingPanel()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;

            this.panel1.BackColor = this.backgroundColor;

            this.progressBar1.Height =  this.progressBar1.Width = 0;
            this.btnInterrupt.Height =  this.btnInterrupt.Width = 0;
        }

        private async void btnInterrupt_Click(object sender, EventArgs e)
        {
            if (this.CancellationTokenSource != null)
            {
                await this.CancellationTokenSource.CancelAsync();

                this.HideLoading();
            }
        }

        public void ShowLoading(Control control)
        {
            this.control = control;

            this.ShowControls(control);

            this.isShowed = true;
        }

        private void ShowControls(Control control)
        {
            this.panel1.BackColor = this.backgroundColor;

            this.Width = control.Width;
            this.Height = control.Height;
            this.Top = control.Top;
            this.Left = control.Left;

            this.progressBar1.Width = this.btnInterrupt.Width = this.buttonWidth;
            this.progressBar1.Height = this.btnInterrupt.Height = this.buttonHeight;

            this.SetControlPosition();

            this.Visible = true;
        }

        public void RefreshStatus()
        {
            if (this.Visible && this.isShowed)
            {
                this.ShowControls(control);
            }
        }

        private void SetControlPosition()
        {
            this.btnInterrupt.Left = this.progressBar1.Left = (int)(this.Width - this.progressBar1.Width) / 2;

            if (!this.InterruptButtonVisible)
            {
                this.btnInterrupt.Visible = false;
                this.progressBar1.Top = (int)(this.Height - this.progressBar1.Height) / 2;
            }
            else
            {
                this.btnInterrupt.Visible = true;
                this.progressBar1.Top = (int)(this.Height - this.progressBar1.Height - this.btnInterrupt.Height - this.buttonMargin) / 2;
                this.btnInterrupt.Top = this.progressBar1.Top + this.progressBar1.Height + this.buttonMargin;
            }

            this.BringToFront();
        }      

        public void HideLoading()
        {
            this.Visible = false;
        }       
    }
}
