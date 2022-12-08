using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class ConstraintInfo
    {
        private ForeignKeyInfo _fk;
        private NameToken _name;
        public ConstraintType Type { get; set; } = ConstraintType.None;
        public NameToken Name
        {
            get { return this._name; }
            set
            {
                this._name = value;

                if (value != null)
                {
                    this._name.Type = TokenType.ConstraintName;
                }
            }
        }
        public List<ColumnName> ColumnNames { get; set; }
        public ForeignKeyInfo ForeignKey
        {
            get { return this._fk; }
            set
            {
                if (value != null)
                {
                    this.Type = ConstraintType.ForeignKey;
                }

                this._fk = value;
            }
        }
        public TokenInfo Definition { get; set; }
    }
    public enum ConstraintType
    {
        None,
        PrimaryKey,
        ForeignKey,
        UniqueIndex,
        Check,
        Default
    }
}
