using System;
using System.Text;

namespace DatabaseInterpreter.Utility
{
    public class ExceptionHelper
    {
        public static string GetExceptionDetails(Exception ex)
        {
            while (ex.InnerException != null)
            {
                return GetExceptionDetails(ex.InnerException);
            }
            
            return  ex.Message + Environment.NewLine + ex.StackTrace;
        }
    }
}
