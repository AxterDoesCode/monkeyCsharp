using Monkey.Object;

namespace Monkey.Evaluator;
public static class MonkeyBuiltins
{
    public static readonly Dictionary<string, Builtin> Builtins = new();

    static MonkeyBuiltins()
    {
        Builtins.Add("len", new Builtin(Len));
        Builtins.Add("first", new Builtin(First));
        Builtins.Add("last", new Builtin(Last));
        Builtins.Add("rest", new Builtin(Rest));
        Builtins.Add("push", new Builtin(Push));
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
            Object.Array arr => new Integer(arr.Elements.Length),
            _ => Evaluator.NewError("argument to len not supported, got {0}", args[0].Type()),
        };
    }

    private static IObject First(params IObject[] args)
    {
        if (args.Length != 1)
        {
            return Evaluator.NewError("wrong number of arguments. got={0}, want=1", args.Length.ToString());
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return Evaluator.NewError("argument to 'first' must be array, got {0}", args[0].Type());
        }

        var arr = (Object.Array)args[0];
        if (arr.Elements.Any())
        {
            return arr.Elements[0];
        }
        return new Null();
    }

    private static IObject Last(params IObject[] args)
    {
        if (args.Length != 1)
        {
            return Evaluator.NewError("wrong number of arguments. got={0}, want=1", args.Length.ToString());
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return Evaluator.NewError("argument to 'last' must be array, got {0}", args[0].Type());
        }

        var arr = (Object.Array)args[0];
        if (arr.Elements.Any())
        {
            return arr.Elements.Last();
        }
        return new Null();
    }

    private static IObject Rest(params IObject[] args)
    {
        if (args.Length != 1)
        {
            return Evaluator.NewError("wrong number of arguments. got={0}, want=1", args.Length.ToString());
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return Evaluator.NewError("argument to 'rest' must be array, got {0}", args[0].Type());
        }

        var arr = (Object.Array)args[0];
        if (arr.Elements.Any())
        {
            return new Object.Array(arr.Elements[1..]);
        }
        return new Null();
    }

    private static IObject Push(params IObject[] args)
    {
        if (args.Length != 2)
        {
            return Evaluator.NewError("wrong number of arguments. got={0}, want=2", args.Length.ToString());
        }

        if (args[0].Type() != ObjectType.ARRAY_OBJ)
        {
            return Evaluator.NewError("argument to 'push' must be array, got {0}", args[0].Type());
        }

        var arr = ((Object.Array)args[0]).Elements.ToList();
        arr.Add(args[1]);
        return new Object.Array(arr.ToArray());
    }
}
