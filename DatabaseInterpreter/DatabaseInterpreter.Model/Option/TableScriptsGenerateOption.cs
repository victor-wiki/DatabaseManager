namespace DatabaseInterpreter.Model
{
    public class TableScriptsGenerateOption
    {
        public bool GenerateIdentity { get; set; } = true;
        public bool GeneratePrimaryKey { get; set; } = true;
        public bool GenerateForeignKey { get; set; } = true;
        public bool GenerateIndex { get; set; } = true;
        public bool GenerateDefaultValue { get; set; } = true;
        public bool GenerateComment { get; set; } = true;
        public bool GenerateTrigger { get; set; } = true;
        public bool GenerateConstraint { get; set; } = true;
    }
}
