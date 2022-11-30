namespace DatabaseInterpreter.Model
{
    public class ViewTableUsage: DbObjectUsage
    {
        public override string ObjectType { get; set; } = "View";
        public override string RefObjectType { get; set; } = "Table";
    }
}
