using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;

namespace SqlAnalyser.Core
{
    public abstract class SqlAnalyserBase
    {
        public string Content { get; set; }
        public abstract SqlRuleAnalyser RuleAnalyser { get; }      

        public abstract SqlSyntaxError Validate();
        public abstract AnalyseResult AnalyseCommon();
        public abstract AnalyseResult AnalyseView();
        public abstract AnalyseResult AnalyseProcedure();
        public abstract AnalyseResult AnalyseFunction();
        public abstract AnalyseResult AnalyseTrigger();

        public SqlAnalyserBase(string content)
        {
            this.Content = content;
        }

        public AnalyseResult Analyse<T>() where T : DatabaseObject
        {
            AnalyseResult result = null;

            if (this.RuleAnalyser.Option.IsCommonScript)
            {
                result = this.AnalyseCommon();
            }
            else
            {
                if (typeof(T) == typeof(Procedure))
                {
                    result = this.AnalyseProcedure();
                }
                else if (typeof(T) == typeof(Function))
                {
                    result = this.AnalyseFunction();
                }
                else if (typeof(T) == typeof(View))
                {
                    result = this.AnalyseView();
                }
                else if (typeof(T) == typeof(TableTrigger))
                {
                    result = this.AnalyseTrigger();
                }
                else
                {
                    throw new NotSupportedException($"Not support analyse for type:{typeof(T).Name}");
                }
            }

            return result;
        }      
    }
}
