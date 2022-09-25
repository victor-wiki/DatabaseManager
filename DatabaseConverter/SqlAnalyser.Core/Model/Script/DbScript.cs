using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class DbScript
    {
        public string Schema { get; set; }
        public TokenInfo Name { get; set; }

        public string NameWithSchema
        {
            get
            {
                if (string.IsNullOrEmpty(this.Schema))
                {
                    return this.Name?.ToString();
                }
                else
                {
                    return $"{this.Schema}.{this.Name}";
                }
            }
        }
    }
}
