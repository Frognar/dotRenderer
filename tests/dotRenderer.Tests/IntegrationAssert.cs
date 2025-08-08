namespace dotRenderer.Tests;

internal static class IntegrationAssert
{
    public static void Renders(string template, TestDictModel model, string expected)
    {
        ITemplate<TestDictModel> compiled = TemplateCompiler.Compile(template, TestDictAccessor.Default);
        string actual = compiled.Render(model);
        Assert.Equal(expected, actual);
    }
}