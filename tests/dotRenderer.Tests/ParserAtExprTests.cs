using DotRenderer;

namespace dotRenderer.Tests;

public class ParserAtExprTests
{
    [Fact]
    public void Should_Parse_AtExpr_Token_Into_InterpolateExprNode_Between_Text_Nodes()
    {
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtExpr("1+2", TextSpan.At(6, 6)), // spans "@(1+2)"
                Token.FromText("!", TextSpan.At(12, 1))
            ],
            new Template([
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateExpr(Expr.FromRaw("1+2"), TextSpan.At(6, 6)),
                Node.FromText("!", TextSpan.At(12, 1))
            ]));
    }
}