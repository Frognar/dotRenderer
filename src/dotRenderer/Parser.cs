namespace dotRenderer;

public static class Parser
{
    public static SequenceNode Parse(IEnumerable<object> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        List<Node> children = [];
        foreach (object token in tokens)
        {
            switch (token)
            {
                case TextToken t:
                    children.Add(new TextNode(t.Text));
                    break;
                case InterpolationToken i:
                    children.Add(new EvalNode(i.Path));
                    break;
                case IfToken i:
                    SequenceNode body = Parse(i.Body);
                    ExprNode cond = ExpressionParser.Parse(i.Condition);
                    children.Add(new IfNode(cond, body));
                    break;
                case OutExprToken o:
                    ExprNode outExpr = ExpressionParser.Parse(o.Expression);
                    children.Add(new OutExprNode(outExpr));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown token of type {token.GetType().Name}");
            }
        }

        return new SequenceNode(children);
    }
}

public abstract record Node;

public sealed record SequenceNode(IReadOnlyList<Node> Children) : Node;

public sealed record TextNode(string Text) : Node;

public sealed record EvalNode(IEnumerable<string> Path) : Node;

public sealed record IfNode(ExprNode Condition, SequenceNode Body) : Node;

public sealed record OutExprNode(ExprNode Expression) : Node;