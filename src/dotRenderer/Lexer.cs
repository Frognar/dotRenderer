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
                    Token tToken = new(TokenKind.Text, text, new Range(textStart, text.Length));
                    tokens.Add(tToken);
                }

                int j = i + 1;
                if (j < length && template[j] == '@')
                {
                    tokens.Add(new Token(TokenKind.Text, "@", new Range(i, 2)));
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
                    Token tIdent = new(TokenKind.AtIdent, name, new Range(i, 1 + (k - j)));
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
            Token tToken = new(TokenKind.Text, tail, new Range(textStart, tail.Length));
            tokens.Add(tToken);
        }

        return Result<ImmutableArray<Token>>.Ok([..tokens]);
    }

    private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_';

    private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_';
}