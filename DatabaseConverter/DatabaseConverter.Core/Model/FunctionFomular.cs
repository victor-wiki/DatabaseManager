using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseConverter.Model
{
    public class FunctionFomular
    {
        private string _body;
        private List<string> _args;
        private string _expression;

        public string Delimiter { get; set; } = ",";

        public string Name { get; set; }
        public int StartIndex { get; set; }
        public int StopIndex { get; set; }

        public int Length => this.StopIndex - this.StartIndex + 1;

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
                        int firstLeftIndex = this.Expression.IndexOf('(');
                        int lastRightIndex = this.Expression.LastIndexOf(')');

                        if (firstLeftIndex > 0 && lastRightIndex > 0 && lastRightIndex > firstLeftIndex)
                        {
                            this._body = this.Expression.Substring(firstLeftIndex + 1, lastRightIndex - firstLeftIndex - 1);
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

                    List<int> commaIndexes = new List<int>();

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
                                    commaIndexes.Add(i);
                                }
                            }

                            i++;
                        }

                        int lastCommaIndex = -1;

                        foreach (int commaIndex in commaIndexes)
                        {
                            int startIndex = lastCommaIndex == -1 ? 0 : lastCommaIndex + 1;
                            int length = commaIndex - startIndex;

                            this._args.Add(this.Body.Substring(startIndex, length));

                            lastCommaIndex = commaIndex;
                        }

                        if (lastCommaIndex < this.Body.Length - 1)
                        {
                            this._args.Add(this.Body.Substring(lastCommaIndex + 1));
                        }
                    }
                    else
                    {
                        int lastIndex = this.Body.ToUpper().LastIndexOf(this.Delimiter);

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
