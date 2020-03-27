using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DatabaseConverter.Core
{
    public class ConstraintTranslator : DbObjectTokenTranslator
    {
        private List<TableConstraint> constraints;

        public ConstraintTranslator(DbInterpreter sourceDbInterpreter, DbInterpreter targetDbInterpreter, List<TableConstraint> constraints) : base(sourceDbInterpreter, targetDbInterpreter)
        {
            this.constraints = constraints;
        }

        public override void Translate()
        {
            if (this.sourceDbInterpreter.DatabaseType == this.targetDbInterpreter.DatabaseType)
            {
                return;
            }

            this.LoadMappings();

            List<TableConstraint> invalidConstraints = new List<TableConstraint>();

            foreach (TableConstraint constraint in this.constraints)
            {
                constraint.Definition = this.ParseDefinition(constraint.Definition);

                if (this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle && constraint.Definition.Contains("SYSDATE"))
                {
                    invalidConstraints.Add(constraint);
                }
                else
                {
                    if (this.targetDbInterpreter.DatabaseType == DatabaseType.Oracle && this.sourceDbInterpreter.DatabaseType != DatabaseType.Oracle)
                    {
                        string likeRegex = $@"(([\w\[\]""`]+)[\s]+(like)[\s]+(['][\[].+[\]][']))"; //example: ([SHELF] like '[A-Za-z]' OR "SHELF"='N/A'), to match: [SHELF] like '[A-Za-z]'

                        MatchCollection matches = Regex.Matches(constraint.Definition, likeRegex, RegexOptions.IgnoreCase);

                        if (matches.Count > 0)
                        {
                            foreach (Match m in matches)
                            {
                                string[] items = m.Value.Split(' ');

                                string newValue = $"REGEXP_LIKE({items[0]},{items[2]})";

                                constraint.Definition = constraint.Definition.Replace(m.Value, newValue);
                            }
                        }
                    }
                    else if (this.sourceDbInterpreter.DatabaseType == DatabaseType.Oracle && this.targetDbInterpreter.DatabaseType != DatabaseType.Oracle)
                    {
                        string likeFunctionName = "REGEXP_LIKE";
                        Regex likeFunctionNameExp = new Regex($"({likeFunctionName})", RegexOptions.IgnoreCase);

                        if (constraint.Definition.ToUpper().Contains(likeFunctionName))
                        {
                            string likeRegex= $@"({likeFunctionName})[\s]?[(][\w\[\]""` ]+[,][\s]?(['][\[].+[\]]['])[)]"; //example: REGEXP_LIKE("SHELF",'[A-Za-z]')

                            MatchCollection matches = Regex.Matches(constraint.Definition, likeRegex, RegexOptions.IgnoreCase);

                            if (matches.Count > 0)
                            {
                                foreach (Match m in matches)
                                {   
                                    string[] items = likeFunctionNameExp.Replace(m.Value, "").Trim('(', ')').Split(',');

                                    string newValue = $"{items[0]} like {items[1]}";

                                    constraint.Definition = constraint.Definition.Replace(m.Value, newValue);
                                }
                            }
                        }
                    }
                }
            }

            this.constraints.RemoveAll(item => invalidConstraints.Contains(item));
        }
    }
}
