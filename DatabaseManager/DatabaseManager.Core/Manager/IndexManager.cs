using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DatabaseManager.Model;
using DatabaseInterpreter.Model;
using DatabaseInterpreter.Utility;


namespace DatabaseManager.Core
{
    public class IndexManager
    {
        public static string GetPrimaryKeyDefaultName(Table table)
        {
            return $"PK_{table.Name}";
        }

        public static string GetIndexDefaultName(string indexType, Table table)
        {
            return $"{(indexType == nameof(IndexType.Unique)? "UK":"IX")}_{table.Name}";
        }

        public static string GetForeignKeyDefaultName(string tableName, string referencedTableName)
        {
            return $"FK_{referencedTableName}_{tableName}";
        }

        public static List<TableIndexDesignerInfo> GetIndexDesignerInfos(DatabaseType databaseType, List<TableIndex> indexes)
        {
            List<TableIndexDesignerInfo> indexDesignerInfos = new List<TableIndexDesignerInfo>();
            
            foreach (var index in indexes)
            {
                TableIndexDesignerInfo indexDesignerInfo = new TableIndexDesignerInfo();

                indexDesignerInfo.OldName = indexDesignerInfo.Name = index.Name;
                indexDesignerInfo.IsPrimary = index.IsPrimary;
                indexDesignerInfo.OldType = index.Type;
                indexDesignerInfo.Comment = index.Comment;               

                string type = index.Type;

                if (!string.IsNullOrEmpty(type))
                {
                    indexDesignerInfo.Type = type;
                }

                if (index.IsPrimary)
                {
                    if(databaseType == DatabaseType.Oracle)
                    {
                        indexDesignerInfo.Type = IndexType.Unique.ToString();
                    }
                    else
                    {
                        indexDesignerInfo.Type = IndexType.Primary.ToString();
                    }             

                    if (indexDesignerInfo.ExtraPropertyInfo == null)
                    {
                        indexDesignerInfo.ExtraPropertyInfo = new TableIndexExtraPropertyInfo();
                    }

                    indexDesignerInfo.ExtraPropertyInfo.Clustered = index.Clustered;
                }
                else if (index.IsUnique)
                {
                    indexDesignerInfo.Type = IndexType.Unique.ToString();
                }
                else if(string.IsNullOrEmpty(index.Type))
                {
                    indexDesignerInfo.Type = IndexType.Normal.ToString();
                }

                if (string.IsNullOrEmpty(indexDesignerInfo.OldType) && !string.IsNullOrEmpty(indexDesignerInfo.Type))
                {
                    indexDesignerInfo.OldType = indexDesignerInfo.Type;
                }

                indexDesignerInfo.Columns.AddRange(index.Columns);

                indexDesignerInfos.Add(indexDesignerInfo);
            }

            return indexDesignerInfos;
        }

        public static List<TableForeignKeyDesignerInfo> GetForeignKeyDesignerInfos(List<TableForeignKey> foreignKeys)
        {
            List<TableForeignKeyDesignerInfo> foreignKeyDesignerInfos = new List<TableForeignKeyDesignerInfo>();

            foreach(TableForeignKey foreignKey in foreignKeys)
            {
                TableForeignKeyDesignerInfo keyDesignerInfo = new TableForeignKeyDesignerInfo();

                ObjectHelper.CopyProperties(foreignKey, keyDesignerInfo);

                keyDesignerInfo.OldName = foreignKey.Name;              
                keyDesignerInfo.Columns = foreignKey.Columns;               

                foreignKeyDesignerInfos.Add(keyDesignerInfo);
            }

            return foreignKeyDesignerInfos;
        }

        public static List<TableConstraintDesignerInfo> GetConstraintDesignerInfos(List<TableConstraint> constraints)
        {
            List<TableConstraintDesignerInfo> constraintDesignerInfos = new List<TableConstraintDesignerInfo>();

            foreach (TableConstraint constraint in constraints)
            {
                TableConstraintDesignerInfo constraintDesignerInfo = new TableConstraintDesignerInfo();

                ObjectHelper.CopyProperties(constraint, constraintDesignerInfo);

                constraintDesignerInfo.OldName = constraint.Name;               

                constraintDesignerInfos.Add(constraintDesignerInfo);
            }

            return constraintDesignerInfos;
        }
    }
}
