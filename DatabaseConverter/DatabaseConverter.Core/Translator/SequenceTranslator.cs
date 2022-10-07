using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using NCalc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class SequenceTranslator : DbObjectTranslator
    {
        private IEnumerable<Sequence> sequences;

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

                if(sequence.StartValue < sequence.MinValue)
                {
                    sequence.StartValue = (int)sequence.MinValue;
                }
            }

            this.FeedbackInfo("End translate sequences.");
        }
        public void ConvertDataType(Sequence sequence)
        {
            if(this.targetDbType == DatabaseType.SqlServer)
            {
                sequence.DataType = "bigint";
            }
        }
    }
}
