using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class ScriptCorrector
    {
        private DbInterpreter dbInterpreter;
        private char quotationLeftChar;
        private char quotationRightChar;

        public ScriptCorrector(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
            this.quotationLeftChar = dbInterpreter.QuotationLeftChar;
            this.quotationRightChar = dbInterpreter.QuotationRightChar;
        }

        public async Task<IEnumerable<ScriptDiagnoseResult>> CorrectNotMatchNames(ScriptDiagnoseType scriptDiagnoseType, IEnumerable<ScriptDiagnoseResult> results)
        {
            List<Script> scripts = new List<Script>();

            Dictionary<int, string> dictDifinition = new Dictionary<int, string>();

            int i = 0;

            foreach(var result in results)
            {
                ScriptGenerator scriptGenerator = new ScriptGenerator(dbInterpreter);

                string script = (await scriptGenerator.Generate(result.DbObject, ScriptAction.ALTER)).Script;

                if(string.IsNullOrEmpty(script))
                {
                    continue;
                }

                string definition = result.DbObject.Definition;

                foreach(var detail in result.Details)
                {
                    script = this.ReplaceDefinition(script, detail.InvalidName, detail.Name);
                    definition = this.ReplaceDefinition(definition, detail.InvalidName, detail.Name);

                    if(scriptDiagnoseType == ScriptDiagnoseType.ViewColumnAliasWithoutQuotationChar)
                    {
                        script = this.ReplaceDuplicateQuotationChar(script, detail.Name);
                        definition = this.ReplaceDuplicateQuotationChar(definition, detail.Name);
                    }
                }              

                if(result.DbObject is View)
                {
                    scripts.Add(new AlterDbObjectScript<View>(script));
                }
                else if(result.DbObject is Procedure)
                {
                    scripts.Add(new AlterDbObjectScript<Procedure>(script));
                }
                else if (result.DbObject is Function)
                {
                    scripts.Add(new AlterDbObjectScript<Function>(script));
                }

                dictDifinition.Add(i, definition);

                i++;
            }

            ScriptRunner scriptRunner = new ScriptRunner();

            await scriptRunner.Run(this.dbInterpreter, scripts);

            i = 0;

            foreach (var result in results)
            {
                result.DbObject.Definition = dictDifinition[i];

                i++;
            }

            return results;
        }

        private string ReplaceDuplicateQuotationChar(string content, string value)
        {
            content = content.Replace($"{this.quotationLeftChar}{value}{this.quotationRightChar}", value);

            return content;
        }

        private string ReplaceDefinition(string definition, string oldValue, string newValue)
        {
            return Regex.Replace(definition, $@"\b{oldValue}\b", newValue, RegexOptions.Multiline);
        }        
    }
}
