using System;
using System.Collections.Generic;
using System.Text;

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
