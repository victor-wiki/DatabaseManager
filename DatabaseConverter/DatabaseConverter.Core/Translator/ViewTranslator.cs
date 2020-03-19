using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ViewTranslator: DbObjectTokenTranslator
    {
        private List<View> views;     
       
        private string targetOwnerName;              

        public ViewTranslator(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, List<View> views, string targetOwnerName = null): base(sourceDbInterpreter, targetDbInterpreter)
        {
            this.views = views;           
            this.targetOwnerName = targetOwnerName;
        }

        public override void Translate()
        {
            if (sourceDbInterpreter.DatabaseType == targetDbInterpreter.DatabaseType)
            {
                return;
            }                      

            if (string.IsNullOrEmpty(targetOwnerName))
            {
                if (targetDbInterpreter is SqlServerInterpreter)
                {
                    targetOwnerName = "dbo";
                }
                else
                {
                    targetOwnerName = DbInterpreterHelper.GetOwnerName(targetDbInterpreter);
                }
            }

            foreach (View view in views)
            {
                try
                {
                    string ownerNameWithQuotation = $"{targetDbInterpreter.QuotationLeftChar}{targetOwnerName}{targetDbInterpreter.QuotationRightChar}";
                    string viewNameWithQuotation = $"{targetDbInterpreter.QuotationLeftChar}{view.Name}{targetDbInterpreter.QuotationRightChar}";

                    string definition = view.Definition;

                    definition = definition
                               .Replace(sourceDbInterpreter.QuotationLeftChar, '"')
                               .Replace(sourceDbInterpreter.QuotationRightChar, '"')
                               .Replace("<>", "!=")
                               .Replace(">", " > ")
                               .Replace("<", " < ")
                               .Replace("!=", "<>");


                    definition = this.ParseDefinition(definition);

                    string createAsClause = $"CREATE VIEW {targetOwnerName}.{viewNameWithQuotation} AS ";

                    if (!definition.Trim().ToLower().StartsWith("create"))
                    {
                        definition = createAsClause + Environment.NewLine + definition;
                    }
                    else
                    {
                        int asIndex = definition.ToLower().IndexOf("as");
                        definition = createAsClause + definition.Substring(asIndex + 2);
                    }

                    view.Definition = definition;
                }
                catch (Exception ex)
                {
                    throw new ViewConvertException(ex)
                    {
                        SourceServer = sourceDbInterpreter.ConnectionInfo.Server,
                        SourceDatabase = sourceDbInterpreter.ConnectionInfo.Database,
                        SourceObject = view.Name,
                        TargetServer = targetDbInterpreter.ConnectionInfo.Server,
                        TargetDatabase = targetDbInterpreter.ConnectionInfo.Database,
                        TargetObject = view.Name
                    };
                }
            }           
        }

        private string ParseDefinition(string definition)
        {
            var tokens = this.GetTokens(definition);
            bool changed = false;

            definition = this.HandleDefinition(definition, tokens, out changed);

            StringBuilder sb = new StringBuilder();           

            if (changed)
            {
                tokens = this.GetTokens(definition);
            }

            definition = this.ParseTokens(tokens);

            #region Handle join cluase for mysql which has no "on", so it needs to make up that.
            try
            {
                if (this.sourceDbInterpreter.GetType() == typeof(MySqlInterpreter))
                {
                    bool hasError = false;
                    string formattedDefinition = this.FormatSql(definition, out hasError);

                    if (!hasError)
                    {
                        string[] lines = formattedDefinition.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        Regex joinRegex = new Regex("\\b(join)\\b", RegexOptions.IgnoreCase);
                        Regex onRegex = new Regex("\\b(on)\\b", RegexOptions.IgnoreCase);
                        Regex wordRegex = new Regex("([a-zA-Z(]+)", RegexOptions.IgnoreCase);

                        sb = new StringBuilder();
                        foreach (string line in lines)
                        {
                            bool hasChanged = false;

                            if (joinRegex.IsMatch(line))
                            {
                                string leftStr = line.Substring(line.ToLower().LastIndexOf("join") + 4);
                                if (!onRegex.IsMatch(line) && !wordRegex.IsMatch(leftStr))
                                {
                                    hasChanged = true;
                                    sb.AppendLine($"{line} ON 1=1 ");
                                }
                            }

                            if (!hasChanged)
                            {
                                sb.AppendLine(line);
                            }
                        }

                        definition = sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                FeedbackInfo info = new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = ExceptionHelper.GetExceptionDetails(ex), Owner = this };
                FeedbackHelper.Feedback(info);
            } 
            #endregion

            return definition.Trim();
        }             
    }
}
