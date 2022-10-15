using DatabaseConverter.Core;
using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class TranslateManager
    {
        private IObserver<FeedbackInfo> observer;
        private string asPattern = @"\b(AS|IS)\b";

        public async Task Translate(DatabaseType sourceDbType, DatabaseType targetDbType, DatabaseObject dbObject, ConnectionInfo connectionInfo, TranslateHandler translateHandler = null)
        {
            DbInterpreterOption sourceScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.None };
            DbInterpreterOption targetScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.WriteToString };

            DbConveterInfo script = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, connectionInfo, sourceScriptOption) };
            DbConveterInfo target = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDbType, new ConnectionInfo(), sourceScriptOption) };

            using (DbConverter dbConverter = new DbConverter(script, target))
            {
                dbConverter.Option.OnlyForTranslate = true;
                dbConverter.Option.GenerateScriptMode = GenerateScriptMode.Schema;
                dbConverter.Option.ExecuteScriptOnTargetServer = false;
                dbConverter.Option.ConvertComputeColumnExpression = true;
                dbConverter.Option.UseOriginalDataTypeIfUdtHasOnlyOneAttr = SettingManager.Setting.UseOriginalDataTypeIfUdtHasOnlyOneAttr;

                dbConverter.Subscribe(this.observer);

                if (translateHandler != null)
                {
                    dbConverter.OnTranslated += translateHandler;
                }

                await dbConverter.Translate(dbObject);
            }
        }

        public string Translate(DatabaseType sourceDbType, DatabaseType targetDbType, string script)
        {
            DbInterpreter sourceDbInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, new ConnectionInfo(), new DbInterpreterOption());
            DbInterpreter targetDbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDbType, new ConnectionInfo(), new DbInterpreterOption());

            dynamic scriptDbObjects = null;

            dynamic translator = null;

            ScriptType scriptType = this.DetectScriptType(script, sourceDbInterpreter);

            if (scriptType == ScriptType.View)
            {
                scriptDbObjects = this.ConvertScriptDbObject<View>(script);
                translator = this.GetScriptTranslator<View>(scriptDbObjects, sourceDbInterpreter, targetDbInterpreter);
            }
            else if (scriptType == ScriptType.Function)
            {
                scriptDbObjects = this.ConvertScriptDbObject<Function>(script);
                translator = this.GetScriptTranslator<Function>(scriptDbObjects, sourceDbInterpreter, targetDbInterpreter);
            }
            else if (scriptType == ScriptType.Procedure)
            {
                scriptDbObjects = this.ConvertScriptDbObject<Procedure>(script);
                translator = this.GetScriptTranslator<Procedure>(scriptDbObjects, sourceDbInterpreter, targetDbInterpreter);
            }
            else if (scriptType == ScriptType.Trigger)
            {
                scriptDbObjects = this.ConvertScriptDbObject<TableTrigger>(script);
                translator = this.GetScriptTranslator<TableTrigger>(scriptDbObjects, sourceDbInterpreter, targetDbInterpreter);
            }
            else if (scriptType == ScriptType.SimpleSelect)
            {
                string viewTemp = $"CREATE VIEW TEMP AS{Environment.NewLine}";
                script = $"{viewTemp}{script}";

                scriptDbObjects = this.ConvertScriptDbObject<View>(script);
                translator = this.GetScriptTranslator<View>(scriptDbObjects, sourceDbInterpreter, targetDbInterpreter);
            }
            else if (scriptType == ScriptType.Other)
            {
                string viewTemp = $"CREATE PROCEDURE TEMP";

                if (sourceDbType == DatabaseType.MySql)
                {
                    script = $"{viewTemp}(){Environment.NewLine}BEGIN{Environment.NewLine}{script}{Environment.NewLine}END";
                }
                else if (sourceDbType == DatabaseType.SqlServer)
                {
                    script = $"{viewTemp} AS {script}";
                }
                else if (sourceDbType == DatabaseType.Oracle)
                {
                    script = $"{viewTemp} AS{Environment.NewLine}BEGIN{Environment.NewLine}{script.TrimEnd(';')};{Environment.NewLine}END TEMP;";
                }
                else if (sourceDbType == DatabaseType.Postgres)
                {
                    script = $"{viewTemp} AS{Environment.NewLine}$${Environment.NewLine}BEGIN{Environment.NewLine}{script}{Environment.NewLine}END{Environment.NewLine}$$;";
                }

                scriptDbObjects = this.ConvertScriptDbObject<Procedure>(script);
                translator = this.GetScriptTranslator<Procedure>(scriptDbObjects, sourceDbInterpreter, targetDbInterpreter);
            }

            if (translator != null)
            {
                translator.Translate();

                if (scriptType == ScriptType.SimpleSelect)
                {
                    var scriptDbObject = (scriptDbObjects as List<View>)[0];
                    scriptDbObject.Definition = this.ExtractRoutineScriptBody<View>(scriptDbObject.Definition);
                }
                else if (scriptType == ScriptType.Other)
                {
                    var scriptDbObject = (scriptDbObjects as List<Procedure>)[0];
                    scriptDbObject.Definition = this.ExtractRoutineScriptBody<Procedure>(scriptDbObject.Definition);
                }
            }

            if (scriptDbObjects != null && scriptDbObjects.Count>0)
            {
                return scriptDbObjects[0].Definition;
            }

            return null;
        }

        private ScriptType DetectScriptType(string script, DbInterpreter dbInterpreter)
        {
            string upperScript = script.ToUpper().Trim();

            ScriptParser scriptParser = new ScriptParser(dbInterpreter, upperScript);

            if (scriptParser.IsCreateOrAlterScript())
            {
                string firstLine = upperScript.Split(Environment.NewLine).FirstOrDefault();

                var asMatch = Regex.Match(firstLine, asPattern);

                int asIndex = asMatch.Index;

                if (asIndex <= 0)
                {
                    asIndex = firstLine.Length;
                }

                string prefix = upperScript.Substring(0, asIndex);

                if (prefix.IndexOf(" VIEW ") > 0)
                {
                    return ScriptType.View;
                }
                else if (prefix.IndexOf(" FUNCTION ") > 0)
                {
                    return ScriptType.Function;
                }
                else if (prefix.IndexOf(" PROCEDURE ") > 0)
                {
                    return ScriptType.Procedure;
                }
                else if (prefix.IndexOf(" TRIGGER ") > 0)
                {
                    return ScriptType.Trigger;
                }
            }
            else if (scriptParser.IsSelect())
            {
                return ScriptType.SimpleSelect;
            }

            return ScriptType.Other;
        }

        private IEnumerable<T> ConvertScriptDbObject<T>(string script) where T : ScriptDbObject
        {
            List<T> list = new List<T>();

            var t = Activator.CreateInstance(typeof(T)) as T;
            t.Definition = script;

            list.Add(t);

            return list;
        }

        private string ExtractRoutineScriptBody<T>(string script) where T : ScriptDbObject
        {
            if (typeof(T) == typeof(View))
            {
                var asMatch = Regex.Match(script.ToUpper(), asPattern);

                if (asMatch != null)
                {
                    return script.Substring(asMatch.Index + 2).Trim();
                }
            }
            else if (typeof(T) == typeof(Procedure))
            {
                string[] lines = script.Split(Environment.NewLine);
                int firstBeginIndex = -1, lastEndIndex = -1;

                int index = 0;

                foreach (string line in lines)
                {
                    if (line.StartsWith("BEGIN") && firstBeginIndex == -1)
                    {
                        firstBeginIndex = index;
                    }
                    else if (line.StartsWith("END"))
                    {
                        lastEndIndex = index;
                    }

                    index++;
                }

                List<string> items = new List<string>();

                index = 0;
                foreach (string line in lines)
                {
                    if (index > firstBeginIndex && index < lastEndIndex)
                    {
                        items.Add(line);
                    }

                    index++;
                }

                script = String.Join(Environment.NewLine, items);
            }

            return script;
        }

        private ScriptTranslator<T> GetScriptTranslator<T>(IEnumerable<T> dbObjects, DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter) where T : ScriptDbObject
        {
            ScriptTranslator<T> translator = new ScriptTranslator<T>(sourceDbInterpreter, targetDbInterpreter, dbObjects) { };
            translator.Option = new DbConverterOption();

            return translator;
        }


        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }
    }

    public enum ScriptType
    {
        SimpleSelect = 1,
        View = 2,
        Function = 3,
        Procedure = 4,
        Trigger = 5,
        Other = 6
    }
}
