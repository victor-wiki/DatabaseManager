using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseConverter.Core
{
    public class PostgresTranslateHelper
    {
        public static string ExtractRountineScriptDeclaresAndBody(string definition)
        {
            string[] lines = definition.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();

            int firstBeginIndex = -1, lastEndIndex = -1;

            int index = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith("DECLARE"))
                {
                    sb.AppendLine(line);
                }
                if ((line.StartsWith("BEGIN") || line.StartsWith("AS")) && firstBeginIndex == -1)
                {
                    firstBeginIndex = index;
                }
                else if (line.StartsWith("END") || line.StartsWith("$"))
                {
                    lastEndIndex = index;
                }

                index++;
            }

            if (lastEndIndex == -1)
            {
                lastEndIndex = lines.Length - 1;
            }

            List<string> items = new List<string>();

            index = 0;

            foreach (string line in lines)
            {
                if (index > firstBeginIndex && index < lastEndIndex)
                {
                    sb.AppendLine(line);
                }

                index++;
            }

            return sb.ToString();
        }

        public static string MergeDefinition(string originalDefinition, string declaresAndBody)
        {
            StringBuilder sb = new StringBuilder();

            var declareAndBodyLines = declaresAndBody.Split(Environment.NewLine);

            var declares = declareAndBodyLines.Where(item => item.StartsWith("DECLARE"));

            var bodyLines = declareAndBodyLines.Where(item => !item.StartsWith("DECLARE"));

            var originalLines = originalDefinition.Split(Environment.NewLine);

            int firstBeginIndex = -1;

            int i = 0;

            foreach (string line in originalLines)
            {
                if (line.StartsWith("BEGIN") && firstBeginIndex == -1)
                {
                    firstBeginIndex = i;

                    foreach (var declare in declares)
                    {
                        sb.AppendLine(declare);
                    }
                }
                else if (firstBeginIndex != -1 && i == (firstBeginIndex + 1))
                {
                    foreach (var bodyLine in bodyLines)
                    {
                        sb.AppendLine(bodyLine);
                    }
                }

                sb.AppendLine(line);

                i++;
            }

            return sb.ToString();
        }
    }
}
