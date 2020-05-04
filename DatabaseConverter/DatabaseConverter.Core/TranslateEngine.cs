using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;

namespace DatabaseConverter.Core
{
    public class TranslateEngine
    {
        private SchemaInfo sourceSchemaInfo;
        private SchemaInfo targetSchemaInfo;
        private DbInterpreter sourceInterpreter;
        private DbInterpreter targetInerpreter;
        private string targetDbOwner;
        private IObserver<FeedbackInfo> observer;
        private DbConverterOption option;
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();
        public const DatabaseObjectType SupportDatabaseObjectType = DatabaseObjectType.TableColumn | DatabaseObjectType.TableConstraint |
                                                       DatabaseObjectType.View | DatabaseObjectType.Function |
                                                       DatabaseObjectType.Procedure | DatabaseObjectType.TableTrigger;


        public TranslateHandler OnTranslated;

        public bool SkipError { get; set; }

        public TranslateEngine(SchemaInfo sourceSchemaInfo, SchemaInfo targetSchemaInfo, DbInterpreter sourceInterpreter, DbInterpreter targetInerpreter, DbConverterOption option = null, string targetDbOwner = null)
        {
            this.sourceSchemaInfo = sourceSchemaInfo;
            this.targetSchemaInfo = targetSchemaInfo;
            this.sourceInterpreter = sourceInterpreter;
            this.targetInerpreter = targetInerpreter;
            this.targetDbOwner = targetDbOwner;
            this.option = option;
        }

        public void Translate(DatabaseObjectType databaseObjectType = DatabaseObjectType.None)
        {
            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.TableColumn))
            {
                ColumnTranslator columnTranslator = new ColumnTranslator(this.sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.TableColumns);
                this.Translate(columnTranslator);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.TableConstraint))
            {
                ConstraintTranslator constraintTranslator = new ConstraintTranslator(sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.TableConstraints) { SkipError = this.SkipError };
                this.Translate(constraintTranslator);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.View))
            {
                ScriptTranslator<View> viewTranslator = this.GetScriptTranslator<View>(this.targetSchemaInfo.Views);
                viewTranslator.Translate();
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Function))
            {
                ScriptTranslator<Function> functionTranslator = this.GetScriptTranslator<Function>(this.targetSchemaInfo.Functions);
                functionTranslator.Translate();
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Procedure))
            {
                ScriptTranslator<Procedure> procedureTranslator = this.GetScriptTranslator<Procedure>(this.targetSchemaInfo.Procedures);
                procedureTranslator.Translate();
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.TableTrigger))
            {
                ScriptTranslator<TableTrigger> triggerTranslator = this.GetScriptTranslator<TableTrigger>(this.targetSchemaInfo.TableTriggers);
                triggerTranslator.Translate();
            }

            this.TranslateOwner();
        }

        private void Translate(DbObjectTranslator translator)
        {
            translator.Option = this.option;
            translator.SourceSchemaInfo = this.sourceSchemaInfo;
            translator.TargetDbOwner = this.targetDbOwner;
            translator.Subscribe(this.observer);
            translator.Translate();
        }

        private bool NeedTranslate(DatabaseObjectType databaseObjectType, DatabaseObjectType currentDbType)
        {
            if (databaseObjectType == DatabaseObjectType.None || databaseObjectType.HasFlag(currentDbType))
            {
                return true;
            }

            return false;
        }

        private ScriptTranslator<T> GetScriptTranslator<T>(List<T> dbObjects) where T : ScriptDbObject
        {
            ScriptTranslator<T> translator = new ScriptTranslator<T>(sourceInterpreter, this.targetInerpreter, dbObjects) { SkipError = this.SkipError };
            translator.Option = this.option;
            translator.UserDefinedTypes = this.UserDefinedTypes;
            translator.TargetDbOwner = this.targetDbOwner;
            translator.OnTranslated += this.ScriptTranslated;
            translator.Subscribe(this.observer);

            return translator;
        }

        private void ScriptTranslated(DatabaseType dbType, DatabaseObject dbObject, TranslateResult result)
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

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }
    }
}
