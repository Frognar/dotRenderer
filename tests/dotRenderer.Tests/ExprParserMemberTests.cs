using DotRenderer;

namespace dotRenderer.Tests;

public class ExprParserMemberTests
{
    [Fact]
    public void Should_Parse_Simple_Member_Access()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("u.name");

        // assert
        Assert.True(result.IsOk);
        IExpr expected = Expr.FromMember(Expr.FromIdent("u"), "name");
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Should_Parse_Nested_Member_Access()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("u.address.city");

        // assert
        Assert.True(result.IsOk);
        IExpr expected =
            Expr.FromMember(
                Expr.FromMember(
                    Expr.FromIdent("u"),
                    "address"
                ),
                "city"
            );
        Assert.Equal(expected, result.Value);
    }
}