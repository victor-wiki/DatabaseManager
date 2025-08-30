using DatabaseManager.Controls;

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
