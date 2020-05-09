using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager.Helper
{
    public delegate void SaveHandler();  
    public delegate void RunScriptsHandler();
    public delegate void RefreshNavigatorFolderHandler();

    public class FormEventCenter
    {       
        public static SaveHandler OnSave;
        public static RunScriptsHandler OnRunScripts;
        public static RefreshNavigatorFolderHandler OnRefreshNavigatorFolder;
    }
}
