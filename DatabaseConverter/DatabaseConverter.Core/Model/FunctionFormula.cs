using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseConverter.Model
{
    public class FunctionFormula
    {
        private string _name;
        private string _body;
        private string _expression; 

        public bool HasParentheses => this._expression?.Contains("(") == true;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(this._name) && !string.IsNullOrEmpty(this._expression))
                {
                    int firstParenthesesIndex = this.Expression.IndexOf('(');

                    if (firstParenthesesIndex > 0)
                    {
                        this._name = this._expression.Substring(0, firstParenthesesIndex);
                    }
                }

                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public string Expression
        {
            get
            {
                return this._expression;
            }
            set
            {
                this._body = null;
                this._expression = value;
            }
        }

        public FunctionFormula(string expression)
        {
            this.Expression = expression;
        }

        public FunctionFormula(string name, string expression)
        {
            this.Name = name;
            this.Expression = expression;
        }

        public string Body
        {
            get
            {
                if (string.IsNullOrEmpty(this._body))
                {
                    if (!string.IsNullOrEmpty(this.Expression))
                    {
                        int firstParenthesesIndexIndex = this.Expression.IndexOf('(');
                        int lastParenthesesIndex = this.Expression.LastIndexOf(')');

                        if (firstParenthesesIndexIndex > 0 && lastParenthesesIndex > 0 && lastParenthesesIndex > firstParenthesesIndexIndex)
                        {
                            this._body = this.Expression.Substring(firstParenthesesIndexIndex + 1, lastParenthesesIndex - firstParenthesesIndexIndex - 1);
                        }
                    }
                }

                return this._body ?? string.Empty;
            }
        }

        public List<string> GetArgs(string delimiter = ",")
        {
            List<string> args = new List<string>();

            string body = this.Body;

            if (string.IsNullOrEmpty(body))
            {
                return args;
            }

            List<int> delimiterIndexes = new List<int>();

            if (delimiter.Length == 1)
            {
                char delimiterChar = delimiter[0];

                int i = 0;

                int leftParenthesesCount = 0;
                int rightParenthesesCount = 0;
                int singleQuotationCharCount = 0;

                foreach (var c in body)
                {
                    if (c == '\'')
                    {
                        singleQuotationCharCount++;
                    }

                    if (c == '(')
                    {
                        if (singleQuotationCharCount % 2 == 0)
                        {
                            leftParenthesesCount++;
                        }
                    }
                    else if (c == ')')
                    {
                        if (singleQuotationCharCount % 2 == 0)
                        {
                            rightParenthesesCount++;
                        }
                    }

                    if (c == delimiterChar)
                    {
                        if ((leftParenthesesCount == rightParenthesesCount) && (singleQuotationCharCount % 2 == 0))
                        {
                            delimiterIndexes.Add(i);
                        }
                    }

                    i++;
                }

                int lastDelimiterIndex = -1;

                foreach (int delimiterIndex in delimiterIndexes)
                {
                    int startIndex = lastDelimiterIndex == -1 ? 0 : lastDelimiterIndex + 1;
                    int length = delimiterIndex - startIndex;

                    if (length > 0)
                    {
                        string value = body.Substring(startIndex, length);

                        args.Add(value.Trim());
                    }

                    lastDelimiterIndex = delimiterIndex;
                }

                if (lastDelimiterIndex < body.Length - 1)
                {
                    args.Add(body.Substring(lastDelimiterIndex + 1).Trim());
                }
            }
            else
            {
                int lastIndex = body.LastIndexOf(delimiter, StringComparison.OrdinalIgnoreCase);

                if (lastIndex >= 0)
                {
                    string firstPart = body.Substring(0, lastIndex).Trim();
                    string lastPart = body.Substring(lastIndex + delimiter.Length).Trim();

                    args.Add(firstPart);
                    args.Add(lastPart);
                }
            }

            return args;
        }
    }
}
