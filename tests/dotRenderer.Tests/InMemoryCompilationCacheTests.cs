namespace dotRenderer.Tests;

public class InMemoryCompilationCacheTests
{
    private sealed class UpperCaseAccessor : IValueAccessor<TestDictModel>
    {
        public string? AccessValue(string path, TestDictModel model)
            => TestDictAccessor.Default.AccessValue(path, model)?.ToUpperInvariant();
    }

    [Fact]
    public void InMemoryCache_Should_Reuse_Ast_For_Same_Template_And_Different_Accessors()
    {
        InMemoryCompilationCache cache = new();
        const string template = "Hello @Model.Name";

        ITemplate<TestDictModel> tDefault = TemplateCompiler.Compile(template, TestDictAccessor.Default, cache);
        ITemplate<TestDictModel> tUpper = TemplateCompiler.Compile(template, new UpperCaseAccessor(), cache);
        ITemplate<TestDictModel> tAgain = TemplateCompiler.Compile(template, TestDictAccessor.Default, cache);

        Assert.Equal("Hello Alice", tDefault.Render(TestDictModel.With(("Name", "Alice"))));
        Assert.Equal("Hello BOB", tUpper.Render(TestDictModel.With(("Name", "Bob"))));
        Assert.Equal("Hello Eve", tAgain.Render(TestDictModel.With(("Name", "Eve"))));
    }

    [Fact]
    public void InMemoryCache_Should_Separate_Different_Templates()
    {
        InMemoryCompilationCache cache = new();

        ITemplate<TestDictModel> t1 = TemplateCompiler.Compile("Hi @Model.X", TestDictAccessor.Default, cache);
        ITemplate<TestDictModel> t2 = TemplateCompiler.Compile("Bye @Model.X", TestDictAccessor.Default, cache);

        Assert.Equal("Hi Z", t1.Render(TestDictModel.With(("X", "Z"))));
        Assert.Equal("Bye Z", t2.Render(TestDictModel.With(("X", "Z"))));
    }
}