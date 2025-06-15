using DatabaseInterpreter.Utility;
using DockSample;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseManager.Forms
{
    public partial class frmMessage : frmDockWindowBase
    {
        public frmMessage()
        {
            InitializeComponent();
        }

        public void ShowMessage(string message)
        {
            this.txtMessage.Text = message;
        }

        public void ShowMessage(FeedbackInfo info)
        {
            this.txtMessage.ForeColor = Color.Black;

            if (info.InfoType == FeedbackInfoType.Error)
            {
                if (!info.IgnoreError)
                {
                    MessageBox.Show(info.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                this.txtMessage.Text = info.Message;               
                this.txtMessage.ForeColor = Color.Red;
            }
            else
            {
                this.txtMessage.Text = info.Message;
            }
        }
    }
}
