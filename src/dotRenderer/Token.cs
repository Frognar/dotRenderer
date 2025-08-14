namespace DotRenderer;

public enum TokenKind
{
    Text,
    AtIdent,
    AtExpr,
    AtIf,
    AtFor,
    Else,
    LBrace,
    RBrace
}

public readonly record struct Token(TokenKind Kind, string Text, TextSpan Range)
{
    public static Token FromText(string text, TextSpan range) => new(TokenKind.Text, text, range);
    public static Token FromAtIdent(string text, TextSpan range) => new(TokenKind.AtIdent, text, range);
    public static Token FromAtExpr(string text, TextSpan range) => new(TokenKind.AtExpr, text, range);
    public static Token FromAtIf(string exprText, TextSpan range) => new(TokenKind.AtIf, exprText, range);
    public static Token FromAtFor(string headerText, TextSpan range) => new(TokenKind.AtFor, headerText, range);
    public static Token FromElse(TextSpan range) => new(TokenKind.Else, "else", range);
    public static Token FromLBrace(TextSpan range) => new(TokenKind.LBrace, "{", range);
    public static Token FromRBrace(TextSpan range) => new(TokenKind.RBrace, "}", range);
}