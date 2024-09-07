using Monkey.Interpreter.Lexing;

namespace Monkey.Ast;
public interface INode
{
    string TokenLiteral();
}

public interface IStatement : INode
{
    void StatementNode();
}

public interface IExpression : INode
{
    void ExpressionNode();
}

public class Program : INode
{
    public IStatement[] Statements;

    public Program(IStatement[] statements)
    {
        Statements = statements;
    }

    public string TokenLiteral()
    {
        if (Statements.Length > 0)
        {
            return Statements[0].TokenLiteral();
        }
        else
        {
            return "";
        }
    }
}

public class LetStatement : IStatement
{
    Token Token;
    Identifier Name;
    IExpression Value;

    public void StatementNode() { }
    public string TokenLiteral() { return Token.Literal; }
}

public class Identifier : IExpression
{
    Token Token;
    string Value;

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
}



