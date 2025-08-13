using System.Diagnostics.Contracts;

namespace DotRenderer;

public static class Evaluator
{
    [Pure]
    public static Result<Value> EvaluateIdent(IValueAccessor accessor, string name, TextSpan range)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        (bool ok, Value value) = accessor.Get(name);
        return !ok
            ? Result<Value>.Err(new EvalError("MissingIdent", range, $"Identifier '{name}' was not found."))
            : Result<Value>.Ok(value);
    }

    [Pure]
    public static Result<Value> EvaluateExpr(IExpr expr, IValueAccessor accessor, TextSpan range)
    {
        return expr switch
        {
            NumberExpr n => Result<Value>.Ok(Value.FromNumber(n.Value)),
            BooleanExpr b => Result<Value>.Ok(Value.FromBool(b.Value)),
            IdentExpr id => EvaluateIdent(accessor, id.Name, range),
            BinaryExpr bin => EvaluateBinaryExpr(bin, accessor, range),
            _ => Result<Value>.Err(new EvalError("UnsupportedExpr", range, "Expression kind not supported yet."))
        };
    }

    private static Result<Value> EvaluateBinaryExpr(BinaryExpr expr, IValueAccessor accessor, TextSpan range)
    {
        Result<Value> l = EvaluateExpr(expr.Left, accessor, range);
        if (!l.IsOk)
        {
            return l;
        }

        Result<Value> r = EvaluateExpr(expr.Right, accessor, range);
        if (!r.IsOk)
        {
            return r;
        }

        Value lv = l.Value;
        Value rv = r.Value;

        if (lv.Kind == ValueKind.Number && rv.Kind == ValueKind.Number)
        {
            return Result<Value>.Ok(Value.FromNumber(lv.Number + rv.Number));
        }

        return Result<Value>.Err(new EvalError("TypeMismatch", range, "Operator '+' expects numbers."));
    }
}