using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace DotRenderer;

public static class Lexer
{
    [Pure]
    public static Result<ImmutableArray<Token>> Lex(string template)
    {
        State s = State.Of(template);
        List<Token> tokens = [];
        int textStart = 0;
        while (!s.Eof)
        {
            char ch = s.Current;
            if (ch == '@')
            {
                (bool matched, State rest, int newTextStart, List<Token> emitted) at = TryLexAtSequence(s, textStart);
                if (at.matched)
                {
                    tokens.AddRange(at.emitted);
                    s = at.rest;
                    textStart = at.newTextStart;
                    continue;
                }

                s = s.Advance();
                continue;
            }

            if (ch == 'e')
            {
                (bool matched, State rest, int newTextStart, List<Token> emitted) els = TryLexElse(s, textStart);
                if (els.matched)
                {
                    tokens.AddRange(els.emitted);
                    s = els.rest;
                    textStart = els.newTextStart;
                    continue;
                }
            }

            if (ch is '{' or '}')
            {
                (State rest, int newTextStart, List<Token> emitted) br = LexBrace(s, textStart);
                tokens.AddRange(br.emitted);
                s = br.rest;
                textStart = br.newTextStart;
                continue;
            }

            s = s.Advance();
        }

        Token? tail = FlushTextIfAny(s.Text, textStart, s.Pos);
        if (tail.HasValue)
        {
            tokens.Add(tail.Value);
        }

        return Result<ImmutableArray<Token>>.Ok([.. tokens]);
    }

    private readonly record struct State(string Text, int Pos)
    {
        public string Text { get; } = Text;
        public int Length => Text.Length;
        public bool Eof => Pos >= Length;
        public char Current => Text[Pos];

        public State Advance(int delta = 1) => this with { Pos = Pos + delta };
        public static State Of(string text) => new(text, 0);
    }

    private static (bool matched, State rest, int newTextStart, List<Token> emitted) TryLexAtSequence(
        State s,
        int textStart)
    {
        Token? flush = FlushTextIfAny(s.Text, textStart, s.Pos);
        (bool matched, State rest) comment = TryLexComment(s);
        if (comment.matched)
        {
            return (true, comment.rest, comment.rest.Pos, NewEmitted(flush));
        }

        (bool matched, State rest, Token token) atExpr = TryLexAtExpr(s);
        if (atExpr.matched)
        {
            List<Token> emitted = NewEmitted(flush, atExpr.token);
            return (true, atExpr.rest, atExpr.rest.Pos, emitted);
        }

        (bool matched, State rest, Token token) atAt = TryLexDoubleAt(s);
        if (atAt.matched)
        {
            List<Token> emitted = NewEmitted(flush, atAt.token);
            return (true, atAt.rest, atAt.rest.Pos, emitted);
        }

        (bool matched, State rest, List<Token> tokens) atElif = TryLexAtElif(s);
        if (atElif.matched)
        {
            List<Token> emitted = NewEmitted(flush, atElif.tokens[0], atElif.tokens[1]);
            return (true, atElif.rest, atElif.rest.Pos, emitted);
        }

        (bool matched, State rest, Token token) atIf = TryLexAtIf(s);
        if (atIf.matched)
        {
            List<Token> emitted = NewEmitted(flush, atIf.token);
            return (true, atIf.rest, atIf.rest.Pos, emitted);
        }

        (bool matched, State rest, Token token) atFor = TryLexAtFor(s);
        if (atFor.matched)
        {
            List<Token> emitted = NewEmitted(flush, atFor.token);
            return (true, atFor.rest, atFor.rest.Pos, emitted);
        }

        (bool matched, State rest, Token token) atIdent = TryLexAtIdent(s);
        if (atIdent.matched)
        {
            List<Token> emitted = NewEmitted(flush, atIdent.token);
            return (true, atIdent.rest, atIdent.rest.Pos, emitted);
        }

        return (false, s, textStart, []);

        static List<Token> NewEmitted(Token? flush, params IEnumerable<Token> tokens)
        {
            List<Token> emitted = [];
            if (flush.HasValue)
            {
                emitted.Add(flush.Value);
            }

            emitted.AddRange(tokens);
            return emitted;
        }
    }

    [Pure]
    private static (bool matched, State rest) TryLexComment(State s)
    {
        int i = s.Pos;
        int n = s.Length;
        int j = i + 1;
        if (i + 1 < n && s.Text[j] == '*')
        {
            int k = i + 2;
            while (k + 1 < n)
            {
                if (s.Text[k] == '*' && s.Text[k + 1] == '@')
                {
                    int advanceBy = k - i + 2;
                    return (true, s.Advance(advanceBy));
                }

                k++;
            }
        }
        
        return (false, s);
    }

    [Pure]
    private static (bool matched, State rest, Token token) TryLexAtExpr(State s)
    {
        int i = s.Pos;
        int n = s.Length;
        int j = i + 1;
        if (j < n && s.Text[j] == '(')
        {
            (bool ok, int closeExclusive) paren = TryScanParen(s.Text, j);
            if (paren.ok)
            {
                int closeExcl = paren.closeExclusive;
                string expr = s.Text[(j + 1)..(closeExcl - 1)];
                return (true, s with { Pos = closeExcl }, Token.FromAtExpr(expr, TextSpan.At(i, closeExcl - i)));
            }
        }

        return (false, s, default);
    }

