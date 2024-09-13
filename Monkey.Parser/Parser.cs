using Monkey.Interpreter.Lexing;
using Monkey.Ast;
using LanguageExt;
using LanguageExt.Common;
namespace Monkey.Parser;
public class Parser
{
    public const int LOWEST = 1;
    public const int EQUALS = 2;
    public const int LESSGREATER = 3;
    public const int SUM = 4;
    public const int PRODUCT = 5;
    public const int PREFIX = 6;
    public const int CALL = 7;

    Lexer _lexer;
    Token curToken;
    Token peekToken;
    List<Error> errors { get; }
    Dictionary<string, Func<Option<IExpression>>> prefixParseFns;
    Dictionary<string, Func<IExpression, IExpression>> infixParseFns;

    public Parser(Lexer l)
    {
        _lexer = l;
        errors = new List<Error> { };
        prefixParseFns = new Dictionary<string, Func<Option<IExpression>>> { };
        infixParseFns = new Dictionary<string, Func<IExpression, IExpression>> { };
        // Call NextToken twice to populate curToken and peekToken fields
        NextToken();
        NextToken();

        RegisterPrefix(Token.IDENT, ParseIdentifier);
        RegisterPrefix(Token.BANG, ParsePre)
    }

    private void NextToken()
    {
        curToken = peekToken;
        peekToken = _lexer.NextToken();
    }

    public Program ParseProgram()
    {
        var program = new Program(new List<IStatement> { });

        while (curToken.Type != Token.EOF)
        {
            var stmt = ParseStatement();
            stmt.Match(
                Some: x => program.Statements.Add(x),
                None: () => Console.WriteLine("Error parsing statement") // Probably remove this
            );
            NextToken();
        }
        return program;
    }

    private Option<IStatement> ParseStatement()
    {
        return curToken.Type switch
        {
            Token.LET => ParseLetStatement(),
            Token.RETURN => ParseReturnStatement(),
            _ => ParseExpressionStatement(), //Default case
        };
    }

    private Option<IStatement> ParseExpressionStatement()
    {
        var stmt = new ExpressionStatement(curToken);
        // TODO: This feels so illegal but for now it'll do I guess
        stmt.Expression = ParseExpression(LOWEST).MatchUnsafe(
                Some: x => x,
                None: (IExpression?)null
                );

        if (PeekTokenIs(Token.SEMICOLON))
        {
            NextToken();
        }
        return stmt;
    }

    private Option<IStatement> ParseReturnStatement()
    {
        var stmt = new ReturnStatement(curToken);

        //TODO: Skipping expressions until we encounter a semicolon currently
        while (!CurTokenIs(Token.SEMICOLON))
        {
            NextToken();
        }
        return Option<IStatement>.Some(stmt);
    }

    private Option<IStatement> ParseLetStatement()
    {
        var stmt = new LetStatement(curToken);

        if (!ExpectPeek(Token.IDENT))
        {
            return Option<IStatement>.None;
        }

        stmt.Name = new Identifier(curToken, curToken.Literal);

        if (!ExpectPeek(Token.ASSIGN))
        {
            return Option<IStatement>.None;
        }

        // TODO: Skipping the expressions currently until we find the semicolon.
        while (!CurTokenIs(Token.SEMICOLON))
        {
            NextToken();
        }

        return Option<IStatement>.Some(stmt);
    }

    private void NoPrefixParseFnError(string tokenType)
    {
        errors.Add(new Error($"No prefix parse fn for {tokenType} found"));
    }

    private Option<IExpression> ParseExpression(int precedence)
    {
        var prefix = prefixParseFns.TryGetValue(Key: curToken.Type);
        return prefix.Match(
                Some: prefixFn => prefixFn().Match(
                        Some: x => Option<IExpression>.Some(x),
                        None: () => { NoPrefixParseFnError(curToken.Type); return Option<IExpression>.None; }),
                None: () => { NoPrefixParseFnError(curToken.Type); return Option<IExpression>.None; });
    }

    private bool CurTokenIs(string tokenType)
    {
        return curToken.Type == tokenType;
    }

    private bool PeekTokenIs(string tokenType)
    {
        return peekToken.Type == tokenType;
    }

    private bool ExpectPeek(string tokenType)
    {
        if (PeekTokenIs(tokenType))
        {
            NextToken();
            return true;
        }
        else
        {
            PeekError(tokenType);
            return false;
        }
    }

    private void PeekError(string tokenType)
    {
        string errMsg = string.Format("expected the next token to be {0}, got {1} instead", tokenType, peekToken.Type);
        var newErr = new Error(errMsg);
        errors.Add(newErr);
    }

    private void RegisterPrefix(string tokenType, Func<Option<IExpression>> fn)
    {
        prefixParseFns.Add(tokenType, fn);
    }

    private void RegisterInfix(string tokenType, Func<IExpression, IExpression> fn)
    {
        infixParseFns.Add(tokenType, fn);
    }

    private Option<IExpression> ParseIdentifier()
    {
        return new Identifier(curToken, curToken.Literal);
    }

    private Option<IExpression> ParseIntegerLiteral()
    {
        var lit = new IntegerLiteral(curToken);
        if (!long.TryParse(curToken.Literal, out long Out))
        {
            errors.Add(new Error("Failed to parse integer"));
            return Option<IExpression>.None;
        }
        lit.Value = Out;
        return lit;
    }

    private Option<IExpression> ParsePrefixExpression()
    {
        var expression = new PrefixExpression(curToken, curToken.Literal);
        NextToken();
        ParseExpression(PREFIX).Match(
                Some: x => { expression.Right = x; return; },
                None: () => { return; }
                );
        return expression;
    }
}
