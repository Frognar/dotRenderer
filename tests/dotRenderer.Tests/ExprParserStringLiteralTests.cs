using DotRenderer;

namespace dotRenderer.Tests;

public class ExprParserStringLiteralTests
{
    [Fact]
    public void Should_Parse_Simple_String_Literal()
    {
        Result<IExpr> result = ExprParser.Parse("\"A\"");
        Assert.True(result.IsOk);
        Assert.Equal(Expr.FromString("A"), result.Value);
    }

    [Fact]
    public void Should_Parse_String_With_Escaped_Quote_And_Backslash()
    {
        Result<IExpr> result = ExprParser.Parse("\"\\\"\\\\\"");
        Assert.True(result.IsOk);
        Assert.Equal(Expr.FromString("\"\\"),
            result.Value);
    }

    [Fact]
    public void Should_Parse_String_With_Escaped_Newline_And_Tab()
    {
        Result<IExpr> result = ExprParser.Parse("\"A\\nB\\tC\"");
        Assert.True(result.IsOk);
        Assert.Equal(Expr.FromString("A\nB\tC"), result.Value);
    }
}