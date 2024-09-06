using Monkey.Interpreter.Lexing;

namespace Monkey.Interpreter.Repl;

public class Repl
{
    public static void Start()
    {
        while (true)
        {
            Console.Write(">> ");
            string line = Console.ReadLine();
            Lexer l = new Lexer(line);
            for (Token tok =l.NextToken(); tok.Type != Token.EOF; tok = l.NextToken()) {
                Console.WriteLine(tok.Type + " " + tok.Literal);
            }
        }
    }
}
