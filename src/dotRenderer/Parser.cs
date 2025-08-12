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
            INode? node = t.Kind switch
            {
                TokenKind.Text => Node.FromText(t.Text, t.Range),
                TokenKind.AtIdent => Node.FromInterpolateIdent(t.Text, t.Range),
                _ => null,
            };

            if (node is not null)
            {
                nodesBuilder.Add(node);
            }
        }

        Template template = new(nodesBuilder.ToImmutable());
        return Result<Template>.Ok(template);
    }
}