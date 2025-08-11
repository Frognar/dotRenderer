using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace DotRenderer;

public static class Parser
{
    [Pure]
    public static Result<Template> Parse(ImmutableArray<Token> tokens)
    {
        ImmutableArray<INode>.Builder nodesBuilder = ImmutableArray.CreateBuilder<INode>();

        foreach (Token t in tokens)
        {
            if (t.Kind == TokenKind.Text)
            {
                nodesBuilder.Add(new TextNode(t.Text, t.Range));
            }
        }

        Template template = new(nodesBuilder.ToImmutable());
        return Result<Template>.Ok(template);
    }
}