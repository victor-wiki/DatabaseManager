using Dapper;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class Analysiser
    {
        private DbInterpreter dbInterpreter;

        public Analysiser(DbInterpreter dbInterpreter)
        {
            this.dbInterpreter = dbInterpreter;
        }

        public async Task<IEnumerable<IndexFragmentation>> GetIndexFragmentations()
        {
            DatabaseType databaseType = dbInterpreter.DatabaseType;

            using (var con = this.dbInterpreter.CreateConnection())
            {
                string sql = null;

                if (databaseType == DatabaseType.SqlServer)
                {
                    sql =
@"SELECT SCHEMA_NAME(t.schema_id) AS [Schema],OBJECT_NAME(i.OBJECT_ID) AS TableName,  i.name AS IndexName,ROUND(avg_fragmentation_in_percent,2) AS FragmentationPercent
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, NULL) indexstats
INNER JOIN sys.indexes i 
JOIN sys.tables t ON i.object_id=t.object_id
ON i.object_id = indexstats.object_id 
AND i.index_id = indexstats.index_id
WHERE avg_fragmentation_in_percent>0";
                }
                else if (databaseType == DatabaseType.Postgres)
                {
                    var tableNames = (await this.dbInterpreter.GetTablesAsync(con)).Select(item => item.Name).ToArray(); ;
                    var indexes = await this.dbInterpreter.GetTableIndexesAsync(con, new SchemaInfoFilter() { TableNames = tableNames }, false);

                    StringBuilder sb = new StringBuilder();

                    int i = 0;
                    foreach (var index in indexes)
                    {
                        sb.AppendLine($@"SELECT '{index.Schema}' AS ""Schema"",'{index.TableName}' AS ""TableName"", '{index.Name}' AS ""IndexName"", leaf_fragmentation AS ""FragmentationPercent"" FROM pgstatindex('{index.Name}') WHERE leaf_fragmentation>0 and leaf_fragmentation<>'NaN'");

                        if (i < indexes.Count - 1)
                        {
                            sb.AppendLine("UNION ALL");
                        }

                        i++;
                    }

                    sql = sb.ToString();
                }
                #region Oracle
                else if (databaseType == DatabaseType.Oracle)
                {
                    var tableNames = (await this.dbInterpreter.GetTablesAsync(con)).Select(item => item.Name).ToArray(); ;
                    var indexes = await this.dbInterpreter.GetTableIndexesAsync(con, new SchemaInfoFilter() { TableNames = tableNames }, false);

                    foreach (var index in indexes)
                    {
                        string execSql = $"analyze index {this.dbInterpreter.GetQuotedDbObjectNameWithSchema(index)} validate structure";

                        await con.ExecuteAsync(execSql);
                    }

                    sql =
$@"SELECT I.TABLE_OWNER AS ""Schema"",I.TABLE_NAME AS ""TableName"",S.NAME AS ""IndexName"",
ROUND(del_lf_rows_len*100.0/lf_rows_len,2) AS ""FragmentationPercent""
FROM sys.INDEX_STATS S
JOIN USER_INDEXES I ON S.NAME=I.INDEX_NAME
WHERE I.TABLE_OWNER='{this.dbInterpreter.ConnectionInfo.Database}' AND del_lf_rows_len*100.0/lf_rows_len>0";

                }
                #endregion
                else
                {
                    throw new NotImplementedException();
                }

                var results = await con.QueryAsync<IndexFragmentation>(sql);

                return results;
            }
        }

        public async Task<OperateResult> RebuildIndex(IndexFragmentation indexFragmentation)
        {
            OperateResult result = new OperateResult();

            TableIndex index = new TableIndex() { Schema = indexFragmentation.Schema, TableName = indexFragmentation.TableName, Name = indexFragmentation.IndexName };

            try
            {
                using (var con = this.dbInterpreter.CreateConnection())
                {
                    await con.OpenAsync();

                    var trans = await con.BeginTransactionAsync();

                    var scriptGenerator = DbScriptGeneratorHelper.GetDbScriptGenerator(this.dbInterpreter);

                    DatabaseType databaseType = dbInterpreter.DatabaseType;

                    Script script = null;

                    if (databaseType == DatabaseType.SqlServer)
                    {
                        script = (scriptGenerator as SqlServerScriptGenerator).RebuildIndex(index);
                    }
                    else if (databaseType == DatabaseType.Postgres)
                    {
                        script = (scriptGenerator as PostgresScriptGenerator).RebuildIndex(index);
                    }
                    else if (databaseType == DatabaseType.Oracle)
                    {
                        script = (scriptGenerator as OracleScriptGenerator).RebuildIndex(index);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    if (script != null)
                    {
                        string sql = script.Content.Trim(' ', ';');

                        await con.ExecuteAsync(sql, null, trans);

                        await trans.CommitAsync();

                        result.IsOK = true;
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
