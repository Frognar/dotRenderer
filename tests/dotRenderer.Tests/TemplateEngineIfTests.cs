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

    [Fact]
    public void Should_Render_Then_When_Equality_Is_True()
    {
        // arrange
        const string template = "A@if(1==1){T}else{E}B";

        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("ATB", result.Value);
    }

    [Theory]
    [InlineData("A@if(2>1){T}else{E}B", "ATB")]
    [InlineData("A@if(1>2){T}else{E}B", "AEB")]
    [InlineData("A@if(2>=1){T}else{E}B", "ATB")]
    [InlineData("A@if(1>=2){T}else{E}B", "AEB")]
    [InlineData("A@if(1<2){T}else{E}B", "ATB")]
    [InlineData("A@if(2<1){T}else{E}B", "AEB")]
    [InlineData("A@if(1<=2){T}else{E}B", "ATB")]
    [InlineData("A@if(2<=1){T}else{E}B", "AEB")]
    public void Should_Render_Correct_Block_For_Comparison_Operators(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }
}