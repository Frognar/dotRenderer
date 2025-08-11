using System.Diagnostics.Contracts;
using System.Text;

namespace DotRenderer;

public static class Renderer
{
    [Pure]
    public static Result<string> Render(Template template)
    {
        template ??= new Template([]);
        StringBuilder sb = new();

        foreach (INode node in template.Children)
        {
            if (node is TextNode textNode)
            {
                sb.Append(textNode.Text);
            }
        }
        
        return Result<string>.Ok(sb.ToString());
    }
}