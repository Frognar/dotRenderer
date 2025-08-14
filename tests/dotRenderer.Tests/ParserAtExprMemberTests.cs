using DotRenderer;

namespace dotRenderer.Tests;

public class ParserAtExprMemberTests
{
    [Fact]
    public void Should_Parse_AtExpr_Member_Access_Between_Text_Nodes()
    {
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtExpr("u.name", TextSpan.At(6, 9)), // spans "@(u.name)"
                Token.FromText("!", TextSpan.At(15, 1))
            ],
            new Template([
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateExpr(
                    Expr.FromMember(Expr.FromIdent("u"), "name"),
                    TextSpan.At(6, 9)
                ),
                Node.FromText("!", TextSpan.At(15, 1))
            ])
        );
    }
}