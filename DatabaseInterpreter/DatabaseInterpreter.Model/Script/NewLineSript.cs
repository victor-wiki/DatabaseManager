using System;

namespace DatabaseInterpreter.Model
{
    public class NewLineScript : Script
    {
        public NewLineScript():base()
        {
            this.Content = Environment.NewLine;
        }
    }
}
