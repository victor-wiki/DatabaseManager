using DatabaseConverter.Core;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Threading.Tasks;

namespace DatabaseConverter.Demo
{
    public class ConverterDemo: IObserver<FeedbackInfo>
    {
        private DbInterpreter sourceInterpreter;
        private DbInterpreter targetInterpreter;

        public DbInterpreter SourceInterpreter => this.sourceInterpreter;
        public DbInterpreter TargetInterpreter => this.targetInterpreter;

        public ConverterDemo(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter)
        {
            this.sourceInterpreter = sourceInterpreter;
            this.targetInterpreter = targetInterpreter;
        }

        public async Task Convert()
        {
            DatabaseType sourceDbType = this.sourceInterpreter.DatabaseType;
            DatabaseType targetDbType = this.targetInterpreter.DatabaseType;     

            DbInterpreterOption sourceScriptOption = new DbInterpreterOption() { ScriptOutputMode = GenerateScriptOutputMode.WriteToString };
            DbInterpreterOption targetScriptOption = new DbInterpreterOption() { ScriptOutputMode = (GenerateScriptOutputMode.WriteToFile | GenerateScriptOutputMode.WriteToString)};

            this.sourceInterpreter.Option = sourceScriptOption;
            this.targetInterpreter.Option = targetScriptOption;

            GenerateScriptMode scriptMode = GenerateScriptMode.Schema | GenerateScriptMode.Data;

            DbConveterInfo source = new DbConveterInfo() { DbInterpreter = sourceInterpreter };
            DbConveterInfo target = new DbConveterInfo() { DbInterpreter = targetInterpreter };            

            try
            {
                using (DbConverter dbConverter = new DbConverter(source, target))
                {                    
                    dbConverter.Option.GenerateScriptMode = scriptMode;

                    dbConverter.Subscribe(this);

                    if (sourceDbType == DatabaseType.MySql)
                    {
                        source.DbInterpreter.Option.InQueryItemLimitCount = 2000;
                    }

                    if (targetDbType == DatabaseType.MySql)
                    {
                        target.DbInterpreter.Option.RemoveEmoji = true;
                    }

                    dbConverter.Option.SplitScriptsToExecute = true;

                    FeedbackHelper.EnableLog = true;

                    await dbConverter.Convert();
                }                   
            }
            catch (Exception ex)
            {
                string msg = ExceptionHelper.GetExceptionDetails(ex);

                this.Feedback(new FeedbackInfo() { InfoType = FeedbackInfoType.Error, Message = msg });
            }
        }

        private void Feedback(FeedbackInfo info)
        {
            Console.WriteLine(info.Message);

            if (info.InfoType == FeedbackInfoType.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }                      
        }

        #region IObserver<FeedbackInfo>
        void IObserver<FeedbackInfo>.OnCompleted()
        {
        }
        void IObserver<FeedbackInfo>.OnError(Exception error)
        {
        }
        void IObserver<FeedbackInfo>.OnNext(FeedbackInfo info)
        {
            this.Feedback(info);
        }
        #endregion
    }
}
