using DatabaseConverter.Core.Model;
using DatabaseConverter.Model;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public const DatabaseObjectType SupportDatabaseObjectType = DatabaseObjectType.Column | DatabaseObjectType.Constraint |
                                                                    DatabaseObjectType.View | DatabaseObjectType.Function | DatabaseObjectType.Procedure |
                                                                    DatabaseObjectType.Trigger | DatabaseObjectType.Sequence | DatabaseObjectType.Type;
        
        public List<TranslateResult> TranslateResults { get; private set; } = new List<TranslateResult>();

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

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Type))
            {
                UserDefinedTypeTranslator userDefinedTypeTranslator = new UserDefinedTypeTranslator(this.sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.UserDefinedTypes);
                this.Translate(userDefinedTypeTranslator);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Sequence))
            {
                SequenceTranslator sequenceTranslator = new SequenceTranslator(this.sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.Sequences);
                this.Translate(sequenceTranslator);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Column))
            {
                ColumnTranslator columnTranslator = new ColumnTranslator(this.sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.TableColumns);
                columnTranslator.Option = this.option;
                columnTranslator.UserDefinedTypes = this.UserDefinedTypes;
                this.Translate(columnTranslator);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Constraint))
            {
                ConstraintTranslator constraintTranslator = new ConstraintTranslator(sourceInterpreter, this.targetInerpreter, this.targetSchemaInfo.TableConstraints) { ContinueWhenErrorOccurs = this.ContinueWhenErrorOccurs };
                constraintTranslator.TableCoumns = this.targetSchemaInfo.TableColumns;
                this.Translate(constraintTranslator);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.View))
            {
                ScriptTranslator<View> viewTranslator = this.GetScriptTranslator<View>(this.targetSchemaInfo.Views);
                viewTranslator.Translate();

                this.TranslateResults.AddRange(viewTranslator.TranslateResults);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Function))
            {
                ScriptTranslator<Function> functionTranslator = this.GetScriptTranslator<Function>(this.targetSchemaInfo.Functions);
                functionTranslator.Translate();

                this.TranslateResults.AddRange(functionTranslator.TranslateResults);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Procedure))
            {
                ScriptTranslator<Procedure> procedureTranslator = this.GetScriptTranslator<Procedure>(this.targetSchemaInfo.Procedures);
                procedureTranslator.Translate();

                this.TranslateResults.AddRange(procedureTranslator.TranslateResults);
            }

            if (this.NeedTranslate(databaseObjectType, DatabaseObjectType.Trigger))
            {
                ScriptTranslator<TableTrigger> triggerTranslator = this.GetScriptTranslator<TableTrigger>(this.targetSchemaInfo.TableTriggers);
                triggerTranslator.Translate();

                this.TranslateResults.AddRange(triggerTranslator.TranslateResults);
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
            translator.Subscribe(this.observer);

            return translator;
        }     

        private void TranslateSchema()
        {
            List<SchemaMappingInfo> schemaMappings = this.option.SchemaMappings;

            if (schemaMappings.Count == 0)
            {
                schemaMappings.Add(new SchemaMappingInfo() { SourceSchema = "", TargetSchema = this.targetInerpreter.DefaultSchema });
            }
            else
            {
                if (this.sourceInterpreter.DefaultSchema != null && this.targetInerpreter.DefaultSchema != null)
                {
                    if (!schemaMappings.Any(item => item.SourceSchema == this.sourceInterpreter.DefaultSchema))
                    {
                        schemaMappings.Add(new SchemaMappingInfo() { SourceSchema = this.sourceInterpreter.DefaultSchema, TargetSchema = this.targetInerpreter.DefaultSchema });
                    }
                }                
            }

            SchemaInfoHelper.MapDatabaseObjectSchema(targetSchemaInfo, schemaMappings);
        }

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }
    }
}
