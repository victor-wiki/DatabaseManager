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
        public GridCellHighlightMode HighlightMode { get; set; } = GridCellHighlightMode.None;
    }

    public enum GridCellHighlightMode
    {
        None = 0,
        Background = 2,
        Foreground = 4
    }
}
