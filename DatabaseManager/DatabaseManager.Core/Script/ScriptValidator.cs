using DatabaseConverter.Core;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;

namespace DatabaseManager.Core
{
    public class ScriptValidator
    {
        public static SqlSyntaxError ValidateSyntax(DatabaseType databaseType, string script)
        {
            var sqlAnalyser = TranslateHelper.GetSqlAnalyser(databaseType);

            SqlSyntaxError sqlSyntaxError = sqlAnalyser.Validate(script);

            return sqlSyntaxError;
        }
    }
}
