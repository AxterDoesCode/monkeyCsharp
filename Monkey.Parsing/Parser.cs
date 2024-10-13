using Monkey.Interpreter.Lexing;
using Monkey.Ast;
using LanguageExt;
namespace Monkey.Parsing;
public class Parser
{
    public const int LOWEST = 1;
    public const int EQUALS = 2;
    public const int LESSGREATER = 3;
    public const int SUM = 4;
    public const int PRODUCT = 5;
    public const int PREFIX = 6;
    public const int CALL = 7;
    public const int INDEX = 8;

    Lexer _lexer;
    Token curToken;
    Token peekToken;
    public List<Error> Errors { get; }
    Dictionary<string, Func<Option<IExpression>>> prefixParseFns;
    Dictionary<string, Func<IExpression, Option<IExpression>>> infixParseFns;
    Dictionary<string, int> precedences;
    public Parser(Lexer l)
    {
        _lexer = l;
        Errors = new List<Error> { };
        prefixParseFns = new Dictionary<string, Func<Option<IExpression>>> { };
        infixParseFns = new Dictionary<string, Func<IExpression, Option<IExpression>>> { };
        precedences = new Dictionary<string, int> {
            {Token.EQ, EQUALS},
            {Token.NOT_EQ, EQUALS},
            {Token.LT, LESSGREATER},
            {Token.GT, LESSGREATER},
            {Token.PLUS, SUM},
            {Token.MINUS, SUM},
            {Token.SLASH, PRODUCT},
            {Token.ASTERISK, PRODUCT},
            {Token.LPAREN, CALL},
            {Token.LBRACKET, INDEX}
        };
        // Call NextToken twice to populate curToken and peekToken fields
        NextToken();
        NextToken();

        RegisterPrefix(Token.IDENT, ParseIdentifier);
        RegisterPrefix(Token.INT, ParseIntegerLiteral);
        RegisterPrefix(Token.TRUE, ParseBoolean);
        RegisterPrefix(Token.FALSE, ParseBoolean);
        RegisterPrefix(Token.BANG, ParsePrefixExpression);
        RegisterPrefix(Token.MINUS, ParsePrefixExpression);
        RegisterPrefix(Token.LPAREN, ParseGroupedExpression);
        RegisterPrefix(Token.IF, ParseIfExpression);
        RegisterPrefix(Token.FUNCTION, ParseFunctionLiteral);
        RegisterPrefix(Token.STRING, ParseStringLiteral);
        RegisterPrefix(Token.LBRACKET, ParseArrayLiteral);

        RegisterInfix(Token.PLUS, ParseInfixExpression);
        RegisterInfix(Token.MINUS, ParseInfixExpression);
        RegisterInfix(Token.SLASH, ParseInfixExpression);
        RegisterInfix(Token.ASTERISK, ParseInfixExpression);
        RegisterInfix(Token.EQ, ParseInfixExpression);
        RegisterInfix(Token.NOT_EQ, ParseInfixExpression);
        RegisterInfix(Token.LT, ParseInfixExpression);
        RegisterInfix(Token.GT, ParseInfixExpression);
        RegisterInfix(Token.LPAREN, ParseCallExpression);
        RegisterInfix(Token.LBRACKET, ParseIndexExpression);

    }

    private Option<IExpression> ParseIndexExpression(IExpression left)
    {
        var exp = new IndexExpression(curToken, left);
        NextToken();
        return ParseExpression(LOWEST).Match(
            None: Option<IExpression>.None,
            Some: x =>
            {
                if (!ExpectPeek(Token.RBRACKET))
                {
                    return Option<IExpression>.None;
                }
                exp.index = x;
                return exp;
            }
        );
    }

    private Option<IExpression> ParseArrayLiteral()
    {
        var array = new ArrayLiteral(curToken);
        return ParseExpressionList(Token.RBRACKET).Match(
            None: Option<IExpression>.None,
            Some: x => { array.Elements = x; return array; }
        );
    }

    private Option<IExpression> ParseGroupedExpression()
    {
        NextToken();
        var exp = ParseExpression(LOWEST);
        if (!ExpectPeek(Token.RPAREN))
        {
            return Option<IExpression>.None;
        }
        return exp;
    }

    private Option<IExpression> ParseCallExpression(IExpression function)
    {
        var exp = new CallExpression(curToken, function);
        return ParseExpressionList(Token.RPAREN).Match(
            None: Option<IExpression>.None,
            Some: x => { exp.Args = x; return exp; }
        );
    }

