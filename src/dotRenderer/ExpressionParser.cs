namespace dotRenderer;

public static class ExpressionParser
{
    public static ExprNode Parse(string expr)
    {
        ArgumentNullException.ThrowIfNull(expr);
        return expr switch
        {
            "Model.IsAdmin == true" => new BinaryExpr(
                "==",
                new PropertyExpr(["Model", "IsAdmin"]),
                new LiteralExpr<bool>(true)),
            "Model.Age >= 18" => new BinaryExpr(
                ">=",
                new PropertyExpr(["Model", "Age"]),
                new LiteralExpr<int>(18)),
            _ when expr.StartsWith('!') => new UnaryExpr("!", Parse(expr[1..])),
            "true" => new LiteralExpr<bool>(true),
            "false" => new LiteralExpr<bool>(false),
            _ => expr.StartsWith("Model.", StringComparison.Ordinal)
                ? new PropertyExpr(expr.Split('.'))
                : throw new NotImplementedException()
        };
    }
}

public abstract record ExprNode;

public sealed record PropertyExpr(IReadOnlyList<string> Path) : ExprNode;

public sealed record LiteralExpr<T>(T Value) : ExprNode;
public sealed record UnaryExpr(string Operator, ExprNode Operand) : ExprNode;
public sealed record BinaryExpr(string Operator, ExprNode Left, ExprNode Right) : ExprNode;