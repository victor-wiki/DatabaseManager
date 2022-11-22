using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class TableInfo
    {
        public bool IsTemporary { get; set; }
        public bool IsGlobal { get; set; } = true;
        public TokenInfo Name { get; set; }
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
    }

    public class ColumnInfo
    {
        private TokenInfo _dataType;
        public ColumnName Name { get; set; }
        public bool IsIdentity { get; set; }
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
    }
}
