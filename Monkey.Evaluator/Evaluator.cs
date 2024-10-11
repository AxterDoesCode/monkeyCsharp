using Monkey.Ast;

namespace Monkey.Evaluator;
public class Evaluator
{
    public Object.IObject Eval(Ast.INode node) {
        switch (node)
        {
            case Ast.Program p:
                return EvalStatements(p.Statements.ToArray());
            case Ast.ExpressionStatement e:
                return Eval(e.Expression);
            case Ast.IntegerLiteral x:
                return new Object.Integer(x.Value);
            // case Ast.Boolean x:
            //     return new Object.Boolean(x.Value);
        }
        return null;
    }

    public Object.IObject EvalStatements(IStatement[] statements) {
        Object.IObject result = new Object.Integer(1);
        foreach (var stmt in statements) {
            result = Eval(stmt);
        }
        return result;
    }
}
