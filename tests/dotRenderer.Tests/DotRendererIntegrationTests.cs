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

    [Fact]
    public void Compile_Should_Remove_Empty_Lines_When_If_Block_Renders_Nothing()
    {
        ITemplate<TestDictModel> compiled = TemplateCompiler.Compile("""
                                                                     Hello
                                                                     @if (false) {
                                                                     - hidden
                                                                     }
                                                                     Footer
                                                                     """, TestDictAccessor.Default);

        Assert.Equal("""
                     Hello
                     Footer
                     """, compiled.Render(TestDictModel.Empty));
    }

    [Fact]
    public void Compile_Should_Collapse_Empty_Lines_For_Nested_If_When_Inner_Is_False()
    {
        ITemplate<TestDictModel> compiled = TemplateCompiler.Compile("""
                                                                     Title
                                                                     @if (true) {
                                                                     - Section
                                                                     @if (false) {Detail}
                                                                     }
                                                                     End
                                                                     """, TestDictAccessor.Default);

        Assert.Equal("""
                     Title
                     - Section
                     End
                     """, compiled.Render(TestDictModel.Empty));
    }

    [Fact]
    public void Compile_Should_Remove_Surrounding_Empty_Lines_For_Admin_Block_When_False()
    {
        ITemplate<TestDictModel> compiled = TemplateCompiler.Compile("""
                                                                     Hello @@world!
                                                                     User: @Model.User.Name

                                                                     @if (Model.IsAdmin) {
                                                                     - Admin panel
                                                                     @if (Model.ShowHint) {"{HINT}"}
                                                                     }

                                                                     Footer.
                                                                     """, TestDictAccessor.Default);

        Assert.Equal("""
                     Hello @world!
                     User: Alice

                     - Admin panel
                     "{HINT}"

                     Footer.
                     """, compiled.Render(TestDictModel.With(
            ("User.Name", "Alice"),
            ("IsAdmin", "true"),
            ("ShowHint", "true")
        )));

        Assert.Equal("""
                     Hello @world!
                     User: Bob

                     - Admin panel

                     Footer.
                     """, compiled.Render(TestDictModel.With(
            ("User.Name", "Bob"),
            ("IsAdmin", "true"),
            ("ShowHint", "false")
        )));

        Assert.Equal("""
                     Hello @world!
                     User: Eve

                     
                     Footer.
                     """, compiled.Render(TestDictModel.With(
            ("User.Name", "Eve"),
            ("IsAdmin", "false"),
            ("ShowHint", "true")
        )));
    }

    [Fact]
    public void Compile_Should_Preserve_Explicit_Multiple_Blank_Lines()
    {
        ITemplate<TestDictModel> compiled = TemplateCompiler.Compile("""
                                                                     A


                                                                     B
                                                                     """, TestDictAccessor.Default);

        Assert.Equal("""
                     A


                     B
                     """, compiled.Render(TestDictModel.Empty));
    }
}