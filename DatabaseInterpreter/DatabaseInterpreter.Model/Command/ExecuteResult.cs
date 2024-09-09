namespace DatabaseInterpreter.Model
{
    public class ExecuteResult
    {
        public int NumberOfRowsAffected { get; set; }
        public bool HasError { get; set; }
        public string Message { get; set; }
        public bool TransactionRollbacked { get; set; }
    }
}
