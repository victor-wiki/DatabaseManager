using System;

namespace DatabaseManager.Core.Model
{
    public class ProfilingInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Started  { get; set; }
        public decimal DurationMilliseconds { get; set; }
        public string MachineName { get; set; }
        public ProfilingRoot Root { get; set; }
    }

    public class ProfilingRoot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal DurationMilliseconds { get; set; }
        public decimal StartMilliseconds { get; set; }
        public ProfilingCustomTiming CustomTimings { get; set; }
    }

    public class ProfilingCustomTiming
    {
        public ProfilingSql[] sql { get; set; }
    }

    public class ProfilingSql
    {
        public string Id { get; set; }
        public string CommandString { get; set; }
        public string ExecuteType { get; set; }
        public decimal DurationMilliseconds { get; set; }
        public decimal StartMilliseconds { get; set; }
        public bool Errored { get; set; }
    }
}
