using System.Text;

namespace dotRenderer;

public static class Renderer
{
    public static string Render(SequenceNode ast, IReadOnlyDictionary<string, object> model)
    {
        ArgumentNullException.ThrowIfNull(ast);
        StringBuilder sb = new();
        foreach (Node node in ast.Children)
        {
            switch (node)
            {
                case TextNode t:
                    sb.Append(t.Text);
                    break;
                case EvalNode e:
                    object? value = TryResolve(model, e.Path);
                    sb.Append(value?.ToString() ?? "");
                    break;
            }
        }

        return sb.ToString();
    }

    private static object? TryResolve(IReadOnlyDictionary<string, object> model, IEnumerable<string> path)
    {
        object? current = model;
        foreach (string segment in path.Skip(1))
        {
            switch (current)
            {
                case IReadOnlyDictionary<string, object> dict:
                    dict.TryGetValue(segment, out current);
                    break;
                case Dictionary<string, string> leafDict when
                    leafDict.TryGetValue(segment, out string? strVal):
                    current = strVal;
                    break;
                default:
                    return null;
            }
        }

        return current;
    }
}