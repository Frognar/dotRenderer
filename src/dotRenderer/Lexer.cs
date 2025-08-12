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
}