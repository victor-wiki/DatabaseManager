using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseInterpreter.Model
{
    public class DatabaseObject
    {
        public string Owner { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
    }
}
