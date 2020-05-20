using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace DatabaseManager.Helper
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
    }
}
