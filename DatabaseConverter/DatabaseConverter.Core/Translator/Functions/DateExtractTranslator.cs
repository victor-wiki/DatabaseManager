using DatabaseConverter.Core.Model.Functions;
using DatabaseConverter.Model;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;

namespace DatabaseConverter.Core.Functions
{
    public class DateExtractTranslator : SpecificFunctionTranslatorBase
    {

        public DateExtractTranslator(FunctionSpecification sourceSpecification, FunctionSpecification targetSpecification) : base(sourceSpecification, targetSpecification) { }


        public override string Translate(FunctionFormula formula)
        {
            string expression = formula.Expression;
            string delimiter = this.SourceSpecification.Delimiter ?? ",";
            var args = formula.GetArgs(delimiter);

            DateExtract? dateExtract = default(DateExtract?);

            string newExpression = expression;

            if (this.SourceDbType == DatabaseType.SqlServer)
            {
                dateExtract = new DateExtract() { Unit = args[0], Date = args[1]};
            }
            else if (this.SourceDbType == DatabaseType.MySql)
            {
                string[] items = args[1].Split(' ');

                dateExtract = new DateExtract() { Unit = items[0].Trim(), Date = args[0] };
            }

            if (dateExtract.HasValue)
            {
                string date = dateExtract.Value.Date;
                string format = DatetimeHelper.GetSqliteStrfTimeFormat(this.SourceDbType, dateExtract.Value.Unit);

                if (this.TargetDbType == DatabaseType.Sqlite)
                {
                    newExpression = $"STRFTIME('%{format}',{date})";
                }
            }

            return newExpression;
        }        
    }
}
