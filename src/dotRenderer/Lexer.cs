using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace DotRenderer;

public static class Lexer
{
    [Pure]
    public static Result<ImmutableArray<Token>> Lex(string template)
    {
        template ??= string.Empty;
        List<Token> tokens = [];
        int length = template.Length;
        int i = 0;
        int textStart = 0;
        while (i < length)
        {
            char ch = template[i];
            if (ch == '@')
            {
                (bool matched, int newI, int newTextStart, List<Token> emitted) =
                    TryLexAtSequence(template, i, textStart);

                if (matched)
                {
                    tokens.AddRange(emitted);
                    i = newI;
                    textStart = newTextStart;
                    continue;
                }

                i++;
                continue;
            }

            if (ch == 'e')
            {
                (bool matched, int newI, int newTextStart, List<Token> emitted) = TryLexElse(template, i, textStart);
                if (matched)
                {
                    tokens.AddRange(emitted);
                    i = newI;
                    textStart = newTextStart;
                    continue;
                }
            }

            if (ch is '{' or '}')
            {
                (int newI, int newTextStart, List<Token> emitted) = LexBrace(template, i, textStart, ch);
                tokens.AddRange(emitted);
                i = newI;
                textStart = newTextStart;
                continue;
            }

            i++;
        }

        Token? tail = FlushTextIfAny(template, textStart, i);
        if (tail.HasValue)
        {
            tokens.Add(tail.Value);
        }

        return Result<ImmutableArray<Token>>.Ok([.. tokens]);
    }

    [Pure]
    private static (bool matched, int newI, int newTextStart, List<Token> emitted) TryLexAtSequence(
        string s,
        int i,
        int textStart)
    {
        Token? flush = FlushTextIfAny(s, textStart, i);
        (bool matched, int newI, Token token) atExpr = TryLexAtExpr(s, i);
        if (atExpr.matched)
        {
            List<Token> emitted = [];
            if (flush.HasValue)
            {
                emitted.Add(flush.Value);
            }

            emitted.Add(atExpr.token);
            return (true, atExpr.newI, atExpr.newI, emitted);
        }

        (bool matched, int newI, Token token) atAt = TryLexDoubleAt(s, i);
        if (atAt.matched)
        {
            List<Token> emitted = [];
            if (flush.HasValue)
            {
                emitted.Add(flush.Value);
            }

            emitted.Add(atAt.token);
            return (true, atAt.newI, atAt.newI, emitted);
        }

        (bool matched, int newI, Token token) atIf = TryLexAtIf(s, i);
        if (atIf.matched)
        {
            List<Token> emitted = [];
            if (flush.HasValue)
            {
                emitted.Add(flush.Value);
            }

            emitted.Add(atIf.token);
            return (true, atIf.newI, atIf.newI, emitted);
        }

        (bool matched, int newI, Token token) atFor = TryLexAtFor(s, i);
        if (atFor.matched)
        {
            List<Token> emitted = [];
            if (flush.HasValue)
            {
                emitted.Add(flush.Value);
            }

            emitted.Add(atFor.token);
            return (true, atFor.newI, atFor.newI, emitted);
        }

        (bool matched, int newI, Token token) atIdent = TryLexAtIdent(s, i);
        if (atIdent.matched)
        {
            List<Token> emitted = [];
            if (flush.HasValue)
            {
                emitted.Add(flush.Value);
            }

            emitted.Add(atIdent.token);
            return (true, atIdent.newI, atIdent.newI, emitted);
        }

        return (false, i + 1, textStart, []);
    }

    [Pure]
    private static (bool matched, int newI, Token token) TryLexAtExpr(string s, int i)
    {
        int n = s.Length;
        int j = i + 1;
        if (j < n && s[j] == '(')
        {
            (bool ok, int closeExclusive) paren = TryScanParen(s, j);
            if (paren.ok)
            {
                int closeExcl = paren.closeExclusive;
                string expr = s[(j + 1)..(closeExcl - 1)];
                return (true, closeExcl, Token.FromAtExpr(expr, TextSpan.At(i, closeExcl - i)));
            }
        }

        return (false, i, default);
    }

    [Pure]
    private static (bool matched, int newI, Token token) TryLexDoubleAt(string s, int i)
    {
        int n = s.Length;
        int j = i + 1;
        if (j < n && s[j] == '@')
        {
            return (true, j + 1, Token.FromText("@", TextSpan.At(i, 2)));
        }

        return (false, i, default);
    }

    [Pure]
    private static (bool matched, int newI, Token token) TryLexAtIf(string s, int i)
    {
        int n = s.Length;
        int j = i + 1;
        if (j + 1 < n && s[j] == 'i' && s[j + 1] == 'f')
        {
            int k = SkipWs(s, j + 2, n);
            if (k < n && s[k] == '(')
            {
                (bool ok, int closeExclusive) paren = TryScanParen(s, k);
                if (paren.ok)
                {
                    int closeExcl = paren.closeExclusive;
                    string expr = s[(k + 1)..(closeExcl - 1)];
                    return (true, closeExcl, Token.FromAtIf(expr, TextSpan.At(i, closeExcl - i)));
                }
            }
        }

        return (false, i, default);
    }

