using Antlr4.Runtime;
using SqlAnalyser.Model;
using System.IO;

namespace SqlAnalyser.Core
{
    public class SqlSyntaxErrorListener : BaseErrorListener
    {
        public bool HasError => this.Error != null && this.Error.Items.Count > 0;
        public SqlSyntaxError Error { get; private set; }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            if (this.Error == null)
            {
                this.Error = new SqlSyntaxError();
            }

            if (offendingSymbol is CommonToken token)
            {
                SqlSyntaxErrorItem errorItem = new SqlSyntaxErrorItem();

                errorItem.StartIndex = token.StartIndex;
                errorItem.StopIndex = token.StopIndex;
                errorItem.Line = token.Line;
                errorItem.Column = token.Column + 1;
                errorItem.Text = token.Text;
                errorItem.Message = msg;

                this.Error.Items.Add(errorItem);
            }

            base.SyntaxError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e);
        }
    }
}
