using DatabaseConverter.Model;
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
       
        private string targetSchemaName;              

        public ViewTranslator(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, List<View> views, string targetSchemaName = null): base(sourceDbInterpreter, targetDbInterpreter)
        {
            this.views = views;           
            this.targetSchemaName = targetSchemaName;
        }

        public override void Translate()
        {
            if (sourceDbInterpreter.DatabaseType == targetDbInterpreter.DatabaseType)
            {
                return;
            }

            if (this.hasError)
            {
                return;
            }

            this.LoadMappings();

            if (string.IsNullOrEmpty(targetSchemaName))
            {
                if (targetDbInterpreter is SqlServerInterpreter)
                {
                    targetSchemaName = "dbo";
                }
                else
                {
                    targetSchemaName = targetDbInterpreter.DefaultSchema;
                }
            }

            foreach (View view in views)
            {
                try
                {                    
                    string viewNameWithQuotation = $"{targetDbInterpreter.QuotationLeftChar}{view.Name}{targetDbInterpreter.QuotationRightChar}";

                    string definition = view.Definition;

                    definition = definition
                               .Replace(sourceDbInterpreter.QuotationLeftChar, '"')
                               .Replace(sourceDbInterpreter.QuotationRightChar, '"')
                               .Replace("<>", "!=")
                               .Replace(">", " > ")
                               .Replace("<", " < ")
                               .Replace("!=", "<>");

                    StringBuilder sb = new StringBuilder();

                    string[] lines = definition.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach(string line in lines)
                    {
                        if(line.StartsWith(this.sourceDbInterpreter.CommentString))
                        {
                            continue;
                        }

                        sb.AppendLine(line);
                    }

                    definition = this.ParseDefinition(sb.ToString());

                    string createClause = this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle ? "CREATE OR REPLACE" : "CREATE";

                    string createAsClause = $"{createClause} VIEW {(string.IsNullOrEmpty(targetSchemaName)? "": targetSchemaName + "." )}{viewNameWithQuotation} AS ";

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

                    if (this.Option.CollectTranslateResultAfterTranslated)
                    {
                        this.TranslateResults.Add(new TranslateResult() { DbObjectType = DatabaseObjectType.View, DbObjectName = view.Name, Data = view.Definition });
                    }
                }
                catch (Exception ex)
                {
                    var vce = new ViewConvertException(ex)
                    {
                        SourceServer = sourceDbInterpreter.ConnectionInfo.Server,
                        SourceDatabase = sourceDbInterpreter.ConnectionInfo.Database,
                        SourceObject = view.Name,
                        TargetServer = targetDbInterpreter.ConnectionInfo.Server,
                        TargetDatabase = targetDbInterpreter.ConnectionInfo.Database,
                        TargetObject = view.Name
                    };

                    if (!this.ContinueWhenErrorOccurs)
                    {
                        throw vce;
                    }
                    else
                    {
                        this.FeedbackError(ExceptionHelper.GetExceptionDetails(ex), this.ContinueWhenErrorOccurs);
                    }                   
                }
            }           
        }

        public override string ParseDefinition(string definition)
        {
            definition = base.ParseDefinition(definition);

            #region Handle join cluase for mysql which has no "on", so it needs to make up that.
            try
            {
                StringBuilder sb = new StringBuilder();

                if (this.sourceDbInterpreter.GetType() == typeof(MySqlInterpreter))
                {
                    bool hasError = false;
                    string formattedDefinition = this.FormatSql(definition, out hasError);

                    if (!hasError)
                    {
                        string[] lines = formattedDefinition.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        Regex joinRegex = new Regex(@"\b(join)\b", RegexOptions.IgnoreCase);
                        Regex onRegex = new Regex(@"\b(on)\b", RegexOptions.IgnoreCase);
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
