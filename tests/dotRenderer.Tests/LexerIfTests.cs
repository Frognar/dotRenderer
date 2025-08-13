using DotRenderer;

namespace dotRenderer.Tests;

public class LexerIfTests
{
    
    [Fact]
    public void Should_Tokenize_AtIf_With_Single_Block_No_Else()
    {
        LexerAssert.Lex("X@if(true){ok}Y",
        [
            Token.FromText("X", TextSpan.At(0, 1)),
            Token.FromAtIf("true", TextSpan.At(1, 9)),     // spans "@if(true)"
            Token.FromLBrace(TextSpan.At(10, 1)),        // "{"
            Token.FromText("ok", TextSpan.At(11, 2)),
            Token.FromRBrace(TextSpan.At(13, 1)),       // "}"
            Token.FromText("Y", TextSpan.At(14, 1)),
        ]);
    }
}