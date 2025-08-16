using DotRenderer;

namespace dotRenderer.Tests;

internal static class ExprParserAssert
{
    public static void Parse(
        string text,
        IExpr expected)
    {
        Result<IExpr> result = ExprParser.Parse(text);

        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }
    
    public static void FailsToParse(
        string text,
        string expectedErrorCode,
        TextSpan expectedSpan,
        string expectedErrorMessage = "")
    {
        Result<IExpr> result = ExprParser.Parse(text);

        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal(expectedErrorCode, e.Code);
        Assert.Equal(expectedSpan, e.Range);
        Assert.Contains(expectedErrorMessage, e.Message, StringComparison.Ordinal);
    }
}