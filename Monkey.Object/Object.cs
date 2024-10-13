using System.Text;
using Microsoft.VisualBasic;

namespace Monkey.Object;
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
public class Integer : IObject
{
    public Integer(long val)
    {
        value = val;
    }
    public long value;
    public string Type() { return ObjectType.INTEGER_OBJ; }
    public string Inspect() { return value.ToString(); }
}

public class Boolean : IObject
{
    public Boolean(bool b)
    {
        value = b;
    }
    public bool value;
    public string Type() { return ObjectType.BOOLEAN_OBJ; }
    public string Inspect() { return value.ToString(); }
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

public class String : IObject
{
    public string Value;

    public String(string s)
    {
        Value = s;
    }
    public string Type() { return ObjectType.STRING_OBJ; }
    public string Inspect() { return Value; }
}
