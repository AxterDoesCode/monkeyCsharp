using System.Text;
using Microsoft.VisualBasic;

namespace Monkey.Object;
using BuiltinFunction = Func<IObject[], IObject>;
public interface IObject
{
    string Type();
    string Inspect();
}

public class ObjectType
{
    public const string INTEGER_OBJ = "INTEGER";
    public const string BOOLEAN_OBJ = "BOOLEAN";
    public const string NULL_OBJ = "NULL";
    public const string RETURN_VALUE_OBJ = "RETURN_VALUE";
    public const string ERROR_OBJ = "ERROR";
    public const string FUNCTION_OBJ = "FUNCTION";
    public const string STRING_OBJ = "STRING";
    public const string BUILTIN_OBJ = "BUILTIN";
    public const string ARRAY_OBJ = "ARRAY";
    public const string HASH_OBJ = "HASH";
}

public interface IHashable
{
    public HashKey HashKey();
}

public class Error : IObject
{
    public Error(string errMsg)
    {
        Message = errMsg;
    }
    public string Message;
    public string Type() => ObjectType.ERROR_OBJ;
    public string Inspect() => $"Error: {Message}";
}

public class ReturnValue : IObject
{
    public ReturnValue(IObject obj)
    {
        Value = obj;
    }
    public IObject Value;
    public string Type() { return ObjectType.RETURN_VALUE_OBJ; }
    public string Inspect() { return Value.Inspect(); }
}
public class Integer : IObject, IHashable
{
    public Integer(long val)
    {
        value = val;
    }
    public long value;
    public string Type() { return ObjectType.INTEGER_OBJ; }
    public string Inspect() { return value.ToString(); }
    public HashKey HashKey()
    {
        return new(Type(), (ulong)value);
    }
}

public class Boolean : IObject, IHashable
{
    public Boolean(bool b)
    {
        value = b;
    }
    public bool value;
    public string Type() { return ObjectType.BOOLEAN_OBJ; }
    public string Inspect() { return value.ToString(); }
    public HashKey HashKey()
    {
        ulong val;
        if (value)
        {
            val = 1;
        }
        else
        {
            val = 0;
        }
        return new HashKey(Type(), val);
    }
}

public class Null : IObject
{
    public string Type() { return ObjectType.NULL_OBJ; }
    public string Inspect() { return "null"; }
}

public class Function : IObject
{
    public Function(Ast.Identifier[] parameters, Ast.BlockStatement body, Environment env)
    {
        Parameters = parameters;
        Body = body;
        Env = env;
    }
    public Ast.BlockStatement Body;
    public Ast.Identifier[] Parameters;
    public Environment Env;
    public string Type() { return ObjectType.FUNCTION_OBJ; }
    public string Inspect()
    {
        var Out = new StringBuilder();
        var Params = new List<string>();
        foreach (var p in Parameters)
        {
            Params.Add(p.String());
        }
        Out.Append("fn(");
        Out.AppendLine(Strings.Join(Params.ToArray(), ", ") + ")");
        Out.AppendLine(Body.String());
        return Out.ToString();
    }
}

public class String : IObject, IHashable
{
    public string Value;

    public String(string s)
    {
        Value = s;
    }
    public string Type() { return ObjectType.STRING_OBJ; }
    public string Inspect() { return Value; }
    public HashKey HashKey()
    {
        var s1 = Value[..(Value.Length / 2)];
        var s2 = Value[(Value.Length / 2)..];
        var hash = (long)s1.GetHashCode() << 32 | (uint)s2.GetHashCode();
        return new(Type(), (ulong)hash);

    }
}

public class Builtin : IObject
{
    public BuiltinFunction Fn;

    public Builtin(BuiltinFunction bfn)
    {
        Fn = bfn;
    }
    public string Type() { return ObjectType.BUILTIN_OBJ; }
    public string Inspect() { return "builtin function"; }
}

public class Array : IObject
{
    public IObject[] Elements;
    public Array(IObject[] elements)
    {
        Elements = elements;
    }
    public string Type() { return ObjectType.ARRAY_OBJ; }
    public string Inspect()
    {
        var Out = new StringBuilder();
        var elements = new List<string>();
        foreach (var el in Elements)
        {
            elements.Add(el.Inspect());
        }
        Out.Append('[');
        Out.Append(Strings.Join(elements.ToArray(), ", "));
        Out.Append(']');
        return Out.ToString();
    }
}

public record HashKey
{
    string Type;
    ulong Value;
    public HashKey(string type, ulong val)
    {
        Type = type;
        Value = val;
    }
}

public record HashPair
{
    public IObject Key;
    public IObject Value;
}

public class Hash : IObject
{
    public Dictionary<HashKey, HashPair> Pairs;
    public Hash(Dictionary<HashKey, HashPair> pairs)
    {
        Pairs = pairs;
    }
    public string Type() { return ObjectType.HASH_OBJ; }
    public string Inspect()
    {
        var Out = new StringBuilder();
        var pairs = new List<string>();

        foreach (var pair in Pairs)
        {
            pairs.Add($"{pair.Value.Key.Inspect()}: {pair.Value.Value.Inspect()}");
        }
        Out.Append('{');
        Out.Append(Strings.Join(pairs.ToArray(), ", "));
        Out.Append('}');
        return Out.ToString();
    }
}
