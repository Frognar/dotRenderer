using System.Text;

namespace dotRenderer;

public static class Tokenizer
{
    public static IEnumerable<object> Tokenize(string template)
    {
        ArgumentException.ThrowIfNullOrEmpty(template);

        var tokens = new List<object>();
        int pos = 0;
        int len = template.Length;
        StringBuilder sb = new();

        while (pos < len)
        {
            if (template[pos] == '@')
            {
                if (pos + 1 < len && template[pos + 1] == '@')
                {
                    sb.Append('@');
                    pos += 2;
                    continue;
                }

                if (template.IndexOf("@Model.", pos, StringComparison.Ordinal) == pos)
                {
                    if (sb.Length > 0)
                    {
                        tokens.Add(new TextToken(sb.ToString()));
                        sb.Clear();
                    }

                    int pathStart = pos + "@Model.".Length;
                    int pathEnd = pathStart;
                    List<string> segments = ["Model"];
                    while (true)
                    {
                        int segStart = pathEnd;
                        while (pathEnd < len && IsIdentifierChar(template[pathEnd]))
                        {
                            pathEnd++;
                        }
                        
                        var name = template[segStart..pathEnd];
                        if (string.IsNullOrEmpty(name))
                        {
                            throw new InvalidOperationException($"No identifier after @Model. at position {pos}");
                        }
                        
                        segments.Add(name);

                        if (pathEnd < len && template[pathEnd] == '.')
                        {
                            pathEnd++;
                            continue;
                        }
                        
                        break;
                    }

                    tokens.Add(new InterpolationToken(segments));
                    pos = pathEnd;
                    continue;
                }

                sb.Append('@');
                pos++;
                continue;
            }

            sb.Append(template[pos]);
            pos++;
        }

        if (sb.Length > 0)
        {
            tokens.Add(new TextToken(sb.ToString()));
        }

        return tokens;
    }

    private static bool IsIdentifierChar(char c)
        => char.IsLetterOrDigit(c) || c == '_';
}

public sealed record TextToken(string Text);

public sealed record InterpolationToken(IEnumerable<string> Path);