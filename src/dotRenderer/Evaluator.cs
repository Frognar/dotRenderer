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
            MemberExpr m => EvaluateMember(m, accessor, range),
            _ => Result<Value>.Err(new EvalError("UnsupportedExpr", range, "Expression kind not supported yet."))
        };
    }

    private static Result<Value> EvaluateMember(MemberExpr expr, IValueAccessor accessor, TextSpan range)
    {
        return EvaluateExpr(expr.Target, accessor, range)
            .Bind(t => t.Kind != ValueKind.Map
                ? Result<Value>.Err(new EvalError("TypeMismatch", range, "Member access requires a map/object value."))
                : t.Map.TryGetValue(expr.Name, out Value value)
                    ? Result<Value>.Ok(value)
                    : Result<Value>.Err(new EvalError("MissingMember", range, $"Member '{expr.Name}' was not found.")));
    }

    private static Result<Value> EvaluateBinaryExpr(BinaryExpr expr, IValueAccessor accessor, TextSpan range)
    {
        return EvaluateExpr(expr.Left, accessor, range)
            .Bind2(
                () => EvaluateExpr(expr.Right, accessor, range),
                (l, r) => (l, r, expr.Op) switch
                {
                    ({ Kind: ValueKind.Number } ln, { Kind: ValueKind.Number } rn, BinaryOp.Add) =>
                        Result<Value>.Ok(Value.FromNumber(ln.Number + rn.Number)),
                    ({ Kind: ValueKind.Number } ln, { Kind: ValueKind.Number } rn, BinaryOp.Eq) =>
                        Result<Value>.Ok(Value.FromBool(Math.Abs(ln.Number - rn.Number) < 0.000001)),
                    ({ Kind: ValueKind.Number } ln, { Kind: ValueKind.Number } rn, BinaryOp.Lt) =>
                        Result<Value>.Ok(Value.FromBool(ln.Number < rn.Number)),
                    ({ Kind: ValueKind.Number } ln, { Kind: ValueKind.Number } rn, BinaryOp.Le) =>
                        Result<Value>.Ok(Value.FromBool(ln.Number <= rn.Number)),
                    ({ Kind: ValueKind.Number } ln, { Kind: ValueKind.Number } rn, BinaryOp.Gt) =>
                        Result<Value>.Ok(Value.FromBool(ln.Number > rn.Number)),
                    ({ Kind: ValueKind.Number } ln, { Kind: ValueKind.Number } rn, BinaryOp.Ge) =>
                        Result<Value>.Ok(Value.FromBool(ln.Number >= rn.Number)),
                    ({ Kind: ValueKind.Boolean } ln, { Kind: ValueKind.Boolean } rn, BinaryOp.Eq) =>
                        Result<Value>.Ok(Value.FromBool(ln.Boolean == rn.Boolean)),
                    ({ Kind: ValueKind.Text } ln, { Kind: ValueKind.Text } rn, BinaryOp.Eq) =>
                        Result<Value>.Ok(Value.FromBool(ln.Text.Equals(rn.Text, StringComparison.Ordinal))),
                    (_, _, BinaryOp.Add) =>
                        Result<Value>.Err(new EvalError("TypeMismatch", range, "Operator '+' expects numbers.")),
                    (_, _, BinaryOp.Eq) =>
                        Result<Value>.Err(new EvalError("TypeMismatch", range, "Operator '==' expects operands of the same scalar type.")),
                    _ =>
                        Result<Value>.Err(new EvalError("UnsupportedOp", range, "Binary operator not supported."))
                });
    }
}