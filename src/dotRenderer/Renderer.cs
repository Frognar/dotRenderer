using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;

namespace DotRenderer;

public static class Renderer
{
    [Pure]
    public static Func<Template, Result<string>> RenderWithAccessor(IValueAccessor? accessor) =>
        template => Render(template, accessor);

    [Pure]
    public static Result<string> Render(Template template, IValueAccessor? globals)
    {
        template ??= new Template([]);
        return RenderChildren(template.Children, globals);
    }

    private static Result<string> RenderChildren(ImmutableArray<INode> nodes, IValueAccessor? globals)
    {
        StringBuilder sb = new();
        bool pendingSkipLeadingNlFromNext = false;

        foreach (INode node in nodes)
        {
            Result<string> partRes = RenderNode(node, globals);
            if (!partRes.IsOk)
            {
                return partRes;
            }

            string part = partRes.Value;

            if (pendingSkipLeadingNlFromNext && part.Length > 0)
            {
                part = StripOneLeadingNewline(part);
                pendingSkipLeadingNlFromNext = false;
            }

            if (IsBlockNode(node) && part.Length == 0)
            {
                bool trimmedPrev = TrimOneTrailingNewline(sb);
                if (!trimmedPrev)
                {
                    pendingSkipLeadingNlFromNext = true;
                }

                continue;
            }

            sb.Append(part);
        }

        return Result<string>.Ok(sb.ToString());
    }

    private static Result<string> RenderNode(INode node, IValueAccessor? globals) =>
        node switch
        {
            TextNode t => Result<string>.Ok(t.Text),
            InterpolateIdentNode id => RenderInterpolateIdent(id, globals),
            InterpolateExprNode ex => RenderInterpolateExpr(ex, globals),
            IfNode i => RenderIfNode(i, globals),
            ForNode f => RenderForNode(f, globals),
            _ => Result<string>.Ok(string.Empty)
        };

    private static Result<string> RenderInterpolateIdent(InterpolateIdentNode node, IValueAccessor? globals)
    {
        if (globals is null)
        {
            return Result<string>.Err(
                new EvalError("MissingIdent", node.Range, $"Identifier '{node.Name}' was not found."));
        }

        Result<Value> got = Evaluator.EvaluateIdent(globals, node.Name, node.Range);
        if (!got.IsOk)
        {
            return Result<string>.Err(got.Error!);
        }

        return ScalarToString(got.Value, node.Range, $"Identifier '{node.Name}' is not a scalar value.");
    }

    private static Result<string> RenderInterpolateExpr(InterpolateExprNode node, IValueAccessor? globals)
    {
        IValueAccessor accessor = globals ?? MapAccessor.Empty;
        Result<Value> got = Evaluator.EvaluateExpr(node.Expr, accessor, node.Range);
        if (!got.IsOk)
        {
            return Result<string>.Err(got.Error!);
        }

        return ScalarToString(got.Value, node.Range, "Expression did not evaluate to a scalar value.");
    }

    private static Result<string> RenderIfNode(IfNode node, IValueAccessor? globals)
    {
        IValueAccessor accessor = globals ?? MapAccessor.Empty;
        Result<Value> cond = Evaluator.EvaluateExpr(node.Condition, accessor, node.Range);
        if (!cond.IsOk)
        {
            return Result<string>.Err(cond.Error!);
        }

        Value cv = cond.Value;
        if (cv.Kind != ValueKind.Boolean)
        {
            return Result<string>.Err(
                new EvalError("TypeMismatch", node.Range, "Condition of @if must be boolean."));
        }

        bool isTrue = cv.Boolean;
        if (isTrue)
        {
            return RenderChildren(node.Then, accessor)
                .Map(TrimOneOuterNewline);
        }

        if (node.Else.Length > 0)
        {
            return RenderChildren(node.Else, accessor)
                .Map(TrimOneOuterNewline);
        }

        return Result<string>.Ok(string.Empty);
    }

    private static Result<string> RenderForNode(ForNode node, IValueAccessor? globals)
    {
        IValueAccessor accessor = globals ?? MapAccessor.Empty;
        Result<Value> seqRes = Evaluator.EvaluateExpr(node.Seq, accessor, node.Range);
        if (!seqRes.IsOk)
        {
            return Result<string>.Err(seqRes.Error!);
        }

        Value seqVal = seqRes.Value;
        if (seqVal.Kind != ValueKind.Sequence)
        {
            return Result<string>.Err(new EvalError(
                "TypeMismatch", node.Range,
                $"Expression of @for must evaluate to a sequence, but got {seqVal.Kind}."));
        }

        ImmutableArray<Value> items = seqVal.Sequence;
        if (items.Length == 0)
        {
            return node.Else.Length > 0
                ? RenderChildren(node.Else, accessor)
                : Result<string>.Ok(string.Empty);
        }

        StringBuilder sb = new();
        int index = 0;
        foreach (Value item in items)
        {
            IValueAccessor scoped = new ChainAccessor(accessor, node.Item, item);
            if (node.Index is not null)
            {
                scoped = new ChainAccessor(scoped, node.Index, Value.FromNumber(index));
            }

            Value loop = BuildLoopValue(index, items.Length);
            scoped = new ChainAccessor(scoped, "loop", loop);
            Result<string> body = RenderChildren(node.Body, scoped);
            if (!body.IsOk)
            {
                return body;
            }

            sb.Append(body.Value);
            index++;
        }

        return Result<string>.Ok(sb.ToString());
    }

    private static Value BuildLoopValue(int index, int count) =>
        Value.FromMap(new Dictionary<string, Value>
        {
            ["index"] = Value.FromNumber(index),
            ["count"] = Value.FromNumber(count),
            ["isFirst"] = Value.FromBool(index == 0),
            ["isLast"] = Value.FromBool(index == count - 1),
            ["isOdd"] = Value.FromBool((index & 1) == 1),
            ["isEven"] = Value.FromBool((index & 1) == 0),
        });

    private static Result<string> ScalarToString(Value value, TextSpan range, string notScalarMessage) =>
        value.Kind switch
        {
            ValueKind.Text or ValueKind.Number or ValueKind.Boolean
                => Result<string>.Ok(value.ToInvariantString()),
            _ => Result<string>.Err(new EvalError("TypeMismatch", range, notScalarMessage))
        };

    private static bool IsBlockNode(INode node) => node is IfNode or ForNode;

    private static string TrimOneOuterNewline(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        if (s.StartsWith("\r\n", StringComparison.Ordinal))
        {
            s = s[2..];
        }
        else if (s.Length > 0 && s[0] == '\n')
        {
            s = s[1..];
        }

        if (s.EndsWith("\r\n", StringComparison.Ordinal))
        {
            s = s[..^2];
        }
        else if (s.Length > 0 && s[^1] == '\n')
        {
            s = s[..^1];
        }

        return s;
    }

    private static bool TrimOneTrailingNewline(StringBuilder sb)
    {
        if (sb.Length == 0)
        {
            return false;
        }

        int len = sb.Length;
        if (len >= 2 && sb[len - 2] == '\r' && sb[len - 1] == '\n')
        {
            sb.Length = len - 2;
            return true;
        }

        if (sb[len - 1] == '\n')
        {
            sb.Length = len - 1;
            return true;
        }

        return false;
    }

    private static string StripOneLeadingNewline(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }

        if (s.StartsWith("\r\n", StringComparison.Ordinal))
        {
            return s[2..];
        }

        if (s[0] == '\n')
        {
            return s[1..];
        }

        return s;
    }
}