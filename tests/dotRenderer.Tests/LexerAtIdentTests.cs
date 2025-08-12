using DotRenderer;

namespace dotRenderer.Tests;

public class LexerAtIdentTests
{
    [Fact]
    public void Should_Tokenize_At_Ident_Between_Text_Fragments()
    {
        LexerAssert.Lex("Hello @name!", [
            Token.FromText("Hello ", new TextSpan(0, 6)),
            Token.FromAtIdent("name", new TextSpan(6, 5)), // includes '@'
            Token.FromText("!", new TextSpan(11, 1))
        ]);
    }
}