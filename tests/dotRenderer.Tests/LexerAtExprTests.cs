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
}