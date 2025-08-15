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

    [Fact]
    public void Should_Tokenize_Else_After_For_Block()
    {
        LexerAssert.Lex("A@for(item in items){x}else{e}B",
        [
            Token.FromText("A", TextSpan.At(0, 1)),
            Token.FromAtFor("item in items", TextSpan.At(1, 19)),
            Token.FromLBrace(TextSpan.At(20, 1)),
            Token.FromText("x", TextSpan.At(21, 1)),
            Token.FromRBrace(TextSpan.At(22, 1)),
            Token.FromElse(TextSpan.At(23, 4)),
            Token.FromLBrace(TextSpan.At(27, 1)),
            Token.FromText("e", TextSpan.At(28, 1)),
            Token.FromRBrace(TextSpan.At(29, 1)),
            Token.FromText("B", TextSpan.At(30, 1)),
        ]);
    }
}