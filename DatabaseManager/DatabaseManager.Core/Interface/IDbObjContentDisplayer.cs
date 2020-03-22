using DatabaseManager.Model;

namespace DatabaseManager.Core
{
    public interface IDbObjContentDisplayer
    {
        void Show(DatabaseObjectDisplayInfo displayInfo);
        void Save(string filePath);
    }
}
