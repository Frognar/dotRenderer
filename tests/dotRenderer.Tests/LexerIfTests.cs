using DotRenderer;

namespace dotRenderer.Tests;

public class LexerIfTests
{
    
    [Fact]
    public void Should_Tokenize_AtIf_With_Single_Block_No_Else()
    {
        LexerAssert.Lex("X@if(1){ok}Y",
        [
            Token.FromText("X", TextSpan.At(0, 1)),
            Token.FromAtIf("1", TextSpan.At(1, 6)),     // spans "@if(1)"
            Token.FromLBrace(TextSpan.At(7, 1)),        // "{"
            Token.FromText("ok", TextSpan.At(8, 2)),
            Token.FromRBrace(TextSpan.At(10, 1)),       // "}"
            Token.FromText("Y", TextSpan.At(11, 1)),
        ]);
    }
}