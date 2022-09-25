using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Model;
using System;
using System.IO;

namespace DatabaseManager.Core
{
    public class SqlServerBackup : DbBackup
    {
        public SqlServerBackup() : base() { }

        public SqlServerBackup(BackupSetting setting, ConnectionInfo connectionInfo) : base(setting, connectionInfo)
        {
        }

        public override string Backup()
        {
            if(this.Setting==null)
            {
                throw new ArgumentException($"There is no backup setting for SqlServer.");
            }                

            string fileNameWithoutExt = this.ConnectionInfo.Database + "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string fileName = fileNameWithoutExt + ".bak";

            string saveFolder = this.CheckSaveFolder();

            string saveFilePath = Path.Combine(saveFolder, fileName);

            SqlServerInterpreter interpreter = new SqlServerInterpreter(this.ConnectionInfo, new DbInterpreterOption());

            string sql = $@"use master; backup database {this.ConnectionInfo.Database} to disk='{saveFilePath}';";

            interpreter.ExecuteNonQueryAsync(sql);

            return saveFilePath;
        }
    }
}
