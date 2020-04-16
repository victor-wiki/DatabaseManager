using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SqlAnalyser.Core
{
    public class SqlSyntaxErrorListener : BaseErrorListener
    {
        public bool HasError { get; private set; }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            this.HasError = true;
            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
        }
    }
}