    private Option<IExpression[]> ParseExpressionList(string end)
    {
        var args = new List<IExpression> { };

        if (PeekTokenIs(end))
        {
            NextToken();
            return args.ToArray();
        }

        NextToken();

        return from _ in ParseExpression(LOWEST).Match(
            None: Option<Unit>.None,
            Some: x =>
            {
                args.Add(x);
                while (PeekTokenIs(Token.COMMA))
                {
                    NextToken();
                    NextToken();
                    var returnFlag = false; // Need this flag to break out of while, can't put a return before that match
                    ParseExpression(LOWEST).Match(
                        None: () => returnFlag = true,
                        Some: arg => args.Add(arg)
                    );
                    if (returnFlag)
                    {
                        return Option<Unit>.None;
                    }
                }
                if (!ExpectPeek(end))
                {
                    return Option<Unit>.None;
                }
                return Unit.Default;
            }
        )
               select args.ToArray();
    }

    private Option<IExpression> ParseFunctionLiteral()
    {
        var literal = new FunctionLiteral(curToken);

        if (!ExpectPeek(Token.LPAREN))
        {
            return Option<IExpression>.None;
        }

        return ParseFunctionParameters().Match(
            None: Option<IExpression>.None,
            Some: x =>
            {
                literal.Parameters = x.ToArray();
                if (!ExpectPeek(Token.LBRACE))
                {
                    return Option<IExpression>.None;
                }
                return ParseBlockStatement().Match(
                    None: Option<IExpression>.None,
                    Some: x => { literal.Body = x; return literal; }
                );
            }
        );
    }

    private Option<List<Identifier>> ParseFunctionParameters()
    {
        var identifiers = new List<Identifier>();
        if (PeekTokenIs(Token.RPAREN))
        {
            NextToken();
            return identifiers;
        }

        NextToken();
        var ident = new Identifier(curToken, curToken.Literal);
        identifiers.Add(ident);

        while (PeekTokenIs(Token.COMMA))
        {
            NextToken();
            NextToken();
            ident = new Identifier(curToken, curToken.Literal);
            identifiers.Add(ident);
        }

        if (!ExpectPeek(Token.RPAREN))
        {
            return Option<List<Identifier>>.None;
        }
        return identifiers;
    }

    private Option<IExpression> ParseIfExpression()
    {
        var expression = new IfExpression(curToken);
        if (!ExpectPeek(Token.LPAREN))
        {
            return Option<IExpression>.None;
        }

        NextToken();
        return ParseExpression(LOWEST).Match
            (
                None: Option<IExpression>.None,
                Some: condition =>
                {
                    expression.Condition = condition;

                    if (!ExpectPeek(Token.RPAREN))
                    {
                        return Option<IExpression>.None;
                    }

                    if (!ExpectPeek(Token.LBRACE))
                    {
                        return Option<IExpression>.None;
                    }

                    return ParseBlockStatement().Match
                        (
                            None: Option<IExpression>.None,
                            Some: consequence =>
                            {
                                expression.Consequence = consequence;

                                if (PeekTokenIs(Token.ELSE))
                                {
                                    NextToken();
                                    if (!ExpectPeek(Token.LBRACE))
                                    {
                                        return Option<IExpression>.None;
                                    }
                                    return ParseBlockStatement().Match
                                    (
                                        Some: alternative => { expression.Alternative = alternative; return expression; },
                                        None: Option<IExpression>.None
                                    );
                                }
                                return expression;
                            }
                        );
                }
            );
    }

    private Option<BlockStatement> ParseBlockStatement()
    {
        var block = new BlockStatement(curToken);
        var statements = new List<IStatement>();

        NextToken();
        while (!CurTokenIs(Token.RBRACE) && !CurTokenIs(Token.EOF))
        {
            ParseStatement().Match(
                None: () => { },
                Some: stmt => statements.Add(stmt)
            );
            NextToken();
        }
        // Below is commented out cos then empty functions break
        // if (!statements.Any())
        // {
        //     return Option<BlockStatement>.None;
        // }

        // TODO: Maybe add a check for RBrace here;
        block.Statements = statements;
        return block;
    }

    private Option<IExpression> ParseBoolean()
    {
        return new Ast.Boolean(curToken, CurTokenIs(Token.TRUE));
    }

