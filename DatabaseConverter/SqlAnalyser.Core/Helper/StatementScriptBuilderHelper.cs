using DatabaseInterpreter.Utility;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class StatementScriptBuilderHelper
    {
        public static TableName GetSelectStatementTableName(SelectStatement statement)
        {
            if (statement.TableName != null)
            {
                return statement.TableName;
            }
            else if (statement.HasFromItems)
            {
                return statement.FromItems.FirstOrDefault()?.TableName;
            }

            return null;
        }

        public static TableName GetUpdateSetTableName(UpdateStatement statement)
        {
            TableName tableName = null;

            var tableNames = statement.TableNames;
            var fromItems = statement.FromItems;
            var setItems = statement.SetItems;

            var setNames = statement.SetItems.Select(item => item.Name);

            List<string> tableNameOrAliases = new List<string>();

            foreach (var setName in setItems)
            {
                if (setName.Name.Symbol.Contains("."))
                {
                    tableNameOrAliases.Add(setName.Name.Symbol.Split(".")[0].Trim().ToLower());
                }
            }

            if (fromItems != null)
            {
                tableName = fromItems.FirstOrDefault(item => item.TableName != null && (tableNameOrAliases.Contains(item.TableName.Symbol.ToLower())
                                                     || (item.TableName.Alias != null && tableNameOrAliases.Contains(item.TableName.Alias.Symbol.ToLower()))))?.TableName;
            }

            if (tableName == null && tableNames != null)
            {
                tableName = tableNames.FirstOrDefault(item => tableNameOrAliases.Contains(item.Symbol.ToLower())
                                                     || (item.Alias != null && tableNameOrAliases.Contains(item.Alias.Symbol.ToLower())));
            }

            return tableName;
        }

        /// <summary>
        /// oracle:update table set (col1,col2)=(select col1,col2 from ...)
        /// </summary>
        /// <param name="statement"></param>
        public static bool IsCompositeUpdateSetColumnName(UpdateStatement statement)
        {
            return statement.SetItems.Any(item => item.Name?.Symbol?.Contains(",") == true);
        }

        public static string ParseCompositeUpdateSet(StatementScriptBuilder builder, UpdateStatement statement)
        {
            StringBuilder sb = new StringBuilder();

            var setItem = statement.SetItems.FirstOrDefault();
            var valueStatement = setItem.ValueStatement;

            if (valueStatement == null)
            {
                return string.Empty;
            }

            var where = valueStatement.Where;

            var colNames = statement.SetItems.FirstOrDefault().Name.Symbol.Trim('(', ')').Split(',');

            var colValues = valueStatement.Columns.Select(item => item.Symbol).ToArray();

            Action buildSet = () =>
            {
                for (int i = 0; i < colNames.Length; i++)
                {
                    sb.Append($"{colNames[i].Trim()}={colValues[i].Trim()}{((i < colNames.Length - 1) ? ", " : "")}");

                    if (i == colNames.Length - 1)
                    {
                        sb.AppendLine();
                    }
                }
            };

            Func<string> getFromTables = () =>
            {
                if (valueStatement.HasFromItems)
                {
                    return String.Join(",", valueStatement.FromItems.Select(item => item.TableName.NameWithAlias));
                }

                return String.Empty;
            };

            Action buildWhere = () =>
            {
                if (where != null)
                {
                    sb.AppendLine($"WHERE {where.Symbol}");
                }
            };

            Action buildFromAndWhere = () =>
            {
                string fromTables = getFromTables();

                sb.AppendLine($"FROM {fromTables}");

                buildWhere();
            };                   

            if (builder is TSqlStatementScriptBuilder || builder is SqliteStatementScriptBuilder)
            {
                buildSet();

                buildFromAndWhere();
            }
            else if (builder is MySqlStatementScriptBuilder)
            {
                string fromTables = getFromTables();

                sb.AppendLine(fromTables);

                sb.AppendLine("SET");

                buildSet();

                buildWhere();
            }
            else if (builder is PostgreSqlStatementScriptBuilder)
            {
                for (int i = 0; i < colNames.Length; i++)
                {
                    if (colNames[i].Contains("."))
                    {
                        colNames[i] = colNames[i].Split('.').Last().Trim();
                    }
                }

                buildSet();

                if (valueStatement.HasFromItems)
                {
                    string fromTables = getFromTables();

                    TableName tableName = GetUpdateSetTableName(statement);

                    string strTableName = tableName?.Symbol;
                    string alias = tableName?.Alias?.Symbol;

                    List<string> fromTableList = new List<string>();

                    foreach (string fromTable in fromTables.Split(','))
                    {
                        var items = fromTable.Split(' ').Select(item => item.Trim());

                        if (!items.Any(item => item == strTableName || item == alias))
                        {
                            fromTableList.Add(fromTable);
                        }
                    }

                    sb.AppendLine($"FROM {string.Join(",", fromTableList)}");
                }

                buildWhere();
            }

            return sb.ToString();
        }

        public static string ConvertToSelectIntoVariable(string name, string value)
        {
            value = value.Trim();

            if (value.StartsWith("(") && value.EndsWith(")"))
            {
                value = StringHelper.GetBalanceParenthesisTrimedValue(value);
            }

            int fromIndex = value.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);

            if (fromIndex < 0)
            {
                return $"{name}:={value};";
            }

            return $"{value.Substring(0, fromIndex)} INTO {name} {value.Substring(fromIndex)};";
        }
    }
}
