namespace DotRenderer;

public enum TokenKind
{
    Text
}

public readonly record struct Token(TokenKind Kind, string Text, Range Range);