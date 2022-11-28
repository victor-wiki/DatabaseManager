namespace DatabaseConverter.Model
{
    public class TokenSymbolNameInfo
    {
        public string Schema { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public TokenSymbolNameType Type { get; set; } = TokenSymbolNameType.Unknown;
    }

    public enum TokenSymbolNameType
    {
        Unknown,
        SchemaTableColumn,
        SchemaTable
    }
}
