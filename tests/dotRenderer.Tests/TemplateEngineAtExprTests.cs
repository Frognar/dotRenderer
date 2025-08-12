using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineAtExprTests
{
    [Fact]
    public void Should_Render_AtExpr_Number_Addition()
    {
        // arrange
        const string template = "Result: @(1+2)!";

        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("Result: 3!", result.Value);
    }
}