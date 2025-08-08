namespace dotRenderer.Tests;

internal static class IntegrationAssert
{
    public static void Renders<TModel>(string template, TModel model, IValueAccessor<TModel> accessor, string expected)
    {
        ITemplate<TModel> compiled = TemplateCompiler.Compile(template, accessor);
        string actual = compiled.Render(model);
        Assert.Equal(expected, actual);
    }
}