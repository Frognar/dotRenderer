using System.Globalization;

namespace dotRenderer;

public static class ExpressionParser
{
    public static ExprNode Parse(string expr)
    {
        ArgumentNullException.ThrowIfNull(expr);
        ReadOnlySpan<char> s = expr.AsSpan();

        (ExprNode node, int pos) = ParseExpression(s, 0);
        pos = SkipWhitespace(s, pos);

        return pos == s.Length
            ? node
            : throw new InvalidOperationException($"Unexpected token at end: '{expr[pos..]}'");
    }

    private static (ExprNode, int) ParseExpression(ReadOnlySpan<char> s, int pos) => ParseOr(s, pos);

    private static (ExprNode, int) ParseOr(ReadOnlySpan<char> s, int pos)
    {
        (ExprNode left, int p) = ParseAnd(s, pos);
        p = SkipWhitespace(s, p);
        while (TryMatch(s, p, "||", out int p2))
        {
            (ExprNode right, int p3) = ParseAnd(s, SkipWhitespace(s, p2));
            left = new BinaryExpr("||", left, right);
            p = SkipWhitespace(s, p3);
        }

        return (left, p);
    }

    private static (ExprNode, int) ParseAnd(ReadOnlySpan<char> s, int pos)
    {
        (ExprNode left, int p) = ParseEquality(s, pos);
        p = SkipWhitespace(s, p);
        while (TryMatch(s, p, "&&", out int p2))
        {
            (ExprNode right, int p3) = ParseEquality(s, SkipWhitespace(s, p2));
            left = new BinaryExpr("&&", left, right);
            p = SkipWhitespace(s, p3);
        }

        return (left, p);
    }

