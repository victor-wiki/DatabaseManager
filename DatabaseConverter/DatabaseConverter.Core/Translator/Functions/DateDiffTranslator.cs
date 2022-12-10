using DatabaseConverter.Core.Model.Functions;
using DatabaseConverter.Model;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;

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

                bool isStringValue1 = ValueHelper.IsStringValue(dateDiff.Value.Date1);
                bool isStringValue2 = ValueHelper.IsStringValue(dateDiff.Value.Date2);
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

                    string strDate1 = $"{date1}{dataType}";
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
                    bool isTimestampStr1 = isStringValue1 && DatetimeHelper.IsTimestampString(date1);
                    bool isTimestampStr2 = isStringValue2 && DatetimeHelper.IsTimestampString(date2);
                    bool isDateStr1 = isStringValue1 && !isTimestampStr1;
                    bool isDateStr2 = isStringValue2 && !isTimestampStr2;

                    Func<string, bool, bool, string> getStrDate = (date, isStringValue, isTimestampStr) =>
                    {
                        if (isStringValue)
                        {
                            date = DatetimeHelper.GetOracleUniformDatetimeString(date, isTimestampStr);
                        }

                        string dataType = isStringValue ? (isTimestampStr ? "TIMESTAMP" : "DATE") : "";

                        string strDate = $"{dataType}{date}";

                        if (!isTimestampStr)
                        {
                            strDate = $"CAST({strDate} AS TIMESTAMP)";
                        }

                        return strDate;
                    };

                    string strDate1 = getStrDate(date1, isStringValue1, isTimestampStr1);
                    string strDate2 = getStrDate(date2, isStringValue2, isTimestampStr2);

                    string strDate1MinusData2 = $"{strDate1}-{strDate2}";
                    string dateFormat = $"'{DatetimeHelper.DateFormat}'";
                    string datetimeFormat = $"'{DatetimeHelper.OracleDatetimeFormat}'";

                    Func<string, bool, bool, string> getDateFormatStr = (date, isDateStr, isTimestampStr) =>
                    {
                        if (isDateStr)
                        {
                            return $"TO_DATE({date}, {dateFormat})";
                        }
                        else if (isTimestampStr)
                        {
                            return $"TO_DATE({date}, {datetimeFormat})";
                        }
                        else
                        {
                            return $"TO_DATE(TO_CHAR({date}, {datetimeFormat}), {datetimeFormat})";
                        }
                    };

                    Func<string, string> getDiffValue = (multiplier) =>
                    {
                        string value = $"({getDateFormatStr(date1, isDateStr1, isTimestampStr1)}-{getDateFormatStr(date2, isDateStr2, isTimestampStr2)})";

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
                        case "WEEK":
                            newExpression = $"ROUND(EXTRACT(DAY FROM ({strDate1MinusData2}))/7)";
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
                else if (this.TargetDbType == DatabaseType.Sqlite)
                {
                    Func<string, string> getDiffValue = (multiplier) =>
                    {
                        string value = $"(JULIANDAY({date1})-JULIANDAY({date2}))"; 

                        if(unit== "YEAR")
                        {
                            return $"FLOOR(ROUND({value}{multiplier},2))";
                        }
                        else
                        {
                            return $"ROUND({value}{multiplier})";
                        }                        
                    };

                    switch (unit)
                    {
                        case "YEAR":
                            newExpression = getDiffValue("/365");
                            break;
                        case "MONTH":
                            newExpression = getDiffValue("/30");
                            break;
                        case "WEEK":
                            newExpression = getDiffValue("/7");
                            break;
                        case "DAY":
                            newExpression = getDiffValue("");
                            break;
                        case "HOUR":
                            newExpression = getDiffValue("*24");
                            break;
                        case "MINUTE":                           
                            newExpression = getDiffValue("24*60");
                            break;
                        case "SECOND":                            
                            newExpression = getDiffValue("24*60*60");
                            break;
                    }
                }
            }

            return newExpression;
        }
    }
}
