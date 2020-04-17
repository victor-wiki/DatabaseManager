namespace DatabaseInterpreter.Model
{
    public class TableTrigger : ScriptDbObject
    {
        public string TableName { get; set; }    
        
        /// <summary>
        /// Use it when Definition has no create clause.
        /// </summary>
        public string CreateClause { get; set; }
    }
}
