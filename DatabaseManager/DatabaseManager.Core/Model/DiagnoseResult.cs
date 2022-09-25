using System;
using System.Collections.Generic;
using System.Text;
using DatabaseInterpreter.Model;

namespace DatabaseManager.Model
{
    public class DiagnoseResult
    {        
        public List<DiagnoseResultItem> Details { get; set; } = new List<DiagnoseResultItem>();      
    }

    public class DiagnoseResultItem
    {
        public DatabaseObject DatabaseObject { get; set; }
        public int RecordCount { get; set; }
        public string Sql { get; set; }
    }

    public enum DiagnoseType
    {
        None = 0,
        NotNullWithEmpty = 1,
        SelfReferenceSame = 2,
        WithLeadingOrTrailingWhitespace = 3
    }
}
