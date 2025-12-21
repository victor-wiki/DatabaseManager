using DatabaseInterpreter.Model;
using System.Collections.Generic;
using System.Data;

namespace DatabaseManager.Core.Model
{
    public class DataCompareResult
    {
        public List<DataCompareResultDetail> Details { get; set; } = new List<DataCompareResultDetail>();
    }

    public class DataCompareResultDetail
    {
        public Table SourceTable { get; set; }
        public Table TargetTable { get; set; }
        public long SourceTableRecordCount { get; set; }
        public long TargetTableRecordCount { get; set; }
        public List<TableColumn> KeyColumns { get; set; } = new List<TableColumn>();
        public List<TableColumn> SourceTableColumns { get; set; } = new List<TableColumn>();
        public List<TableColumn> TargetTableColumns { get; set; } = new List<TableColumn>();
        public List<TableColumn> SameTableColumns { get; set; } = new List<TableColumn>();

        public List<DataRow> OnlyInSourceKeyRows { get; set; } = new List<DataRow>();
        public List<DataRow> OnlyInTargetKeyRows { get; set; } = new List<DataRow>();
        public List<DataRow> IdenticalKeyRows { get; set; } = new List<DataRow>();
        public List<DataRow> DifferentKeyRows { get; set; } = new List<DataRow>();

        public int KeyColumnsCount => this.KeyColumns.Count;
        public bool HasDifferent => this.DifferentKeyRows.Count > 0;
        public bool IsIndentical => this.SourceTableRecordCount > 0 && !this.HasDifferent && this.OnlyInSourceKeyRows.Count == 0 && this.OnlyInTargetKeyRows.Count == 0;
    
        public int DifferentCount => this.DifferentKeyRows.Count;
        public int IdenticalCount => this.IdenticalKeyRows.Count;
        public int OnlyInSourceCount => this.OnlyInSourceKeyRows.Count;
        public int OnlyInTargetCount => this.OnlyInTargetKeyRows.Count;    
        public int Order { get; set; }
    }
}
