using System.Text;

namespace dotRenderer;

public static class Renderer
{
    public static string Render(SequenceNode ast, IReadOnlyDictionary<string, string> model)
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
                    string key = e.Path.Last();
                    sb.Append(model.GetValueOrDefault(key, ""));
                    break;
            }
        }

        return sb.ToString();
    }
}