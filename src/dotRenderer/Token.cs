namespace DotRenderer;

public enum TokenKind
{
    Text,
    AtIdent
}

public readonly record struct Token(TokenKind Kind, string Text, Range Range);