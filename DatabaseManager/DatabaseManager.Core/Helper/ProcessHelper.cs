using System;
using System.Diagnostics;

namespace DatabaseManager.Helper
{
    public class ProcessHelper
    {
        public static string RunExe(string exeFilePath, string args, string[] inputs = null, DataReceivedEventHandler errorEventHandler = null)
        {
            using (Process proc = new Process())
            {
                if (errorEventHandler != null)
                {
                    proc.ErrorDataReceived += errorEventHandler;
                }

                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.FileName = exeFilePath;
                proc.StartInfo.Arguments = args;
                proc.StartInfo.CreateNoWindow = true;

                proc.Start();

                if (inputs != null && inputs.Length > 0)
                {
                    foreach (var cmd in inputs)
                    {
                        proc.StandardInput.WriteLine(cmd);
                    }
                }

                string output = proc.StandardOutput.ReadToEnd();

                proc.WaitForExit();

                return output;
            }
        }
    }
}
