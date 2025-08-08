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
}