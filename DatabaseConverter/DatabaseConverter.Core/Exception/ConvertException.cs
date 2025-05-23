﻿using System;
using System.Text;

namespace DatabaseConverter.Core
{
    public abstract class ConvertException : Exception
    {
        public Exception BaseException { get; set; }
        public abstract string ObjectType { get; }

        public ConvertException(Exception ex)
        {
            this.BaseException = ex;            
        }

        public string SourceServer { get; set; }
        public string SourceDatabase { get; set; }
        public string SourceObject { get; set; }

        public string TargetServer { get; set; }
        public string TargetDatabase { get; set; }
        public string TargetObject { get; set; }

        public override string Message => BaseException.Message;

        public override string StackTrace
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"ObjectType:{this.ObjectType}");

                if(!string.IsNullOrEmpty(this.SourceServer))
                {
                    sb.AppendLine($"SourceServer:{this.SourceServer}");
                }
               
                if(!string.IsNullOrEmpty(this.SourceDatabase))
                {
                    sb.AppendLine($"SourceDatabase:{this.SourceDatabase}");
                }               

                if(!string.IsNullOrEmpty(this.SourceObject))
                {
                    sb.AppendLine($"SourceObject:{this.SourceObject}");
                }

                if(!string.IsNullOrEmpty(this.TargetServer))
                {
                    sb.AppendLine($"TargetServer:{this.TargetServer}");
                }

                if(!string.IsNullOrEmpty(this.TargetDatabase))
                {
                    sb.AppendLine($"TargetDatabase:{this.TargetDatabase}");
                }                

                if(!string.IsNullOrEmpty(this.TargetObject))
                {
                    sb.AppendLine($"TargetObject:{this.TargetObject}");
                }

                if(!string.IsNullOrEmpty(BaseException?.StackTrace))
                {
                    sb.AppendLine(BaseException?.StackTrace);
                }

                return sb.ToString();
            }
        }
    }    
}
