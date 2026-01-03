namespace DatabaseManager.Core.Model
{
    public enum TableColumnProperty
    {
        None = 0,
        Name = 1,
        DataType = 2,
        IsNullable = 4,
        IsPrimary = 5,
        IsIdentity = 6,
        DefaultValue = 7,
        Comment = 8
    }
}
