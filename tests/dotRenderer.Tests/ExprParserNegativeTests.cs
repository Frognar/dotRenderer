using DotRenderer;

namespace dotRenderer.Tests;

public class ExprParserNegativeTests
{
    [Fact]
    public void Should_Error_When_Expression_Is_Empty()
    {
        Result<IExpr> result = ExprParser.Parse("");
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("ExprEmpty", e.Code);
        Assert.Equal(TextSpan.At(0, 0), e.Range);
    }

    [Fact]
    public void Should_Error_When_Trailing_Input_After_Expr()
    {
        Result<IExpr> result = ExprParser.Parse("1 2");
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("ExprTrailing", e.Code);
        Assert.Equal(TextSpan.At(2, 1), e.Range);
    }

    [Fact]
    public void Should_Error_On_Unexpected_Char()
    {
        Result<IExpr> result = ExprParser.Parse("@");
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("UnexpectedChar", e.Code);
        Assert.Equal(TextSpan.At(0, 1), e.Range);
    }

    [Fact]
    public void Should_Error_When_Missing_RParen()
    {
        Result<IExpr> result = ExprParser.Parse("(1+2");
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("MissingRParen", e.Code);
        Assert.Equal(TextSpan.At(4, 0), e.Range);
    }

    [Fact]
    public void Should_Error_On_Number_Format()
    {
        Result<IExpr> result = ExprParser.Parse("1.2.");
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("NumberFormat", e.Code);
        Assert.Equal(TextSpan.At(0, 4), e.Range);
    }

    [Fact]
    public void Should_Error_On_Unterminated_String()
    {
        Result<IExpr> result = ExprParser.Parse("\"abc");
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("StringUnterminated", e.Code);
        Assert.Equal(TextSpan.At(0, 4), e.Range);
    }

    [Fact]
    public void Should_Error_On_Unsupported_String_Escape()
    {
        Result<IExpr> result = ExprParser.Parse("\"\\x\"");
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("StringEscape", e.Code);
        Assert.Equal(TextSpan.At(1, 2), e.Range);
    }

    [Fact]
    public void Should_Error_On_Member_Without_Name()
    {
        Result<IExpr> result = ExprParser.Parse("a.");
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("MemberName", e.Code);
        Assert.Equal(TextSpan.At(2, 0), e.Range);
    }
}