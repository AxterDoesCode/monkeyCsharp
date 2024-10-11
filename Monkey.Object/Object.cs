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
}

public class Integer : IObject
{
    public Integer (long val) {
        value = val;
    }
    public long value;
    public string Type() { return ObjectType.INTEGER_OBJ; }
    public string Inspect() { return value.ToString(); }
}

public class Boolean : IObject
{
    public Boolean (bool b) {
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
