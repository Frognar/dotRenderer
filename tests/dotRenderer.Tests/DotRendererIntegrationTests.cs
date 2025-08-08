namespace dotRenderer.Tests;

public class DotRendererIntegrationTests
{
    [Fact]
    public void Compile_Generic_Should_Render_Model_With_Custom_Accessor()
    {
        IntegrationAssert.Renders(
            "<h1>Hi @Model.Name!</h1>",
            TestDictModel.With(("Name", "Alice")),
            "<h1>Hi Alice!</h1>");
    }

    [Fact]
    public void Compile_Should_Render_With_Arithmetic_In_Condition()
    {
        ITemplate<TestDictModel> compiled =
            TemplateCompiler.Compile("@if (Model.Age + 1 > 18) {OK}", TestDictAccessor.Default);

        Assert.Equal("OK", compiled.Render(TestDictModel.With(("Age", "18"))));
        Assert.Equal("", compiled.Render(TestDictModel.With(("Age", "17"))));
    }

    [Fact]
    public void Compile_Should_Render_With_Scientific_Notation_Literal()
    {
        ITemplate<TestDictModel> compiled =
            TemplateCompiler.Compile("@if (1e-3 < 0.01) {OK}", TestDictAccessor.Default);

        Assert.Equal("OK", compiled.Render(TestDictModel.Empty));
    }

    [Fact]
    public void Compile_Should_Render_With_Scientific_Notation_From_Model()
    {
        ITemplate<TestDictModel> compiled =
            TemplateCompiler.Compile("@if (Model.Eps == 1e-3) {OK}", TestDictAccessor.Default);

        Assert.Equal("OK", compiled.Render(TestDictModel.With(("Eps", "1e-3"))));
        Assert.Equal("", compiled.Render(TestDictModel.With(("Eps", "0.0010001"))));
    }
}