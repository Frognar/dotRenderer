using DotRenderer;

namespace dotRenderer.Tests;

public class LexerForTests
{
    [Fact]
    public void Should_Tokenize_AtFor_With_Single_Block_No_Else()
    {
        // A@for(item in items){x}B
        LexerAssert.Lex("A@for(item in items){x}B",
        [
            Token.FromText("A", TextSpan.At(0, 1)),
            Token.FromAtFor("item in items", TextSpan.At(1, 19)), // spans "@for(item in items)"
            Token.FromLBrace(TextSpan.At(20, 1)),
            Token.FromText("x", TextSpan.At(21, 1)),
            Token.FromRBrace(TextSpan.At(22, 1)),
            Token.FromText("B", TextSpan.At(23, 1)),
        ]);
    }
}