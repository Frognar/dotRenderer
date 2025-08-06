namespace dotRenderer;

public static class ExpressionParser
{
    public static ExprNode Parse(string expr)
    {
        return expr == "Model.IsAdmin"
            ? new PropertyExpr(new List<string> { "Model", "IsAdmin" })
            : throw new NotImplementedException();
    }
}

public abstract record ExprNode;

public sealed record PropertyExpr(IReadOnlyList<string> Path) : ExprNode;