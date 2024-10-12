using LanguageExt;

namespace Monkey.Object;

public class Environment
{
    public Dictionary<string, IObject> store;
    public Option<Environment> Outer { get; set; }
    public static Environment NewEnvironment()
    {
        return new Environment(new Dictionary<string, IObject>());
    }

    public static Environment NewEnclosedEnvironement(Environment outer)
    {
        var env = NewEnvironment();
        env.Outer = outer;
        return env;
    }

    private Environment(Dictionary<string, IObject> s)
    {
        store = s;
    }

    public Option<IObject> Get(string name)
    {
        return store.TryGetValue(Key: name).Match(
            None: Outer.Match(
                None: Option<IObject>.None,
                Some: v => v.Get(name)
            ),
            Some: x => Option<IObject>.Some(x)
        );
    }

    public IObject Set(string name, IObject val)
    {
        store.Add(name, val);
        return val;
    }
}

