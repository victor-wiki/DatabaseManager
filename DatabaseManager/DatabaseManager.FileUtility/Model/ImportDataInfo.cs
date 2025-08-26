using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseManager.FileUtility
{
    public class ImportDataInfo
    {
        public string FilePath { get; set; }
        public bool FirstRowIsColumnName { get; set; }
    }
}
