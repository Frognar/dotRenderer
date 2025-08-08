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
            if (IsAtIf(template, pos) && (pos == 0 || char.IsWhiteSpace(template[pos - 1])))
            {
                if (sb.Length > 0)
                {
                    tokens.Add(new TextToken(sb.ToString()));
                    sb.Clear();
                }

                pos += "@if".Length;
                while (pos < end && char.IsWhiteSpace(template[pos]))
                {
                    pos++;
                }

                if (pos >= end || template[pos] != '(')
                {
                    throw new InvalidOperationException("Expected '(' after @if");
                }

                pos++;
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
                (int adjStart, int adjEnd) = TrimSingleNewlines(template, blockStart, blockEnd);
                IEnumerable<object> bodyTokens = Tokenize(template, adjStart, adjEnd);
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

    private static bool IsAtIf(string template, int pos)
        => template.AsSpan(pos).StartsWith("@if", StringComparison.Ordinal);

    private static (int start, int end) TrimSingleNewlines(string s, int start, int end)
    {
        if (start < end)
        {
            if (s[start] == '\r' && start + 1 < end && s[start + 1] == '\n')
            {
                start += 2;
            }
            else if (s[start] == '\n')
            {
                start += 1;
            }
        }

        if (start < end)
        {
            if (end >= start + 2 && s[end - 2] == '\r' && s[end - 1] == '\n')
            {
                end -= 2;
            }
            else if (s[end - 1] == '\n')
            {
                end -= 1;
            }
        }

        if (end < start)
        {
            end = start;
        }

        return (start, end);
    }

    private static (int start, int end, int nextPos) FindParenthesizedSpan(string template, int pos, int end)
        => FindDelimitedSpan(template, pos, end, '(', ')');

    private static (int start, int end, int nextPos) FindBracedSpan(string template, int pos, int end)
        => FindDelimitedSpan(template, pos, end, '{', '}');

    private static (int start, int end, int nextPos) FindDelimitedSpan(
        string s,
        int pos,
        int end,
        char open,
        char close)
    {
        int depth = 1;
        int start = pos;
        bool inString = false;
        bool escaped = false;

        while (pos < end)
        {
            char c = s[pos];
            if (inString)
            {
                if (!escaped && c == '\\')
                {
                    escaped = true;
                    pos++;
                    continue;
                }

                if (!escaped && c == '"')
                {
                    inString = false;
                    pos++;
                    continue;
                }

                escaped = false;
                pos++;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                pos++;
                continue;
            }

            if (c == open)
            {
                depth++;
            }
            else if (c == close)
            {
                depth--;
            }

            pos++;
            if (depth == 0)
            {
                break;
            }
        }

        if (depth != 0)
        {
            throw new InvalidOperationException(
                close == ')' ? "Unclosed @if condition: missing ')'" : "Unclosed @if block: missing '}'");
        }
        
        return (start, pos - 1, pos);
    }

    private static bool IsIdentifierChar(char c)
        => char.IsLetterOrDigit(c) || c == '_';
}

public sealed record TextToken(string Text);

public sealed record InterpolationToken(IEnumerable<string> Path);

public sealed record IfToken(string Condition, IEnumerable<object> Body);