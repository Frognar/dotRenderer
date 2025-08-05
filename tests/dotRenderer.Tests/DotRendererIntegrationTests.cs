namespace dotRenderer.Tests;

public class DotRendererIntegrationTests
{
    [Fact]
    public void Compile_And_Render_Should_Produce_Expected_Html()
    {
        string template = "<h1>Hi @Model.Name!</h1>";
        ITemplate compiled = TemplateCompiler.Compile(template);

        Dictionary<string, object> model = new() { { "Name", "Alice" } };
        string html = compiled.Render(model);

        Assert.Equal("<h1>Hi Alice!</h1>", html);
    }
}