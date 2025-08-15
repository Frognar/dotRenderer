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

    public static IfNode FromIf(
        IExpr condition,
        ImmutableArray<INode> thenNodes,
        ImmutableArray<INode> elseNodes,
        TextSpan range) =>
        new(condition, thenNodes, elseNodes, range);

    public static IfNode FromIf(IExpr condition, ImmutableArray<INode> thenNodes, TextSpan range) =>
        new(condition, thenNodes, [], range);

    public static ForNode FromFor(string item, IExpr seq, ImmutableArray<INode> body, TextSpan range) =>
        new(item, seq, body, range);
}

public sealed record TextNode(string Text, TextSpan Range) : INode;

public sealed record InterpolateIdentNode(string Name, TextSpan Range) : INode;

public sealed record InterpolateExprNode(IExpr Expr, TextSpan Range) : INode;

public sealed record IfNode(IExpr Condition, ImmutableArray<INode> Then, ImmutableArray<INode> Else, TextSpan Range)
    : INode;

public sealed record ForNode(string Item, IExpr Seq, ImmutableArray<INode> Body, TextSpan Range) : INode;

public sealed record Template(ImmutableArray<INode> Children);

public interface IExpr;

public enum BinaryOp
{
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Eq,
    Lt,
    Le,
    Gt,
    Ge,
    And,
    Or
}

public enum UnaryOp
{
    Not,
    Neg
}

public static class Expr
{
    public static RawExpr FromRaw(string text) => new(text);
    public static NumberExpr FromNumber(double value) => new(value);
    public static BooleanExpr FromBoolean(bool value) => new(value);
    public static StringExpr FromString(string value) => new(value);
    public static IdentExpr FromIdent(string name) => new(name);
    public static BinaryExpr FromBinaryAdd(IExpr left, IExpr right) => new(BinaryOp.Add, left, right);
    public static BinaryExpr FromBinarySub(IExpr left, IExpr right) => new(BinaryOp.Sub, left, right);
    public static BinaryExpr FromBinaryMul(IExpr left, IExpr right) => new(BinaryOp.Mul, left, right);
    public static BinaryExpr FromBinaryDiv(IExpr left, IExpr right) => new(BinaryOp.Div, left, right);
    public static BinaryExpr FromBinaryMod(IExpr left, IExpr right) => new(BinaryOp.Mod, left, right);
    public static BinaryExpr FromBinaryEq(IExpr left, IExpr right) => new(BinaryOp.Eq, left, right);
    public static BinaryExpr FromBinaryLt(IExpr left, IExpr right) => new(BinaryOp.Lt, left, right);
    public static BinaryExpr FromBinaryLe(IExpr left, IExpr right) => new(BinaryOp.Le, left, right);
    public static BinaryExpr FromBinaryGt(IExpr left, IExpr right) => new(BinaryOp.Gt, left, right);
    public static BinaryExpr FromBinaryGe(IExpr left, IExpr right) => new(BinaryOp.Ge, left, right);
    public static BinaryExpr FromBinaryAnd(IExpr left, IExpr right) => new(BinaryOp.And, left, right);
    public static BinaryExpr FromBinaryOr(IExpr left, IExpr right) => new(BinaryOp.Or, left, right);
    public static MemberExpr FromMember(IExpr target, string name) => new(target, name);
    public static UnaryExpr FromUnaryNot(IExpr operand) => new(UnaryOp.Not, operand);
    public static UnaryExpr FromUnaryNeg(IExpr operand) => new(UnaryOp.Neg, operand);
}

public sealed record RawExpr(string Text) : IExpr;

public sealed record NumberExpr(double Value) : IExpr;

public sealed record BooleanExpr(bool Value) : IExpr;
public sealed record StringExpr(string Value) : IExpr;

public sealed record IdentExpr(string Name) : IExpr;

public sealed record BinaryExpr(BinaryOp Op, IExpr Left, IExpr Right) : IExpr;

public sealed record UnaryExpr(UnaryOp Op, IExpr Operand) : IExpr;

public sealed record MemberExpr(IExpr Target, string Name) : IExpr;