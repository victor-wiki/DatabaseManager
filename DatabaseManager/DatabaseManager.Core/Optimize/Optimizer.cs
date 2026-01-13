using Dapper;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class Optimizer
    {
        private DbInterpreter dbInterpreter;
        public Optimizer(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
        }

        public async Task<OptimizeResult> Optimize()
        {
            OptimizeResult result = new OptimizeResult();

            DatabaseType databaseType = dbInterpreter.DatabaseType;

            ConnectionInfo connectionInfo = this.dbInterpreter.ConnectionInfo;

            result.Details = new List<OptimizeResultDetail>();

            try
            {
                using (var con = this.dbInterpreter.CreateConnection())
                {
                    if (databaseType == DatabaseType.Sqlite)
                    {
                        string filePath = connectionInfo.Database;

                        string fileName = Path.GetFileName(filePath);

                        var fileInfo = new FileInfo(filePath);

                        OptimizeResultDetail detail = new OptimizeResultDetail() { ObjectType = nameof(Database), ObjectName = Path.GetFileNameWithoutExtension(fileName) };

                        detail.DataLengthBeforeOptimization = FileHelper.GetFileSizeInMB(fileInfo.Length);

                        string sql = "VACUUM";

                        await con.ExecuteAsync(sql);

                        detail.IsOK = true;

                        fileInfo = new FileInfo(filePath);

                        detail.DataLengthAfterOptimization = FileHelper.GetFileSizeInMB(fileInfo.Length);

                        result.Details.Add(detail);

                        result.IsOK = true;
                    }
                    else if (databaseType == DatabaseType.MySql)
                    {
                        Func<Task<IEnumerable<TableDataLength>>> getTableDataLengths = async () =>
                        {
                            string sql =
                               $@"SELECT TABLE_SCHEMA AS `Schema`, TABLE_NAME AS `Name`, DATA_LENGTH + INDEX_LENGTH AS `Length`
                               FROM information_schema.TABLES
                               WHERE TABLE_SCHEMA = '{connectionInfo.Database}'";

                            var results = await con.QueryAsync<TableDataLength>(sql);

                            return results;
                        };

                        IEnumerable<TableDataLength> tableDataLengths = await getTableDataLengths();

                        string sql =
                            $@"SELECT TABLE_SCHEMA AS `Schema`, TABLE_NAME AS `Name`  FROM INFORMATION_SCHEMA.`TABLES`
                               WHERE TABLE_TYPE ='BASE TABLE' AND TABLE_SCHEMA ='{connectionInfo.Database}' AND ENGINE='InnoDB'";

                        var tables = await con.QueryAsync<Table>(sql);

                        foreach (var table in tables)
                        {
                            OptimizeResultDetail detail = new OptimizeResultDetail() { ObjectType = nameof(Table), ObjectName = table.Name };

                            long length = tableDataLengths.FirstOrDefault(item => item.Name == table.Name).Length;

                            detail.DataLengthBeforeOptimization = FileHelper.GetFileSizeInMB(length);

                            try
                            {
                                sql = $"ALTER TABLE {this.dbInterpreter.GetQuotedString(table.Name)} ENGINE='InnoDB'";

                                await con.ExecuteAsync(sql);

                                detail.IsOK = true;
                            }
                            catch (Exception ex)
                            {
                                detail.Message = ex.Message;
                            }

                            result.Details.Add(detail);
                        }

                        tableDataLengths = await getTableDataLengths();

                        foreach(var detail in result.Details)
                        {
                            var tableDataLength = tableDataLengths.FirstOrDefault(item => item.Name == detail.ObjectName);

                            detail.DataLengthAfterOptimization = FileHelper.GetFileSizeInMB(tableDataLength.Length);
                        }

                        result.IsOK = true;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }
    }    
}
