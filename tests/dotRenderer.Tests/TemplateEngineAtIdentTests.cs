using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineAtIdentTests
{
    [Fact]
    public void Should_Render_Template_With_Interpolated_Identifier()
    {
        // arrange
        const string template = "Hello @name!";

        // act
        Result<string> result = TemplateEngine.Render(template, MapAccessor.With(("name", Value.FromString("Alice"))));

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("Hello Alice!", result.Value);
    }
}