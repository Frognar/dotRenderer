using DotRenderer;

namespace dotRenderer.Tests;

public class ParserForTests
{
    [Fact]
    public void Should_Parse_AtFor_Header_And_Block()
    {
        ParserAssert.Parse(
            [
                Token.FromText("A", TextSpan.At(0, 1)),
                Token.FromAtFor("item in items", TextSpan.At(1, 19)),
                Token.FromLBrace(TextSpan.At(20, 1)),
                Token.FromText("x", TextSpan.At(21, 1)),
                Token.FromRBrace(TextSpan.At(22, 1)),
                Token.FromText("B", TextSpan.At(23, 1)),
            ],
            new Template([
                Node.FromText("A", TextSpan.At(0, 1)),
                Node.FromFor(
                    "item",
                    Expr.FromIdent("items"),
                    [
                        Node.FromText("x", TextSpan.At(21, 1))
                    ],
                    TextSpan.At(1, 19)
                ),
                Node.FromText("B", TextSpan.At(23, 1))
            ])
        );
    }
    
    
    [Fact]
    public void Should_Parse_AtFor_Header_With_Item_And_Index()
    {
        ParserAssert.Parse(
            [
                Token.FromText("A", TextSpan.At(0, 1)),
                Token.FromAtFor("item, i in items", TextSpan.At(1, 21)),
                Token.FromLBrace(TextSpan.At(22, 1)),
                Token.FromText("x", TextSpan.At(23, 1)),
                Token.FromRBrace(TextSpan.At(24, 1)),
                Token.FromText("B", TextSpan.At(25, 1)),
            ],
            new Template([
                Node.FromText("A", TextSpan.At(0, 1)),
                Node.FromFor(
                    "item",
                    "i",
                    Expr.FromIdent("items"),
                    [
                        Node.FromText("x", TextSpan.At(23, 1))
                    ],
                    TextSpan.At(1, 21)
                ),
                Node.FromText("B", TextSpan.At(25, 1))
            ])
        );
    }
    
    [Fact]
    public void Should_Parse_For_With_Else_Block()
    {
        ParserAssert.Parse(
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
            ],
            new Template([
                Node.FromText("A", TextSpan.At(0, 1)),
                Node.FromFor(
                    "item",
                    Expr.FromIdent("items"),
                    [
                        Node.FromText("x", TextSpan.At(21, 1))
                    ],
                    [
                        Node.FromText("e", TextSpan.At(28, 1))
                    ],
                    TextSpan.At(1, 19)
                ),
                Node.FromText("B", TextSpan.At(30, 1))
            ])
        );
    }
}