using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Diagnostics;
using System.IO;

namespace DatabaseManager.Core
{
    public class FileHelper
    {
        public static void Zip(string sourceFilePath, string zipFilePath)
        {
            var folderPath = Path.GetDirectoryName(zipFilePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            ZipFile zip = ZipFile.Create(zipFilePath);

            zip.BeginUpdate();

            zip.Add(sourceFilePath, Path.GetFileName(sourceFilePath));

            zip.CommitUpdate();
            zip.Close();
        }

        public static void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var psi = new ProcessStartInfo(filePath) { UseShellExecute = true };

                Process.Start(psi);
            }
        }

        public static void SelectFileInExplorer(string filePath)
        {
            if (File.Exists(filePath))
            {
                string cmd = "explorer.exe";
                string arg = "/select," + filePath;

                var psi = new ProcessStartInfo(cmd, arg) { UseShellExecute = true };

                Process.Start(psi);
            }
        }

        public static decimal GetFileSizeInMB(long size)
        {
            return Math.Round((decimal)size / 1024 / 1024, 2);
        }
    }
}
