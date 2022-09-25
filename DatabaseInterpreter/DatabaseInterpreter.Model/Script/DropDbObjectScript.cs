namespace DatabaseInterpreter.Model
{
    public class DropDbObjectScript<T>: DatabaseObjectScript<T>
        where T : DatabaseObject
    {
        public DropDbObjectScript(string script) : base(script) { }       
    }
}
