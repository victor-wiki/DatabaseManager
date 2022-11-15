using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;

namespace DatabaseConverter.Core
{
    public class SequenceTranslator : DbObjectTranslator
    {
        private IEnumerable<Sequence> sequences;
        public const string SqlServerSequenceNextValueFlag = "NEXT VALUE FOR";
        public const string PostgreSeqenceNextValueFlag = "NEXTVAL";
        public const string OracleSequenceNextValueFlag = "NEXTVAL";

        public SequenceTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter) : base(sourceInterpreter, targetInterpreter)
        {

        }

        public SequenceTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, IEnumerable<Sequence> sequences) : base(sourceInterpreter, targetInterpreter)
        {
            this.sequences = sequences;
        }

        public override void Translate()
        {
            if (this.sourceDbType == this.targetDbType)
            {
                return;
            }

            this.FeedbackInfo("Begin to translate sequences.");

            foreach (Sequence sequence in this.sequences)
            {
                this.ConvertDataType(sequence);

                if (sequence.StartValue < sequence.MinValue)
                {
                    sequence.StartValue = (int)sequence.MinValue;
                }
            }

            this.FeedbackInfo("End translate sequences.");
        }

        public void ConvertDataType(Sequence sequence)
        {
            if (this.targetDbType == DatabaseType.SqlServer)
            {
                sequence.DataType = "bigint";
            }
        }

        public static bool IsSequenceValueFlag(DatabaseType databaseType, string value)
        {
            string upperValue = value.ToUpper();

            if (databaseType == DatabaseType.SqlServer)
            {
                return upperValue.Contains(SqlServerSequenceNextValueFlag);
            }
            else if (databaseType == DatabaseType.Postgres)
            {
                return upperValue.Contains(PostgreSeqenceNextValueFlag);
            }
            else if (databaseType == DatabaseType.Oracle)
            {
                return upperValue.Contains(OracleSequenceNextValueFlag);
            }

            return false;
        }

        public string HandleSequenceValue(string value)
        {
            string nextValueFlag = "";
            string sequencePart = "";

            if (this.sourceDbType == DatabaseType.SqlServer)
            {
                nextValueFlag = SequenceTranslator.SqlServerSequenceNextValueFlag;
            }
            else if (this.sourceDbType == DatabaseType.Postgres)
            {
                nextValueFlag = SequenceTranslator.PostgreSeqenceNextValueFlag;
                value = value.Replace("::regclass", "");
            }
            else if (this.sourceDbType == DatabaseType.Oracle)
            {
                nextValueFlag = SequenceTranslator.OracleSequenceNextValueFlag;
            }

            sequencePart = StringHelper.GetBalanceParenthesisTrimedValue(value.Replace(nextValueFlag, "", StringComparison.OrdinalIgnoreCase).Trim()).Trim('\'');

            string schema = null, seqenceName = null;

            if (sequencePart.Contains("."))
            {
                var items = sequencePart.Split('.');
                schema = this.GetTrimedName(items[0]);
                seqenceName = this.GetTrimedName(items[1]);
            }
            else
            {
                seqenceName = this.GetTrimedName(sequencePart);
            }           

            string mappedSchema = this.GetMappedSchema(schema);

            return ConvertSequenceValue(this.targetDbInterpreter, mappedSchema, seqenceName);
        }

        public static string ConvertSequenceValue(DbInterpreter targetDbInterpreter, string schema, string sequenceName)
        {
            DatabaseType targetDbType = targetDbInterpreter.DatabaseType;

            if (targetDbType == DatabaseType.SqlServer)
            {
                return $"{SqlServerSequenceNextValueFlag} {targetDbInterpreter.GetQuotedDbObjectNameWithSchema(schema, sequenceName)}";
            }
            else if (targetDbType == DatabaseType.Postgres)
            {
                return $"{PostgreSeqenceNextValueFlag}('{targetDbInterpreter.GetQuotedDbObjectNameWithSchema(schema, sequenceName)}')";
            }
            else if (targetDbType == DatabaseType.Oracle)
            {
                return $"{targetDbInterpreter.GetQuotedDbObjectNameWithSchema(schema, sequenceName)}.{OracleSequenceNextValueFlag}";
            }

            return targetDbInterpreter.GetQuotedDbObjectNameWithSchema(schema, sequenceName);
        }

        private string GetMappedSchema(string schema)
        {
            string mappedSchema = SchemaInfoHelper.GetMappedSchema(this.GetTrimedName(schema), this.Option.SchemaMappings);

            if (mappedSchema == null)
            {
                mappedSchema = this.targetDbInterpreter.DefaultSchema;
            }

            return mappedSchema;
        }
    }
}
