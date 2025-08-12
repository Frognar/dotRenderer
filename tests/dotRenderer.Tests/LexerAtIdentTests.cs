using DotRenderer;

namespace dotRenderer.Tests;

public class LexerAtIdentTests
{
    [Fact]
    public void Should_Tokenize_At_Ident_Between_Text_Fragments()
    {
        LexerAssert.Lex("Hello @name!", [
            Token.FromText("Hello ", TextSpan.At(0, 6)),
            Token.FromAtIdent("name", TextSpan.At(6, 5)), // includes '@'
            Token.FromText("!", TextSpan.At(11, 1))
        ]);
    }
}