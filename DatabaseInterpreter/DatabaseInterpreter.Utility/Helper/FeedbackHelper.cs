using DatabaseInterpreter.Model;
using System;

namespace DatabaseInterpreter.Utility
{
    public class FeedbackHelper
    {
        public static bool EnableLog { get; set; }
        public static bool EnableDebug { get; set; }

        public static void Feedback(FeedbackInfo info, bool enableLog = true)
        {
            if (EnableLog && enableLog)
            {
                string prefix = "";

                if (info.Owner != null)
                {
                    if (info.Owner.GetType() == typeof(string))
                    {
                        prefix = info.Owner.ToString();
                    }
                    else
                    {
                        prefix = info.Owner.GetType().Name;
                    }
                    prefix += ":";
                }

                string logContent = $"{prefix}{info.Message}";

                if((LogHelper.LogType.HasFlag(LogType.Info) && info.InfoType == FeedbackInfoType.Info)                   )
                {
                    LogHelper.LogInfo(logContent);
                }            
                else if(LogHelper.LogType.HasFlag(LogType.Error) && info.InfoType == FeedbackInfoType.Error)
                {
                    LogHelper.LogError(logContent);
                }
            }

            if (EnableDebug)
            {
                Console.WriteLine(info.Message);
            }
        }


        public static void Feedback(IObserver<FeedbackInfo> observer, FeedbackInfo info, bool enableLog = true)
        {
            if (observer != null)
            {
                observer.OnNext(info);
            }

            Feedback(info, enableLog);
        }
    }
}
