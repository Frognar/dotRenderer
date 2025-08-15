using DotRenderer;

namespace dotRenderer.Tests;

public class LexerAtExprTests
{
    [Fact]
    public void Should_Tokenize_At_Expr_Between_Text_Fragments()
    {
        LexerAssert.Lex("Hello @(1+2)!",
        [
            Token.FromText("Hello ", TextSpan.At(0, 6)),
            Token.FromAtExpr("1+2", TextSpan.At(6, 6)), // spans "@(1+2)"
            Token.FromText("!", TextSpan.At(12, 1))
        ]);
    }

    [Fact]
    public void Should_Tokenize_At_Expr_With_String_Concat()
    {
        LexerAssert.Lex("Hello @(\"A\" + \"B\")!",
        [
            Token.FromText("Hello ", TextSpan.At(0, 6)),
            Token.FromAtExpr("\"A\" + \"B\"", TextSpan.At(6, 12)), // spans "@(\"A\" + \"B\")"
            Token.FromText("!", TextSpan.At(18, 1))
        ]);
    }

    [Fact]
    public void Should_Tokenize_At_Expr_With_Paren_Inside_String()
    {
        LexerAssert.Lex("A@(\"(\")B",
        [
            Token.FromText("A", TextSpan.At(0, 1)),
            Token.FromAtExpr("\"(\"", TextSpan.At(1, 6)), // spans "@(\"(\")"
            Token.FromText("B", TextSpan.At(7, 1))
        ]);
    }
}