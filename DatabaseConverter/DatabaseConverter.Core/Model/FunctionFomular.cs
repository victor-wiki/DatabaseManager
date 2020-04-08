using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseConverter.Model
{
    public class FunctionFomular
    {
        public string Name { get; set; }
        public string Expression { get; set; }
        public int StartIndex { get; set; }
        public int StopIndex { get; set; }

        public int Length => this.StopIndex - this.StartIndex + 1;

        public string Body
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Expression))
                {
                    int firstLeftIndex = this.Expression.IndexOf('(');
                    int lastRightIndex = this.Expression.LastIndexOf(')');

                    if (firstLeftIndex > 0 && lastRightIndex > 0 && lastRightIndex > firstLeftIndex)
                    {
                        return this.Expression.Substring(firstLeftIndex + 1, lastRightIndex - firstLeftIndex - 1);
                    }
                }

                return this.Expression;
            }
        }
    }
}
