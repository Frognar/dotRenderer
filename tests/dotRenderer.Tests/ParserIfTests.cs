using DotRenderer;

namespace dotRenderer.Tests;

public class ParserIfTests
{
    [Fact]
    public void Should_Parse_AtIf_With_Single_Block_No_Else()
    {
        ParserAssert.Parse(
            [
                Token.FromText("X", TextSpan.At(0, 1)),
                Token.FromAtIf("true", TextSpan.At(1, 9)),     // "@if(true)"
                Token.FromLBrace(TextSpan.At(10, 1)),        // "{"
                Token.FromText("ok", TextSpan.At(11, 2)),
                Token.FromRBrace(TextSpan.At(13, 1)),       // "}"
                Token.FromText("Y", TextSpan.At(14, 1)),
            ],
            new Template([
                Node.FromText("X", TextSpan.At(0, 1)),
                Node.FromIf(
                    Expr.FromBoolean(true),
                    [
                        Node.FromText("ok", TextSpan.At(11, 2))
                    ],
                    TextSpan.At(1, 9) // range of "@if(true)"
                ),
                Node.FromText("Y", TextSpan.At(14, 1)),
            ])
        );
    }
    
    [Fact]
    public void Should_Parse_AtIf_With_Then_And_Else_Blocks()
    {        ParserAssert.Parse(
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
            ],
            new Template([
                Node.FromText("A", TextSpan.At(0, 1)),
                Node.FromIf(
                    Expr.FromBoolean(true),
                    [
                        Node.FromText("T", TextSpan.At(11, 1))
                    ],
                    [
                        Node.FromText("E", TextSpan.At(18, 1))
                    ],
                    TextSpan.At(1, 9) // range of "@if(true)"
                ),
                Node.FromText("B", TextSpan.At(20, 1)),
            ])
        );
    }
}