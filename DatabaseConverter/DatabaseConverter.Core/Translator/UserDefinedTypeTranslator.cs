using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System.Collections.Generic;

namespace DatabaseConverter.Core
{
    public class UserDefinedTypeTranslator : DbObjectTranslator
    {
        private IEnumerable<UserDefinedType> userDefinedTypes;
        private DataTypeTranslator dataTypeTranslator;

        public UserDefinedTypeTranslator(DbInterpreter sourceInterpreter, DbInterpreter targetInterpreter, IEnumerable<UserDefinedType> userDefinedTypes) : base(sourceInterpreter, targetInterpreter)
        {
            this.userDefinedTypes = userDefinedTypes;
            this.dataTypeTranslator = new DataTypeTranslator(this.sourceDbInterpreter, this.targetDbInterpreter);
        }

        public override void Translate()
        {
            if (this.sourceDbType == this.targetDbType)
            {
                return;
            }

            this.FeedbackInfo("Begin to translate user defined types.");

            foreach (UserDefinedType udt in this.userDefinedTypes)
            {
                foreach(UserDefinedTypeAttribute attr in udt.Attributes)
                {
                    DataTypeInfo dataTypeInfo = new DataTypeInfo()
                    {
                        DataType = attr.DataType,
                        MaxLength = attr.MaxLength,
                        Precision = attr.Precision,
                        Scale = attr.Scale
                    };

                    this.dataTypeTranslator.Translate(dataTypeInfo);

                    attr.DataType = dataTypeInfo.DataType;
                    attr.MaxLength = dataTypeInfo.MaxLength;
                    attr.Precision = dataTypeInfo.Precision;
                    attr.Scale = dataTypeInfo.Scale;
                }              
            }

            this.FeedbackInfo("End translate user defined types.");
        }       
    }
}
