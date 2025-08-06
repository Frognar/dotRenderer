using System.Text;

namespace dotRenderer;

public static class Tokenizer
{
    public static IEnumerable<object> Tokenize(string template)
    {
        ArgumentException.ThrowIfNullOrEmpty(template);

        List<object> tokens = [];
        int pos = 0;
        int len = template.Length;
        StringBuilder sb = new();

        while (pos < len)
        {
            if (template.IndexOf("@if (", pos, StringComparison.Ordinal) == pos)
            {
                if (sb.Length > 0)
                {
                    tokens.Add(new TextToken(sb.ToString()));
                    sb.Clear();
                }

                pos += "@if (".Length;
                int condEnd = template.IndexOf(')', pos);
                if (condEnd == -1)
                {
                    throw new InvalidOperationException("Unclosed @if condition: missing ')'");
                }

                string condition = template.Substring(pos, condEnd - pos).Trim();
                pos = condEnd + 1;

                while (pos < len && char.IsWhiteSpace(template[pos])) pos++;
                if (pos >= len || template[pos] != '{')
                {
                    throw new InvalidOperationException("Expected '{' after @if condition");
                }

                int bodyStart = pos + 1;
                int bodyEnd = template.IndexOf('}', bodyStart);
                if (bodyEnd == -1)
                {
                    throw new InvalidOperationException("Unclosed @if block: missing '}'");
                }

                string bodyContent = template.Substring(bodyStart, bodyEnd - bodyStart);

                List<object> bodyTokens = [];
                if (!string.IsNullOrEmpty(bodyContent))
                {
                    if (bodyContent.Contains("@Model.", StringComparison.Ordinal))
                    {
                        int at = bodyContent.IndexOf("@Model.", StringComparison.Ordinal);
                        if (at > 0)
                        {
                            bodyTokens.Add(new TextToken(bodyContent[..at]));
                        }

                        bodyTokens.Add(new InterpolationToken(["Model", "Name"]));
                    }
                    else
                    {
                        bodyTokens.Add(new TextToken(bodyContent));
                    }
                }

                tokens.Add(new IfToken(condition, bodyTokens));
                pos = bodyEnd + 1;
                continue;
            }

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

                        string name = template[segStart..pathEnd];
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

public sealed record IfToken(string Condition, IEnumerable<object> Body);