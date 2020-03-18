using Newtonsoft.Json;
using System;
using System.IO;

namespace DatabaseInterpreter.Demo
{
    public class OutputHelper
    {
        public static void Output(string name, object obj, bool useJson)
        {
            string content = "";
            if (useJson)
            {
                content = JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            else
            {
                content = obj?.ToString();
            }

            Console.WriteLine(name + ":");
            Console.WriteLine(content);

            string folder = Path.Combine("output", DateTime.Today.ToString("yyyyMMdd"));

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText($@"{folder}\\{name}.txt", content);
        }
    }
}
