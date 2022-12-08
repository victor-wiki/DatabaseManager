using SqlAnalyser.Model;

namespace SqlAnalyser.Core
{
    public class MySqlAnalyser : SqlAnalyserBase
    {
        private MySqlRuleAnalyser ruleAnalyser = null;

        public override SqlRuleAnalyser RuleAnalyser => this.ruleAnalyser;

        public MySqlAnalyser(string content):base(content)
        {
            this.ruleAnalyser = new MySqlRuleAnalyser(content);
        }

        public override SqlSyntaxError Validate()
        {
            return this.ruleAnalyser.Validate();
        }

        public override AnalyseResult AnalyseCommon()
        {
            return this.ruleAnalyser.AnalyseCommon();
        }

        public override AnalyseResult AnalyseView()
        {
            return this.ruleAnalyser.AnalyseView();
        }

        public override AnalyseResult AnalyseProcedure()
        {
            return this.ruleAnalyser.AnalyseProcedure();
        }

        public override AnalyseResult AnalyseFunction()
        {
            return this.ruleAnalyser.AnalyseFunction();
        }

        public override AnalyseResult AnalyseTrigger()
        {
            return this.ruleAnalyser.AnalyseTrigger();
        }        
    }
}
