using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ConstraintTranslator : DbObjectTokenTranslator
    {
        private List<TableConstraint> constraints;
        private IEnumerable<DataTypeSpecification> sourceDataTypeSpecifications;       

        internal List<TableColumn> TableCoumns { get; set; }
        public ConstraintTranslator(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, List<TableConstraint> constraints) : base(sourceDbInterpreter, targetDbInterpreter)
        {
            this.constraints = constraints;          
        }

        public override void Translate()
        {
            if (this.sourceDbInterpreter.DatabaseType == this.targetDbInterpreter.DatabaseType)
            {
                return;
            }

            if (this.hasError)
            {
                return;
            }

            this.FeedbackInfo("Begin to translate constraints.");

            this.LoadMappings();           

            List<TableConstraint> invalidConstraints = new List<TableConstraint>();

            foreach (TableConstraint constraint in this.constraints)
            {
                constraint.Definition = this.ParseDefinition(constraint.Definition);

                if (this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle || this.targetDbInterpreter.DatabaseType == DatabaseType.Postgres)
                {
                    if (this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle)
                    {
                        if (constraint.Definition.Contains("SYSDATE"))
                        {
                            invalidConstraints.Add(constraint);
                            continue;
                        }
                    }

                    string likeExp = $@"(([\w\[\]""`]+)[\s]+(like)[\s]+(['][\[].+[\]][']))"; //example: ([SHELF] like '[A-Za-z]' OR "SHELF"='N/A'), to match: [SHELF] like '[A-Za-z]'

                    MatchCollection matches = Regex.Matches(constraint.Definition, likeExp, RegexOptions.IgnoreCase);

                    if (matches.Count > 0)
                    {
                        foreach (Match m in matches)
                        {
                            string[] items = m.Value.Split(' ');

                            string newValue = null;

                            if (this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle || this.targetDbInterpreter.DatabaseType== DatabaseType.MySql)
                            {
                                newValue = $"REGEXP_LIKE({items[0]},{items[2]})";
                            }
                            else if (this.targetDbInterpreter.DatabaseType == DatabaseType.Postgres)
                            {
                                newValue = $"{items[0]} similar to ('({items[2].Trim('\'')})')";
                            }

                            if (!string.IsNullOrEmpty(newValue))
                            {
                                constraint.Definition = constraint.Definition.Replace(m.Value, newValue);
                            }
                        }
                    }

                    if (this.targetDbInterpreter.DatabaseType == DatabaseType.Postgres)
                    {
                        if (this.TableCoumns != null)
                        {
                            var isMoneyConstraint = this.TableCoumns.Any(item =>
                              item.TableName == constraint.TableName &&
                              item.DataType == "money" && constraint.Definition.Contains(item.Name)
                            );

                            if (isMoneyConstraint && !constraint.Definition.ToLower().Contains("::money"))
                            {
                                constraint.Definition = TranslateHelper.ConvertNumberToPostgresMoney(constraint.Definition);
                            }
                        }
                    }
                }

                if (this.sourceDbInterpreter.DatabaseType == DatabaseType.Oracle || this.sourceDbInterpreter.DatabaseType == DatabaseType.MySql)
                {
                    string likeFunctionName = "REGEXP_LIKE";
                    Regex likeFunctionNameExp = new Regex($"({likeFunctionName})", RegexOptions.IgnoreCase);

                    if (constraint.Definition.IndexOf(likeFunctionName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string likeExp = $@"({likeFunctionName})[\s]?[(][\w\[\]""` ]+[,][\s]?(['][\[].+[\]]['])[)]"; //example: REGEXP_LIKE("SHELF",'[A-Za-z]')

                        MatchCollection matches = Regex.Matches(constraint.Definition, likeExp, RegexOptions.IgnoreCase);

                        if (matches.Count > 0)
                        {
                            foreach (Match m in matches)
                            {
                                string[] items = likeFunctionNameExp.Replace(m.Value, "").Trim('(', ')').Split(',');

                                string newValue = null;

                                if (this.targetDbInterpreter.DatabaseType == DatabaseType.SqlServer)
                                {
                                    newValue = $"{items[0]} like {items[1]}";
                                }
                                else if (this.targetDbInterpreter.DatabaseType == DatabaseType.Postgres)
                                {
                                    newValue = $"{items[0]} similar to ('({items[1].Trim('\'')})')";
                                }

                                if (!string.IsNullOrEmpty(newValue))
                                {
                                    constraint.Definition = constraint.Definition.Replace(m.Value, newValue);
                                }
                            }
                        }
                    }
                }
                else if (this.sourceDbInterpreter.DatabaseType == DatabaseType.Postgres)
                {
                    constraint.Definition = constraint.Definition.Replace("NOT VALID", "");

                    if (constraint.Definition.Contains("::")) //datatype convert operator
                    {
                        this.LoadSourceDataTypeSpecifications();                      

                        constraint.Definition = TranslateHelper.RemovePostgresDataTypeConvertExpression(constraint.Definition, this.sourceDataTypeSpecifications, this.targetDbInterpreter.QuotationLeftChar, this.targetDbInterpreter.QuotationRightChar);
                    }

                    if (constraint.Definition.Contains("similar_to_escape"))
                    {
                        //example:  ((((([Shelf]) ~ similar_to_escape('(A-Za-z)')) OR (([Shelf]) = 'N/A')))),
                        //to match (([Shelf]) ~ similar_to_escape('(A-Za-z)'))
                        string likeExp = $@"(([(][\w\{this.targetDbInterpreter.QuotationLeftChar}\{this.targetDbInterpreter.QuotationRightChar}]+[)])[\s][~][\s](similar_to_escape)([(]['][(].+[)]['][)]))"; 

                        MatchCollection matches = Regex.Matches(constraint.Definition, likeExp, RegexOptions.IgnoreCase);

                        if (matches.Count > 0)
                        {
                            foreach (Match m in matches)
                            {
                                string[] items = m.Value.Split('~');

                                string columnName = items[0].Trim(new char[] { ' ', '(', ')' });
                                string expression = $"'{items[1].Replace("similar_to_escape", "").Trim(new char[] { ' ', '(', ')', '\'' })}'"; ;

                                string newValue = null;

                                if (this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle || this.targetDbInterpreter.DatabaseType == DatabaseType.MySql)
                                {
                                    newValue = $"REGEXP_LIKE({columnName},{expression})";
                                }
                                else if (this.targetDbInterpreter.DatabaseType == DatabaseType.SqlServer)
                                {
                                    newValue = $"{columnName} like {expression}";
                                }

                                if (!string.IsNullOrEmpty(newValue))
                                {
                                    constraint.Definition = constraint.Definition.Replace(m.Value, newValue);
                                }
                            }
                        }
                    }
                }               
            }

            this.constraints.RemoveAll(item => invalidConstraints.Contains(item));

            this.FeedbackInfo("End translate constraints.");
        }

        private void LoadSourceDataTypeSpecifications()
        {
            if (this.sourceDataTypeSpecifications == null)
            {
                this.sourceDataTypeSpecifications = DataTypeManager.GetDataTypeSpecifications(this.sourceDbInterpreter.DatabaseType);
            }
        }
    }
}
