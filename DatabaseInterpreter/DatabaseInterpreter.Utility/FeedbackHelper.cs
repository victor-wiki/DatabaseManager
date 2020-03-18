using System;

namespace DatabaseInterpreter.Utility
{
    public class FeedbackHelper
    {
        public static bool EnableLog { get; set; }

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

                LogHelper.LogInfo($"{prefix}{info.Message}");
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
