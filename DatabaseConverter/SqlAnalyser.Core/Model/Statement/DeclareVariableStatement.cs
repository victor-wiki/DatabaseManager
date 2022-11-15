namespace SqlAnalyser.Model
{
    public class DeclareVariableStatement : Statement
    {
        private TokenInfo _dataType;
        public TokenInfo Name { get; set; }
        public TokenInfo DefaultValue { get; set; }

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

        /// <summary>
        /// Whether data type is variable%TYPE
        /// </summary>
        public bool IsCopyingDataType { get; set; }
    }
}
