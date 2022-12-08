using System;

namespace DatabaseInterpreter.Core
{
    public class DbCommandException : Exception
    {
        public Exception BaseException { get; internal set; }

        public string CustomMessage { get; internal set; }

        public bool HasRollbackedTransaction { get; internal set;  }     

        public DbCommandException(Exception ex)
        {
            this.BaseException = ex;
        }

        public DbCommandException(Exception ex, string msg)
        {
            this.BaseException = ex;
            this.CustomMessage = msg;
        }

        public override string Message
        {
            get
            {
                return $"{this.BaseException?.Message}{Environment.NewLine}{this.CustomMessage}";
            }
        }
    }
}
