namespace DatabaseInterpreter.Model
{
    public class Function : ScriptDbObject
    {              
        public string DataType { get; set; }
        public bool IsTriggerFunction { get; set; }
    }
}