    [Pure]
    private static (bool matched, int newI, Token token) TryLexAtFor(string s, int i)
    {
        int n = s.Length;
        int j = i + 1;
        if (j + 2 < n && s.AsSpan(j, 3).SequenceEqual("for".AsSpan()))
        {
            int k = SkipWs(s, j + 3, n);
            if (k < n && s[k] == '(')
            {
                (bool ok, int closeExclusive) paren = TryScanParen(s, k);
                if (paren.ok)
                {
                    int closeExcl = paren.closeExclusive;
                    string header = s[(k + 1)..(closeExcl - 1)];
                    return (true, closeExcl, Token.FromAtFor(header, TextSpan.At(i, closeExcl - i)));
                }
            }
        }

        return (false, i, default);
    }

    [Pure]
    private static (bool matched, int newI, Token token) TryLexAtIdent(string s, int i)
    {
        int n = s.Length;
        int j = i + 1;
        if (j < n && IsIdentStart(s[j]))
        {
            int k = j + 1;
            while (k < n && IsIdentPart(s[k]))
            {
                k++;
            }

            string name = s[j..k];
            return (true, k, Token.FromAtIdent(name, TextSpan.At(i, 1 + (k - j))));
        }

        return (false, i, default);
    }

    [Pure]
    private static (bool matched, int newI, int newTextStart, List<Token> emitted) TryLexElse(
        string s,
        int i,
        int textStart)
    {
        if (IsElseKeyword(s, i))
        {
            List<Token> emitted = [];
            Token? flush = FlushTextIfAny(s, textStart, i);
            if (flush.HasValue)
            {
                emitted.Add(flush.Value);
            }

            emitted.Add(Token.FromElse(TextSpan.At(i, 4)));
            int newI = i + 4;
            return (true, newI, newI, emitted);
        }

        return (false, i, textStart, []);
    }

    [Pure]
    private static (int newI, int newTextStart, List<Token> emitted) LexBrace(
        string s,
        int i,
        int textStart,
        char brace)
    {
        List<Token> emitted = [];
        Token? flush = FlushTextIfAny(s, textStart, i);
        if (flush.HasValue)
        {
            emitted.Add(flush.Value);
        }

        emitted.Add(brace == '{'
            ? Token.FromLBrace(TextSpan.At(i, 1))
            : Token.FromRBrace(TextSpan.At(i, 1)));

        int newI = i + 1;
        return (newI, newI, emitted);
    }

    [Pure]
    private static Token? FlushTextIfAny(string s, int start, int end)
    {
        if (end > start)
        {
            string text = s[start..end];
            return Token.FromText(text, TextSpan.At(start, text.Length));
        }

        return null;
    }

    [Pure]
    private static (bool ok, int closeExclusive) TryScanParen(string s, int openIndex)
    {
        int n = s.Length;
        int i = openIndex + 1;
        int depth = 1;
        bool inString = false;
        bool escape = false;
        while (i < n)
        {
            char c = s[i];

            if (inString)
            {
                if (escape)
                {
                    escape = false;
                    i++;
                    continue;
                }

                if (c == '\\')
                {
                    escape = true;
                    i++;
                    continue;
                }

                if (c == '"')
                {
                    inString = false;
                    i++;
                    continue;
                }

                i++;
                continue;
            }

            if (c == '"')
            {
                inString = true;
                i++;
                continue;
            }

            if (c == '(')
            {
                depth++;
                i++;
                continue;
            }

            if (c == ')')
            {
                depth--;
                i++;
                if (depth == 0)
                {
                    return (true, i);
                }

                continue;
            }

            i++;
        }

        return (false, -1);
    }

    [Pure]
    private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_';

    [Pure]
    private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_';

    [Pure]
    private static bool IsElseKeyword(string s, int i)
    {
        int n = s.Length;
        if (i + 4 > n)
        {
            return false;
        }

        if (!s.AsSpan(i, 4).SequenceEqual("else".AsSpan()))
        {
            return false;
        }

        return IsWordBoundaryBefore(s, i)
               && IsWordBoundaryAfter(s, i + 4)
               && NextNonWsIsLBrace(s, i + 4);
    }

    [Pure]
    private static bool IsWordBoundaryBefore(string s, int idx)
        => idx == 0 || !IsIdentPart(s[idx - 1]);

    [Pure]
    private static bool IsWordBoundaryAfter(string s, int idxAfterWord)
        => idxAfterWord >= s.Length || !IsIdentPart(s[idxAfterWord]);

    [Pure]
    private static bool NextNonWsIsLBrace(string s, int start)
    {
        int i = SkipWs(s, start, s.Length);
        return i < s.Length && s[i] == '{';
    }

    [Pure]
    private static int SkipWs(string s, int i, int n)
    {
        while (i < n && char.IsWhiteSpace(s[i]))
        {
            i++;
        }

        return i;
    }
}