using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using DatabaseManager.Core;
using DatabaseManager.Model;
using DatabaseManager.Helper;
using DatabaseInterpreter.Utility;

namespace DatabaseManager.Controls
{
    public partial class UC_TableDesigner : UserControl, IDbObjContentDisplayer
    {
        private DatabaseObjectDisplayInfo displayInfo;

        public UC_TableDesigner()
        {
            InitializeComponent();
        }

        private void UC_TableDesigner_Load(object sender, EventArgs e)
        {
        }

        private async void InitControls()
        {
            this.ucColumns.InitControls();

            if (this.displayInfo.IsNew)
            {
                this.LoadDatabaseOwners();
            }
            else
            {
                this.cboOwner.Enabled = false;

                DbInterpreter dbInterpreter = this.GetDbInterpreter();

                SchemaInfoFilter filter = new SchemaInfoFilter() { Strict = true, TableNames = new string[] { this.displayInfo.Name } };
                filter.DatabaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.TableColumn | DatabaseObjectType.TablePrimaryKey;

                SchemaInfo schemaInfo = await dbInterpreter.GetSchemaInfoAsync(filter);

                Table table = schemaInfo.Tables.FirstOrDefault();

                if (table != null)
                {
                    this.txtTableName.Text = table.Name;
                    this.cboOwner.Text = table.Owner;
                    this.txtTableComment.Text = table.Comment;

                    List<TableColumnDesingerInfo> columnDesingerInfos = new List<TableColumnDesingerInfo>();

                    foreach (TableColumn column in schemaInfo.TableColumns)
                    {
                        TableColumnDesingerInfo columnDesingerInfo = new TableColumnDesingerInfo()
                        {
                            OldName = column.Name,
                            IsPrimary = schemaInfo.TablePrimaryKeys.Any(item => item.ColumnName == column.Name),
                            Length = dbInterpreter.GetColumnDataLength(column)
                        };

                        ObjectHelper.CopyProperties(column, columnDesingerInfo);

                        columnDesingerInfos.Add(columnDesingerInfo);
                    }                   

                    this.ucColumns.LoadColumns(schemaInfo.Tables.First(), columnDesingerInfos, schemaInfo.TablePrimaryKeys);
                }
                else
                {
                    MessageBox.Show("Table is not existed");
                }
            }
        }

        private async void LoadDatabaseOwners()
        {
            DbInterpreter dbInterpreter = this.GetDbInterpreter();

            List<string> items = new List<string>();
            string defaultItem = null;

            if (this.displayInfo.DatabaseType == DatabaseType.SqlServer)
            {
                List<DatabaseOwner> owners = await dbInterpreter.GetDatabaseOwnersAsync();

                items.AddRange(owners.Select(item => item.Name));

                defaultItem = "dbo";
            }
            else if (this.displayInfo.DatabaseType == DatabaseType.Oracle)
            {
                this.lblOwner.Text = "Tablespace";
                items.Add(dbInterpreter.ConnectionInfo.Database);
            }
            else if (this.displayInfo.DatabaseType == DatabaseType.MySql)
            {
                items.Add(dbInterpreter.ConnectionInfo.Database);
            }

            cboOwner.Items.AddRange(items.ToArray());

            if (cboOwner.Items.Count == 1)
            {
                cboOwner.SelectedIndex = 0;
            }
            else
            {
                if (defaultItem != null)
                {
                    cboOwner.Text = defaultItem;
                }
            }
        }

        public void Show(DatabaseObjectDisplayInfo displayInfo)
        {
            this.displayInfo = displayInfo;
            this.ucColumns.DatabaseType = displayInfo.DatabaseType;

            this.InitControls();
        }

        private DbInterpreter GetDbInterpreter()
        {
            DbInterpreter dbInterpreter = DbInterpreterHelper.GetDbInterpreter(this.displayInfo.DatabaseType, this.displayInfo.ConnectionInfo, new DbInterpreterOption());

            return dbInterpreter;
        }

        private SchemaDesignerInfo GetSchemaDesingerInfo()
        {
            SchemaDesignerInfo schemaDesingerInfo = new SchemaDesignerInfo();

            TableDesignerInfo tableDesignerInfo = new TableDesignerInfo()
            {
                Name = this.txtTableName.Text.Trim(),
                Owner = this.cboOwner.Text.Trim(),
                Comment = this.txtTableComment.Text.Trim(),
                OldName = this.displayInfo.DatabaseObject?.Name
            };

            schemaDesingerInfo.TableDesignerInfo = tableDesignerInfo;

            var columns = this.ucColumns.GetColumns();

            columns.ForEach(item => { item.Owner = tableDesignerInfo.Owner; item.TableName = tableDesignerInfo.Name; });

            schemaDesingerInfo.TableColumnDesingerInfos.AddRange(columns);

            return schemaDesingerInfo;
        }

        public ContentSaveResult Save(ContentSaveInfo info)
        {
            this.ucColumns.EndEdit();

            ContentSaveResult result = Task.Run(() => this.SaveTable()).Result;

            if (!result.IsOK)
            {
                MessageBox.Show(result.Message);
            }
            else
            {
                Table table = result.ResultData as Table;               

                this.displayInfo.DatabaseObject = table;
                this.ucColumns.OnSaved();

                if(this.displayInfo.IsNew || table.Name != this.displayInfo.Name)
                {
                    if (FormEventCenter.OnRefreshNavigatorFolder != null)
                    {
                        FormEventCenter.OnRefreshNavigatorFolder();
                    }
                }                
            }

            return result;
        }

        private async Task<ContentSaveResult> SaveTable()
        {
            SchemaDesignerInfo schemaDesignerInfo = this.GetSchemaDesingerInfo();

            DbInterpreter dbInterpreter = this.GetDbInterpreter();

            TableManager tableManager = new TableManager(dbInterpreter);

            return await tableManager.Save(schemaDesignerInfo, this.displayInfo.IsNew);
        }
    }
}
