namespace Monkey.Interpreter.Lexing;

public class Lexer
{
    private string _input;
    private int _position;
    private int _readPosition;
    private char _ch;

    public Lexer(string input)
    {
        _input = input;
        ReadChar();
    }

    private void ReadChar()
    {
        if (_readPosition >= _input.Length)
        {
            _ch = '\0'; // EOF
        }
        else
        {
            _ch = _input[_readPosition];
        }
        _position = _readPosition;
        _readPosition += 1;
    }

    private bool IsLetter(char ch)
    {
        return char.IsLetter(ch) || ch == '_';
    }

    private string ReadIdentifier()
    {
        int pos = _position;
        while (IsLetter(_ch))
        {
            ReadChar();
        }
        return _input[pos.._position];
    }

    private string ReadString()
    {
        int pos = _position + 1;
        while(true)
        {
            ReadChar();
            if (_ch == '"' || _ch == 0)
            {
                break;
            }
        }
        return _input[pos.._position];
    }

    private void SkipWhiteSpace()
    {
        while (_ch == ' ' || _ch == '\t' || _ch == '\n' || _ch == '\r')
        {
            ReadChar();
        }
    }

    private char PeekChar()
    {
        if (_readPosition >= _input.Length)
        {
            return '\0';
        }
        else
        {
            return _input[_readPosition];
        }
    }

    private string ReadNumber()
    {
        int pos = _position;
        while (char.IsDigit(_ch))
        {
            ReadChar();
        }
        return _input[pos.._position];
    }

    public Token NextToken()
    {
        Token tok = null;
        SkipWhiteSpace();
        switch (_ch)
        {
            case '=':
                if (PeekChar() == '=')
                {
                    char ch = _ch;
                    ReadChar();
                    string literal = ch.ToString() + _ch.ToString();
                    tok = new Token(Token.EQ, literal);
                }
                else
                {
                    tok = new Token(Token.ASSIGN, _ch);
                }
                break;
            case ';':
                tok = new Token(Token.SEMICOLON, _ch);
                break;
            case '(':
                tok = new Token(Token.LPAREN, _ch);
                break;
            case ')':
                tok = new Token(Token.RPAREN, _ch);
                break;
            case ',':
                tok = new Token(Token.COMMA, _ch);
                break;
            case '+':
                tok = new Token(Token.PLUS, _ch);
                break;
            case '-':
                tok = new Token(Token.MINUS, _ch);
                break;
            case '!':
                if (PeekChar() == '=')
                {
                    char ch = _ch;
                    ReadChar();
                    string literal = ch.ToString() + _ch.ToString();
                    tok = new Token(Token.NOT_EQ, literal);
                }
                else
                {
                    tok = new Token(Token.BANG, _ch);
                }
                break;
            case '*':
                tok = new Token(Token.ASTERISK, _ch);
                break;
            case '/':
                tok = new Token(Token.SLASH, _ch);
                break;
            case '<':
                tok = new Token(Token.LT, _ch);
                break;
            case '>':
                tok = new Token(Token.GT, _ch);
                break;
            case '{':
                tok = new Token(Token.LBRACE, _ch);
                break;
            case '}':
                tok = new Token(Token.RBRACE, _ch);
                break;
            case '[':
                tok = new Token(Token.LBRACKET, _ch);
                break;
            case ']':
                tok = new Token(Token.RBRACKET, _ch);
                break;
            case '\0':
                tok = new Token(Token.EOF, _ch);
                break;
            case '"':
                tok = new Token(Token.STRING, ReadString());
                break;
            default:
                if (IsLetter(_ch))
                {
                    var literal = ReadIdentifier();
                    var type = Token.LookupIdent(literal);
                    tok = new Token(type, literal);
                    return tok;
                }
                else if (char.IsDigit(_ch))
                {
                    string literal = ReadNumber();
                    tok = new Token(Token.INT, literal);
                    return tok;
                }
                else
                {
                    tok = new Token(Token.ILLEGAL, _ch);
                    break;
                }
        }
        ReadChar();
        return tok;
    }
}
