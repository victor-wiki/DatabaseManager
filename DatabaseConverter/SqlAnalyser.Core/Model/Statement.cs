using System.Collections.Generic;

namespace SqlAnalyser.Model
{
    public class Statement
    {
    }

    public class SetStatement : Statement
    {
        public TokenInfo Key { get; set; }
        public TokenInfo Value { get; set; }
    }

    public class WhileStatement : Statement
    {
        public TokenInfo Condition { get; set; }

        public List<Statement> Statements { get; set; } = new List<Statement>();
    }

    public class ReturnStatement : Statement
    {
        public TokenInfo Value { get; set; }
    }

    public class DeclareStatement : Statement
    {
        public TokenInfo Name { get; set; }
        public TokenInfo DataType { get; set; }
        public DeclareType Type { get; set; }
        public TemporaryTable Table { get; set; }
    }

    public class TemporaryTable
    {
        public TokenInfo Name { get; set; }
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
    }

    public enum DeclareType
    {
        Variable = 0,
        Table = 1,
        Cursor = 2
    }

    public class IfStatement : Statement
    {
        public List<IfStatementItem> Items { get; set; } = new List<IfStatementItem>();
    }

    public class IfStatementItem
    {
        public IfStatementType Type { get; set; }
        public TokenInfo Condition { get; set; }
        public List<Statement> Statements { get; set; } = new List<Statement>();
    }

    public enum IfStatementType
    {
        IF = 0,
        ELSEIF = 1,
        ELSE = 2
    }

    public class SelectStatement : Statement
    {
        public List<TokenInfo> Columns { get; set; } = new List<TokenInfo>();
        public TokenInfo IntoTableName { get; set; }
        public TokenInfo TableName { get; set; }
        public TokenInfo Condition { get; set; }
        public List<SelectStatement> UnionStatements { get; set; }
        public List<WithStatement> WithStatements { get; set; }
        public TokenInfo OrderBy { get; set; }
        public TokenInfo Option { get; set; }
    }

    public class WithStatement : Statement
    {
        public TokenInfo Name { get; set; }
        public List<TokenInfo> Columns { get; set; } = new List<TokenInfo>();
        public List<SelectStatement> SelectStatements { get; set; }
    }

    public class InsertStatement : Statement
    {
        public TokenInfo TableName { get; set; }
        public List<TokenInfo> Columns { get; set; } = new List<TokenInfo>();
        public List<TokenInfo> Values { get; set; } = new List<TokenInfo>();
        public List<SelectStatement> SelectStatements { get; set; }
    }

    public class UpdateStatement : Statement
    {
        public List<TokenInfo> TableNames { get; set; } = new List<TokenInfo>();
        public List<NameValueItem> SetItems { get; set; } = new List<NameValueItem>();
        public List<UpdateFromItem> FromItems { get; set; }
        public TokenInfo Condition { get; set; }
        public TokenInfo Option { get; set; }
    }

    public class UpdateFromItem
    {
        public TokenInfo TableName { get; set; }
        public List<TokenInfo> Joins { get; set; } = new List<TokenInfo>();
    }

    public class DeleteStatement : Statement
    {
        public TokenInfo TableName { get; set; }
        public TokenInfo Condition { get; set; }
    }

    public class TryCatchStatement : Statement
    {
        public List<Statement> TryStatements { get; set; } = new List<Statement>();
        public List<Statement> CatchStatements { get; set; } = new List<Statement>();
    }

    public class PrintStatement : Statement
    {
        public TokenInfo Content { get; set; }
    }

    public class ExecuteStatement : Statement
    {
        public TokenInfo Content { get; set; }
    }

    public class TransactionStatement : Statement
    {
        public TransactionCommandType CommandType { get; set; }
        public TokenInfo Content { get; set; }
    }

    public class LeaveStatement:Statement
    {
        public TokenInfo Content { get; set; }
    }

    public enum TransactionCommandType
    {
        BEGIN,
        COMMIT,
        ROLLBACK
    }

    public class NameValueItem
    {
        public TokenInfo Name { get; set; }
        public TokenInfo Value { get; set; }
    }
}
