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
                if (i > textStart)
                {
                    string text = template[textStart..i];
                    Token tToken = Token.FromText(text, TextSpan.At(textStart, text.Length));
                    tokens.Add(tToken);
                }

                int j = i + 1;
                if (j < length && template[j] == '(')
                {
                    int k = j + 1;
                    int depth = 1;
                    while (k < length && depth > 0)
                    {
                        char c = template[k];
                        if (c == '(')
                        {
                            depth++;
                        }
                        else if (c == ')')
                        {
                            depth--;
                        }

                        k++;
                    }

                    if (depth == 0)
                    {
                        int closeIndexExclusive = k;
                        int closeIndexInclusive = closeIndexExclusive - 1;
                        string expr = template[(j + 1)..closeIndexInclusive];
                        Token tExpr = Token.FromAtExpr(expr, TextSpan.At(i, closeIndexExclusive - i));
                        tokens.Add(tExpr);

                        i = closeIndexExclusive;
                        textStart = i;
                        continue;
                    }

                    i++;
                    textStart = i;
                    continue;
                }

                if (j < length && template[j] == '@')
                {
                    tokens.Add(Token.FromText("@", TextSpan.At(i, 2)));
                    i = j + 1;
                    textStart = i;
                    continue;
                }

                if (j + 1 < length && template[j] == 'i' && template[j + 1] == 'f')
                {
                    int k = j + 2;
                    while (k < length && char.IsWhiteSpace(template[k]))
                    {
                        k++;
                    }

                    if (k < length && template[k] == '(')
                    {
                        k++;
                        int depth = 1;
                        int exprStart = k;

                        while (k < length && depth > 0)
                        {
                            char c = template[k];
                            if (c == '(')
                            {
                                depth++;
                            }
                            else if (c == ')')
                            {
                                depth--;
                            }

                            k++;
                        }

                        if (depth == 0)
                        {
                            int closeExclusive = k;
                            int closeInclusive = closeExclusive - 1;
                            string expr = template[exprStart..closeInclusive];
                            tokens.Add(Token.FromAtIf(expr, TextSpan.At(i, closeExclusive - i)));

                            i = closeExclusive;
                            textStart = i;
                            continue;
                        }
                    }

                    i++;
                    textStart = i;
                    continue;
                }

                if (j + 2 < length && template[j] == 'f' && template[j + 1] == 'o' && template[j + 2] == 'r')
                {
                    int k = j + 3;
                    while (k < length && char.IsWhiteSpace(template[k])) k++;

                    if (k < length && template[k] == '(')
                    {
                        k++;
                        int depth = 1;
                        int headerStart = k;

                        while (k < length && depth > 0)
                        {
                            char c = template[k];
                            if (c == '(')
                            {
                                depth++;
                            }
                            else if (c == ')')
                            {
                                depth--;
                            }

                            k++;
                        }

                        if (depth == 0)
                        {
                            int closeExclusive = k;
                            int closeInclusive = closeExclusive - 1;
                            string header = template[headerStart..closeInclusive];
                            tokens.Add(Token.FromAtFor(header, TextSpan.At(i, closeExclusive - i)));

                            i = closeExclusive;
                            textStart = i;
                            continue;
                        }
                    }

                    // malformed "@for" — consume '@'
                    i++;
                    textStart = i;
                    continue;
                }

                if (j < length && IsIdentStart(template[j]))
                {
                    int k = j + 1;
                    while (k < length && IsIdentPart(template[k]))
                    {
                        k++;
                    }

                    string name = template[j..k];
                    Token tIdent = Token.FromAtIdent(name, TextSpan.At(i, 1 + (k - j)));
                    tokens.Add(tIdent);

                    i = k;
                    textStart = i;
                    continue;
                }

                i++;
                continue;
            }

            if (ch == 'e' && i + 4 <= length && IsElseKeyword(template, i))
            {
                if (i > textStart)
                {
                    string text = template[textStart..i];
                    tokens.Add(Token.FromText(text, TextSpan.At(textStart, text.Length)));
                }

                tokens.Add(Token.FromElse(TextSpan.At(i, 4)));
                i += 4;
                textStart = i;
                continue;
            }

            if (ch == '{')
            {
                if (i > textStart)
                {
                    string text = template[textStart..i];
                    tokens.Add(Token.FromText(text, TextSpan.At(textStart, text.Length)));
                }

                tokens.Add(Token.FromLBrace(TextSpan.At(i, 1)));
                i++;
                textStart = i;
                continue;
            }

            if (ch == '}')
            {
                if (i > textStart)
                {
                    string text = template[textStart..i];
                    tokens.Add(Token.FromText(text, TextSpan.At(textStart, text.Length)));
                }

                tokens.Add(Token.FromRBrace(TextSpan.At(i, 1)));
                i++;
                textStart = i;
                continue;
            }

            i++;
        }

        if (i > textStart)
        {
            string tail = template[textStart..i];
            Token tToken = Token.FromText(tail, TextSpan.At(textStart, tail.Length));
            tokens.Add(tToken);
        }

        return Result<ImmutableArray<Token>>.Ok([..tokens]);
    }

    private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_';

    private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static bool IsElseKeyword(string s, int i)
    {
        if (i + 4 > s.Length)
        {
            return false;
        }

        return s.AsSpan(i, 4).SequenceEqual("else".AsSpan()) &&
               IsWordBoundaryBefore(s, i) &&
               IsWordBoundaryAfter(s, i + 4) &&
               NextNonWsIsLBrace(s, i + 4);
    }

    private static bool IsWordBoundaryBefore(string s, int idx)
        => idx == 0 || !IsIdentPart(s[idx - 1]);

    private static bool IsWordBoundaryAfter(string s, int idxAfterWord)
        => idxAfterWord >= s.Length || !IsIdentPart(s[idxAfterWord]);

    private static bool NextNonWsIsLBrace(string s, int start)
    {
        int i = start;
        while (i < s.Length && char.IsWhiteSpace(s[i]))
        {
            i++;
        }

        return i < s.Length && s[i] == '{';
    }
}