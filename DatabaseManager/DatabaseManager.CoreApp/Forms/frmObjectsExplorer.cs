using DatabaseManager.Controls;
using DockSample;
using System.Reflection.Metadata;
using WeifenLuo.WinFormsUI.Docking;

namespace DatabaseManager.Forms
{
    public partial class frmObjectsExplorer : frmDockWindowBase
    {
        public UC_DbObjectsExplorer Explorer => this.ucExplorer;

        public frmObjectsExplorer()
        {
            InitializeComponent();            
        }       
    }
}
