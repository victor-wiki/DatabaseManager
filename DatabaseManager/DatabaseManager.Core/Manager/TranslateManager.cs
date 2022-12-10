using DatabaseConverter.Core;
using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class TranslateManager
    {
        private IObserver<FeedbackInfo> observer;

        public async Task<TranslateResult> Translate(DatabaseType sourceDbType, DatabaseType targetDbType, DatabaseObject dbObject, ConnectionInfo connectionInfo, bool removeCarriagRreturnChar = false)
        {
            DbInterpreterOption sourceScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.None, ObjectFetchMode = DatabaseObjectFetchMode.Details };
            DbInterpreterOption targetScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.WriteToString };

            DbConveterInfo source = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, connectionInfo, sourceScriptOption) };
            DbConveterInfo target = new DbConveterInfo() { DbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDbType, new ConnectionInfo(), targetScriptOption) };

            using (DbConverter dbConverter = new DbConverter(source, target))
            {
                var option = dbConverter.Option;

                option.OnlyForTranslate = true;
                option.GenerateScriptMode = GenerateScriptMode.Schema;
                option.ExecuteScriptOnTargetServer = false;
                option.ConvertComputeColumnExpression = true;
                option.UseOriginalDataTypeIfUdtHasOnlyOneAttr = SettingManager.Setting.UseOriginalDataTypeIfUdtHasOnlyOneAttr;
                option.RemoveCarriagRreturnChar = removeCarriagRreturnChar;
                option.ConvertConcatChar = TranslateHelper.NeedConvertConcatChar(SettingManager.Setting.ConvertConcatCharTargetDatabases, targetDbType);

                dbConverter.Subscribe(this.observer);               

                DbConvertResult result = await dbConverter.Translate(dbObject);

                return result.TranslateResults.FirstOrDefault();
            }
        }

        public TranslateResult Translate(DatabaseType sourceDbType, DatabaseType targetDbType, string script)
        {
            TranslateResult result = new TranslateResult();

            DbInterpreter sourceDbInterpreter = DbInterpreterHelper.GetDbInterpreter(sourceDbType, new ConnectionInfo(), new DbInterpreterOption());
            DbInterpreter targetDbInterpreter = DbInterpreterHelper.GetDbInterpreter(targetDbType, new ConnectionInfo(), new DbInterpreterOption());

            dynamic scriptDbObjects = null;

            dynamic translator = null;

            ScriptType scriptType = ScriptParser.DetectScriptType(script, sourceDbInterpreter);

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
            else 
            {
                scriptDbObjects = new List<ScriptDbObject>() { new ScriptDbObject() { Definition = script } };

                translator = this.GetScriptTranslator<ScriptDbObject>(scriptDbObjects, sourceDbInterpreter, targetDbInterpreter);

                translator.IsCommonScript = true;
            }

            //use default schema
            if (scriptDbObjects != null)
            {
                foreach (var sdo in scriptDbObjects)
                {
                    sdo.Schema = targetDbInterpreter.DefaultSchema;
                }
            }

            if (translator != null)
            {
                translator.Translate();

                List<TranslateResult> results = translator.TranslateResults;

                result = results.FirstOrDefault();
            }              

            return result;
        }

        private IEnumerable<T> ConvertScriptDbObject<T>(string script) where T : ScriptDbObject
        {
            List<T> list = new List<T>();

            var t = Activator.CreateInstance(typeof(T)) as T;
            t.Definition = script;

            list.Add(t);

            return list;
        }        

        private ScriptTranslator<T> GetScriptTranslator<T>(IEnumerable<T> dbObjects, DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter) where T : ScriptDbObject
        {
            ScriptTranslator<T> translator = new ScriptTranslator<T>(sourceDbInterpreter, targetDbInterpreter, dbObjects) { };
            translator.Option = new DbConverterOption() { OutputRemindInformation = false };
            translator.Option.ConvertConcatChar = TranslateHelper.NeedConvertConcatChar(SettingManager.Setting.ConvertConcatCharTargetDatabases, targetDbInterpreter.DatabaseType);
            translator.AutoMakeupSchemaName = false;

            return translator;
        }      

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }
    }
}
