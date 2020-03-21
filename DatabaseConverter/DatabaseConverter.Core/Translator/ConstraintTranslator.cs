using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System.Collections.Generic;

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
            }

            this.constraints.RemoveAll(item => invalidConstraints.Contains(item));
        }
    }
}
