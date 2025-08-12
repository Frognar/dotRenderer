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
            switch (t.Kind)
            {
                case TokenKind.Text:
                    nodesBuilder.Add(Node.FromText(t.Text, t.Range));
                    break;
                case TokenKind.AtIdent:
                    nodesBuilder.Add(Node.FromInterpolateIdent(t.Text, t.Range));
                    break;
                case TokenKind.AtExpr:
                    Result<IExpr> parsed = ExprParser.Parse(t.Text);
                    if (!parsed.IsOk)
                    {
                        IError err = parsed.Error!;
                        return Result<Template>.Err(new ParseError(err.Code, t.Range, err.Message));
                    }

                    nodesBuilder.Add(Node.FromInterpolateExpr(parsed.Value, t.Range));
                    break;
            }
        }

        Template template = new(nodesBuilder.ToImmutable());
        return Result<Template>.Ok(template);
    }
}