using DotRenderer;
using Range = DotRenderer.Range;

namespace dotRenderer.Tests;

public class LexerAtIdentTests
{
    [Fact]
    public void Should_Tokenize_At_Ident_Between_Text_Fragments()
    {
        LexerAssert.Lex("Hello @name!", [
            Token.FromText("Hello ", new Range(0, 6)),
            Token.FromAtIdent("name", new Range(6, 5)), // includes '@'
            Token.FromText("!", new Range(11, 1))
        ]);
    }
}