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

                    int nameStart = pos + "@Model.".Length;
                    int nameEnd = nameStart;
                    while (nameEnd < len && IsIdentifierChar(template[nameEnd]))
                    {
                        nameEnd++;
                    }

                    var name = template[nameStart..nameEnd];
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new InvalidOperationException($"Brak identyfikatora po @Model. w pozycji {pos}");
                    }

                    tokens.Add(new InterpolationToken(new[] { "Model", name }));
                    pos = nameEnd;
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