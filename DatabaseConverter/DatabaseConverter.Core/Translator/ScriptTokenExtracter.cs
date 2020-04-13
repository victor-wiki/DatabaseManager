using SqlAnalyser.Model;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DatabaseConverter.Core
{
    public class ScriptTokenExtracter
    {
        public DbScript Script { get; set; }

        private List<TokenInfo> tokens = new List<TokenInfo>();

        public ScriptTokenExtracter(DbScript script)
        {
            this.Script = script;
        }

        public List<TokenInfo> Extract()
        {
            this.tokens.Clear();

            this.ExtractTokens(this.Script);

            return this.tokens;
        }

        private void ExtractTokens(dynamic obj)
        {
            Type type = obj.GetType();

            if (obj is TokenInfo token)
            {
                this.AddToken(token);

                if (token.Tag != null)
                {
                    this.ExtractTokens(token.Tag);
                }

                return;
            }

            var properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                dynamic value = property.GetValue(obj);

                if (value == null)
                {
                    continue;
                }

                else if (value is TokenInfo t)
                {
                    this.ExtractTokens(t);
                }
                else if (property.PropertyType.IsGenericType && !(property.DeclaringType == typeof(CommonScript) && property.Name == nameof(CommonScript.Functions)))
                {
                    foreach (var v in value)
                    {
                        this.ExtractTokens(v);
                    }
                }
                else if (value is Statement || value is TemporaryTable)
                {
                    this.ExtractTokens(value);
                }
            }
        }

        private void AddToken(TokenInfo token)
        {
            this.tokens.Add(token);
        }
    }
}
