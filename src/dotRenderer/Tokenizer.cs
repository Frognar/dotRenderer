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
            (IfToken token, int nextPos)? ifMatch = TryParseIf(template, pos, end);
            if (ifMatch is { } m1)
            {
                FlushPendingText(sb, tokens);
                tokens.Add(m1.token);
                pos = m1.nextPos;
                continue;
            }

            (OutExprToken token, int nextPos)? outExprMatch = TryParseOutputExpr(template, pos, end);
            if (outExprMatch is { } mOut)
            {
                FlushPendingText(sb, tokens);
                tokens.Add(mOut.token);
                pos = mOut.nextPos;
                continue;
            }

            int? afterEscape = TryParseEscapedAt(template, pos, end);
            if (afterEscape is { } m2)
            {
                sb.Append('@');
                pos = m2;
                continue;
            }

            (InterpolationToken token, int nextPos)? interpMatch = TryParseInterpolation(template, pos, end);
            if (interpMatch is { } m3)
            {
                FlushPendingText(sb, tokens);
                tokens.Add(m3.token);
                pos = m3.nextPos;
                continue;
            }

            sb.Append(template[pos]);
            pos++;
        }

        FlushPendingText(sb, tokens);
        return tokens;
    }

    private static (IfToken token, int nextPos)? TryParseIf(string s, int pos, int end)
    {
        if (!StartsWithAtIf(s, pos, end))
        {
            return null;
        }

        int p = pos + 3;
        p = SkipWhitespace(s, p, end);

        if (p >= end || s[p] != '(')
        {
            throw new InvalidOperationException("Expected '(' after @if");
        }

        p++;
        (int condStart, int condEnd, int afterCond) = FindParenthesizedSpan(s, p, end);
        string condition = s[condStart..condEnd].Trim();

        int p2 = SkipWhitespace(s, afterCond, end);
        if (p2 >= end || s[p2] != '{')
        {
            throw new InvalidOperationException("Expected '{' after @if condition");
        }

        p2++;
        (int bodyStart, int bodyEnd, int afterBody) = FindBracedSpan(s, p2, end);
        IEnumerable<object> bodyTokens = Tokenize(s, bodyStart, bodyEnd);

        return (new IfToken(condition, bodyTokens), afterBody);
    }

    private static (OutExprToken token, int nextPos)? TryParseOutputExpr(string s, int pos, int end)
    {
        if (pos + 1 >= end || s[pos] != '@' || s[pos + 1] != '(')
        {
            return null;
        }

        int p = pos + 2;
        (int exprStart, int exprEnd, int after) = FindParenthesizedSpan(s, p, end);
        string expr = s[exprStart..exprEnd].Trim();
        return (new OutExprToken(expr), after);

    }

    private static int? TryParseEscapedAt(string s, int pos, int end)
        => pos + 1 < end && s[pos] == '@' && s[pos + 1] == '@' ? pos + 2 : null;

    private static (InterpolationToken token, int nextPos)? TryParseInterpolation(string s, int pos, int end)
    {
        const string prefix = "@Model.";
        if (!s.AsSpan(pos).StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        int p = pos + prefix.Length;
        List<string> segments = ["Model"];

        while (true)
        {
            int segStart = p;
            while (p < end && IsIdentifierChar(s[p]))
            {
                p++;
            }

            string name = s[segStart..p];
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException($"No identifier after @Model. at position {pos}");
            }

            segments.Add(name);
            if (p < end && s[p] == '.')
            {
                p++;
                continue;
            }

            break;
        }

        return (new InterpolationToken(segments), p);
    }

    private static void FlushPendingText(StringBuilder sb, List<object> tokens)
    {
        if (sb.Length > 0)
        {
            tokens.Add(new TextToken(sb.ToString()));
            sb.Clear();
        }
    }

    private static bool StartsWithAtIf(string s, int pos, int end)
        => pos + 3 <= end && s.AsSpan(pos, 3).SequenceEqual("@if".AsSpan());

    private static int SkipWhitespace(string s, int pos, int end)
    {
        while (pos < end && char.IsWhiteSpace(s[pos]))
        {
            pos++;
        }

        return pos;
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
public sealed record OutExprToken(string Expression);
