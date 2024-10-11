using Monkey.Evaluator;
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
            var evaluator = new Evaluator.Evaluator();
            var p = new Parser(l);
            var program = p.ParseProgram();
            if (p.Errors.Any())
            {
                foreach (var error in p.Errors)
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
            }
            else
            {
                // Console.WriteLine(program.String());
                var evaluated = evaluator.Eval(program);
                if (evaluated != null) {
                    Console.WriteLine(evaluated.Inspect());
                }
            }
        }
    }
}
