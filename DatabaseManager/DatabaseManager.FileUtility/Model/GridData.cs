using System.Collections.Generic;
using System.Drawing;

namespace DatabaseManager.FileUtility.Model
{
    public class GridData
    {
        public List<GridColumn> Columns { get; set; }
        public List<GridRow> Rows { get; set; }
    }

    public class GridRow
    {
        public int RowIndex { get; set; }
        
        public List<GridCell> Cells { get; set; }

        public List<GridRowComment> Comments { get; set; }
    }

    public class GridRowComment
    {
        public string Comment { get; set; }
        public List<int> ColumnIndexes { get; set; }
    }

    public class GridCell
    {
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; }
        public object Content { get; set; }
        public string Comment { get; set; }
        public bool NeedHighlight { get; set; }
    }
}
