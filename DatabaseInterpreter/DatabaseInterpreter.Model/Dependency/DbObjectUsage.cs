namespace DatabaseInterpreter.Model
{
    public class DbObjectUsage
    {
        public string ObjectCatalog { get; set; }
        public string ObjectSchema { get; set; }
        public string ObjectName { get; set; }
        public string RefObjectCatalog { get; set; }
        public string RefObjectSchema { get; set; }
        public string RefObjectName { get; set; }       
    }
}
