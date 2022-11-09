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
