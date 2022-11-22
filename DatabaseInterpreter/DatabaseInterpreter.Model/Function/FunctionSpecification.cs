namespace DatabaseInterpreter.Model
{
    public class FunctionSpecification
    {
        private string _delimiter;
        public string Name { get; set; }
        public string Args { get; set; }
        public bool NoParenthesess { get; set; }
        public bool IsString { get; set; }

        public string Delimiter
        {
            get
            {
                if (string.IsNullOrEmpty(_delimiter))
                {
                    return ",";
                }

                return _delimiter;
            }
            set
            {
                _delimiter = value;
            }
        }
    }
}
