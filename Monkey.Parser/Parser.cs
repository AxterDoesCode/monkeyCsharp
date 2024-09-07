using Monkey.Interpreter.Lexing;
using Monkey.Ast;
namespace Monkey.Parser;
public class Parser {
    Lexer _lexer;
    Token curToken;
    Token peekToken;

    public Parser(Lexer l) {
        _lexer = l;
        // Call NextToken twice to populate curToken and peekToken fields
        NextToken();
        NextToken();
    }

    private void NextToken() {
        curToken = peekToken;
        peekToken = _lexer.NextToken();
    }

    public Program ParseProgram() {
        return null; // Remove this
    }
}
