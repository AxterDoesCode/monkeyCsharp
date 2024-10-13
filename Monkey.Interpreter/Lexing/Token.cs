namespace Monkey.Interpreter.Lexing;

public class Token
{
    public const string ILLEGAL = "ILLEGAL";
    public const string EOF = "EOF";

    // Identifiers + literals
    public const string IDENT = "IDENT";
    public const string INT = "INT";
    public const string STRING = "STRING";

    // Operators
    public const string ASSIGN = "=";
    public const string PLUS = "+";
    public const string MINUS = "-";
    public const string BANG = "!";
    public const string ASTERISK = "*";
    public const string SLASH = "/";
    public const string LT = "<";
    public const string GT = ">";

    // Delimiters
    public const string COMMA = ",";
    public const string SEMICOLON = ";";

    public const string LPAREN = "(";
    public const string RPAREN = ")";
    public const string LBRACE = "{";
    public const string RBRACE = "}";
    public const string LBRACKET = "[";
    public const string RBRACKET = "]";

    // Keywords
    public const string FUNCTION = "FUNCTION";
    public const string LET = "LET";
    public const string TRUE = "TRUE";
    public const string FALSE = "FALSE";
    public const string IF = "IF";
    public const string ELSE = "ELSE";
    public const string RETURN = "RETURN";

    // Equality
    public const string EQ = "==";
    public const string NOT_EQ = "!=";

    private static Dictionary<string, string> keywords = new Dictionary<string, string> {
        {"fn", FUNCTION},
        {"let", LET},
        {"true", TRUE},
        {"false", FALSE},
        {"if", IF},
        {"else", ELSE},
        {"return", RETURN},
    };


    public Token(string tokenType, char literal)
    {
        Type = tokenType;
        Literal = literal.ToString();
    }
    public Token(string tokenType, string literal)
    {
        Type = tokenType;
        Literal = literal;
    }

    public string Type { get; }
    public string Literal { get; }

    public static string LookupIdent(string ident)
    {
        if (keywords.ContainsKey(ident)) {
            return keywords[ident];
        }
        return IDENT;
    }
}
