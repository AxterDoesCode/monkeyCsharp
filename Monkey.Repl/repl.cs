using Monkey.Interpreter.Lexing;
using Monkey.Parsing;

namespace Monkey.Repl;

public class Repl
{
    public static void Start()
    {
        while (true)
        {
            Console.Write(">> ");
            string line = Console.ReadLine();
            Lexer l = new Lexer(line);
            var p = new Parser(l);
            var program = p.ParseProgram();
            Console.WriteLine(program.String());
            foreach(var error in p.Errors) {
                Console.WriteLine($"Error: {error.ErrorMessage}");
            }
        }
    }
}
