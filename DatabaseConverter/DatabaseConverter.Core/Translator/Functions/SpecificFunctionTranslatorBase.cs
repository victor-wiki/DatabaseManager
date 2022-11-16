using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Text;

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
