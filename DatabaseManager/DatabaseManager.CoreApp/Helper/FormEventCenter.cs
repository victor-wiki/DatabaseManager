namespace DatabaseManager.Helper
{
    public delegate void RefreshNavigatorFolderHandler();

    public class FormEventCenter
    {       
        public static RefreshNavigatorFolderHandler OnRefreshNavigatorFolder;
    }
}