    private int PeekPrecedence()
    {
        var p = precedences.TryGetValue(Key: peekToken.Type);
        return p.Match(
            Some: x => x,
            None: LOWEST
        );
    }

    private int CurPrecedence()
    {
        var p = precedences.TryGetValue(Key: curToken.Type);
        return p.Match(
            Some: x => x,
            None: LOWEST
        );
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

        return ParseExpression(LOWEST).Match(
            None: Option<IStatement>.None,
            Some: x =>
                {
                    stmt.Expression = x;

                    if (PeekTokenIs(Token.SEMICOLON))
                    {
                        NextToken();
                    }
                    return stmt;
                }
            );
    }

    private Option<IStatement> ParseReturnStatement()
    {
        var stmt = new ReturnStatement(curToken);
        NextToken();
        return ParseExpression(LOWEST).Match(
            None: Option<IStatement>.None,
            Some: x =>
            {
                stmt.ReturnValue = x;
                if (PeekTokenIs(Token.SEMICOLON))
                {
                    NextToken();
                }
                return stmt;
            }
        );
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

        NextToken();

        return ParseExpression(LOWEST).Match(
            None: Option<IStatement>.None,
            Some: x =>
            {
                stmt.Value = x;
                if (PeekTokenIs(Token.SEMICOLON))
                {
                    NextToken();
                }
                return stmt;
            }
        );

    }

    private void NoPrefixParseFnError(string tokenType)
    {
        Errors.Add(new Error($"No prefix parse fn for {tokenType} found"));
    }

    private Option<IExpression> ParseExpression(int precedence)
    {
        var prefix = prefixParseFns.TryGetValue(Key: curToken.Type);
        Option<IExpression> leftExp = Option<IExpression>.None; // Init the leftExp
        prefix.Match(
            Some: x => { leftExp = x(); },
            None: () => { leftExp = Option<IExpression>.None; NoPrefixParseFnError(curToken.Type); }
        );

        // Surely I can fucking combine this into a single match I don't believe it

        if (leftExp.IsNone) { return leftExp; }

        while (!PeekTokenIs(Token.SEMICOLON) && precedence < PeekPrecedence())
        {
            var infix = infixParseFns.TryGetValue(Key: peekToken.Type);
            if (infix.IsNone) { return leftExp; }
            infix.Match(
                Some: x =>
                {
                    NextToken(); leftExp = leftExp.Match(
                    None: Option<IExpression>.None,
                    Some: l => x(l));
                },
                None: () => { } // This will never happen btw
            );
        }

        return leftExp;
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
        Errors.Add(newErr);
    }

    private void RegisterPrefix(string tokenType, Func<Option<IExpression>> fn)
    {
        prefixParseFns.Add(tokenType, fn);
    }

    private void RegisterInfix(string tokenType, Func<IExpression, Option<IExpression>> fn)
    {
        infixParseFns.Add(tokenType, fn);
    }

    private Option<IExpression> ParseIdentifier()
    {
        return new Identifier(curToken, curToken.Literal);
    }

    private Option<IExpression> ParseIntegerLiteral()
    {
        Option<IExpression> lit = Option<IExpression>.None;
        if (!long.TryParse(curToken.Literal, out long Out))
        {
            Errors.Add(new Error("Failed to parse integer"));
            return lit;
        }
        lit = new IntegerLiteral(curToken, Out);
        return lit;
    }

    private Option<IExpression> ParsePrefixExpression()
    {
        Option<IExpression> expression = Option<IExpression>.None;
        var tempCurToken = curToken;
        // Advance token for the right of the prefix expression
        NextToken();
        ParseExpression(PREFIX).Match(
                Some: r => expression = new PrefixExpression(tempCurToken, tempCurToken.Literal, r),
                None: () => { }
                );
        return expression;
    }

    private Option<IExpression> ParseInfixExpression(IExpression l)
    {
        Option<IExpression> expression = Option<IExpression>.None;
        var tempCurToken = curToken;

        var precedence = CurPrecedence();
        // Advance token to parse right side
        NextToken();
        ParseExpression(precedence).Match(
            Some: r => { expression = new InfixExpression(tempCurToken, tempCurToken.Literal, l, r); },
            None: () => { expression = new InfixExpression(curToken, curToken.Literal, l); }
        );
        return expression;
    }

    private Option<IExpression> ParseStringLiteral()
    {
        return new StringLiteral(curToken, curToken.Literal);
    }
}
