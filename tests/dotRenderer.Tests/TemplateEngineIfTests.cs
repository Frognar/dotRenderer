using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineIfTests
{
    [Theory]
    [InlineData("A@if(true){T}else{E}B","ATB")]
    [InlineData("A@if(false){T}else{E}B", "AEB")]
    public void Should_Render_If_Else_Using_TemplateEngine(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }
}