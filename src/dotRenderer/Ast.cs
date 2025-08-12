using System.Collections.Immutable;

namespace DotRenderer;

public interface INode
{
    TextSpan Range { get; }
}

public static class Node
{
    public static TextNode FromText(string text, TextSpan range) => new(text, range);
    public static InterpolateIdentNode FromInterpolateIdent(string name, TextSpan range) => new(name, range);
    public static InterpolateExprNode FromInterpolateExpr(IExpr expr, TextSpan range) => new(expr, range);
}

public sealed record TextNode(string Text, TextSpan Range) : INode;

public sealed record InterpolateIdentNode(string Name, TextSpan Range) : INode;

public sealed record InterpolateExprNode(IExpr Expr, TextSpan Range) : INode;

public sealed record Template(ImmutableArray<INode> Children);

public interface IExpr;

public enum BinaryOp
{
    Add
}

public static class Expr
{
    public static RawExpr FromRaw(string text) => new(text);
    public static NumberExpr FromNumber(double value) => new(value);
    public static IdentExpr FromIdent(string name) => new(name);
    public static BinaryExpr FromBinaryAdd(IExpr left, IExpr right) => new(BinaryOp.Add, left, right);
}

public sealed record RawExpr(string Text) : IExpr;

public sealed record NumberExpr(double Value) : IExpr;

public sealed record IdentExpr(string Name) : IExpr;

public sealed record BinaryExpr(BinaryOp Op, IExpr Left, IExpr Right) : IExpr;