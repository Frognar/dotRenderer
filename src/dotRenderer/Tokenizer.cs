using System.Text;

namespace dotRenderer;

public static class Tokenizer
{
    public static IEnumerable<object> Tokenize(string template)
        => Tokenize(template ?? throw new ArgumentNullException(nameof(template)), 0, template.Length);

    private static List<object> Tokenize(string template, int start, int end)
    {
        List<object> tokens = [];
        StringBuilder sb = new();
        int pos = start;

        while (pos < end)
        {
            if (template.IndexOf("@if (", pos, StringComparison.Ordinal) == pos
                && (pos == 0 || char.IsWhiteSpace(template[pos - 1])))
            {
                if (sb.Length > 0)
                {
                    tokens.Add(new TextToken(sb.ToString()));
                    sb.Clear();
                }

                pos += "@if (".Length;
                (int condStart, int condEnd, pos) = FindParenthesizedSpan(template, pos, end);
                string condition = template[condStart..condEnd].Trim();
                while (pos < end && char.IsWhiteSpace(template[pos]))
                {
                    pos++;
                }

                if (pos >= end || template[pos] != '{')
                {
                    throw new InvalidOperationException("Expected '{' after @if condition");
                }

                pos++;
                (int blockStart, int blockEnd, pos) = FindBracedSpan(template, pos, end);
                IEnumerable<object> bodyTokens = Tokenize(template, blockStart, blockEnd);
                tokens.Add(new IfToken(condition, bodyTokens));
                continue;
            }

            if (template[pos] == '@' && pos + 1 < end && template[pos + 1] == '@')
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
                    while (pathEnd < end && IsIdentifierChar(template[pathEnd]))
                    {
                        pathEnd++;
                    }

                    string name = template[segStart..pathEnd];
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new InvalidOperationException($"No identifier after @Model. at position {pos}");
                    }

                    segments.Add(name);

                    if (pathEnd < end && template[pathEnd] == '.')
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

            sb.Append(template[pos]);
            pos++;
        }

        if (sb.Length > 0)
        {
            tokens.Add(new TextToken(sb.ToString()));
        }

        return tokens;
    }

    private static (int start, int end, int nextPos) FindParenthesizedSpan(string template, int pos, int end)
    {
        int parens = 1;
        int start = pos;
        while (pos < end && parens > 0)
        {
            parens = template[pos] switch
            {
                '(' => parens + 1,
                ')' => parens - 1,
                _ => parens,
            };

            pos++;
        }

        if (parens != 0)
        {
            throw new InvalidOperationException("Unclosed @if condition: missing ')'");
        }

        int finish = pos - 1;
        return (start, finish, pos);
    }

    private static (int start, int end, int nextPos) FindBracedSpan(string template, int pos, int end)
    {
        int braces = 1;
        int start = pos;
        while (pos < end && braces > 0)
        {
            braces = template[pos] switch
            {
                '{' => braces + 1,
                '}' => braces -1,
                _ => braces,
            };

            pos++;
        }

        if (braces != 0)
        {
            throw new InvalidOperationException("Unclosed @if block: missing '}'");
        }

        int finish = pos - 1;
        return (start, finish, pos);
    }

    private static bool IsIdentifierChar(char c)
        => char.IsLetterOrDigit(c) || c == '_';
}

public sealed record TextToken(string Text);

public sealed record InterpolationToken(IEnumerable<string> Path);

public sealed record IfToken(string Condition, IEnumerable<object> Body);