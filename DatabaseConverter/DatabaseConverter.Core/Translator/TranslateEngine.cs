using DatabaseConverter.Core.Model;
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
        private IObserver<FeedbackInfo> observer;
        private DbConverterOption option;
        public List<UserDefinedType> UserDefinedTypes { get; set; } = new List<UserDefinedType>();
        public const DatabaseObjectType SupportDatabaseObjectType = DatabaseObjectType.TableColumn | DatabaseObjectType.TableConstraint |
                                                       DatabaseObjectType.View | DatabaseObjectType.Function |
                                                       DatabaseObjectType.Procedure | DatabaseObjectType.TableTrigger | DatabaseObjectType.Sequence;


        public TranslateHandler OnTranslated;

        public bool ContinueWhenErrorOccurs { get; set; }

        public TranslateEngine(SchemaInfo sourceSchemaInfo, SchemaInfo targetSchemaInfo, DbInterpreter sourceInterpreter, DbInterpreter targetInerpreter, DbConverterOption option = null)
        {
            this.sourceSchemaInfo = sourceSchemaInfo;
            this.targetSchemaInfo = targetSchemaInfo;
            this.sourceInterpreter = sourceInterpreter;
            this.targetInerpreter = targetInerpreter;           
            this.option = option;
        }

        public void Translate(DatabaseObjectType databaseObjectType = DatabaseObjectType.None)
        {
            this.TranslateSchema();

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Sequence))
            {
                SequenceTranslator sequenceTranslator = new SequenceTranslator(this.sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.Sequences);
                this.Translate(sequenceTranslator);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.TableColumn))
            {
                ColumnTranslator columnTranslator = new ColumnTranslator(this.sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.TableColumns);
                this.Translate(columnTranslator);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.TableConstraint))
            {
                ConstraintTranslator constraintTranslator = new ConstraintTranslator(sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.TableConstraints) { ContinueWhenErrorOccurs = this.ContinueWhenErrorOccurs };
                constraintTranslator.TableCoumns = this.targetSchemaInfo.TableColumns;
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
        }

        private void Translate(DbObjectTranslator translator)
        {
            translator.Option = this.option;
            translator.SourceSchemaInfo = this.sourceSchemaInfo;          
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
            ScriptTranslator<T> translator = new ScriptTranslator<T>(sourceInterpreter, this.targetInerpreter, dbObjects) { ContinueWhenErrorOccurs = this.ContinueWhenErrorOccurs };
            translator.Option = this.option;
            translator.UserDefinedTypes = this.UserDefinedTypes;        
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

        private void TranslateSchema()
        {
            List<SchemaMappingInfo> schemaMappings = this.option.SchemaMappings;   

            if (schemaMappings.Count == 0)
            {
                schemaMappings.Add(new SchemaMappingInfo() { SourceSchema = "", TargetSchema = this.targetInerpreter.DefaultSchema });
            }

            SchemaInfoHelper.MapDatabaseObjectSchema(targetSchemaInfo, schemaMappings);
        }        

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }
    }
}
