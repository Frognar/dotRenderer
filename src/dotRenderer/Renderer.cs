using System.Diagnostics.Contracts;
using System.Text;

namespace DotRenderer;

public static class Renderer
{
    [Pure]
    public static Result<string> Render(Template template) => Render(template, null);

    [Pure]
    public static Func<Template, Result<string>> RenderWithAccessor(IValueAccessor? accessor) =>
        template => Render(template, accessor);

    [Pure]
    public static Result<string> Render(Template template, IValueAccessor? globals)
    {
        template ??= new Template([]);
        StringBuilder sb = new();

        foreach (INode node in template.Children)
        {
            if (node is TextNode textNode)
            {
                sb.Append(textNode.Text);
            }

            if (node is InterpolateIdentNode identNode)
            {
                if (globals is null)
                {
                    return Result<string>.Err(new EvalError("MissingIdent", identNode.Range,
                        $"Identifier '{identNode.Name}' was not found."));
                }

                Result<Value> got = Evaluator.EvaluateIdent(globals, identNode.Name, identNode.Range);
                if (!got.IsOk)
                {
                    return Result<string>.Err(got.Error!);
                }

                Value value = got.Value;

                switch (value.Kind)
                {
                    case ValueKind.Text:
                    case ValueKind.Number:
                    case ValueKind.Boolean:
                        sb.Append(value.ToInvariantString());
                        break;
                    default:
                        return Result<string>.Err(new EvalError("TypeMismatch", identNode.Range,
                            $"Identifier '{identNode.Name}' is not a scalar value."));
                }

                continue;
            }

            if (node is InterpolateExprNode exprNode)
            {
                IValueAccessor accessor = globals ?? MapAccessor.Empty;

                Result<Value> got = Evaluator.EvaluateExpr(exprNode.Expr, accessor, exprNode.Range);
                if (!got.IsOk)
                {
                    return Result<string>.Err(got.Error!);
                }

                Value value = got.Value;

                switch (value.Kind)
                {
                    case ValueKind.Text:
                    case ValueKind.Number:
                    case ValueKind.Boolean:
                        sb.Append(value.ToInvariantString());
                        break;
                    default:
                        return Result<string>.Err(new EvalError("TypeMismatch", exprNode.Range,
                            "Expression did not evaluate to a scalar value."));
                }

                continue;
            }

            if (node is IfNode ifNode)
            {
                IValueAccessor accessorForIf = globals ?? MapAccessor.Empty;
                Result<Value> cond = Evaluator.EvaluateExpr(ifNode.Condition, accessorForIf, ifNode.Range);
                if (!cond.IsOk)
                {
                    return Result<string>.Err(cond.Error!);
                }

                Value cv = cond.Value;
                if (cv.Kind != ValueKind.Boolean)
                {
                    return Result<string>.Err(new EvalError("TypeMismatch", ifNode.Range,
                        "Condition of @if must be boolean."));
                }

                bool isTrue = cv.Boolean;
                if (isTrue)
                {
                    Result<string> thenRendered = Render(new Template(ifNode.Then), accessorForIf);
                    if (!thenRendered.IsOk)
                    {
                        return thenRendered;
                    }

                    sb.Append(thenRendered.Value);
                }
                else if (ifNode.Else.Length > 0)
                {
                    Result<string> elseRendered = Render(new Template(ifNode.Else), accessorForIf);
                    if (!elseRendered.IsOk)
                    {
                        return elseRendered;
                    }

                    sb.Append(elseRendered.Value);
                }

                continue;
            }
        }

        return Result<string>.Ok(sb.ToString());
    }
}