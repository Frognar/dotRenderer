using System.Diagnostics.Contracts;
using System.Text;

namespace DotRenderer;

public static class Renderer
{
    [Pure]
    public static Result<string> Render(Template template) => Render(template, null);

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
                    return Result<string>.Err(new EvalError("MissingIdent", identNode.Range, $"Identifier '{identNode.Name}' was not found."));
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
                        return Result<string>.Err(new EvalError("TypeMismatch", identNode.Range, $"Identifier '{identNode.Name}' is not a scalar value."));
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
                        return Result<string>.Err(new EvalError("TypeMismatch", exprNode.Range, "Expression did not evaluate to a scalar value."));
                }

                continue;
            }
        }
        
        return Result<string>.Ok(sb.ToString());
    }
}