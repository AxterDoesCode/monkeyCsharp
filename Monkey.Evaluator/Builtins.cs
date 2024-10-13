using Monkey.Object;

namespace Monkey.Evaluator;
public static class MonkeyBuiltins
{
    public static readonly Dictionary<string, Builtin> Builtins = new();

    static MonkeyBuiltins()
    {
        Builtins.Add("len", new Builtin(Len));
    }

    private static IObject Len(params IObject[] args)
    {
        if (args.Length != 1)
        {
            return Evaluator.NewError("wrong number of arguments. got={0}, want=1", args.Length.ToString());
        }

        return args[0] switch
        {
            Object.String arg => new Integer(arg.Value.Length),
            _ => Evaluator.NewError("argument to len not supported, got {0}", args[0].Type()),
        };
    }
}
