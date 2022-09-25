using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseConverter.Model
{
    public class FunctionFomular
    {
        private string _name;
        private string _body;
        private List<string> _args;
        private string _expression;

        public string Delimiter { get; set; } = ",";
        public int StartIndex { get; set; }
        public int StopIndex { get; set; }

        public int Length => this.StopIndex - this.StartIndex + 1;

        public string Name
        {
            get
            {
                if(string.IsNullOrEmpty(this._name) && !string.IsNullOrEmpty(this._expression))
                {
                    int firstParenthesesIndex = this.Expression.IndexOf('(');

                    if(firstParenthesesIndex>0)
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
                this._args = null;
                this._expression = value;
            }
        }

        public FunctionFomular(string expression)
        {
            this.Expression = expression;
        }

        public FunctionFomular(string name , string expression)
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

                return this._body ?? this.Expression;
            }
        }

        public List<string> Args
        {
            get
            {
                if (this._args == null || this._args.Count == 0)
                {
                    this._args = new List<string>();

                    List<int> delimiterIndexes = new List<int>();

                    if (this.Delimiter.Length == 1)
                    {
                        char delimiter = this.Delimiter[0];

                        int i = 0;

                        foreach (var c in this.Body)
                        {
                            if (c == ',')
                            {
                                string leftContent = this.Body.Substring(0, i);
                                string rightContent = this.Body.Substring(i + 1);

                                bool leftClosed = leftContent.Count(item => item == '(') == leftContent.Count(item => item == ')');

                                bool rightClosed = rightContent.Count(item => item == ')') == rightContent.Count(item => item == '(');

                                if (leftClosed && rightClosed)
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

                            this._args.Add(this.Body.Substring(startIndex, length));

                            lastDelimiterIndex = delimiterIndex;
                        }

                        if (lastDelimiterIndex < this.Body.Length - 1)
                        {
                            this._args.Add(this.Body.Substring(lastDelimiterIndex + 1));
                        }
                    }
                    else
                    {
                        int lastIndex = this.Body.LastIndexOf(this.Delimiter, StringComparison.OrdinalIgnoreCase);

                        if (lastIndex >= 0)
                        {
                            string firstPart = this.Body.Substring(0, lastIndex);
                            string lastPart = this.Body.Substring(lastIndex + this.Delimiter.Length);

                            this._args.Add(firstPart);
                            this._args.Add(lastPart);
                        }
                    }
                }

                return this._args;
            }
        }
    }
}
