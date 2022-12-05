using DatabaseInterpreter.Model;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseInterpreter.Core
{
    public class PostgreBulkCopy : IDisposable
    {
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _externalTransaction { get; set; }
        private static Dictionary<string, NpgsqlDbType> dataTypeMappings = new Dictionary<string, NpgsqlDbType>()
        {
            { "smallint",  NpgsqlDbType.Smallint},
            { "integer",  NpgsqlDbType.Integer},
            { "bigint",  NpgsqlDbType.Bigint},
            { "real",  NpgsqlDbType.Real},
            { "double precision",  NpgsqlDbType.Double},
            { "numeric",  NpgsqlDbType.Numeric},
            { "money",  NpgsqlDbType.Money},
            { "date",  NpgsqlDbType.Date},
            { "text",  NpgsqlDbType.Text},
            { "character varying",  NpgsqlDbType.Varchar}
        };

        private bool _ownsTheConnection = false;

        public PostgreBulkCopy(string connectionString) : this(new NpgsqlConnection(connectionString))
        {
            this._ownsTheConnection = true;
        }

        public PostgreBulkCopy(NpgsqlConnection connection) : this(connection, null) { }

        public PostgreBulkCopy(NpgsqlConnection connection, NpgsqlTransaction transation = null)
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
        public bool ColumnNameNeedQuoted { get; set; }
        public bool DetectDateTimeTypeByValues { get; set; }
        public int BulkCopyTimeout { get; set; }

        public IEnumerable<TableColumn> TableColumns { get; set; }
        private void ValidateConnection()
        {
            if (_connection == null)
            {
                throw new Exception("Postgres Database Connection is required");
            }

            if (_externalTransaction != null && _externalTransaction.Connection != _connection)
            {
                throw new Exception("Postgres Transaction mismatch with Oracle Database Connection");
            }
        }
        private async Task OpenConnectionAsync()
        {
            if (this._ownsTheConnection && this._connection.State != ConnectionState.Open)
            {
                await this._connection.OpenAsync();
            }
        }
        public async Task<ulong> WriteToServerAsync(DataTable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            return await this.CopyData(table);
        }
        private async Task<ulong> CopyData(DataTable table)
        {
            NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();

            string columnList = this.GetColumnList(table);

            this.ValidateConnection();
            await this.OpenConnectionAsync();

            this._connection.TypeMapper.UseNetTopologySuite();

            string commandText = $"COPY {this.DestinationTableName}({columnList}) FROM STDIN (FORMAT BINARY)";

            using (var writer = this._connection.BeginBinaryImport(commandText))
            {
                writer.Timeout = TimeSpan.FromSeconds(this.BulkCopyTimeout);

                foreach (DataRow row in table.Rows)
                {
                    await writer.StartRowAsync();

                    foreach (DataColumn col in table.Columns)
                    {
                        var result = this.ParseDbTypeFromDotnetType(col.ColumnName, row[col.ColumnName], col.DataType);

                        await writer.WriteAsync(result.Value, result.Type);
                    }
                }

                return await writer.CompleteAsync();
            }
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
                if (this._ownsTheConnection)
                {
                    _connection.Dispose();
                }

                this._connection = null;
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public (dynamic Value, NpgsqlDbType Type) ParseDbTypeFromDotnetType(string columnName, dynamic value, Type t)
        {
            NpgsqlDbType dbType = NpgsqlDbType.Unknown;

            if (t == typeof(Guid))
            {
                dbType = NpgsqlDbType.Varchar;
            }
            else if (t == typeof(byte[]))
            {
                dbType = NpgsqlDbType.Bytea;
            }
            else if (t == typeof(string))
            {
                dbType = NpgsqlDbType.Text;
            }
            else if (t == typeof(DateTimeOffset))
            {
                dbType = NpgsqlDbType.TimestampTz;
            }
            else if (t == typeof(Int32))
            {
                dbType = NpgsqlDbType.Integer;
            }
            else if (t == typeof(sbyte))
            {
                dbType = NpgsqlDbType.Smallint;
            }
            else if (t == typeof(byte))
            {
                dbType = NpgsqlDbType.Smallint;
            }
            else if (t == typeof(TimeSpan))
            {
                dbType = NpgsqlDbType.Time;
            }
            else if (t == typeof(bool))
            {
                dbType = NpgsqlDbType.Boolean;
            }
            else
            {
                string targetColumnDataType = this.FindTableColumnType(columnName)?.ToLower();

                if (t == typeof(Int16))
                {
                    NpgsqlDbType? mappedDataType = this.GetMappedDataType(targetColumnDataType);

                    if (mappedDataType.HasValue)
                    {
                        dbType = mappedDataType.Value;
                    }
                    else
                    {
                        dbType = NpgsqlDbType.Smallint;
                    }
                }
                if (t == typeof(Int64))
                {
                    NpgsqlDbType? mappedDataType = this.GetMappedDataType(targetColumnDataType);

                    if (mappedDataType.HasValue)
                    {
                        dbType = mappedDataType.Value;
                    }
                    else
                    {
                        dbType = NpgsqlDbType.Bigint;
                    }
                }
                else if (t == typeof(float))
                {
                    NpgsqlDbType? mappedDataType = this.GetMappedDataType(targetColumnDataType);

                    if (mappedDataType.HasValue)
                    {
                        dbType = mappedDataType.Value;
                    }
                    else
                    {
                        dbType = NpgsqlDbType.Real;
                    }
                }
                else if (t == typeof(double))
                {
                    NpgsqlDbType? mappedDataType = this.GetMappedDataType(targetColumnDataType);

                    if (mappedDataType.HasValue)
                    {
                        dbType = mappedDataType.Value;
                    }
                    else
                    {
                        dbType = NpgsqlDbType.Double;
                    }
                }
                else if (t == typeof(decimal))
                {
                    NpgsqlDbType? mappedDataType = this.GetMappedDataType(targetColumnDataType);

                    if (mappedDataType.HasValue)
                    {
                        dbType = mappedDataType.Value;
                    }
                    else
                    {
                        dbType = NpgsqlDbType.Numeric;
                    }
                }
                else if (t == typeof(DateTime))
                {
                    NpgsqlDbType? mappedDataType = this.GetMappedDataType(targetColumnDataType);

                    if (mappedDataType.HasValue)
                    {
                        dbType = mappedDataType.Value;
                    }
                    else
                    {
                        dbType = NpgsqlDbType.Timestamp;
                    }
                }
                else if (Enum.TryParse(t.Name, out NpgsqlDbType _))
                {
                    dbType = (NpgsqlDbType)Enum.Parse(typeof(NpgsqlDbType), t.Name);
                }
            }

            if (dbType == NpgsqlDbType.Unknown)
            {
                dbType = NpgsqlDbType.Varchar;
            }

            return (value, dbType);
        }

        private string FindTableColumnType(string columnName)
        {
            if (this.TableColumns != null)
            {
                return this.TableColumns.FirstOrDefault(item => item.Name == columnName)?.DataType;
            }

            return null;
        }

        private NpgsqlDbType? GetMappedDataType(string dataType)
        {
            if(dataType!=null && dataTypeMappings.ContainsKey(dataType))
            {
                return dataTypeMappings[dataType];
            }

            return default(NpgsqlDbType?);
        }
    }
}