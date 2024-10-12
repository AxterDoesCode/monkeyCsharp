using System.Dynamic;
using LanguageExt;
using Monkey.Ast;
using Monkey.Object;

namespace Monkey.Evaluator;
public class Evaluator
{
    public readonly Object.Boolean TRUE = new(true);
    public readonly Object.Boolean FALSE = new(false);
    public Null NULL = new() { };

    public IObject Eval(INode node)
    {
        switch (node)
        {
            case Program p:
                return EvalProgram(p.Statements.ToArray());
            case ExpressionStatement e:
                return Eval(e.Expression);
            case IntegerLiteral x:
                return new Object.Integer(x.Value);
            case Ast.Boolean x:
                return NativeBoolToBooleanObject(x.Value);
            case PrefixExpression x:
                var right = Eval(x.Right);
                if (IsError(right))
                {
                    return right;
                }
                return EvalPrefixExpression(x.Operator, right);
            case InfixExpression x:
                var left = Eval(x.Left);
                if (IsError(left))
                {
                    return left;
                }
                var right2 = Eval(x.Right);
                if (IsError(right2))
                {
                    return right2;
                }
                return EvalInfixExpression(x.Operator, left, right2);
            case IfExpression x:
                return EvalIfExpression(x);
            case BlockStatement x:
                return EvalBlockStatement(x.Statements.ToArray());
            case ReturnStatement x:
                var val = Eval(x.ReturnValue);
                return IsError(val) ? val : new ReturnValue(val);
        }
        return NULL;
    }

    private bool IsError(IObject obj)
    {
        if (!obj.IsNull())
        {
            return obj.Type() == ObjectType.ERROR_OBJ;
        }
        return false;
    }

    private IObject EvalIfExpression(IfExpression ie)
    {
        var condition = Eval(ie.Condition);
        if (IsError(condition))
        {
            return condition;
        }
        if (IsTruthy(condition))
        {
            return Eval(ie.Consequence);
        }
        else
        {
            return ie.Alternative.Match(
                None: NULL,
                Some: x => Eval(x)
            );
        }
    }

    private bool IsTruthy(IObject obj)
    {
        return obj switch
        {
            Object.Boolean x => x.value,
            Object.Null => false,
            _ => false,
        };
    }

    private IObject EvalInfixExpression(string op, IObject l, IObject r)
    {
        if (l.Type() == ObjectType.INTEGER_OBJ && r.Type() == ObjectType.INTEGER_OBJ)
        {
            return EvalIntegerInfixExpression(op, l, r);
        }
        else if (op == "==")
        {
            return NativeBoolToBooleanObject(l == r);
        }
        else if (op == "!=")
        {
            return NativeBoolToBooleanObject(l != r);
        }
        else if (l.Type() != r.Type())
        {
            return NewError("type mismatch: {0} {1} {2}", l.Type(), op, r.Type());
        }
        else
        {
            return NewError("unknown operator: {0} {1} {2}", l.Type(), op, r.Type());
        }
    }

    private IObject EvalIntegerInfixExpression(string op, IObject l, IObject r)
    {
        var leftVal = ((Integer)l).value;
        var rightVal = ((Integer)r).value;
        return op switch
        {
            "+" => new Integer(leftVal + rightVal),
            "-" => new Integer(leftVal - rightVal),
            "*" => new Integer(leftVal * rightVal),
            "/" => new Integer(leftVal / rightVal),
            "<" => NativeBoolToBooleanObject(leftVal < rightVal),
            ">" => NativeBoolToBooleanObject(leftVal > rightVal),
            "==" => NativeBoolToBooleanObject(leftVal == rightVal),
            "!=" => NativeBoolToBooleanObject(leftVal != rightVal),
            _ => NewError("unknown operator: {0} {1} {2}", l.Type(), op, r.Type()),
        };
    }

    private IObject EvalPrefixExpression(string op, IObject right)
    {
        return op switch
        {
            "!" => EvalBangOperator(right),
            "-" => EvalMinusOperator(right),
            _ => NewError("unknown operator: {0}{1}", op, right.Type()),
        };
    }

    private IObject EvalMinusOperator(IObject right)
    {
        if (right.Type() != ObjectType.INTEGER_OBJ)
        {
            return NewError("unknown operator: -{0}", right.Type());
        }
        var value = ((Integer)right).value;
        return new Integer(-value);
    }

    private IObject EvalBangOperator(IObject right)
    {
        return right switch
        {
            Object.Boolean x => x.value ? FALSE : TRUE,
            Object.Null => TRUE,
            _ => FALSE,
        };
    }

    public IObject EvalProgram(IStatement[] statements)
    {
        IObject result = NULL;
        foreach (var stmt in statements)
        {
            result = Eval(stmt);

            switch (result)
            {
                case ReturnValue r:
                    return r.Value;
                case Error e:
                    return e;
            }
        }
        return result;
    }

    private IObject EvalBlockStatement(IStatement[] statements)
    {
        IObject result = NULL;
        foreach (var stmt in statements)
        {
            result = Eval(stmt);
            if (result != null && (result.Type() == ObjectType.RETURN_VALUE_OBJ || result.Type() == ObjectType.ERROR_OBJ))
            {
                return result;
            }
        }
        return result;
    }

    private IObject NativeBoolToBooleanObject(bool input)
    {
        return input ? TRUE : FALSE;
    }

    private Object.Error NewError(string format, params string[] args)
    {
        return new Object.Error(String.Format(format, args));
    }
}
