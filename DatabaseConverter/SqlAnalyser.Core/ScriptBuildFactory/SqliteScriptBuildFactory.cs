using DatabaseInterpreter.Model;
using SqlAnalyser.Core.Model;
using SqlAnalyser.Model;
using System;
using System.Linq;
using System.Text;

namespace SqlAnalyser.Core
{
    public class SqliteScriptBuildFactory : ScriptBuildFactory
    {
        public override DatabaseType DatabaseType => DatabaseType.Sqlite;

        public override ScriptBuildResult GenearteTriggerScripts(TriggerScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();

            string time = (script.Time == TriggerTime.INSTEAD_OF) ? "INSTEAD OF" : script.Time.ToString();
            TriggerEvent @event =  script.Events.FirstOrDefault();
            string columnNames = @event == TriggerEvent.UPDATE ? $" {string.Join(",", script.ColumnNames)}" : "";
            string strEvent = @event == TriggerEvent.UPDATE ? "UPDATE OF" : @event.ToString();           

            sb.AppendLine($"CREATE TRIGGER {script.Name}");
            sb.AppendLine($"{time} {strEvent}{columnNames} ON {script.TableName} FOR EACH ROW");

            sb.AppendLine("BEGIN");

            result.BodyStartIndex = sb.Length;

            foreach (Statement statement in script.Statements)
            {
                sb.Append(this.BuildStatement(statement));
            }

            result.BodyStopIndex = sb.Length - 1;

            sb.AppendLine("END");

            result.Script = sb.ToString();

            return result;
        }

        public override ScriptBuildResult GenearteViewScripts(ViewScript script)
        {
            ScriptBuildResult result = new ScriptBuildResult();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"CREATE VIEW {script.Name} AS");

            result.BodyStartIndex = sb.Length;

            foreach (Statement statement in script.Statements)
            {
                sb.AppendLine(this.BuildStatement(statement));
            }

            result.BodyStopIndex = sb.Length - 1;

            result.Script = sb.ToString();

            return result;
        }

        public override ScriptBuildResult GenerateRoutineScripts(RoutineScript script)
        {
            throw new NotSupportedException();
        }
    }
}
