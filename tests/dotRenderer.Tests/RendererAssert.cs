using DotRenderer;

namespace dotRenderer.Tests;

internal static class RendererAssert
{
    public static void Render(Template template, IValueAccessor valueAccessor, string expected)
    {
        Func<Template, Result<string>> renderWithAccessor = Renderer.RenderWithAccessor(valueAccessor);
        Result<string> result = renderWithAccessor(template);

        Assert.True(result.IsOk, result.Error?.ToString() ?? "");
        Assert.Equal(expected, result.Value);
    }

    public static void FailsToRender(
        Template template,
        IValueAccessor valueAccessor,
        string expectedErrorCode,
        TextSpan expectedSpan,
        string expectedErrorMessage = "")
    {
        Result<string> result = Renderer.Render(template, valueAccessor);

        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal(expectedErrorCode, e.Code);
        Assert.Equal(expectedSpan, e.Range);
        Assert.Contains(expectedErrorMessage, e.Message, StringComparison.Ordinal);
    }
}