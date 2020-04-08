using Antlr4.Runtime;
using System.IO;

public abstract class PlSqlLexerBase : Lexer
{
    public PlSqlLexerBase(ICharStream input)
        : base(input)
    {
    }

    public PlSqlLexerBase(ICharStream input, TextWriter output, TextWriter errorOutput)
    :base(input, output, errorOutput)
    {

    }

    protected bool IsNewlineAtPos(int pos)
    {
        int la = InputStream.LA(pos);
        return la == -1 || la == '\n';
    }
}