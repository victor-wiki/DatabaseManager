using DatabaseConverter.Core.Model.Functions;
using DatabaseConverter.Model;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;

namespace DatabaseConverter.Core.Functions
{
    public class DateAddTranslator : SpecificFunctionTranslatorBase
    {

        public DateAddTranslator(FunctionSpecification sourceSpecification, FunctionSpecification targetSpecification) : base(sourceSpecification, targetSpecification) { }


        public override string Translate(FunctionFormula formula)
        {
            string expression = formula.Expression;
            string delimiter = this.SourceSpecification.Delimiter ?? ",";
            var args = formula.GetArgs(delimiter);           

            DateAdd? dateAdd = default(DateAdd?);

            string newExpression = expression;

            if (this.SourceDbType == DatabaseType.SqlServer)
            {
                dateAdd = new DateAdd() { Unit = args[0], Date = args[2], IntervalNumber = args[1] };
            }
            else if (this.SourceDbType == DatabaseType.MySql)
            {
                string[] items = args[1].Split(' ');

                dateAdd = new DateAdd() { Unit = items[2].Trim(), Date = args[0], IntervalNumber = items[1].Trim() };
            }

            if (dateAdd.HasValue)
            {
                string unit = DatetimeHelper.GetMappedUnit(this.SourceDbType, this.TargetDbType, dateAdd.Value.Unit);

                bool isStringValue = ValueHelper.IsStringValue(dateAdd.Value.Date);
                string date = dateAdd.Value.Date;
                string intervalNumber = dateAdd.Value.IntervalNumber;
                bool isTimestampStr = isStringValue && date.Contains(" ");

                if (this.TargetDbType == DatabaseType.SqlServer)
                {
                    newExpression = $"DATEADD({unit}, {intervalNumber},{date})";
                }
                else if (this.TargetDbType == DatabaseType.MySql)
                {
                    newExpression = $"DATE_ADD({date},INTERVAL {intervalNumber} {unit})";
                }
                else if (this.TargetDbType == DatabaseType.Postgres)
                {
                    string dataType = isStringValue? (isTimestampStr ? "::TIMESTAMP" : "::DATE"):"";

                    string strDate = $"{date}{dataType}"; ;

                    newExpression = $"{strDate}+ INTERVAL '{intervalNumber} {unit}'";
                }
                else if (this.TargetDbType == DatabaseType.Oracle)
                {
                    bool isDateStr = isStringValue && !date.Contains(" ");

                    if (isStringValue)
                    {
                        date = DatetimeHelper.GetOracleUniformDatetimeString(date, isTimestampStr);                       
                    }

                    string dataType = isStringValue ? (isTimestampStr ? "TIMESTAMP" : "DATE") : "";

                    string strDate = $"{dataType}{date}";

                    newExpression = $"{strDate} + INTERVAL '{intervalNumber}' {unit}";                   
                }
                else if(this.TargetDbType == DatabaseType.Sqlite)
                {
                    if(unit == "WEEK")
                    {
                        intervalNumber = intervalNumber.StartsWith("-")?  "-7":"7";
                        unit = "DAY";
                    }

                    string function = isStringValue ? (isTimestampStr ? "DATETIME" : "DATE") : "";                    

                    newExpression = $"{function}({date}, '{intervalNumber} {unit}')";
                }
            }

            return newExpression;
        }        
    }
}
