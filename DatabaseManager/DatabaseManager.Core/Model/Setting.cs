using DatabaseInterpreter.Model;

namespace DatabaseManager.Model
{
    public class Setting: DbInterpreterSetting
    {      
        public bool UseOriginalDataTypeIfUdtHasOnlyOneAttr { get; set; } = true;       
        public DatabaseType PreferredDatabase { get; set; }
        public bool RememberPasswordDuringSession { get; set; } = true;
        public bool EnableEditorHighlighting { get; set; } = true;
        public bool EnableEditorIntellisence { get; set; } = true;    
        public string ScriptsDefaultOutputFolder { get; set; }
    }   
}
