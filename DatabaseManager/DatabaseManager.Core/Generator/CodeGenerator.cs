using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class CodeGenerator
    {
        private DbInterpreter dbInterpreter;
        private CodeGenerateOption option;
        private IObserver<FeedbackInfo> observer;

        public CodeGenerator(DbInterpreter dbInterpreter, CodeGenerateOption option)
        {
            this.dbInterpreter = dbInterpreter;
            this.option = option;
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task<CodeGenerateResult> Generate(CancellationToken cancellationToken)
        {
            CodeGenerateResult result = new CodeGenerateResult();

            var tables = this.option.Tables;
            var views = this.option.Views;

            var tableNames = tables.Select(item => item.Name).ToArray();
            var viewNames = views.Select(item => item.Name).ToArray();           

            try
            {
                this.Feedback("Begin to get columns...");

                var tableColumns = await this.dbInterpreter.GetTableColumnsAsync(new SchemaInfoFilter() { TableNames = tableNames });
                var viewColumns = await this.dbInterpreter.GetTableColumnsAsync(new SchemaInfoFilter() { TableNames = viewNames, IsForView = true });

                this.Feedback("End get columns.");

                ProgrammingLanguage language = this.option.Language;

                List<DatabaseObject> dbObjects = new List<DatabaseObject>();

                dbObjects.AddRange(tables);
                dbObjects.AddRange(views);

                int total = tables.Count + views.Count;
                int count = 0;

                foreach (var dbObject in dbObjects)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    count++;

                    string objectType = dbObject.GetType().Name;
                    string objectName = dbObject.Name;
                    bool isView = dbObject is View;

                    this.Feedback($@"({count}/{total})Begin to generate {objectType} ""{dbObject.Name}""...");

                    IEnumerable<TableColumn> columns = null;
                    
                    if(!isView)
                    {
                        columns = tableColumns.Where(item => item.TableName == dbObject.Name && item.Schema == dbObject.Schema);
                    }
                    else
                    {
                        columns = viewColumns.Where(item => item.TableName == dbObject.Name && item.Schema == dbObject.Schema);
                    }

                    (string FileName, string FileContent) res = default((string, string));

                    switch (language)
                    {
                        case ProgrammingLanguage.CSharp:
                            res = this.GenerateCSharpCode(dbObject, columns);
                            break;
                        case ProgrammingLanguage.Java:
                            res = this.GenerateJavaCode(dbObject, columns);
                            break;
                    }

                    if (!string.IsNullOrEmpty(res.FileName))
                    {
                        this.WriteToFile(res.FileName, res.FileContent);
                    }

                    this.Feedback($@"({count}/{total})End generate {objectType} ""{dbObject.Name}"".");
                }         

                result.IsOK = true;
            }
            catch (Exception ex)
            {
                string message = ExceptionHelper.GetExceptionDetails(ex);

                result.Message = message;

                this.Feedback(this, message, FeedbackInfoType.Error);
            }

            return result;
        }

        private void WriteToFile(string fileName, string fileContent)
        {
            string folder = this.option.OutputFolder;

            string filePath = Path.Combine(folder, fileName);

            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
        }

        private (string FileName, string FileContent) GenerateCSharpCode(DatabaseObject tableOrView, IEnumerable<TableColumn> columns)
        {
            string fileName = $"{tableOrView.Name.Replace(" ","_")}.cs";

            List<string> usingNamespaces = new List<string>();

            StringBuilder sb = new StringBuilder();

            string @namespace = this.option.Namespace;
            bool hasNamespace = !string.IsNullOrEmpty(@namespace);

            if (hasNamespace)
            {
                sb.AppendLine($"namespace {@namespace}");
                sb.AppendLine("{");
            }

            string classIndent = hasNamespace ? "\t" : "";

            sb.AppendLine($"{classIndent}public class {tableOrView.Name}");
            sb.AppendLine(classIndent + "{");

            foreach (var column in columns)
            {
                string dataType = column.DataType;
                bool isNullable = column.IsNullable;

                string type = null;
                string nullableFlag = "";
                DatabaseType databaseType = this.dbInterpreter.DatabaseType;

                if (!dataType.EndsWith("[]"))
                {
                    (string Type, List<string> Namespaces) result = GetPropertyTypeByDbDataType(databaseType, dataType, this.option.Language);

                    type = result.Type;

                    usingNamespaces.AddRange(result.Namespaces);

                    if (isNullable)
                    {
                        if (!(type == "string" || type == "object" || type == "byte[]" || DataTypeHelper.IsSpecialDataType(dataType)))
                        {
                            nullableFlag = "?";
                        }
                    }
                }
                else
                {
                    (string Type, List<string> Namespaces) result = GetPropertyTypeByDbDataType(databaseType, dataType.TrimEnd('[', ']'), this.option.Language);

                    type = result.Type + "[]";
                }

                sb.AppendLine(classIndent + "\tpublic " + type + $"{nullableFlag} " + column.Name + " {get; set;}");
            }

            sb.AppendLine(classIndent + "}");

            if (hasNamespace)
            {
                sb.AppendLine("}");
            }

            if (usingNamespaces.Count > 0)
            {
                sb.Insert(0, Environment.NewLine);
                sb.Insert(0, string.Join(Environment.NewLine, usingNamespaces.Distinct().Select(item => $"using {item};")));
            }

            return (fileName, sb.ToString());
        }

        private (string FileName, string FileContent) GenerateJavaCode(DatabaseObject tableOrView, IEnumerable<TableColumn> columns)
        {
            string fileName = $"{tableOrView.Name.Replace(" ", "_")}.java";

            List<string> packages = new List<string>();

            StringBuilder sb = new StringBuilder();          

            List<string> fields = new List<string>();
            List<string> methods = new List<string>();

            foreach (var column in columns)
            {
                string dataType = column.DataType;
                bool isNullable = column.IsNullable;

                string type = null;
                DatabaseType databaseType = this.dbInterpreter.DatabaseType;

                if (!dataType.EndsWith("[]"))
                {
                    (string Type, List<string> Namespaces) result = GetPropertyTypeByDbDataType(databaseType, dataType, this.option.Language);

                    type = result.Type;

                    packages.AddRange(result.Namespaces);

                    if (isNullable)
                    {
                        if (!(type == "String" || type == "Object" || type == "byte[]" || DataTypeHelper.IsSpecialDataType(dataType)))
                        {
                            if (type == "int")
                            {
                                type = "Integer";
                            }
                            else if (type == "short" || type == "long" || type == "double" || type == "float")
                            {
                                type = type.First().ToString().ToUpper() + type.Substring(1);
                            }
                            else if (type == "char")
                            {
                                type = "Character";
                            }

                            if (type != result.Type)
                            {
                                packages.Add("java.lang.*");
                            }
                        }
                    }
                }
                else
                {
                    (string Type, List<string> Namespaces) result = GetPropertyTypeByDbDataType(databaseType, dataType.TrimEnd('[', ']'), this.option.Language);

                    type = result.Type + "[]";
                }

                string fieldName = column.Name.First().ToString().ToLower() + column.Name.Substring(1);
                string propertyName = column.Name.First().ToString().ToUpper() + column.Name.Substring(1);

                fields.Add($"\tprivate {type} {fieldName};");

                StringBuilder sbMethod = new StringBuilder();

                sbMethod.AppendLine($"\tpublic {type} get{propertyName}" + " {");
                sbMethod.AppendLine($"\t\treturn this.{fieldName};");
                sbMethod.AppendLine("\t}");

                sbMethod.AppendLine();

                sbMethod.AppendLine($"\tpublic void set{propertyName}({type} {fieldName}) " + "{");
                sbMethod.AppendLine($"\t\tthis.{fieldName}={fieldName};");
                sbMethod.AppendLine("\t}");

                methods.Add(sbMethod.ToString());
            }

            if (packages.Count > 0)
            {
                sb.AppendLine(string.Join(Environment.NewLine, packages.Distinct().Select(item => $"import {item};")));
                sb.AppendLine();
            }

            string package = this.option.Namespace;

            bool hasPackage = !string.IsNullOrEmpty(package);

            if (hasPackage)
            {
                sb.AppendLine($"package {package};");
                sb.AppendLine();
            }

            sb.AppendLine($"public class {tableOrView.Name}" + " {");
            sb.AppendLine();

            foreach (var filed in fields)
            {
                sb.AppendLine(filed);
            }

            sb.AppendLine();

            for (int i = 0; i < methods.Count; i++)
            {
                string method = i<methods.Count-1 ? methods[i]: methods[i].TrimEnd();

                sb.AppendLine(method);               
            }

            sb.AppendLine("}");

            return (fileName, sb.ToString().TrimEnd());
        }

        public static (string Type, List<string> Namespaces) GetPropertyTypeByDbDataType(DatabaseType databaseType, string dataType, ProgrammingLanguage language)
        {
            dataType = dataType.ToLower();

            List<string> namespaces = new List<string>();

            Action<string> addNamespace = (ns) =>
            {
                if (!namespaces.Contains(ns))
                {
                    namespaces.Add(ns);
                }
            };

            string type = null;

            switch (language)
            {
                #region CSharp
                case ProgrammingLanguage.CSharp:

                    switch (databaseType)
                    {
                        #region SqlServer
                        case DatabaseType.SqlServer:
                            switch (dataType)
                            {
                                case "bigint":
                                    type = "long";
                                    break;
                                case "int":
                                    type = "int";
                                    break;
                                case "smallint":
                                    type = "short";
                                    break;
                                case "tinyint":
                                    type = "byte";
                                    break;
                                case "bit":
                                    type = "bool";
                                    break;
                                case "decimal":
                                case "numeric":
                                case "money":
                                case "smallmoney":
                                    type = "decimal";
                                    break;
                                case "float":
                                    type = "double";
                                    break;
                                case "real":
                                    type = "float";
                                    break;
                                case "date":
                                case "smalldatetime":
                                case "datetime":
                                case "datetime2":
                                    type = nameof(DateTime);
                                    break;
                                case "datetimeoffset":
                                    type = nameof(DateTimeOffset);
                                    break;
                                case "time":
                                    type = nameof(TimeSpan);
                                    break;
                                case "timestamp":
                                    type = "byte[]";
                                    break;
                                case "varchar":
                                case "nvarchar":
                                case "char":
                                case "nchar":
                                case "text":
                                case "ntext":
                                case "xml":
                                    type = "string";
                                    break;
                                case "binary":
                                case "varbinary":
                                case "image":
                                    type = "byte[]";
                                    break;
                                case "uniqueidentifier":
                                    type = nameof(Guid);
                                    break;
                                case "geography":
                                    type = nameof(SqlGeography);
                                    addNamespace(typeof(SqlGeography).Namespace);
                                    break;
                                case "geometry":
                                    type = nameof(SqlGeometry);
                                    addNamespace(typeof(SqlGeometry).Namespace);
                                    break;
                                case "hierarchyid":
                                    type = nameof(SqlHierarchyId);
                                    addNamespace(typeof(SqlHierarchyId).Namespace);
                                    break;
                                case "sql_variant":
                                    type = "object";
                                    break;
                                default:
                                    type = "string";
                                    break;
                            }
                            break;
                        #endregion
                        #region MySql
                        case DatabaseType.MySql:
                            switch (dataType)
                            {
                                case "bigint":
                                    type = "long";
                                    break;
                                case "int":
                                case "mediumint":
                                case "tinyint":
                                case "year":
                                    type = "int";
                                    break;
                                case "smallint":
                                    type = "short";
                                    break;
                                case "binary":
                                case "varbinary":
                                case "tinyblob":
                                case "longblob":
                                case "mediumblob":
                                    type = "byte[]";
                                    break;
                                case "bit":
                                    type = "sbyte";
                                    break;
                                case "bool":
                                case "boolean":
                                    type = "bool";
                                    break;
                                case "char":
                                    type = "string";
                                    break;
                                case "date":
                                case "datetime":
                                    type = nameof(DateTime);
                                    break;
                                case "time":
                                    type = nameof(TimeSpan);
                                    break;
                                case "timestamp":
                                    type = "byte[]";
                                    break;
                                case "decimal":
                                case "float":
                                case "numeric":
                                case "real":
                                    type = "decimal";
                                    break;
                                case "double":
                                    type = "double";
                                    break;
                                case "enum":
                                case "set":
                                case "geometry":
                                case "geomcollection":
                                case "linestring":
                                case "multilinestring":
                                case "point":
                                case "multipoint":
                                case "polygon":
                                case "multipolygon":
                                case "json":
                                case "text":
                                case "longtext":
                                case "tinytext":
                                case "mediumtext":
                                case "varchar":
                                    type = "string";
                                    break;
                                default:
                                    type = "string";
                                    break;
                            }
                            break;
                        #endregion
                        #region Oracle
                        case DatabaseType.Oracle:
                            switch (dataType)
                            {
                                case "bfile":
                                case "blob":
                                case "raw":
                                case "long raw":
                                case "timestamp":
                                    type = "byte[]";
                                    break;
                                case "binary_double":
                                case "float":
                                    type = "double";
                                    break;
                                case "binary_float":
                                case "real":
                                    type = "float";
                                    break;
                                case "date":
                                case "timestamp with local time zone":
                                case "timestamp with time zone":
                                    type = nameof(DateTime);
                                    break;
                                case "interval day to second":
                                    type = nameof(TimeSpan);
                                    break;
                                case "int":
                                case "integer":
                                case "smallint":
                                    type = "int";
                                    break;
                                case "interval year to month":
                                    type = "long";
                                    break;
                                case "decimal":
                                case "double precision":
                                case "number":
                                    type = "decimal";
                                    break;
                                case "char":
                                case "nchar":
                                case "varchar":
                                case "varchar2":
                                case "nvarchar2":
                                case "clob":
                                case "nclob":
                                case "long":
                                case "sdo_geometry":
                                case "st_geometry":
                                    type = "string";
                                    break;
                                default:
                                    type = "string";
                                    break;
                            }
                            break;
                        #endregion
                        #region Postgres
                        case DatabaseType.Postgres:
                            switch (dataType)
                            {
                                case "bigint":
                                case "bigserial":
                                    type = "long";
                                    break;
                                case "integer":
                                case "serial":
                                    type = "int";
                                    break;
                                case "smallint":
                                case "smallserial":
                                    type = "short";
                                    break;
                                case "real":
                                    type = "float";
                                    break;
                                case "double precision":
                                    type = "double";
                                    break;
                                case "money":
                                case "numeric":
                                    type = "decimal";
                                    break;
                                case "oid":
                                case "regconfig":
                                case "regtype":
                                case "xid":
                                    type = "uint";
                                    break;
                                case "xid8":
                                    type = "ulong";
                                    break;
                                case "oidvector":
                                    type = "uint[]";
                                    break;
                                case "uuid":
                                    type = nameof(Guid);
                                    break;
                                case "bytea":
                                    type = "byte[]";
                                    break;
                                case "boolean":
                                    type = "bool";
                                    break;
                                case """char""":
                                    type = "char";
                                    break;
                                case "date":
                                    type = nameof(DateOnly);
                                    break;
                                case "time without time zone":
                                    type = nameof(TimeOnly);
                                    break;
                                case "interval":
                                    type = nameof(TimeSpan);
                                    break;
                                case "time with time zone":
                                    type = nameof(DateTimeOffset);
                                    break;
                                case "timestamp with time zone":
                                case "timestamp without time zone":
                                    type = nameof(DateTime);
                                    break;
                                case "inet":
                                case "cidr":
                                    type = nameof(IPAddress);
                                    addNamespace(typeof(IPAddress).Namespace);
                                    break;
                                case "macaddr":
                                case "macaddr8":
                                    type = nameof(PhysicalAddress);
                                    addNamespace(typeof(PhysicalAddress).Namespace);
                                    break;
                                case "character":
                                case "character varying":
                                case "box":
                                case "circle":
                                case "cstring":
                                case "datemultirange":
                                case "daterange":
                                case "geography":
                                case "geometry":
                                case "gtsvector":
                                case "int2vector":
                                case "line":
                                case "point":
                                case "polygon":
                                case "path":
                                case "lseg":
                                case "pg_lsn":
                                case "json":
                                case "jsonb":
                                case "jsonpath":
                                case "name":
                                case "refcursor":
                                case "text":
                                case "tid":
                                case "xml":
                                    type = "string";
                                    break;
                                case "int4range":
                                case "int8range":
                                case "int4multirange":
                                    type = "IPAddress[]";
                                    addNamespace(typeof(IPAddress).Namespace);
                                    break;
                                case "bit":
                                case "bit varying":
                                case "numrange":
                                case "nummultirange":
                                case "pg_brin_bloom_summary":
                                case "pg_dependencies":
                                case "pg_mcv_list":
                                case "pg_ndistinct":
                                case "pg_node_tree":
                                case "pg_snapshot":
                                case "regclass":
                                case "regcollation":
                                case "regdictionary":
                                case "regnamespace":
                                case "regoper":
                                case "regoperator":
                                case "regproc":
                                case "regprocedure":
                                case "regrole":
                                case "tsquery":
                                case "tsrange":
                                case "tstzrange":
                                case "tsvector":
                                case "txid_snapshot":
                                    type = "object";
                                    break;
                                case "pg_brin_minmax_multi_summary":
                                case "tsmultirange[]":
                                case "tstzmultirange[]":
                                    type = "object[]";
                                    break;
                                default:
                                    type = "string";
                                    break;
                            }
                            break;
                        #endregion
                        #region Sqlite
                        case DatabaseType.Sqlite:
                            switch (dataType)
                            {
                                case "blob":
                                    type = "byte[]";
                                    break;
                                case "integer":
                                    type = "int";
                                    break;
                                case "numeric":
                                    type = "decimal";
                                    break;
                                case "real":
                                    type = "double";
                                    break;
                                case "text":
                                    type = "string";
                                    break;
                                default:
                                    type = "string";
                                    break;
                            }
                            break;
                            #endregion
                    }
                    break;
                #endregion
                #region Java
                case ProgrammingLanguage.Java:

                    switch (databaseType)
                    {
                        #region SqlServer
                        case DatabaseType.SqlServer:
                            switch (dataType)
                            {
                                case "bigint":
                                    type = "long";
                                    break;
                                case "int":
                                    type = "int";
                                    break;
                                case "smallint":
                                    type = "short";
                                    break;
                                case "tinyint":
                                    type = "byte";
                                    break;
                                case "bit":
                                    type = "boolean";
                                    break;
                                case "decimal":
                                case "numeric":
                                case "money":
                                case "smallmoney":
                                    type = "BigDecimal";
                                    addNamespace("java.math.BigDecimal");
                                    break;
                                case "float":
                                    type = "double";
                                    break;
                                case "real":
                                    type = "float";
                                    break;
                                case "date":
                                case "smalldatetime":
                                case "datetime":
                                case "datetime2":
                                    type = "LocalDateTime";
                                    addNamespace("java.time.LocalDateTime");
                                    break;
                                case "datetimeoffset":
                                    type = "OffsetDateTime";
                                    addNamespace("java.time.OffsetDateTime");
                                    break;
                                case "time":
                                    type = "Timestamp";
                                    addNamespace("java.sql.Timestamp");
                                    break;
                                case "timestamp":
                                    type = "byte[]";
                                    break;
                                case "varchar":
                                case "nvarchar":
                                case "char":
                                case "nchar":
                                case "text":
                                case "ntext":
                                case "xml":
                                case "uniqueidentifier":
                                case "geography":
                                case "geometry":
                                case "hierarchyid":
                                    type = "String";
                                    break;
                                case "binary":
                                case "varbinary":
                                case "image":
                                    type = "byte[]";
                                    break;
                                case "sql_variant":
                                    type = "Object";
                                    break;
                                default:
                                    type = "String";
                                    break;
                            }
                            break;
                        #endregion
                        #region MySql
                        case DatabaseType.MySql:
                            switch (dataType)
                            {
                                case "bigint":
                                    type = "long";
                                    break;
                                case "int":
                                case "mediumint":
                                case "tinyint":
                                case "year":
                                    type = "int";
                                    break;
                                case "smallint":
                                    type = "short";
                                    break;
                                case "binary":
                                case "varbinary":
                                case "tinyblob":
                                case "longblob":
                                case "mediumblob":
                                    type = "byte[]";
                                    break;
                                case "bit":
                                    type = "byte";
                                    break;
                                case "bool":
                                case "boolean":
                                    type = "boolean";
                                    break;
                                case "char":
                                    type = "String";
                                    break;
                                case "date":
                                case "datetime":
                                    type = "LocalDateTime";
                                    addNamespace("java.time.LocalDateTime");
                                    break;
                                case "time":
                                    type = "Timestamp";
                                    addNamespace("java.sql.Timestamp");
                                    break;
                                case "timestamp":
                                    type = "byte[]";
                                    break;
                                case "decimal":
                                case "float":
                                case "numeric":
                                case "real":
                                    type = "BigDecimal";
                                    addNamespace("java.math.BigDecimal");
                                    break;
                                case "double":
                                    type = "double";
                                    break;
                                case "enum":
                                case "set":
                                case "geometry":
                                case "geomcollection":
                                case "linestring":
                                case "multilinestring":
                                case "point":
                                case "multipoint":
                                case "polygon":
                                case "multipolygon":
                                case "json":
                                case "text":
                                case "longtext":
                                case "tinytext":
                                case "mediumtext":
                                case "varchar":
                                    type = "String";
                                    break;
                                default:
                                    type = "String";
                                    break;
                            }
                            break;
                        #endregion
                        #region Oracle
                        case DatabaseType.Oracle:
                            switch (dataType)
                            {
                                case "bfile":
                                case "blob":
                                case "raw":
                                case "long raw":
                                case "timestamp":
                                    type = "byte[]";
                                    break;
                                case "binary_double":
                                case "float":
                                    type = "double";
                                    break;
                                case "binary_float":
                                case "real":
                                    type = "float";
                                    break;
                                case "date":
                                case "timestamp with local time zone":
                                case "timestamp with time zone":
                                    type = "ZonedDateTime";
                                    addNamespace("java.time.ZonedDateTime");
                                    break;
                                case "interval day to second":
                                    type = "Timestamp";
                                    addNamespace("java.sql.Timestamp");
                                    break;
                                case "int":
                                case "integer":
                                case "smallint":
                                    type = "int";
                                    break;
                                case "interval year to month":
                                    type = "long";
                                    break;
                                case "decimal":
                                case "double precision":
                                case "number":
                                    type = "BigDecimal";
                                    addNamespace("java.math.BigDecimal");
                                    break;
                                case "char":
                                case "nchar":
                                case "varchar":
                                case "varchar2":
                                case "nvarchar2":
                                case "clob":
                                case "nclob":
                                case "long":
                                case "sdo_geometry":
                                case "st_geometry":
                                    type = "String";
                                    break;
                                default:
                                    type = "String";
                                    break;
                            }
                            break;
                        #endregion
                        #region Postgres
                        case DatabaseType.Postgres:
                            switch (dataType)
                            {
                                case "bigint":
                                case "bigserial":
                                    type = "long";
                                    break;
                                case "integer":
                                case "serial":
                                    type = "int";
                                    break;
                                case "smallint":
                                case "smallserial":
                                    type = "short";
                                    break;
                                case "real":
                                    type = "float";
                                    break;
                                case "double precision":
                                    type = "double";
                                    break;
                                case "money":
                                case "numeric":
                                    type = "BigDecimal";
                                    addNamespace("java.math.BigDecimal");
                                    break;
                                case "oid":
                                case "regconfig":
                                case "regtype":
                                case "xid":
                                    type = "int";
                                    break;
                                case "xid8":
                                    type = "long";
                                    break;
                                case "oidvector":
                                    type = "int[]";
                                    break;
                                case "bytea":
                                    type = "byte[]";
                                    break;
                                case "boolean":
                                    type = "boolean";
                                    break;
                                case """char""":
                                    type = "char";
                                    break;
                                case "date":
                                    type = "LocalDate";
                                    addNamespace("java.time.LocalDate");
                                    break;
                                case "time without time zone":
                                    type = "LocalTime";
                                    addNamespace("java.time.LocalTime");
                                    break;
                                case "interval":
                                    type = "Timestamp";
                                    addNamespace("java.sql.Timestamp");
                                    break;
                                case "time with time zone":
                                    type = "OffsetDateTime";
                                    addNamespace("java.time.OffsetDateTime");
                                    break;
                                case "timestamp with time zone":
                                    type = "ZonedDateTime";
                                    addNamespace("java.time.ZonedDateTime");
                                    break;
                                case "timestamp without time zone":
                                    type = "LocalDateTime";
                                    addNamespace("java.time.LocalDateTime");
                                    break;
                                case "inet":
                                case "cidr":
                                    type = "InetAddress";
                                    addNamespace("java.net.InetAddress");
                                    break;
                                case "macaddr":
                                case "macaddr8":
                                    type = "byte[]";
                                    break;
                                case "character":
                                case "character varying":
                                case "box":
                                case "circle":
                                case "cstring":
                                case "datemultirange":
                                case "daterange":
                                case "geography":
                                case "geometry":
                                case "gtsvector":
                                case "int2vector":
                                case "line":
                                case "point":
                                case "polygon":
                                case "path":
                                case "lseg":
                                case "pg_lsn":
                                case "json":
                                case "jsonb":
                                case "jsonpath":
                                case "name":
                                case "refcursor":
                                case "text":
                                case "tid":
                                case "xml":
                                case "uuid":
                                    type = "String";
                                    break;
                                case "int4range":
                                case "int8range":
                                case "int4multirange":
                                    type = "InetAddress[]";
                                    addNamespace("java.net.InetAddress");
                                    break;
                                case "bit":
                                case "bit varying":
                                case "numrange":
                                case "nummultirange":
                                case "pg_brin_bloom_summary":
                                case "pg_dependencies":
                                case "pg_mcv_list":
                                case "pg_ndistinct":
                                case "pg_node_tree":
                                case "pg_snapshot":
                                case "regclass":
                                case "regcollation":
                                case "regdictionary":
                                case "regnamespace":
                                case "regoper":
                                case "regoperator":
                                case "regproc":
                                case "regprocedure":
                                case "regrole":
                                case "tsquery":
                                case "tsrange":
                                case "tstzrange":
                                case "tsvector":
                                case "txid_snapshot":
                                    type = "Object";
                                    break;
                                case "pg_brin_minmax_multi_summary":
                                case "tsmultirange[]":
                                case "tstzmultirange[]":
                                    type = "Object[]";
                                    break;
                                default:
                                    type = "String";
                                    break;
                            }
                            break;
                        #endregion
                        #region Sqlite
                        case DatabaseType.Sqlite:
                            switch (dataType)
                            {
                                case "blob":
                                    type = "byte[]";
                                    break;
                                case "integer":
                                    type = "int";
                                    break;
                                case "numeric":
                                    type = "BigDecimal";
                                    addNamespace("java.math.BigDecimal");
                                    break;
                                case "real":
                                    type = "double";
                                    break;
                                case "text":
                                    type = "String";
                                    break;
                                default:
                                    type = "String";
                                    break;
                            }
                            break;
                            #endregion
                    }
                    break;
                    #endregion
            }

            return (type, namespaces);
        }

        public void Feedback(string info)
        {
            this.Feedback(this, info);
        }

        public void Feedback(object owner, string content, FeedbackInfoType infoType = FeedbackInfoType.Info, bool enableLog = true, bool suppressError = false)
        {
            FeedbackInfo info = new FeedbackInfo() { InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(content), Owner = owner };

            FeedbackHelper.Feedback(suppressError ? null : this.observer, info, enableLog);
        }
    }
}