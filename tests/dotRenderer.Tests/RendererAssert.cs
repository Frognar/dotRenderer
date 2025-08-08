namespace dotRenderer.Tests;

internal static class RendererAssert
{
    public static void Renders(
        SequenceNode ast,
        TestDictModel model,
        string expected)
    {
        string html = Renderer.Render(ast, model, TestDictAccessor.Default);
        Assert.Equal(expected, html);
    }

    public static void Throws<TException>(
        SequenceNode ast,
        TestDictModel model,
        string messageFragment,
        params IEnumerable<string> messageFragments)
        where TException : Exception
    {
        TException ex = Assert.Throws<TException>(() => Renderer.Render(ast, model, TestDictAccessor.Default));
        foreach (string fragment in messageFragments.Prepend(messageFragment))
        {
            Assert.Contains(fragment, ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}