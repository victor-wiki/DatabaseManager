using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseInterpreter.Model
{
    public class TableConstraint: DatabaseObject
    {
        public string TableName { get; set; }
        public string Definition { get; set; }
    }
}
