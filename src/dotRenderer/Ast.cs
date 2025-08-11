using System.Collections.Immutable;

namespace DotRenderer;

public interface INode
{
    Range Range { get; }
}

public sealed record TextNode(string Text, Range Range) : INode;

public sealed record Template(ImmutableArray<INode> Children);