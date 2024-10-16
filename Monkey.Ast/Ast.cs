﻿using System.Text;
using LanguageExt;
using Microsoft.VisualBasic;
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
    public IExpression Expression { get; set; }
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

public class BlockStatement : IStatement
{
    public Token Token;
    public List<IStatement> Statements { get; set; }

    public BlockStatement(Token tok)
    {
        Token = tok;
    }

    public void StatementNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        var Out = new StringBuilder();
        foreach (IStatement s in Statements)
        {
            Out.Append(s.String());
        }
        return Out.ToString();
    }
}

public class IntegerLiteral : IExpression
{
    public Token Token;
    public long Value { get; set; }

    public IntegerLiteral(Token tok, long val)
    {
        Token = tok;
        Value = val;
    }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String() { return Token.Literal; }
}

public class StringLiteral : IExpression
{
    public Token Token;
    public string Value { get; set; }

    public StringLiteral(Token tok, string val)
    {
        Token = tok;
        Value = val;
    }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String() { return Token.Literal; }
}


public class PrefixExpression : IExpression
{
    public Token Token;
    public string Operator;
    public IExpression? Right { get; set; }

    public PrefixExpression(Token tok, string op)
    {
        Token = tok;
        Operator = op;
    }

    public PrefixExpression(Token tok, string op, IExpression right)
    {
        Token = tok;
        Operator = op;
        Right = right;
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        string rightStr = Right != null ? Right.String() : "";
        return $"({Operator}{rightStr})";
    }
}

public class InfixExpression : IExpression
{
    public Token Token;
    public string Operator;
    public IExpression Left { get; set; }
    public IExpression? Right { get; set; }

    public InfixExpression(Token tok, string op, IExpression left)
    {
        Token = tok;
        Operator = op;
        Left = left;
    }

    public InfixExpression(Token tok, string op, IExpression left, IExpression right)
    {
        Token = tok;
        Operator = op;
        Left = left;
        Right = right;
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        string rightStr = Right != null ? Right.String() : "";
        return $"({Left.String()}{Operator}{rightStr})";
    }
}

public class Boolean : IExpression
{
    public Token Token;
    public bool Value;

    public Boolean(Token tok, bool value)
    {
        Token = tok;
        Value = value;
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String() { return Token.Literal; }
}

public class IfExpression : IExpression
{
    public Token Token;
    public IExpression Condition { get; set; }
    public BlockStatement Consequence { get; set; }
    public Option<BlockStatement> Alternative = Option<BlockStatement>.None;

    public IfExpression(Token tok)
    {
        Token = tok;
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {

        var Out = new StringBuilder();
        Out.Append("if ");
        Out.Append(Condition.String());
        Out.Append(' ');
        Out.Append(Consequence.String());
        Alternative.Match(
            Some: x =>
            {
                Out.Append(" else ");
                Out.Append(x.String());
            },
            None: () => { Out.Append(""); }
            );
        return Out.ToString();
    }
}

public class FunctionLiteral : IExpression
{
    public Token Token;
    public Identifier[] Parameters { get; set; }
    public BlockStatement Body { get; set; }

    public FunctionLiteral(Token tok)
    {
        Token = tok;
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        var Out = new StringBuilder();
        Out.Append(TokenLiteral());
        Out.Append('(');
        foreach (var p in Parameters)
        {
            Out.Append($"{p.String()}, ");
        }
        Out.Append(')');
        Out.Append(Body.String());
        return Out.ToString();
    }
}

public class CallExpression : IExpression
{
    public Token Token { get; set; }
    public IExpression Function { get; set; }
    public IExpression[] Args { get; set; }

    public CallExpression(Token tok, IExpression fn)
    {
        Token = tok;
        Function = fn;
    }

    public void ExpressionNode()
    { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        var Out = new StringBuilder();
        var args = new List<string> { };
        foreach (var arg in Args)
        {
            args.Add(arg.String());
        }
        Out.Append(Function.String());
        Out.Append('(');
        Out.Append(Strings.Join(args.ToArray()));
        Out.Append(')');
        return Out.ToString();
    }
}

public class ArrayLiteral : IExpression
{
    public Token Token;
    public IExpression[] Elements { get; set; }

    public ArrayLiteral(Token tok)
    {
        Token = tok;
    }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        var Out = new StringBuilder();
        var elements = new List<string>();
        foreach (var el in Elements)
        {
            elements.Add(el.String());
        }
        return Out.ToString();
    }
}

public class IndexExpression : IExpression
{
    public Token Token;
    public IExpression left;
    public IExpression index;

    public IndexExpression(Token tok, IExpression l)
    {
        Token = tok;
        left = l;
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        var Out = new StringBuilder();
        Out.Append('(');
        Out.Append(left.String());
        Out.Append('[');
        Out.Append(index.String());
        Out.Append("])");
        return Out.ToString();
    }
}

public class HashLiteral : IExpression
{
    public Token Token;
    public Dictionary<IExpression, IExpression> Pairs;

    public HashLiteral(Token tok)
    {
        Token = tok;
        Pairs = new Dictionary<IExpression, IExpression>();
    }

    public void ExpressionNode() { }
    public string TokenLiteral() { return Token.Literal; }
    public string String()
    {
        var Out = new StringBuilder();
        var pairs = new List<string>();
        foreach (KeyValuePair<IExpression, IExpression> entry in Pairs)
        {
            pairs.Add($"{entry.Key.String()}:{entry.Value.String()}");
        }
        Out.Append('{');
        Out.Append(Strings.Join(pairs.ToArray(), ", "));
        Out.Append('}');
        return Out.ToString();
    }
}
