using DatabaseConverter.Core.Model.Functions;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseConverter.Core.Functions
{
    public class DateDiffTranslator : SpecificFunctionTranslatorBase
    {

        public DateDiffTranslator(FunctionSpecification sourceSpecification, FunctionSpecification targetSpecification) : base(sourceSpecification, targetSpecification) { }


        public override string Translate(FunctionFormula formula)
        {
            string functionName = formula.Name;
            string targetFunctionName = this.TargetSpecification?.Name;
            string expression = formula.Expression;
            string delimiter = this.SourceSpecification.Delimiter ?? ",";
            var args = formula.GetArgs(delimiter);

            bool argsReversed = false;

            DateDiff? dateDiff = default(DateDiff?);

            string newExpression = expression;

            if (this.SourceDbType == DatabaseType.SqlServer)
            {
                dateDiff = new DateDiff() { Unit = args[0], Date1 = args[2], Date2 = args[1] };

                argsReversed = true;
            }
            else if (this.SourceDbType == DatabaseType.MySql)
            {
                if (functionName == "DATEDIFF")
                {
                    dateDiff = new DateDiff() { Unit = "DAY", Date1 = args[0], Date2 = args[1] };
                }
                else if (functionName == "TIMESTAMPDIFF")
                {
                    dateDiff = new DateDiff() { Unit = args[0], Date1 = args[2], Date2 = args[1] };

                    argsReversed = true;
                }
            }

            if (dateDiff.HasValue)
            {
                string unit = dateDiff.Value.Unit.ToUpper();

                bool isStringValue = ValueHelper.IsStringValue(dateDiff.Value.Date1);
                string date1 = dateDiff.Value.Date1;
                string date2 = dateDiff.Value.Date2;

                Action reverseDateArguments = () =>
                {
                    string temp;
                    temp = date1;
                    date1 = date2;
                    date2 = temp;
                };

                if (this.TargetDbType == DatabaseType.SqlServer)
                {
                    if (argsReversed)
                    {
                        reverseDateArguments();
                    }

                    newExpression = $"DATEDIFF({unit}, {date1},{date2})";
                }
                else if (this.TargetDbType == DatabaseType.MySql)
                {
                    if (targetFunctionName == "TIMESTAMPDIFF")
                    {
                        if (argsReversed)
                        {
                            reverseDateArguments();
                        }

                        newExpression = $"TIMESTAMPDIFF({unit}, {date1},{date2})";
                    }
                    else
                    {
                        newExpression = $"DATEDIFF({date1}, {date2})";
                    }
                }
                else if (this.TargetDbType == DatabaseType.Postgres)
                {
                    string dataType = "::TIMESTAMP";

                    string strDate1 = $"{date1}{dataType}"; ;
                    string strDate2 = $"{date2}{dataType}";
                    string strDate1MinusData2 = $"{strDate1}-{strDate2}";

                    switch (unit)
                    {
                        case "YEAR":
                            newExpression = $"DATE_PART('YEAR', {strDate1}) - DATE_PART('YEAR', {strDate2})";
                            break;
                        case "MONTH":
                            newExpression = $"(DATE_PART('YEAR', {strDate1}) - DATE_PART('YEAR', {strDate2})) * 12 +(DATE_PART('MONTH', {strDate1}) - DATE_PART('MONTH', {strDate2}))";
                            break;
                        case "WEEK":
                            newExpression = $"TRUNC(DATE_PART('DAY', {strDate1MinusData2})/7)";
                            break;
                        case "DAY":
                            newExpression = $"DATE_PART('{unit}',{strDate1MinusData2})";
                            break;
                        case "HOUR":
                            newExpression = $"DATE_PART('DAY', {strDate1MinusData2}) * 24 +(DATE_PART('HOUR', {strDate1MinusData2}))";
                            break;
                        case "MINUTE":
                            newExpression = $"(DATE_PART('DAY', {strDate1MinusData2}) * 24 + DATE_PART('HOUR', {strDate1MinusData2})) * 60 + DATE_PART('MINUTE', {strDate1MinusData2})";
                            break;
                        case "SECOND":
                            newExpression = $"((DATE_PART('DAY', {strDate1MinusData2}) * 24 +  DATE_PART('HOUR', {strDate1MinusData2})) * 60 + DATE_PART('MINUTE', {strDate1MinusData2})) * 60 +  DATE_PART('SECOND', {strDate1MinusData2})";
                            break;
                    }
                }
                else if (this.TargetDbType == DatabaseType.Oracle)
                {
                    bool isTimestampStr = isStringValue && date1.Contains(" ");
                    bool isDateStr = isStringValue && !date1.Contains(" ");

                    if (isStringValue)
                    {
                        date1 = DatetimeHelper.GetOracleUniformDatetimeString(date1, isTimestampStr);
                        date2 = DatetimeHelper.GetOracleUniformDatetimeString(date2, isTimestampStr);
                    }

                    string dataType = isStringValue ? (isTimestampStr ? "TIMESTAMP" : "DATE") : "";

                    string strDate1 = $"{dataType}{date1}";
                    string strDate2 = $"{dataType}{date2}";

                    if (!isTimestampStr)
                    {
                        strDate1 = $"CAST({strDate1} AS TIMESTAMP)";
                        strDate2 = $"CAST({strDate2} AS TIMESTAMP)";
                    }

                    string strDate1MinusData2 = $"{strDate1}-{strDate2}";
                    string dateFormat = "'yyyy-MM-dd'";
                    string datetimeFormat = "'yyyy-MM-dd HH24:mi:ss'";

                    Func<string, string> getDiffValue = (multiplier) =>
                    {
                        string value = "";

                        if(isDateStr)
                        {
                            value =  $"(TO_DATE({date1}, {dateFormat})-TO_DATE({date2}, {dateFormat}))";
                        }
                        else if(isTimestampStr)
                        {
                            value = $"(TO_DATE({date1}, {datetimeFormat})-TO_DATE({date2}, {datetimeFormat}))";
                        }
                        else
                        {
                            value = $"(TO_DATE(TO_CHAR({date1}, {datetimeFormat}))-TO_DATE(TO_CHAR({date2}, {datetimeFormat})))";
                        }

                        return $"ROUND({value}*{multiplier})";
                    };

                    switch (unit)
                    {
                        case "YEAR":
                            newExpression = $"ROUND(EXTRACT(DAY FROM ({strDate1MinusData2}))/365)";
                            break;
                        case "MONTH":
                            newExpression = $"MONTHS_BETWEEN({strDate1},{strDate2})";
                            break;
                        case "DAY":
                            newExpression = $"EXTRACT(DAY FROM ({strDate1MinusData2}))";
                            break;
                        case "HOUR":
                            newExpression = $"EXTRACT(DAY FROM ({strDate1MinusData2}))*24 + EXTRACT(HOUR FROM ({strDate1MinusData2}))";
                            break;
                        case "MINUTE":
                            //newExpression = $"(EXTRACT(DAY FROM {strDate1MinusData2}) * 24 + EXTRACT(HOUR FROM {strDate1MinusData2})) * 60 + EXTRACT(MINUTE FROM {strDate1MinusData2})";
                            newExpression = getDiffValue("24*60");
                            break;
                        case "SECOND":
                            //newExpression = $"((EXTRACT(DAY FROM {strDate1MinusData2}) * 24 +  EXTRACT(HOUR FROM {strDate1MinusData2})) * 60 + EXTRACT(MINUTE FROM {strDate1MinusData2})) * 60 +  EXTRACT(SECOND FROM {strDate1MinusData2})";
                            newExpression = getDiffValue("24*60*60");
                            break;
                    }
                }
            }

            return newExpression;
        }       
    }
}
