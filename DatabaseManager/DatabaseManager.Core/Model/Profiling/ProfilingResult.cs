using System;
using System.Collections.Generic;

namespace DatabaseManager.Core.Model
{
    public class ProfilingResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Started { get; set; }
        public decimal Duration { get; set; }
        public List<ProfilingResultDetail> Details { get; set; }
    }

    public class ProfilingResultDetail
    {
        public string Id { get; set; }
        public string Sql { get; set; }
        public string ExecuteType { get; set; }
        public decimal Duration { get; set; }       
        public bool HasError { get; set; }
    }
}
