using System;

namespace DatabaseInterpreter.Model
{
    public class NewLineSript : Script
    {
        public NewLineSript():base()
        {
            this.Content = Environment.NewLine;
        }
    }
}
