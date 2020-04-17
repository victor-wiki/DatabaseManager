using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class DbScript
    {
        public TokenInfo Owner { get; set; }
        public TokenInfo Name { get; set; }

        public string FullName
        {
            get
            {
                if (this.Owner == null || this.Owner.Symbol == null)
                {
                    return this.Name?.ToString();
                }
                else
                {
                    return this.Owner + "." + this.Name;
                }
            }
        }
    }
}
