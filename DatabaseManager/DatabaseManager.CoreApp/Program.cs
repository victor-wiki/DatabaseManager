using DatabaseInterpreter.Core;
using DatabaseManager.Core;
using DatabaseManager.Profile.Manager;
using System;
using System.Windows.Forms;

namespace DatabaseManager
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DbInterpreter.Setting = SettingManager.GetInterpreterSetting();

            ProfileBaseManager.Init();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
    }
}
