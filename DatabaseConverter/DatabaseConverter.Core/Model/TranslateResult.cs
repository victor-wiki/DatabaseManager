using DatabaseInterpreter.Model;

namespace DatabaseConverter.Model
{
    public class TranslateResult
    {
        public DatabaseObjectType DbObjectType { get; set; } = DatabaseObjectType.None;
        public string DbObjectSchema { get; set; }
        public string DbObjectName { get; set; }
        public object Error { get; set; }
        public object Data { get; set; }

        public bool HasError => this.Error != null;
    }
}
