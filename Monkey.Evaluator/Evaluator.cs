using LanguageExt;
using Monkey.Ast;
using Monkey.Object;

namespace Monkey.Evaluator;
public class Evaluator
{
    public readonly Object.Boolean TRUE = new(true);
    public readonly Object.Boolean FALSE = new(false);
    public Null NULL = new() { };

    public IObject Eval(INode node, ref Object.Environment env)
    {
        switch (node)
        {
            case Program p:
                return EvalProgram(p.Statements.ToArray(), ref env);
            case ExpressionStatement e:
                return Eval(e.Expression, ref env);
            case IntegerLiteral x:
                return new Object.Integer(x.Value);
            case Ast.Boolean x:
                return NativeBoolToBooleanObject(x.Value);
            case PrefixExpression x:
                var right = Eval(x.Right, ref env);
                if (IsError(right))
                {
                    return right;
                }
                return EvalPrefixExpression(x.Operator, right);
            case InfixExpression x:
                var left = Eval(x.Left, ref env);
                if (IsError(left))
                {
                    return left;
                }
                var right2 = Eval(x.Right, ref env);
                if (IsError(right2))
                {
                    return right2;
                }
                return EvalInfixExpression(x.Operator, left, right2);
            case IfExpression x:
                return EvalIfExpression(x, env);
            case BlockStatement x:
                return EvalBlockStatement(x.Statements.ToArray(), ref env);
            case ReturnStatement x:
                var val = Eval(x.ReturnValue, ref env);
                return IsError(val) ? val : new ReturnValue(val);
            case LetStatement x:
                var ls = Eval(x.Value, ref env);
                if (IsError(ls))
                {
                    return ls;
                }
                env.Set(x.Name.Value, ls);
                break;
            case Identifier x:
                return EvalIdentifier(x, ref env);
            case FunctionLiteral x:
                var Params = x.Parameters;
                var body = x.Body;
                return new Function(Params, body, env);
            case CallExpression x:
                var func = Eval(x.Function, ref env);
                if (IsError(func))
                {
                    return func;
                }
                var args = EvalExpressions(x.Args, env);
                if (args.Length() == 1 && IsError(args[0]))
                {
                    return args[0];
                }
                return ApplyFunction(func, args);
        }
        return null;
    }

    private IObject ApplyFunction(IObject fn, IObject[] args)
    {
        if (fn is not Object.Function)
        {
            return NewError("not a function: {0}", fn.Type());
        }

        var extendedEnv = ExtendFunctionEnv((Object.Function)fn, args);
        var evaluated = Eval(((Object.Function)fn).Body, ref extendedEnv);
        return UnwrapReturnValue(evaluated);
    }

    private IObject UnwrapReturnValue(IObject obj)
    {
        if(obj is ReturnValue rv) {
            return rv.Value;
        }
        return obj;
    }

    private Object.Environment ExtendFunctionEnv(Object.Function fn, IObject[] args)
    {
        var env = Object.Environment.NewEnclosedEnvironement(fn.Env);
        var idx = 0;
        foreach (var param in fn.Parameters)
        {
            env.Set(param.Value, args[idx]);
            idx++;
        }
        return env;
    }

    private IObject[] EvalExpressions(Ast.IExpression[] exps, Object.Environment env)
    {
        var res = new List<IObject>();
        foreach (var e in exps)
        {
            var evaluated = Eval(e, ref env);
            if (IsError(evaluated))
            {
                return new IObject[] { evaluated };
            }
            res.Add(evaluated);
        }
        return res.ToArray();
    }

    private IObject EvalIdentifier(Identifier node, ref Object.Environment env)
    {
        return env.Get(node.Value).Match(
            None: NewError("Identifier not found: {0}", node.Value),
            Some: x => x
        );
    }

    private bool IsError(IObject obj)
    {
        if (!obj.IsNull())
        {
            return obj.Type() == ObjectType.ERROR_OBJ;
        }
        return false;
    }

    private IObject EvalIfExpression(IfExpression ie, Object.Environment env)
    {
        var condition = Eval(ie.Condition, ref env);
        if (IsError(condition))
        {
            return condition;
        }
        if (IsTruthy(condition))
        {
            return Eval(ie.Consequence, ref env);
        }
        else
        {
            return ie.Alternative.Match(
                None: NULL,
                Some: x => Eval(x, ref env)
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

    public IObject EvalProgram(IStatement[] statements, ref Object.Environment env)
    {
        IObject result = NULL;
        foreach (var stmt in statements)
        {
            result = Eval(stmt, ref env);

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

    private IObject EvalBlockStatement(IStatement[] statements, ref Object.Environment env)
    {
        IObject result = NULL;
        foreach (var stmt in statements)
        {
            result = Eval(stmt, ref env);
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
