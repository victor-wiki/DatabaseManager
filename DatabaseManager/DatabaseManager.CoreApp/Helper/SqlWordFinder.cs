using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Data;
using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;

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

            SchemaInfo schemaInfo = DataStore.GetSchemaInfo(databaseType);

            if (schemaInfo != null)
            {
                if (IsTypeMatched(tokenType, SqlWordTokenType.Schema))
                {
                    var owners = schemaInfo.Tables.Where(item => ContainsWithNull(item.Schema, search)).Select(item => item.Schema).Distinct();

                    words.AddRange(owners.Select(item => new SqlWord() { Type = SqlWordTokenType.Schema, Text = item, Source = item }));
                }

                FilterDbObjects(words, schemaInfo.Tables, SqlWordTokenType.Table, tokenType, search, parentName);

                FilterDbObjects(words, schemaInfo.Views, SqlWordTokenType.View, tokenType, search, parentName);

                if (tokenType == SqlWordTokenType.TableColumn)
                {
                    IEnumerable<TableColumn> columns = schemaInfo.TableColumns;

                    if (!string.IsNullOrEmpty(parentName))
                    {
                        columns = schemaInfo.TableColumns.Where(item => item.TableName.ToUpper() == parentName.ToUpper());
                    }

                    if (!string.IsNullOrEmpty(search))
                    {
                        columns = columns.Where(item => ContainsWithNull(item.Name, search));
                    }

                    words.AddRange(columns.Select(item => new SqlWord() { Type = SqlWordTokenType.TableColumn, Text = item.Name, Source = item }));
                }

                FilterDbObjects(words, schemaInfo.Functions, SqlWordTokenType.Function, tokenType, search, parentName);
            }

            if (IsTypeMatched(tokenType, SqlWordTokenType.BuiltinFunction))
            {
                var builtinFunctions = FunctionManager.GetFunctionSpecifications(databaseType).Where(item => ContainsWithNull(item.Name, search));

                words.AddRange(builtinFunctions.Select(item => new SqlWord() { Type = SqlWordTokenType.BuiltinFunction, Text = item.Name, Source = item }));
            }

            return words;
        }

        private static void FilterDbObjects<T>(List<SqlWord> words, IEnumerable<T> dbObjects, SqlWordTokenType currentTokenType, SqlWordTokenType tokenType = SqlWordTokenType.None, string search = null, string parentName = null)
            where T : DatabaseObject
        {
            if (IsTypeMatched(tokenType, currentTokenType))
            {
                IEnumerable<T> objs = dbObjects;

                if (!string.IsNullOrEmpty(parentName))
                {
                    objs = objs.Where(item => item.Schema.ToUpper() == parentName.ToUpper());
                }

                objs = objs.Where(item => ContainsWithNull(item.Name, search));

                words.AddRange(objs.Select(item => new SqlWord() { Type = currentTokenType, Text = item.Name, Source = item }));
            }
        }

        public static bool IsTypeMatched(SqlWordTokenType tokenType, SqlWordTokenType currentType)
        {
            if (tokenType == SqlWordTokenType.None || tokenType.HasFlag(currentType))
            {
                return true;
            }
            return false;
        }

        public static bool ContainsWithNull(string source, string search)
        {
            return search == null || Contains(source, search);
        }

        public static bool Contains(string source, string search)
        {
            if (source == null || search == null)
            {
                return false;
            }

            return source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
