using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DatabaseConverter.Core
{
    public class TranslateEngine
    {
        private SchemaInfo targetSchemaInfo;
        private DbInterpreter sourceInterpreter;
        private DbInterpreter targetInerpreter;
        private string targetDbOwner;
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();

        public TranslateHandler OnTranslated;

        public bool SkipError { get; set; }

        public TranslateEngine(SchemaInfo targetSchemaInfo, DbInterpreter sourceInterpreter, DbInterpreter targetInerpreter, string targetDbOwner = null)
        {
            this.targetSchemaInfo = targetSchemaInfo;
            this.sourceInterpreter = sourceInterpreter;
            this.targetInerpreter = targetInerpreter;
            this.targetDbOwner = targetDbOwner;
        }

        public void Translate()
        {
            this.TranslateOwner();

            ColumnTranslator columnTranslator = new ColumnTranslator(this.sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.TableColumns);
            columnTranslator.Translate();

            ConstraintTranslator constraintTranslator = new ConstraintTranslator(sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.TableConstraints) { SkipError = this.SkipError };
            constraintTranslator.Translate();

            //Currently, ANTLR can't parse some complex tsql accurately, so it uses general strategy.
            if (this.sourceInterpreter.DatabaseType == DatabaseType.SqlServer)
            {
                ViewTranslator viewTranslator = new ViewTranslator(sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.Views, this.targetDbOwner) { SkipError = this.SkipError };
                viewTranslator.OnTranslated += this.ScriptTranslated;
                viewTranslator.Translate();
            }
            else
            {
                ScriptTranslator<View> viewTranslator = this.GetScriptTranslator<View>(this.targetSchemaInfo.Views);
                viewTranslator.Translate();
            }

            ScriptTranslator<Procedure> procedureTranslator = this.GetScriptTranslator<Procedure>(this.targetSchemaInfo.Procedures);
            procedureTranslator.Translate();

            ScriptTranslator<Function> functionTranslator = this.GetScriptTranslator<Function>(this.targetSchemaInfo.Functions);
            functionTranslator.Translate();

            ScriptTranslator<TableTrigger> triggerTranslator = this.GetScriptTranslator<TableTrigger>(this.targetSchemaInfo.TableTriggers);
            triggerTranslator.Translate();
        }

        private ScriptTranslator<T> GetScriptTranslator<T>(List<T> dbObjects) where T : ScriptDbObject
        {
            ScriptTranslator<T> translator = new ScriptTranslator<T>(sourceInterpreter, this.targetInerpreter, dbObjects) { SkipError = this.SkipError };
            translator.UserDefinedTypes = this.UserDefinedTypes;
            translator.TargetDbOwner = this.targetDbOwner;
            translator.OnTranslated += this.ScriptTranslated;

            return translator;
        }

        private void ScriptTranslated(DatabaseType dbType, DatabaseObject dbObject, object result)
        {
            if (this.OnTranslated != null)
            {
                this.OnTranslated(dbType, dbObject, result);
            }
        }

        private void TranslateOwner()
        {
            if (!string.IsNullOrEmpty(this.targetDbOwner))
            {
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.UserDefinedTypes);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.Functions);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.Tables);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.TableColumns);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.TablePrimaryKeys);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.TableForeignKeys);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.TableIndexes);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.TableTriggers);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.TableConstraints);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.Views);
                this.SetDatabaseObjectsOwner(this.targetSchemaInfo.Procedures);
            }
        }

        private void SetDatabaseObjectsOwner<T>(List<T> dbObjects) where T : DatabaseObject
        {
            dbObjects.ForEach(item => item.Owner = this.targetDbOwner);
        }
    }
}
