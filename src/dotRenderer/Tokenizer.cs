namespace dotRenderer;

public static class Tokenizer
{
    public static IEnumerable<object> Tokenize(string template)
    {
        ArgumentException.ThrowIfNullOrEmpty(template);
        List<object> tokens = [];
        int pos = 0;
        int len = template.Length;

        while (pos < len)
        {
            int at = template.IndexOf("@Model.", pos, StringComparison.Ordinal);
            if (at == -1)
            {
                if (pos < len)
                {
                    tokens.Add(new TextToken(template[pos..]));
                }

                break;
            }

            if (at > pos)
            {
                tokens.Add(new TextToken(template[pos..at]));
            }

            int nameStart = at + "@Model.".Length;
            int nameEnd = nameStart;
            while (nameEnd < len && IsIdentifierChar(template[nameEnd]))
            {
                nameEnd++;
            }

            string name = template[nameStart..nameEnd];
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException($"No identifier after @Model. at position {at}");
            }

            tokens.Add(new InterpolationToken(["Model", name]));
            pos = nameEnd;
        }

        return tokens;
    }

    private static bool IsIdentifierChar(char c)
        => char.IsLetterOrDigit(c) || c == '_';
}

public sealed record TextToken(string Text);

public sealed record InterpolationToken(IEnumerable<string> Path);