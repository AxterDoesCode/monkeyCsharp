using System.Linq.Expressions;
using System.Text;
using Monkey.Interpreter.Lexing;

namespace Monkey.Ast;
public interface INode
{
    string TokenLiteral();
    string String();
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
    public List<IStatement> Statements;

    public Program(List<IStatement> statements)
    {
        Statements = statements;
    }

    public string TokenLiteral()
    {
        if (Statements.Count > 0)
        {
            return Statements[0].TokenLiteral();
        }
        else
        {
            return "";
        }
    }

    public string String()
    {
        var Out = new StringBuilder();
        Statements.ForEach(s => Out.Append(s.String()));
        return Out.ToString();
    }
}

public class LetStatement : IStatement
{
    public Token Token { get; set; }
    public Identifier Name { get; set; }
    public IExpression Value { get; set; }

    public LetStatement(Token tok)
    {
        Token = tok;
    }

    public void StatementNode() { }
    public string TokenLiteral() { return Token.Literal; }

    public string String()
    {
        var Out = new StringBuilder();
        Out.Append(TokenLiteral() + " ");
        Out.Append(Name.String());
        Out.Append(" = ");
        if (Value != null)
        { // need to eventually remove null checks
            Out.Append(Value.String());
        }
        Out.Append(';');
        return Out.ToString();
    }
}

public class Identifier : IExpression
{
    public Token Token;
    public string Value;

    public Identifier(Token tok, string value)
    {
        Token = tok;
        Value = value;
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        return Value;
    }
}

public class ReturnStatement : IStatement
{
    public Token Token;
    public IExpression ReturnValue;

    public ReturnStatement(Token tok)
    {
        Token = tok;
    }

    public void StatementNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        var Out = new StringBuilder();
        Out.Append(TokenLiteral() + " ");

        if (ReturnValue != null)
        { // Will remove null check once we can eval expressions
            Out.Append(ReturnValue.String());
        }
        Out.Append(';');
        return Out.ToString();
    }
}

public class ExpressionStatement : IStatement
{
    public Token Token; // The first token of the expression
    public IExpression Expression;
    public ExpressionStatement(Token tok)
    {
        Token = tok;
    }

    public void StatementNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        if (Expression != null)
        {
            return Expression.String();
        }
        return "";
    }
}

public class IntegerLiteral : IExpression
{
    public Token Token;
    public long Value { get; set; }

    public IntegerLiteral(Token tok)
    {
        Token = tok;
    }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String() { return Token.Literal; }
}

public class PrefixExpression : IExpression
{
    public Token Token;
    public string Operator;
    public IExpression Right { get; set; }

    public PrefixExpression(Token tok, string op)
    {
        Token = tok;
        Operator = op;
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        return $"({Operator}{Right.String()})";
    }
}

public class InfixExpression : IExpression
{
    public Token Token;
    public string Operator;
    public IExpression Right { get; set; }
    public IExpression Left { get; set; }

    public InfixExpression(Token tok, string op, IExpression left, IExpression right) {
        Token = tok;
        Operator = op;
        Left = left;
        Right = right;
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        return $"({Left.String()}{Operator}{Right.String()})";
    }
}
