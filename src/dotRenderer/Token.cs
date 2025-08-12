namespace DotRenderer;

public enum TokenKind
{
    Text,
    AtIdent
}

public readonly record struct Token(TokenKind Kind, string Text, TextSpan Range)
{
    public static Token FromText(string text, TextSpan range) => new(TokenKind.Text, text, range);
    public static Token FromAtIdent(string text, TextSpan range) => new(TokenKind.AtIdent, text, range);
}