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
        if (expr is NumberExpr n)
        {
            return Result<Value>.Ok(Value.FromNumber(n.Value));
        }

        if (expr is BooleanExpr b)
        {
            return Result<Value>.Ok(Value.FromBool(b.Value));
        }

        if (expr is IdentExpr id)
        {
            return EvaluateIdent(accessor, id.Name, range);
        }

        if (expr is not BinaryExpr { Op: BinaryOp.Add } bin)
        {
            return Result<Value>.Err(new EvalError("UnsupportedExpr", range, "Expression kind not supported yet."));
        }

        Result<Value> l = EvaluateExpr(bin.Left, accessor, range);
        if (!l.IsOk)
        {
            return l;
        }

        Result<Value> r = EvaluateExpr(bin.Right, accessor, range);
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