using Antlr4.Runtime;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;
using System.Text.RegularExpressions;

namespace SqlAnalyser.Core
{
    public abstract class SqlAnalyserBase
    {      
        public abstract DatabaseType DatabaseType { get; }
        public abstract string GenerateScripts(CommonScript script);
        public abstract ViewScript AnalyseView(string content);
        public abstract RoutineScript AnalyseProcedure(string content);
        public abstract RoutineScript AnalyseFunction(string content);
        public abstract TriggerScript AnalyseTrigger(string content);

        public CommonScript Analyse<T>(string content) where T:DatabaseObject
        {
            CommonScript script = null;

            if (typeof(T) == typeof(Procedure))
            {
                script = this.AnalyseProcedure(content);
            }
            else if (typeof(T) == typeof(Function))
            {
                script = this.AnalyseFunction(content);
            }
            else if (typeof(T) == typeof(View))
            {
                script = this.AnalyseView(content);
            }
            else if (typeof(T) == typeof(TableTrigger))
            {
                script = this.AnalyseTrigger(content);
            }
            else
            {
                throw new NotSupportedException($"Not support analyse for type:{typeof(T).Name}");
            }

            return script;
        }

        public string FormatScripts(string scripts)
        {
            Regex regex = new Regex(@"([;]+[\s]*[;]+)|(\r\n[\s]*[;])");

            return regex.Replace(scripts, ";");
        }
    }
}
