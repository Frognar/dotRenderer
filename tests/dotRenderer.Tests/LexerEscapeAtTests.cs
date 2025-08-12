using DotRenderer;

namespace dotRenderer.Tests;

public class LexerEscapeAtTests
{
    [Fact]
    public void Should_Tokenize_Double_At_As_Single_Literal_At_In_Text()
    {
        LexerAssert.Lex("Hello @@world", [
            Token.FromText("Hello ", new TextSpan(0, 6)),
            Token.FromText("@", new TextSpan(6, 2)), // spans '@@' in source
            Token.FromText("world", new TextSpan(8, 5))
        ]);
    }
}