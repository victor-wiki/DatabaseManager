using DatabaseInterpreter.Model;
using System;
using System.IO;

namespace DatabaseInterpreter.Utility
{
    public class LogHelper
    {
        public static LogType LogType { get; set; }
        private static object obj = new object();

        public static void LogInfo(string message)
        {
            Log(LogType.Info, message);
        }

        public static void LogError(string message)
        {
            Log(LogType.Error, message);
        }

        private static void Log(LogType logType, string message)
        {
            string logFolder = "log";

            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            string filePath = Path.Combine(logFolder, DateTime.Today.ToString("yyyyMMdd") + ".txt");

            string content = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}({logType}):{message}";

            lock (obj)
            {
                File.AppendAllLines(filePath, new string[] { content });
            }
        }
    }    
}
