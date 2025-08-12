using DotRenderer;
using Range = DotRenderer.Range;

namespace dotRenderer.Tests;

public class LexerEscapeAtTests
{
    [Fact]
    public void Should_Tokenize_Double_At_As_Single_Literal_At_In_Text()
    {
        LexerAssert.Lex("Hello @@world", [
            Token.FromText("Hello ", new Range(0, 6)),
            Token.FromText("@", new Range(6, 2)), // spans '@@' in source
            Token.FromText("world", new Range(8, 5))
        ]);
    }
}