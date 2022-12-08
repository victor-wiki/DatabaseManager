using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class ColumnInfo
    {
        private TokenInfo _dataType;
        public ColumnName Name { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsNullable { get; set; } = true;
        public TokenInfo DataType
        {
            get { return this._dataType; }
            set
            {
                this._dataType = value;

                if (value != null)
                {
                    this._dataType.Type = TokenType.DataType;
                }
            }
        }
        public TokenInfo DefaultValue { get; set; }
        public TokenInfo ComputeExp { get; set; }

        public bool IsComputed => this.ComputeExp != null;
        public List<ConstraintInfo> Constraints { get; set; }
    }
}
