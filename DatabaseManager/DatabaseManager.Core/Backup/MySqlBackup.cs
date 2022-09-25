using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.IO;

namespace DatabaseManager.Core
{
    public class MySqlBackup : DbBackup
    {
        public MySqlBackup() : base() { }

        public MySqlBackup(BackupSetting setting, ConnectionInfo connectionInfo) : base(setting, connectionInfo)
        {
        }

        public override string Backup()
        {
            if(this.Setting==null)
            {
                throw new ArgumentException($"There is no backup setting for MySql.");
            }

            string exeFilePath = this.Setting.ClientToolFilePath;

            if (string.IsNullOrEmpty(exeFilePath))
            {
                throw new ArgumentNullException("client backup file path is empty.");
            }

            if (!File.Exists(exeFilePath))
            {
                throw new ArgumentException($"The backup file path is not existed:{this.Setting.ClientToolFilePath}.");
            }
            else if (Path.GetFileName(exeFilePath).ToLower() != "mysqldump.exe")
            {
                throw new ArgumentException($"The backup file should be mysqldump.exe");
            }

            string fileNameWithoutExt = this.ConnectionInfo.Database + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = fileNameWithoutExt + ".sql";

            string saveFolder = this.CheckSaveFolder();

            string saveFilePath = Path.Combine(saveFolder, fileName);

            string server = this.ConnectionInfo.Server;
            string port = string.IsNullOrEmpty(this.ConnectionInfo.Port) ? MySqlInterpreter.DEFAULT_PORT.ToString() : this.ConnectionInfo.Port;
            string userId = this.ConnectionInfo.UserId;
            string password = this.ConnectionInfo.Password;
            string database = this.ConnectionInfo.Database;
            string charset = SettingManager.Setting.MySqlCharset;
            string skipQuotationNames = SettingManager.Setting.DbObjectNameMode == DbObjectNameMode.WithoutQuotation ? "--skip-quote-names" : "";

            string cmdArgs = $"--quick --default-character-set={charset} {skipQuotationNames} --lock-tables --force --host={server}  --port={port} --user={userId} --password={password} {database} -r \"{saveFilePath}\"";
               
            ProcessHelper.RunExe(this.Setting.ClientToolFilePath, cmdArgs);

            string zipFilePath = Path.Combine(saveFolder, fileNameWithoutExt + ".zip");

            if (this.Setting.ZipFile)
            {
                saveFilePath = this.ZipFile(saveFilePath, zipFilePath);
            }

            return saveFilePath;
        }
    }
}
