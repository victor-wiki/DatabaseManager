using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using DatabaseInterpreter.Model;
using SqlAnalyser.Model;
using System;

namespace SqlAnalyser.Core
{
    public abstract class SqlRuleAnalyser
    {
        public abstract Lexer GetLexer(string content);

        public virtual ICharStream GetCharStreamFromString(string content)
        {
            return CharStreams.fromstring(content);
        }

        public abstract Parser GetParser(CommonTokenStream tokenStream);

        public virtual Parser GetParser(string content)
        {
            Lexer lexer = this.GetLexer(content);

            CommonTokenStream tokens = new CommonTokenStream(lexer);

            Parser parser = this.GetParser(tokens);

            return parser;
        }

        public virtual CommonScript Analyse<T>(string content)
            where T : DatabaseObject
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

        public abstract RoutineScript AnalyseProcedure(string content);

        public abstract RoutineScript AnalyseFunction(string content);

        public abstract TriggerScript AnalyseTrigger(string content);

        public abstract ViewScript AnalyseView(string content);

        public abstract void ExtractFunctions(CommonScript script, ParserRuleContext node);

        public void ExtractFunctions(CommonScript script, ParserRuleContext node, Func<IParseTree, bool> func)
        {
            if (node == null)
            {
                return;
            }

            foreach (var child in node.children)
            {
                if (func(child))
                {
                    script.Functions.Add(new TokenInfo(child as ParserRuleContext));
                }
                else if (child is ParserRuleContext)
                {
                    this.ExtractFunctions(script, child as ParserRuleContext, func);
                }
            }
        }
    }
}
