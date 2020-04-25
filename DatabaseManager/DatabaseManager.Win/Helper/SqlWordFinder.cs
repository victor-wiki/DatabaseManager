using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseManager.Data;
using DatabaseInterpreter.Model;
using DatabaseConverter.Core;

namespace DatabaseManager.Helper
{
    public class SqlWordFinder
    {
        public static List<SqlWord> FindWords(DatabaseType databaseType, string search, SqlWordTokenType tokenType = SqlWordTokenType.None, string parentName = null)
        {
            List<SqlWord> words = new List<SqlWord>();

            //if (IsTypeMatched(tokenType, SqlWordTokenType.Keyword))
            //{
            //    var keywords = KeywordManager.GetKeywords(databaseType).Where(item => Contains(item, search));

            //    words.AddRange(keywords.Select(item => new SqlWord() { Type = SqlWordTokenType.Keyword, Text = item }));
            //}

            if (IsTypeMatched(tokenType, SqlWordTokenType.BuiltinFunction))
            {
                var builtinFunctions = FunctionManager.GetFunctionSpecifications(databaseType).Where(item => Contains(item.Name, search));

                words.AddRange(builtinFunctions.Select(item => new SqlWord() { Type = SqlWordTokenType.BuiltinFunction, Text = item.Name, Source = item }));
            }

            SchemaInfo schemaInfo = DataStore.GetSchemaInfo(databaseType);

            if (schemaInfo != null)
            {
                if (IsTypeMatched(tokenType, SqlWordTokenType.Table))
                {
                    var tables = schemaInfo.Tables.Where(item => Contains(item.Name, search));

                    words.AddRange(tables.Select(item => new SqlWord() { Type = SqlWordTokenType.Table, Text = item.Name, Source = item }));
                }

                if (IsTypeMatched(tokenType, SqlWordTokenType.Function))
                {
                    var tables = schemaInfo.Functions.Where(item => Contains(item.Name, search));

                    words.AddRange(tables.Select(item => new SqlWord() { Type = SqlWordTokenType.Function, Text = item.Name, Source = item }));
                }

                if (IsTypeMatched(tokenType, SqlWordTokenType.View))
                {
                    var views = schemaInfo.Views.Where(item => Contains(item.Name, search));

                    words.AddRange(views.Select(item => new SqlWord() { Type = SqlWordTokenType.View, Text = item.Name, Source = item }));
                }

                if (IsTypeMatched(tokenType, SqlWordTokenType.TableColumn))
                {
                    IEnumerable<TableColumn> columns = schemaInfo.TableColumns;

                    if (!string.IsNullOrEmpty(parentName))
                    {
                        columns = schemaInfo.TableColumns.Where(item => item.TableName.ToUpper() == parentName.ToUpper());
                    }

                    if (!string.IsNullOrEmpty(search))
                    {
                        columns = columns.Where(item => Contains(item.Name, search));
                    }

                    words.AddRange(columns.Select(item => new SqlWord() { Type = SqlWordTokenType.TableColumn, Text = item.Name, Source = item }));
                }
            }

            return words;
        }

        public static bool IsTypeMatched(SqlWordTokenType tokenType, SqlWordTokenType currentType)
        {
            if (tokenType == SqlWordTokenType.None || tokenType.HasFlag(currentType))
            {
                return true;
            }
            return false;
        }

        public static bool Contains(string source, string search)
        {
            return search == null || source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
