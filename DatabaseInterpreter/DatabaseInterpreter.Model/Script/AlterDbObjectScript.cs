namespace DatabaseInterpreter.Model
{
    public class AlterDbObjectScript<T>: DatabaseObjectScript<T>
        where T : DatabaseObject
    {
        public AlterDbObjectScript(string script) : base(script) { }       
    }
}
