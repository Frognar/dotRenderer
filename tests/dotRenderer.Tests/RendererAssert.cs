namespace dotRenderer.Tests;

internal static class RendererAssert
{
    public static void Renders<TModel>(
        SequenceNode ast,
        TModel model,
        IValueAccessor<TModel> accessor,
        string expected)
    {
        string html = Renderer.Render(ast, model, accessor);
        Assert.Equal(expected, html);
    }

    public static void Throws<TException, TModel>(
        SequenceNode ast,
        TModel model,
        IValueAccessor<TModel> accessor,
        string messageFragment,
        params IEnumerable<string> messageFragments)
        where TException : Exception
    {
        TException ex = Assert.Throws<TException>(() => Renderer.Render(ast, model, accessor));
        foreach (string fragment in messageFragments.Prepend(messageFragment))
        {
            Assert.Contains(fragment, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}