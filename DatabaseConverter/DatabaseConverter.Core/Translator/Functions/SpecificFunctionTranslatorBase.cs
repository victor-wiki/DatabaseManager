using DatabaseConverter.Model;
using DatabaseInterpreter.Model;

namespace DatabaseConverter.Core.Functions
{
    public abstract class SpecificFunctionTranslatorBase
    {
        protected FunctionSpecification SourceSpecification;
        protected FunctionSpecification TargetSpecification;
        public DatabaseType SourceDbType { get; set; }
        public DatabaseType TargetDbType { get; set; }
        public SpecificFunctionTranslatorBase(FunctionSpecification sourceSpecification, FunctionSpecification targetSpecification) 
        {
            this.SourceSpecification = sourceSpecification;
            this.TargetSpecification = targetSpecification;
        }

        public abstract string Translate(FunctionFormula formula);        
    }
}
