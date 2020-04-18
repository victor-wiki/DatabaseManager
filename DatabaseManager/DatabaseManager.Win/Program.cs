using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                Application.Run(new frmMain());
            }
            catch(Exception ex)
            {
                string errMsg = ExceptionHelper.GetExceptionDetails(ex);

                LogHelper.LogError(errMsg);

                MessageBox.Show(errMsg);
            }
        }
    }
}
