using DatabaseInterpreter.Model;
using System.Collections.Generic;

namespace DatabaseManager.Core.Model
{
    public class CodeGenerateOption
    {
        public string OutputFolder { get; set; }
        public ProgrammingLanguage Language { get; set; } = ProgrammingLanguage.None;
        public string Namespace { get; set; }
        public List<Table> Tables { get; set; }
        public List<View> Views { get; set; }
    }
}
