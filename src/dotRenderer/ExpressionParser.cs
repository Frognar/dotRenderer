namespace dotRenderer;

public static class ExpressionParser
{
    public static ExprNode Parse(string expr)
    {
        ArgumentNullException.ThrowIfNull(expr);
        return expr.StartsWith("Model.", StringComparison.Ordinal)
            ? new PropertyExpr(expr.Split('.'))
            : throw new NotImplementedException();
    }
}

public abstract record ExprNode;

public sealed record PropertyExpr(IReadOnlyList<string> Path) : ExprNode;