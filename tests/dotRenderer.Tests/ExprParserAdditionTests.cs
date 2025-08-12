using DotRenderer;

namespace dotRenderer.Tests;

public class ExprParserAdditionTests
{
    [Fact]
    public void Should_Parse_Number_Addition_Into_BinaryExpr()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("1+2");

        // assert
        Assert.True(result.IsOk);
        IExpr expected = Expr.FromBinaryAdd(Expr.FromNumber(1), Expr.FromNumber(2));
        Assert.Equal(expected, result.Value);
    }
}