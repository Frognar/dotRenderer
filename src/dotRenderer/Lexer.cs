using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace DotRenderer;

public static class Lexer
{
    [Pure]
    public static Result<ImmutableArray<Token>> Lex(string template)
    {
        template ??= string.Empty;
        Token token = new(TokenKind.Text, template, new Range(0, template.Length));
        return Result<ImmutableArray<Token>>.Ok([token]);
    }
}