using DotRenderer;

namespace dotRenderer.Tests;

public class ParserAtExprStringTests
{
    [Fact]
    public void Should_Parse_AtExpr_With_String_Concat()
    {
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtExpr("\"A\" + \"B\"", TextSpan.At(6, 12)), // "@(\"A\" + \"B\")"
                Token.FromText("!", TextSpan.At(18, 1))
            ],
            new Template([
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateExpr(
                    Expr.FromBinaryAdd(
                        Expr.FromString("A"),
                        Expr.FromString("B")
                    ),
                    TextSpan.At(6, 12)
                ),
                Node.FromText("!", TextSpan.At(18, 1))
            ])
        );
    }
}