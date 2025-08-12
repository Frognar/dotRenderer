using DotRenderer;

namespace dotRenderer.Tests;

internal static class RendererAssert
{
    public static void Render(Template template, IValueAccessor valueAccessor, string expected)
    {
        Result<string> result = Renderer.Render(template, valueAccessor);

        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }
}