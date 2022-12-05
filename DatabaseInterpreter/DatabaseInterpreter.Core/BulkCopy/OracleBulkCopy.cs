/***
 * This file refers to: https://github.com/TylerHaigh/OracleBulkCopy/blob/master/OracleBulkCopy.Standard/OracleBulkCopy.cs
 * 
***/
using DatabaseInterpreter.Geometry;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class OracleBulkCopy : IDisposable
    {
        // https://github.com/Microsoft/referencesource/blob/master/System.Data/System/Data/SqlClient/SqlBulkCopy.cs
        // https://stackoverflow.com/questions/47942691/how-to-make-a-bulk-insert-using-oracle-managed-data-acess-c-sharp
        // https://github.com/DigitalPlatform/dp2/blob/master/DigitalPlatform.rms.db/OracleBulkCopy.cs
        // https://msdn.microsoft.com/en-us/library/system.data.oracleclient.oracletype(v=vs.110).aspx

        private OracleConnection _connection;
        private OracleTransaction _externalTransaction { get; set; }

        /// <summary>
        /// Set to TRUE if the BulkCopy object was not instantiated with an external OracleConnection
        /// and thus it is up to the BulkCopy object to open and close connections
        /// </summary>
        private bool _ownsTheConnection = false;

        public OracleBulkCopy(string connectionString) : this(new OracleConnection(connectionString))
        {
            this._ownsTheConnection = true;
        }

        public OracleBulkCopy(OracleConnection connection) : this(connection, null) { }

        public OracleBulkCopy(OracleConnection connection, OracleTransaction transation = null)
        {
            this._connection = connection;
            this._externalTransaction = transation;            
        }

        private string _destinationTableName;

        public string DestinationTableName
        {
            get { return _destinationTableName; }
            set
            {
                if (value == null || value.Length == 0)
                {
                    throw new ArgumentException("Destination table name cannot be null or empty string");
                }

                _destinationTableName = value;
            }
        }

        private int _batchSize = 0;

        public int BatchSize
        {
            get { return _batchSize; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Batch Size must be a positive integer");
                }

                _batchSize = value;
            }
        }

        public int BulkCopyTimeout { get; set; }

        private bool UploadEverythingInSingleBatch { get { return _batchSize == 0; } }

        public bool ColumnNameNeedQuoted { get; set; }
        public bool DetectDateTimeTypeByValues { get; set; }

        private void ValidateConnection()
        {
            if (_connection == null)
            {
                throw new Exception("Oracle Database Connection is required");
            }

            if (_externalTransaction != null && _externalTransaction.Connection != _connection)
            {
                throw new Exception("Oracle Transaction mismatch with Oracle Database Connection");
            }
        }

        private async Task OpenConnectionAsync()
        {
            if (this._ownsTheConnection && this._connection.State != ConnectionState.Open)
            {
                await this._connection.OpenAsync();
            }
        }

        public async Task<int> WriteToServerAsync(DataTable table)
        {
            // https://stackoverflow.com/questions/47942691/how-to-make-a-bulk-insert-using-oracle-managed-data-acess-c-sharp
            // https://github.com/Microsoft/referencesource/blob/master/System.Data/System/Data/SqlClient/SqlBulkCopy.cs

            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            // TODO: Validate TableName to prevent SQL Injection
            // https://oracle-base.com/articles/10g/dbms_assert_10gR2
            // SELECT SYS.DBMS_ASSERT.qualified_sql_name('object_name') FROM dual;

            if (this.UploadEverythingInSingleBatch)
            {
                return await this.WriteToServerInSingleBatchAsync(table);
            }
            else
            {
                return await this.WriteToServerInMultipleBatchesAsync(table);
            }
        }

        private async Task<int> WriteToServerInSingleBatchAsync(DataTable table)
        {
            string commandText = this.BuildCommandText(table);

            return await this.WriteSingleBatchOfData(table, 0, commandText, table.Rows.Count);
        }

        private async Task<int> WriteToServerInMultipleBatchesAsync(DataTable table)
        {
            // Calculate number of batches
            int numBatchesRequired = (int)Math.Ceiling(table.Rows.Count / (double)BatchSize);

            string commandText = this.BuildCommandText(table);

            int count = 0;

            for (int i = 0; i < numBatchesRequired; i++)
            {
                int skipOffset = i * this.BatchSize;
                int batchSize = Math.Min(this.BatchSize, table.Rows.Count - skipOffset);
                count += await this.WriteSingleBatchOfData(table, skipOffset, commandText, batchSize);
            }

            return count;
        }

        private string BuildCommandText(DataTable table)
        {
            // Build the command string
            string commandText = "INSERT INTO " + this.DestinationTableName + " ( @@ColumnList@@ ) VALUES ( @@ValueList@@ )";
            string columnList = this.GetColumnList(table);
            string valueList = this.GetValueList(table);

            // Replace the placeholders with actual values
            commandText = commandText.Replace("@@ColumnList@@", columnList);
            commandText = commandText.Replace("@@ValueList@@", valueList);

            // TODO: Validate commandText to prevent SQL Injection
            // https://oracle-base.com/articles/10g/dbms_assert_10gR2

            return commandText;
        }

        private async Task<int> WriteSingleBatchOfData(DataTable table, int skipOffset, string commandText, int batchSize)
        {
            List<OracleParameter> parameters = this.GetParameters(table, batchSize, skipOffset);

            OracleCommand cmd = this._connection.CreateCommand();
            cmd.CommandText = commandText;
            cmd.ArrayBindCount = batchSize;
            parameters.ForEach(p => cmd.Parameters.Add(p));

            // Validate and open the connection
            this.ValidateConnection();
            await this.OpenConnectionAsync();

            cmd.CommandTimeout = this.BulkCopyTimeout;

            return await cmd.ExecuteNonQueryAsync();
        }

        private List<OracleParameter> GetParameters(DataTable data, int batchSize, int skipOffset = 0)
        {
            List<OracleParameter> parameters = new List<OracleParameter>();

            foreach (DataColumn c in data.Columns)
            {
                OracleDbType dbType = GetOracleDbTypeFromDotnetType(c.DataType);

                // https://stackoverflow.com/a/23735845/2442468
                // https://stackoverflow.com/a/17595403/2442468

                var columnData = data.AsEnumerable().Select(r => r.Field<object>(c.ColumnName));
                object[] paramDataArray = (UploadEverythingInSingleBatch)
                    ? columnData.ToArray()
                    : columnData.Skip(skipOffset).Take(batchSize).ToArray();

                if (this.DetectDateTimeTypeByValues)
                {
                    if (dbType == OracleDbType.Date)
                    {
                        if (c.AllowDBNull)
                        {
                            if (paramDataArray.Cast<DateTime?>().Any(item => item.HasValue && item.Value.Millisecond > 0))
                            {
                                dbType = OracleDbType.TimeStamp;
                            }
                        }
                        else
                        {
                            if (paramDataArray.Cast<DateTime>().Any(item => item.Millisecond > 0))
                            {
                                dbType = OracleDbType.TimeStamp;
                            }
                        }
                    }
                }

                OracleParameter param = new OracleParameter();
                param.OracleDbType = dbType;
                param.Value = paramDataArray;

                if(c.DataType.CustomAttributes!=null)
                {
                    foreach(var customAttrType in c.DataType.CustomAttributes)
                    {
                        if(customAttrType.AttributeType == typeof(OracleCustomTypeMappingAttribute))
                        {
                            param.UdtTypeName = customAttrType.ConstructorArguments.FirstOrDefault().ToString();
                            break;
                        }
                    }
                }                

                parameters.Add(param);
            }

            return parameters;
        }


        private string GetColumnList(DataTable data)
        {
            string[] columnNames = data.Columns.Cast<DataColumn>()
                                   .Select(x => this.GetColumnName(x.ColumnName)).ToArray();

            string columnList = string.Join(",", columnNames);
            return columnList;
        }

        private string GetColumnName(string columnName)
        {
            if (this.ColumnNameNeedQuoted || columnName.Contains(" "))
            {
                return $@"""{columnName}""";
            }
            else
            {
                return columnName;
            }
        }

        private string GetValueList(DataTable data)
        {
            const string Delimiter = ", ";

            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= data.Columns.Count; i++)
            {
                sb.Append(string.Format(":{0}", i));
                sb.Append(Delimiter);
            }

            sb.Length -= Delimiter.Length;

            string valueList = sb.ToString();
            return valueList;
        }

        public void Dispose()
        {
            if (this._connection != null)
            {
                // Only close the connection if the BulkCopy instance owns the connection
                if (this._ownsTheConnection)
                {
                    _connection.Dispose();
                }

                // Always set to null
                this._connection = null;
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public static OracleDbType GetOracleDbTypeFromDotnetType(Type t)
        {
            if (t == typeof(Guid)) return OracleDbType.Raw;
            if (t == typeof(byte[])) return OracleDbType.Blob;
            if (t == typeof(string)) return OracleDbType.Varchar2;
            if (t == typeof(DateTime)) return OracleDbType.Date;
            if (t == typeof(DateTimeOffset)) return OracleDbType.TimeStampTZ;
            if (t == typeof(decimal)) return OracleDbType.Decimal;
            if (t == typeof(Int32)) return OracleDbType.Int32;
            if (t == typeof(Int64)) return OracleDbType.Int64;
            if (t == typeof(Int16)) return OracleDbType.Int16;
            if (t == typeof(sbyte)) return OracleDbType.Byte;
            if (t == typeof(byte)) return OracleDbType.Int16;    // <== unverified
            if (t == typeof(float)) return OracleDbType.Single;
            if (t == typeof(double)) return OracleDbType.Double;
            if (t == typeof(TimeSpan)) return OracleDbType.IntervalDS;
            if (t == typeof(bool)) return OracleDbType.Int16;

            if (t.BaseType?.Name?.Contains("OracleCustomType") == true)
            {
                return OracleDbType.Object;
            }

            // Tylers
            //if (o is bool) return OracleDbType.Boolean;
            //if (o is char) return OracleDbType.Char;

            return OracleDbType.Varchar2;
        }
    }
}