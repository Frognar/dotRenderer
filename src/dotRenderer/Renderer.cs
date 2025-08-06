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
                    object value = ResolveOrThrow(model, e.Path);
                    sb.Append(value);
                    break;
            }
        }

        return sb.ToString();
    }

    public static string Render<TModel>(SequenceNode ast, TModel model, IValueAccessor<TModel> accessor)
    {
        ArgumentNullException.ThrowIfNull(ast);
        ArgumentNullException.ThrowIfNull(accessor);
        StringBuilder sb = new();
        foreach (Node node in ast.Children)
        {
            switch (node)
            {
                case TextNode t:
                    sb.Append(t.Text);
                    break;
                case EvalNode e:
                    string[] path = e.Path.Skip(1).ToArray();
                    string last = path.Last();
                    string value = accessor.AccessValue(last, model)
                        ?? throw new KeyNotFoundException(
                            $"Missing key '{last}' in model (path: {string.Join(".", path)}");
                        
                    sb.Append(value);
                    break;
            }
        }

        return sb.ToString();
    }

    private static object ResolveOrThrow(IReadOnlyDictionary<string, object> model, IEnumerable<string> path)
    {
        object? current = model;
        foreach (var segment in path.Skip(1))
        {
            switch (current)
            {
                case IReadOnlyDictionary<string, object> dict:
                {
                    if (!dict.TryGetValue(segment, out current))
                    {
                        throw new KeyNotFoundException(
                            $"Missing key '{segment}' in model (path: {string.Join(".", path)})");
                    }

                    break;
                }
                case Dictionary<string, string> leafDict:
                {
                    if (!leafDict.TryGetValue(segment, out var strVal))
                    {
                        throw new KeyNotFoundException(
                            $"Missing key '{segment}' in model (path: {string.Join(".", path)})");
                    }

                    current = strVal;
                    break;
                }
                default:
                    throw new KeyNotFoundException(
                        $"Missing key '{segment}' in model (path: {string.Join(".", path)})");
            }
        }

        return current;
    }
}