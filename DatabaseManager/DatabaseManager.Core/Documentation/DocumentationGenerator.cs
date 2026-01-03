using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;
using DatabaseManager.Core.Model;
using DatabaseManager.FileUtility;
using DatabaseManager.FileUtility.Model;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatabaseManager.Core
{
    public class DocumentationGenerator
    {
        private IObserver<FeedbackInfo> observer;

        public static string True_DisplayText = "True";
        public static string False_DisplayText = "False";

        public void Subscribe(IObserver<FeedbackInfo> observer)
        {
            this.observer = observer;
        }

        public async Task<DocumentationGenerateResult> Generate(DbInterpreter dbInterpreter, GenerateColumnDocumentationOption option, CancellationToken cancellationToken)
        {
            DocumentationGenerateResult result = new DocumentationGenerateResult();

            try
            {
                dbInterpreter.Option.ObjectFetchMode = DatabaseObjectFetchMode.Details;

                SchemaInfoFilter filter = new SchemaInfoFilter();
                filter.DatabaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.Column | DatabaseObjectType.PrimaryKey;

                var schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

                var tables = schemaInfo.Tables.OrderBy(item => item.Name);
                var columns = schemaInfo.TableColumns;
                var primaryKeys = schemaInfo.TablePrimaryKeys;

                DocumentBody body = new DocumentBody();

                int total = tables.Count();
                int count = 0;

                foreach (var table in tables)
                {
                    if(cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    count++;

                    this.Feedback($@"({count}/{total})Begin to process table ""{table.Name}""...");

                    string tableComment = table.Comment;
                    var keyColumns = primaryKeys.FirstOrDefault(item => item.Schema == table.Schema && item.TableName == table.Name)?.Columns;

                    DocumentPart part = new DocumentPart();

                    part.Title = table.Name;

                    if (option.ShowTableComment && !string.IsNullOrEmpty(tableComment))
                    {
                        part.Comment = table.Comment;
                    }

                    GridData gridData = new GridData();

                    gridData.Columns = new List<GridColumn>();

                    foreach (var property in option.Properties)
                    {
                        GridColumn gridColumn = new GridColumn();

                        gridColumn.Name = !string.IsNullOrEmpty(property.DisplayName) ? property.DisplayName : ManagerUtil.GetDisplayTitle(property.PropertyName);

                        gridData.Columns.Add(gridColumn);
                    }

                    var tableColumns = columns.Where(item => item.Schema == table.Schema && item.TableName == table.Name);

                    gridData.Rows = new List<GridRow>();

                    foreach (var column in tableColumns)
                    {
                        GridRow gridRow = new GridRow();

                        List<string> values = new List<string>();

                        foreach (var property in option.Properties)
                        {
                            switch (property.PropertyName)
                            {
                                case nameof(TableColumnProperty.Name):
                                    values.Add(column.Name);
                                    break;
                                case nameof(TableColumnProperty.DataType):
                                    values.Add(dbInterpreter.ParseDataType(column));
                                    break;
                                case nameof(TableColumnProperty.IsNullable):
                                    values.Add(this.GetTrueFalseDisplayText(column.IsNullable));
                                    break;
                                case nameof(TableColumnProperty.IsPrimary):
                                    values.Add(this.GetTrueFalseDisplayText(keyColumns?.Any(item => item.ColumnName == column.Name) == true));
                                    break;
                                case nameof(TableColumnProperty.IsIdentity):
                                    values.Add(this.GetTrueFalseDisplayText(column.IsIdentity));
                                    break;
                                case nameof(TableColumnProperty.DefaultValue):
                                    values.Add(StringHelper.GetBalanceParenthesisTrimedValue(column.DefaultValue));
                                    break;
                                case nameof(TableColumnProperty.Comment):
                                    values.Add(column.Comment);
                                    break;
                            }
                        }

                        gridRow.Cells = new List<GridCell>();

                        foreach (var value in values)
                        {
                            gridRow.Cells.Add(new GridCell() { Content = value });
                        }

                        gridData.Rows.Add(gridRow);
                    }

                    part.Data = gridData;

                    body.Parts.Add(part);

                    this.Feedback($@"({count}/{total})End process table ""{table.Name}"".");
                }

                if(!cancellationToken.IsCancellationRequested)
                {
                    WordGenerationOption generationOption = new WordGenerationOption();
                    generationOption.FilePath = option.FilePath;
                    generationOption.GridColumnHeaderFontIsBold = option.GridColumnHeaderFontIsBold;
                    generationOption.GridColumnHeaderBackgroundColor = option.GridColumnHeaderBackgroundColor;
                    generationOption.GridColumnHeaderForegroundColor = option.GridColumnHeaderForegroundColor;

                    this.Feedback($@"Begin to generate...");

                    WordWriter writer = new WordWriter(generationOption);

                    this.Feedback($@"End generate.");

                    result.FilePath = writer.Write(body);
                    result.IsOK = true;
                }                
            }
            catch (Exception ex)
            {
                result.Message = ExceptionHelper.GetExceptionDetails(ex);
            }

            return result;
        }

        private string GetTrueFalseDisplayText(bool value)
        {
            if(value)
            {
                return True_DisplayText;
            }
            else
            {
                return False_DisplayText;
            }
        }

        public void Feedback(string info)
        {
            this.Feedback(this, info);
        }

        public void Feedback(object owner, string content, FeedbackInfoType infoType = FeedbackInfoType.Info, bool enableLog = true, bool suppressError = false)
        {
            FeedbackInfo info = new FeedbackInfo() { InfoType = infoType, Message = StringHelper.ToSingleEmptyLine(content), Owner = owner };

            FeedbackHelper.Feedback(suppressError ? null : this.observer, info, enableLog);
        }
    }
}
