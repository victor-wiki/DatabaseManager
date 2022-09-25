using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Helper;
using DatabaseManager.Model;
using System;
using System.IO;

namespace DatabaseManager.Core
{
    public class OracleBackup : DbBackup
    {
        public OracleBackup() : base() { }

        public OracleBackup(BackupSetting setting, ConnectionInfo connectionInfo) : base(setting, connectionInfo)
        {
        }

        public override string Backup()
        {
            if(this.Setting==null)
            {
                throw new ArgumentException($"There is no backup setting for Oracle.");
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
            else if (Path.GetFileName(exeFilePath).ToLower() != "exp.exe")
            {
                throw new ArgumentException($"The backup file should be exp.exe");
            }

            string server = this.ConnectionInfo.Server;
            string port = string.IsNullOrEmpty(this.ConnectionInfo.Port) ? OracleInterpreter.DEFAULT_PORT.ToString() : this.ConnectionInfo.Port;

            string serviceName = OracleInterpreter.DEFAULT_SERVICE_NAME;

            if (server != null && server.Contains("/"))
            {
                string[] serverService = server.Split('/');
                server = serverService[0];
                serviceName = serverService[1];
            }

            string connectArgs = "";

            if(this.ConnectionInfo.IntegratedSecurity)
            {
                connectArgs = "/";
            }
            else
            {
                connectArgs = $"{this.ConnectionInfo.UserId}/{this.ConnectionInfo.Password}@{server}:{port}/{serviceName} OWNER={this.ConnectionInfo.UserId}";       
            }

            if (this.ConnectionInfo.IsDba)
            {
                connectArgs += " AS SYSDBA";
            }            

            string cmdArgs = $"-L -S {connectArgs} FULL=Y DIRECT=Y";

            string sqlplusFilePath = Path.Combine(Path.GetDirectoryName(this.Setting.ClientToolFilePath), "sqlplus.exe");

            string output = ProcessHelper.RunExe(sqlplusFilePath, cmdArgs, new string[] { "exit" });

            if (!string.IsNullOrEmpty(output) && output.ToUpper().Contains("ERROR"))
            {
                throw new Exception("Login failed.");
            }

            string fileNameWithoutExt = this.ConnectionInfo.Database + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = fileNameWithoutExt + ".dmp";

            string saveFolder = this.CheckSaveFolder();

            string saveFilePath = Path.Combine(saveFolder, fileName);

            cmdArgs = $"{connectArgs} file='{saveFilePath}'";

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
