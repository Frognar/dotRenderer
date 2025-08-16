using DotRenderer;

namespace dotRenderer.Tests;

internal static class TemplateEngineAssert
{
    public static void Render(string template, IValueAccessor valueAccessor, string expected)
    {
        Result<string> result = TemplateEngine.Render(template, valueAccessor);

        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }

    public static void FailsToRender(
        string template,
        IValueAccessor valueAccessor,
        string expectedErrorCode,
        TextSpan expectedSpan,
        string expectedErrorMessage = "")
    {
        Result<string> result = TemplateEngine.Render(template, valueAccessor);

        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal(expectedErrorCode, e.Code);
        Assert.Equal(expectedSpan, e.Range);
        Assert.Contains(expectedErrorMessage, e.Message, StringComparison.Ordinal);
    }
}