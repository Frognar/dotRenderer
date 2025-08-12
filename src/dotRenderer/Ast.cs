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
}

public sealed record TextNode(string Text, TextSpan Range) : INode;

public sealed record InterpolateIdentNode(string Name, TextSpan Range) : INode;

public sealed record Template(ImmutableArray<INode> Children);