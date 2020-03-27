namespace DatabaseInterpreter.Model
{
    public class CreateDbObjectScript<T> : DatabaseObjectScript<T>
        where T: DatabaseObject
    {
        public CreateDbObjectScript(string script) : base(script) { }        
    }
}
