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

        public string GetTemplateContent(DatabaseObjectType databaseObjectType, ScriptAction scriptAction, DatabaseObject databaseObject)           
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

            templateContent = this.ReplaceTemplatePlaceHolders(templateContent, databaseObjectType, scriptAction, databaseObject);

            return templateContent;
        }

        private string ReplaceTemplatePlaceHolders(string templateContent, DatabaseObjectType databaseObjectType, ScriptAction scriptAction, DatabaseObject databaseObject)
        {
            string nameTemplate = $"{databaseObjectType.ToString().ToUpper()}_NAME";

            string name = this.dbInterpreter.DatabaseType == DatabaseType.SqlServer ? this.dbInterpreter.GetQuotedDbObjectNameWithSchema(databaseObject?.Schema, nameTemplate)
                : this.dbInterpreter.GetQuotedString(nameTemplate);

            string tableName = databaseObjectType == DatabaseObjectType.Trigger && databaseObject!=null ? this.dbInterpreter.GetQuotedDbObjectNameWithSchema(databaseObject)
                            : this.dbInterpreter.GetQuotedString($"TABLE_NAME");


            templateContent = templateContent.Replace("$ACTION$", scriptAction.ToString())
                .Replace("$NAME$", name)
                .Replace("$TABLE_NAME$", tableName);           

            return templateContent;
        }
    }
}
