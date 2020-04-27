using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Model;

namespace DatabaseManager.Core
{
    public class ScriptTemplate
    {
        private DbInterpreter dbInterpreter;
        private const string commonTemplateFileName = "Common";
        private const string commonTemplateFileExtension = ".txt";
        public string TemplateFolder => Path.Combine(PathHelper.GetAssemblyFolder(), "Config/Template");

        public ScriptTemplate(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
        }

        public string GetTemplateContent(DatabaseObjectType databaseObjectType, ScriptAction scriptAction)           
        {
            string scriptTypeName = databaseObjectType.ToString();
            string scriptTypeFolder = Path.Combine(TemplateFolder, scriptTypeName);

            string scriptTemplateFilePath = Path.Combine(scriptTypeFolder, this.dbInterpreter.DatabaseType.ToString() + commonTemplateFileExtension);

            if(!File.Exists(scriptTemplateFilePath))
            {
                scriptTemplateFilePath = Path.Combine(scriptTypeFolder, commonTemplateFileName + commonTemplateFileExtension);
            }
            
            if(!File.Exists(scriptTemplateFilePath))
            {
                return string.Empty;
            }

            string templateContent = File.ReadAllText(scriptTemplateFilePath);

            templateContent = this.ReplaceTemplatePlaceHolders(templateContent, databaseObjectType, scriptAction);

            return templateContent;
        }

        private string ReplaceTemplatePlaceHolders(string templateContent, DatabaseObjectType databaseObjectType, ScriptAction scriptAction)
        {
            templateContent = templateContent.Replace("$ACTION$", scriptAction.ToString())
                .Replace("$NAME$", this.dbInterpreter.GetQuotedString($"{databaseObjectType.ToString().ToUpper()}_NAME"))
                .Replace("$TABLE_NAME$", this.dbInterpreter.GetQuotedString($"TABLE_NAME"));           

            return templateContent;
        }
    }
}
