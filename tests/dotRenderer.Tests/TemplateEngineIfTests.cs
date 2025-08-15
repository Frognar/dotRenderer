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

    [Theory]
    [InlineData("A@if(2>1 && !false || false){T}else{E}B", "ATB")]
    [InlineData("A@if(0>1 && !false || false){T}else{E}B", "AEB")]
    [InlineData("A@if(0>1 && !true || false){T}else{E}B", "AEB")]
    [InlineData("A@if(2>1 && !true || false){T}else{E}B", "AEB")]
    [InlineData("A@if(2>1 && !false || true){T}else{E}B", "ATB")]
    [InlineData("A@if(0>1 && !false || true){T}else{E}B", "ATB")]
    [InlineData("A@if(0>1 && !true || true){T}else{E}B", "ATB")]
    [InlineData("A@if(2>1 && !true || true){T}else{E}B", "ATB")]
    public void Should_Render_Correct_Block_For_And_Or_And_Not_Operators(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);
        
        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Should_Report_Error_Even_When_Left_ShortCircuits()
    {
        // arrange
        const string template = "A@if(false && 1 + true > 0){T}else{E}B";

        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.False(result.IsOk);
        IError error = result.Error!;
        Assert.Equal("TypeMismatch", error.Code);
        Assert.Equal(TextSpan.At(1, 26), error.Range); // spans "@if(false && 1 + true > 0)"
        Assert.Equal("Operator '+' expects numbers.", error.Message);
    }

    [Theory]
    [InlineData("A@if((false || true) && true){T}else{E}B", "ATB")]
    [InlineData("A@if((true || true) && true){T}else{E}B", "ATB")]
    [InlineData("A@if((true || false) && true){T}else{E}B", "ATB")]
    [InlineData("A@if((false || false) && true){T}else{E}B", "AEB")]
    public void Should_Render_Then_When_Grouped_Condition_Is_True(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }
}