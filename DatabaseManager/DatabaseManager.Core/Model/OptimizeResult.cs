using System.Collections.Generic;

namespace DatabaseManager.Core.Model
{
    public class OptimizeResult:OperateResult
    {
        public List<OptimizeResultDetail> Details { get; set; }
    }

    public class OptimizeResultDetail
    {
        public string ObjectType { get; set; }
        public string ObjectName { get; set; }
        public bool IsOK { get; set; }
        public decimal DataLengthBeforeOptimization { get; set; }
        public decimal DataLengthAfterOptimization { get; set; }
        public string Message { get; set; }
    }
}
