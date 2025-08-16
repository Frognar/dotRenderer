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
            Token.FromAtIf("true", TextSpan.At(1, 9)), // spans "@if(true)"
            Token.FromLBrace(TextSpan.At(10, 1)), // "{"
            Token.FromText("ok", TextSpan.At(11, 2)),
            Token.FromRBrace(TextSpan.At(13, 1)), // "}"
            Token.FromText("Y", TextSpan.At(14, 1)),
        ]);
    }

    [Fact]
    public void Should_Tokenize_Else_After_If_Block()
    {
        LexerAssert.Lex("A@if(true){T}else{E}B",
        [
            Token.FromText("A", TextSpan.At(0, 1)),
            Token.FromAtIf("true", TextSpan.At(1, 9)),
            Token.FromLBrace(TextSpan.At(10, 1)),
            Token.FromText("T", TextSpan.At(11, 1)),
            Token.FromRBrace(TextSpan.At(12, 1)),
            Token.FromElse(TextSpan.At(13, 4)),
            Token.FromLBrace(TextSpan.At(17, 1)),
            Token.FromText("E", TextSpan.At(18, 1)),
            Token.FromRBrace(TextSpan.At(19, 1)),
            Token.FromText("B", TextSpan.At(20, 1)),
        ]);
    }

    [Fact]
    public void Should_Not_Tokenize_Else_When_Part_Of_A_Bigger_Word()
    {
        LexerAssert.Lex("xelse{y}",
        [
            Token.FromText("xelse", TextSpan.At(0, 5)),
            Token.FromLBrace(TextSpan.At(5, 1)),
            Token.FromText("y", TextSpan.At(6, 1)),
            Token.FromRBrace(TextSpan.At(7, 1))
        ]);
    }

    [Fact]
    public void Should_Not_Tokenize_Else_Without_Following_LBrace()
    {
        LexerAssert.Lex("A else B",
        [
            Token.FromText("A else B", TextSpan.At(0, 8))
        ]);
    }
}