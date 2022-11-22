using DatabaseInterpreter.Model;
using System.Collections.Generic;

namespace DatabaseConverter.Core.Model
{
    public class DbConverterOption
    {
        public bool EnsurePrimaryKeyNameUnique { get; set; } = true;
        public bool EnsureIndexNameUnique { get; set; } = true;
        public bool SplitScriptsToExecute { get; set; }
        public bool ExecuteScriptOnTargetServer { get; set; } = true;
        public GenerateScriptMode GenerateScriptMode { get; set; } = GenerateScriptMode.Schema | GenerateScriptMode.Data;
        public bool BulkCopy { get; set; }
        public bool UseTransaction { get; set; }
        /// <summary>
        /// For function, procedure, trigger and view
        /// </summary>
        public bool ContinueWhenErrorOccurs { get; set; }
        public bool OnlyForTranslate { get; set; }
        public bool OnlyForTableCopy { get; set; }
        public bool ConvertComputeColumnExpression { get; set; }
        public bool OnlyCommentComputeColumnExpressionInScript { get; set; }
        public bool RenameTableChildren { get; set; }
        public bool IgnoreNotSelfForeignKey { get; set; }
        public bool UseOriginalDataTypeIfUdtHasOnlyOneAttr { get; set; } = true;
        public bool RemoveCarriagRreturnChar { get; set; }
        public bool CreateSchemaIfNotExists { get; set; }
        public bool ConvertConcatChar { get; set; }
        public bool NcharToDoubleChar { get; set; } = true;
        public bool CollectTranslateResultAfterTranslated { get; set; } = true;
        public bool OutputRemindInformation { get; set; } = true;

        public List<SchemaMappingInfo> SchemaMappings = new List<SchemaMappingInfo>();
    }
}
