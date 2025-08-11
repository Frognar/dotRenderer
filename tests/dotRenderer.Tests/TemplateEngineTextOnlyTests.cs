using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineTextOnlyTests
{
    [Fact]
    public void Should_Return_Plain_Text_Unchanged()
    {
        // arrange
        const string template = "Plain text only.";

        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(template, result.Value);
    }
}