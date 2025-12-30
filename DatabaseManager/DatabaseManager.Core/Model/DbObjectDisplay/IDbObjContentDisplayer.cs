namespace DatabaseManager.Core.Model
{
    public interface IDbObjContentDisplayer
    {
        void Show(DatabaseObjectDisplayInfo displayInfo);
        ContentSaveResult Save(ContentSaveInfo saveInfo);
    }
}
