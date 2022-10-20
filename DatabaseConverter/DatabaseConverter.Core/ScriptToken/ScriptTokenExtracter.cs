using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DatabaseConverter.Core
{
    public class ScriptTokenExtracter
    {
        public Statement Statement { get; set; }

        private List<TokenInfo> tokens = new List<TokenInfo>();

        public ScriptTokenExtracter(Statement statement)
        {
            this.Statement = statement;
        }

        public IEnumerable<TokenInfo> Extract()
        {
            this.tokens.Clear();

            this.ExtractTokens(this.Statement);

            return this.tokens;
        }

        private void ExtractTokens(dynamic obj)
        {
            Type type = obj.GetType();

            Action readProperties = () =>
            {
                var properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    if (property.Name == nameof(TokenInfo.Parent))
                    {
                        continue;
                    }

                    dynamic value = property.GetValue(obj);

                    if (value == null)
                    {
                        continue;
                    }

                    if (value is TokenInfo)
                    {
                        if (!value.Equals(obj))
                        {
                            this.ExtractTokens(value);
                        }
                    }
                    else if (value.GetType().IsClass && property.PropertyType.IsGenericType && !(property.DeclaringType == typeof(CommonScript) && property.Name == nameof(CommonScript.Functions)))
                    {
                        foreach (var v in value)
                        {
                            this.ExtractTokens(v);
                        }
                    }
                    else if (value is Statement || value is TemporaryTable || value is StatementItem || value is SelectTopInfo)
                    {
                        this.ExtractTokens(value);
                    }
                }
            };

            if (obj is TokenInfo token)
            {
                this.AddToken(token);

                readProperties();

                return;
            }

            readProperties();
        }

        private void AddToken(TokenInfo token)
        {
            if (token == null)
            {
                return;
            }            

            this.tokens.Add(token);
        }
    }
}
