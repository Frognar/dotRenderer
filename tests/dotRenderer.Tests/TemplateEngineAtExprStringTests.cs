using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineAtExprStringTests
{
    [Fact]
    public void Should_Render_AtExpr_String_Concatenation()
    {
        const string template = "Hello @(\"A\" + \"B\")!";
        Result<string> result = TemplateEngine.Render(template);
        Assert.True(result.IsOk);
        Assert.Equal("Hello AB!", result.Value);
    }
}