using DatabaseInterpreter.Model;
using System.Collections.Generic;

namespace DatabaseManager.Model
{
    public class Setting : DbInterpreterSetting
    {
        public bool UseOriginalDataTypeIfUdtHasOnlyOneAttr { get; set; } = true;
        public DatabaseType PreferredDatabase { get; set; }
        public bool RememberPasswordDuringSession { get; set; } = true;
        public bool EnableEditorHighlighting { get; set; } = true;
        public bool EnableEditorIntellisence { get; set; } = true;
        public string ScriptsDefaultOutputFolder { get; set; }
        public bool ValidateScriptsAfterTranslated { get; set; } = true;
        public List<string> ConvertConcatCharTargetDatabases { get; set; } = new List<string>();
        public TextEditorOption TextEditorOption { get; set; } = new TextEditorOption();
        public ThemeOption ThemeOption { get; set; } = new ThemeOption();
        public string CustomMappingFolder { get; set; }
    }

    public class TextEditorOption
    {
        public bool ShowLineNumber { get; set; } = true;
        public string FontName { get; set; } = "Courier New";
        public float FontSize { get; set; } = 10;
    }

    public class ThemeOption
    {
        public ThemeType ThemeType { get; set; } = ThemeType.Light;
    }

    public enum ThemeType
    {
        Light = 1,
        Blue = 2,
        Dark = 3
    }
}
