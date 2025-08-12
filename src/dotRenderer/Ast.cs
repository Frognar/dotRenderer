using System.Collections.Immutable;

namespace DotRenderer;

public interface INode
{
    TextSpan Range { get; }
}

public sealed record TextNode(string Text, TextSpan Range) : INode;

public sealed record InterpolateIdentNode(string Name, TextSpan Range) : INode;

public sealed record Template(ImmutableArray<INode> Children);