    [Pure]
    private static (bool matched, State rest, Token token) TryLexDoubleAt(State s)
    {
        int j = s.Pos + 1;
        if (j < s.Length && s.Text[j] == '@')
        {
            return (true, s.Advance(2), Token.FromText("@", TextSpan.At(s.Pos, 2)));
        }

        return (false, s, default);
    }

    [Pure]
    private static (bool matched, State rest, List<Token> tokens) TryLexAtElif(State s)
    {
        int i = s.Pos;
        int n = s.Length;
        int j = i + 1;
        if (j + 3 < n && s.Text.AsSpan(j, 4).SequenceEqual("elif".AsSpan()))
        {
            int k = SkipWs(s.Text, j + 4, n);
            if (k < n && s.Text[k] == '(')
            {
                (bool ok, int closeExclusive) paren = TryScanParen(s.Text, k);
                if (paren.ok)
                {
                    int closeExcl = paren.closeExclusive;
                    Token elseTok = Token.FromElse(TextSpan.At(i, 4));
                    string expr = s.Text[(k + 1)..(closeExcl - 1)];
                    Token ifTok = Token.FromAtIf(expr, TextSpan.At(i, closeExcl - i));
                    return (true, s with { Pos = closeExcl }, [elseTok, ifTok]);
                }
            }
        }
        return (false, s, []);
    }

    [Pure]
    private static (bool matched, State rest, Token token) TryLexAtIf(State s)
    {
        int i = s.Pos;
        int n = s.Length;
        int j = i + 1;
        if (j + 1 < n && s.Text.AsSpan(j, 2).SequenceEqual("if".AsSpan()))
        {
            int k = SkipWs(s.Text, j + 2, n);
            if (k < n && s.Text[k] == '(')
            {
                (bool ok, int closeExclusive) paren = TryScanParen(s.Text, k);
                if (paren.ok)
                {
                    int closeExcl = paren.closeExclusive;
                    string expr = s.Text[(k + 1)..(closeExcl - 1)];
                    return (true, s with { Pos = closeExcl }, Token.FromAtIf(expr, TextSpan.At(i, closeExcl - i)));
                }
            }
        }

        return (false, s, default);
    }

    [Pure]
    private static (bool matched, State rest, Token token) TryLexAtFor(State s)
    {
        int i = s.Pos;
        int n = s.Length;
        int j = i + 1;
        if (j + 2 < n && s.Text.AsSpan(j, 3).SequenceEqual("for".AsSpan()))
        {
            int k = SkipWs(s.Text, j + 3, n);
            if (k < n && s.Text[k] == '(')
            {
                (bool ok, int closeExclusive) paren = TryScanParen(s.Text, k);
                if (paren.ok)
                {
                    int closeExcl = paren.closeExclusive;
                    string header = s.Text[(k + 1)..(closeExcl - 1)];
                    return (true, s with { Pos = closeExcl }, Token.FromAtFor(header, TextSpan.At(i, closeExcl - i)));
                }
            }
        }

        return (false, s, default);
    }

    [Pure]
    private static (bool matched, State rest, Token token) TryLexAtIdent(State s)
    {
        int i = s.Pos;
        int n = s.Length;
        int j = i + 1;
        if (j < n && IsIdentStart(s.Text[j]))
        {
            int k = j + 1;
            while (k < n && IsIdentPart(s.Text[k]))
            {
                k++;
            }

            string name = s.Text[j..k];
            return (true, s with { Pos = k }, Token.FromAtIdent(name, TextSpan.At(i, 1 + (k - j))));
        }

        return (false, s, default);
    }

    [Pure]
    private static (bool matched, State rest, int newTextStart, List<Token> emitted) TryLexElse(State s, int textStart)
    {
        if (IsElseKeyword(s.Text, s.Pos))
        {
            List<Token> emitted = [];
            Token? flush = FlushTextIfAny(s.Text, textStart, s.Pos);
            if (flush.HasValue)
            {
                emitted.Add(flush.Value);
            }

            emitted.Add(Token.FromElse(TextSpan.At(s.Pos, 4)));
            State rest = s.Advance(4);
            return (true, rest, rest.Pos, emitted);
        }

        return (false, s, textStart, []);
    }

    [Pure]
    private static (State rest, int newTextStart, List<Token> emitted) LexBrace(State s, int textStart)
    {
        List<Token> emitted = [];
        Token? flush = FlushTextIfAny(s.Text, textStart, s.Pos);
        if (flush.HasValue)
        {
            emitted.Add(flush.Value);
        }

        emitted.Add(s.Current == '{'
            ? Token.FromLBrace(TextSpan.At(s.Pos, 1))
            : Token.FromRBrace(TextSpan.At(s.Pos, 1)));

        State rest = s.Advance();
        return (rest, rest.Pos, emitted);
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