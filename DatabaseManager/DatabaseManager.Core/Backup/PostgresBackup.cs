using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.IO;

namespace DatabaseManager.Core
{
    public class PostgresBackup : DbBackup
    {
        public PostgresBackup() : base() { }

        public PostgresBackup(BackupSetting setting, ConnectionInfo connectionInfo) : base(setting, connectionInfo)
        {
        }

        public override string Backup()
        {
            if(this.Setting==null)
            {
                throw new ArgumentException($"There is no backup setting for Postgres.");
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
            else if (Path.GetFileName(exeFilePath).ToLower() != "pg_dump.exe")
            {
                throw new ArgumentException($"The backup file should be pg_dump.exe");
            }

            string server = this.ConnectionInfo.Server;
            string port = string.IsNullOrEmpty(this.ConnectionInfo.Port) ? PostgresInterpreter.DEFAULT_PORT.ToString() : this.ConnectionInfo.Port;
            string database = this.ConnectionInfo.Database;
            string userName = this.ConnectionInfo.UserId;
            string password = this.ConnectionInfo.Password;
            string strPassword = this.ConnectionInfo.IntegratedSecurity ? "" : $":{password}";

            string fileNameWithoutExt = this.ConnectionInfo.Database + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = fileNameWithoutExt + ".tar";

            string saveFolder = this.CheckSaveFolder();

            string saveFilePath = Path.Combine(saveFolder, fileName);

            string cmdArgs = $@"--dbname=postgresql://{userName}{strPassword}@{server}:{port}/{database}  -Ft -f ""{saveFilePath}""";

            string dumpFilePath = Path.Combine(Path.GetDirectoryName(this.Setting.ClientToolFilePath), "pg_dump.exe");

            string result = ProcessHelper.RunExe(dumpFilePath, cmdArgs, new string[] { "exit" });           
            
            if(!string.IsNullOrEmpty(result))
            {
                throw new Exception(result);
            }

            return saveFilePath;
        }
    }
}
