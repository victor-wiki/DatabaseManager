using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;

namespace SqlAnalyser.Core
{
    public class StatementScriptBuilderHelper
    {
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
                tableName = fromItems.FirstOrDefault(item => tableNameOrAliases.Contains(item.TableName.Symbol.ToLower())
                                                     || (item.TableName.Alias != null && tableNameOrAliases.Contains(item.TableName.Alias.Symbol.ToLower())))?.TableName;
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
            return statement.SetItems.Any(item => item.Name.Symbol.Contains(","));
        }

        public static string ParseCompositeUpdateSet(StatementScriptBuilder builder, UpdateStatement statement)
        {
            StringBuilder sb = new StringBuilder();

            var colNames = statement.SetItems.First().Name.Symbol.Trim('(', ')').Split(',');
            string valueSymbol = statement.SetItems.First().Value.Symbol.Trim();

            if (valueSymbol.StartsWith('(') && valueSymbol.EndsWith(')'))
            {
                valueSymbol = StringHelper.TrimParenthesis(valueSymbol);
            }

            int fromIndex = valueSymbol.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);
            int whereIndex = valueSymbol.LastIndexOf("WHERE", StringComparison.OrdinalIgnoreCase);

            var colValues = valueSymbol.Substring(0, fromIndex).Replace("SELECT", "", StringComparison.OrdinalIgnoreCase).Split(',');

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

            Action buildFromAndWhere = () =>
            {
                if (fromIndex > 0)
                {
                    sb.AppendLine(valueSymbol.Substring(fromIndex).Trim() + ';');
                }
            };

            Action buildWhere = () =>
            {
                if (whereIndex > 0)
                {
                    sb.AppendLine(valueSymbol.Substring(whereIndex));
                }
            };

            Func<string> getFromTables = () =>
            {
                string fromTables = null;

                if (whereIndex > 0)
                {
                    fromTables = valueSymbol.Substring(fromIndex + 4, whereIndex - fromIndex -4).Trim();
                }
                else
                {
                    fromTables = valueSymbol.Substring(fromIndex + 4);
                }

                return fromTables;
            };

            if (builder is TSqlStatementScriptBuilder)
            {
                buildSet();

                buildFromAndWhere();
            }
            else if (builder is MySqlStatementScriptBuilder)
            {
                if (fromIndex > 0)
                {
                    string fromTables = getFromTables();

                    sb.AppendLine(fromTables);
                }

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

                if (fromIndex > 0)
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
                value = StringHelper.TrimParenthesis(value);
            }

            int fromIndex = value.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);

            return  $"{value.Substring(0, fromIndex)} INTO {name} {value.Substring(fromIndex)};";
        }
    }
}
