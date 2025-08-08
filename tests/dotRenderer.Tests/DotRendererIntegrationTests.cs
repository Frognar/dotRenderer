using System.Globalization;

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

    [Fact]
    public void Compile_Should_Handle_Condition_With_Closing_Paren_In_String()
    {
        ITemplate<TestDictModel> compiled =
            TemplateCompiler.Compile("@if (Model.S == \")\") {OK}", TestDictAccessor.Default);

        Assert.Equal("OK", compiled.Render(TestDictModel.With(("S", ")"))));
        Assert.Equal("", compiled.Render(TestDictModel.With(("S", "("))));
    }

    [Fact]
    public void Compile_Should_Respect_InvariantCulture_For_Doubles_Under_plPL()
    {
        CultureInfo prev = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("pl-PL");

            ITemplate<TestDictModel> compiled =
                TemplateCompiler.Compile("@if (Model.Value > 1.0) {GT}", TestDictAccessor.Default);

            Assert.Equal("GT", compiled.Render(TestDictModel.With(("Value", "1.5"))));
            Assert.Equal("", compiled.Render(TestDictModel.With(("Value", "0.9"))));
        }
        finally
        {
            CultureInfo.CurrentCulture = prev;
        }
    }
}