    private static (ExprNode, int) ParseEquality(ReadOnlySpan<char> s, int pos)
    {
        (ExprNode left, int p) = ParseRelational(s, pos);
        p = SkipWhitespace(s, p);
        while (true)
        {
            if (TryMatch(s, p, "==", out int pEq))
            {
                (ExprNode right, int pNext) = ParseRelational(s, SkipWhitespace(s, pEq));
                left = new BinaryExpr("==", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            if (TryMatch(s, p, "!=", out int pNe))
            {
                (ExprNode right, int pNext) = ParseRelational(s, SkipWhitespace(s, pNe));
                left = new BinaryExpr("!=", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            break;
        }

        return (left, p);
    }

    private static (ExprNode, int) ParseRelational(ReadOnlySpan<char> s, int pos)
    {
        (ExprNode left, int p) = ParseAdditive(s, pos);
        p = SkipWhitespace(s, p);
        while (true)
        {
            if (TryMatch(s, p, ">=", out int pGe))
            {
                (ExprNode right, int pNext) = ParseAdditive(s, SkipWhitespace(s, pGe));
                left = new BinaryExpr(">=", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            if (TryMatch(s, p, "<=", out int pLe))
            {
                (ExprNode right, int pNext) = ParseAdditive(s, SkipWhitespace(s, pLe));
                left = new BinaryExpr("<=", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            if (TryMatch(s, p, ">", out int pGt))
            {
                (ExprNode right, int pNext) = ParseAdditive(s, SkipWhitespace(s, pGt));
                left = new BinaryExpr(">", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            if (TryMatch(s, p, "<", out int pLt))
            {
                (ExprNode right, int pNext) = ParseAdditive(s, SkipWhitespace(s, pLt));
                left = new BinaryExpr("<", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            break;
        }

        return (left, p);
    }

    private static (ExprNode, int) ParseAdditive(ReadOnlySpan<char> s, int pos)
    {
        (ExprNode left, int p) = ParseMultiplicative(s, pos);
        p = SkipWhitespace(s, p);
        while (true)
        {
            if (TryMatch(s, p, "+", out int pPlus))
            {
                (ExprNode right, int pNext) = ParseMultiplicative(s, SkipWhitespace(s, pPlus));
                left = new BinaryExpr("+", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            if (TryMatch(s, p, "-", out int pMinus))
            {
                (ExprNode right, int pNext) = ParseMultiplicative(s, SkipWhitespace(s, pMinus));
                left = new BinaryExpr("-", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            break;
        }

        return (left, p);
    }

    private static (ExprNode, int) ParseMultiplicative(ReadOnlySpan<char> s, int pos)
    {
        (ExprNode left, int p) = ParseUnary(s, pos);
        p = SkipWhitespace(s, p);
        while (true)
        {
            if (TryMatch(s, p, "*", out int pMul))
            {
                (ExprNode right, int pNext) = ParseUnary(s, SkipWhitespace(s, pMul));
                left = new BinaryExpr("*", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            if (TryMatch(s, p, "/", out int pDiv))
            {
                (ExprNode right, int pNext) = ParseUnary(s, SkipWhitespace(s, pDiv));
                left = new BinaryExpr("/", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            if (TryMatch(s, p, "%", out int pMod))
            {
                (ExprNode right, int pNext) = ParseUnary(s, SkipWhitespace(s, pMod));
                left = new BinaryExpr("%", left, right);
                p = SkipWhitespace(s, pNext);
                continue;
            }

            break;
        }

        return (left, p);
    }

    private static (ExprNode, int) ParseUnary(ReadOnlySpan<char> s, int pos)
    {
        int p = SkipWhitespace(s, pos);
        if (TryMatch(s, p, "!", out int pNot))
        {
            (ExprNode operand, int pNext) = ParseUnary(s, pNot);
            return (new UnaryExpr("!", operand), pNext);
        }

        if (TryMatch(s, p, "-", out int pNeg))
        {
            (ExprNode operand, int pNext) = ParseUnary(s, pNeg);
            return (new UnaryExpr("-", operand), pNext);
        }

        return ParsePrimary(s, p);
    }

    private static (ExprNode, int) ParsePrimary(ReadOnlySpan<char> s, int pos)
    {
        int p = SkipWhitespace(s, pos);
        if (Peek(s, p) == '(')
        {
            p++;
            (ExprNode node, int pInner) = ParseExpression(s, p);
            pInner = SkipWhitespace(s, pInner);
            if (Peek(s, pInner) != ')')
            {
                throw new InvalidOperationException("Unclosed parenthesis");
            }

            return (node, pInner + 1);
        }

        if (TryMatch(s, p, "true", out int pTrue))
        {
            return (new LiteralExpr<bool>(true), pTrue);
        }

        if (TryMatch(s, p, "false", out int pFalse))
        {
            return (new LiteralExpr<bool>(false), pFalse);
        }

        if (Peek(s, p) == '"')
        {
            int start = p + 1;
            int i = start;
            while (i < s.Length && s[i] != '"')
            {
                i++;
            }

            if (i >= s.Length)
            {
                throw new InvalidOperationException("Unclosed string literal");
            }

            string lit = new string(s[start..i]);
            return (new LiteralExpr<string>(lit), i + 1);
        }

        if (char.IsDigit(Peek(s, p)))
        {
            int i = p;
            bool hasDot = false;
            bool hasExp = false;

            while (i < s.Length)
            {
                char c = s[i];
                if (char.IsDigit(c))
                {
                    i++;
                    continue;
                }

                if (c == '.')
                {
                    if (hasDot)
                    {
                        throw new InvalidOperationException("Multiple dots in number");
                    }

                    hasDot = true;
                    i++;
                    continue;
                }

                if ((c == 'e' || c == 'E') && !hasExp)
                {
                    hasExp = true;
                    i++;
                    if (i < s.Length && (s[i] == '+' || s[i] == '-'))
                    {
                        i++;
                    }

                    if (i >= s.Length || !char.IsDigit(Peek(s, i)))
                    {
                        throw new InvalidOperationException("Invalid scientific notation");
                    }

                    while (i < s.Length && char.IsDigit(s[i]))
                    {
                        i++;
                    }

                    continue;
                }

                break;
            }

            ReadOnlySpan<char> litSpan = s[p..i];
            if (hasDot || hasExp)
            {
                return (new LiteralExpr<double>(double.Parse(litSpan, CultureInfo.InvariantCulture)), i);
            }

            return (new LiteralExpr<int>(int.Parse(litSpan, CultureInfo.InvariantCulture)), i);
        }

        if (StartsWith(s, p, "Model."))
        {
            int i = p + "Model.".Length;
            List<string> segments = new() { "Model" };

            while (true)
            {
                int segStart = i;
                while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_'))
                {
                    i++;
                }

                if (segStart == i)
                {
                    throw new InvalidOperationException("Expected property segment after '.'");
                }

                segments.Add(new string(s[segStart..i]));

                if (i < s.Length && s[i] == '.')
                {
                    i++;
                    continue;
                }

                break;
            }

            return (new PropertyExpr(segments), i);
        }

        string remaining = new string(s[p..]);
        throw new InvalidOperationException($"Unknown token near: '{remaining}'");
    }

    private static int SkipWhitespace(ReadOnlySpan<char> s, int pos)
    {
        int i = pos;
        while (i < s.Length && char.IsWhiteSpace(s[i]))
        {
            i++;
        }

        return i;
    }

    private static bool TryMatch(ReadOnlySpan<char> s, int pos, string token, out int nextPos)
    {
        int p = SkipWhitespace(s, pos);
        if (StartsWith(s, p, token))
        {
            nextPos = p + token.Length;
            return true;
        }

        nextPos = pos;
        return false;
    }

    private static bool StartsWith(ReadOnlySpan<char> s, int pos, string token)
        => s[pos..].StartsWith(token.AsSpan(), StringComparison.Ordinal);

    private static char Peek(ReadOnlySpan<char> s, int pos) => pos < s.Length ? s[pos] : '\0';
}

public abstract record ExprNode;

public sealed record PropertyExpr(IReadOnlyList<string> Path) : ExprNode;

public sealed record LiteralExpr<T>(T Value) : ExprNode;

public sealed record UnaryExpr(string Operator, ExprNode Operand) : ExprNode;

public sealed record BinaryExpr(string Operator, ExprNode Left, ExprNode Right) : ExprNode;