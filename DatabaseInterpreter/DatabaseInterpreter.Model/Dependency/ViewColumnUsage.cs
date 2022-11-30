namespace DatabaseInterpreter.Model
{
    public class ViewColumnUsage: DbObjectUsage
    {
        public override string ObjectType { get; set; } = "View";
        public override string RefObjectType { get; set; } = "Column";
        public string ColumnName { get; set; }
    }
}
