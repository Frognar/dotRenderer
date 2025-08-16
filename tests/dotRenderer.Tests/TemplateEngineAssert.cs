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
}