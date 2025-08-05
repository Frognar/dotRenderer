namespace dotRenderer;

public static class Parser
{
    public static SequenceNode Parse(IEnumerable<object> tokens)
        => throw new NotImplementedException();
}

public abstract record Node;
public sealed record SequenceNode(IReadOnlyList<Node> Children) : Node;
public sealed record TextNode(string Text) : Node;
public sealed record EvalNode(IEnumerable<string> Path) : Node;