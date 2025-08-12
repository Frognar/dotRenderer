namespace DotRenderer;

public enum TokenKind
{
    Text,
    AtIdent
}

public readonly record struct Token(TokenKind Kind, string Text, Range Range)
{
    public static Token FromText(string text, Range range) => new(TokenKind.Text, text, range);
    public static Token FromAtIdent(string text, Range range) => new(TokenKind.AtIdent, text, range);
}