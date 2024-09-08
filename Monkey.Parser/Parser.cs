using Monkey.Interpreter.Lexing;
using Monkey.Ast;
using LanguageExt;
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
    Dictionary<string, Func<IExpression>> prefixParseFns;
    Dictionary<string, Func<IExpression, IExpression>> infixParseFns;

    public Parser(Lexer l)
    {
        _lexer = l;
        errors = new List<Error> { };
        // Call NextToken twice to populate curToken and peekToken fields
        NextToken();
        NextToken();
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
        stmt.Expression = ParseExpression(LOWEST);

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

    private Option<IExpression> ParseExpression(int precedence)
    {
        var prefix = prefixParseFns.TryGetValue(Key: curToken.Type);
        return prefix.Match(
                None: Option<IExpression>.None,
                Some: prefixFn => Option<IExpression>.Some(prefixFn())
                );
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

    private void RegisterPrefix(string tokenType, Func<IExpression> fn)
    {
        prefixParseFns.Add(tokenType, fn);
    }

    private void RegisterInfix(string tokenType, Func<IExpression, IExpression> fn)
    {
        infixParseFns.Add(tokenType, fn);
    }
}
