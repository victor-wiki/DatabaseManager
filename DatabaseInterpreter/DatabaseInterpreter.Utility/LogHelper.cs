using System;
using System.Text;
using System.IO;

namespace DatabaseInterpreter.Utility
{
    public class LogHelper
    {
        public static bool EnableDebug { get; set; }
        private static object obj = new object();

        public static void LogInfo(string message)
        {
            string logFolder = "log";
            if(!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            string filePath = Path.Combine(logFolder, DateTime.Today.ToString("yyyyMMdd") + ".txt");

            string content = $"{DateTime.Now.ToString("yyyyMMdd HH.mm.ss")}:{message}";          

            lock (obj)
            {
                File.AppendAllLines(filePath, new string[] { content });
            }

            if (EnableDebug)
            {
                Console.WriteLine(content);
            }
        }      
    }
}
