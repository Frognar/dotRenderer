using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineArithmeticPrecedenceTests
{
    [Theory]
    [InlineData("Result: @(1 + 2 * (3 - 1))", "Result: 5")]
    [InlineData("Result: @(-1 + 2 * (3 - 1))", "Result: 3")]
    [InlineData("Result: @(2 / (3 - 1))", "Result: 1")]
    [InlineData("Result: @(2 % 2)", "Result: 0")]
    public void Should_Render_Add_Multiply_With_Grouping_Precedence(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Should_Report_Error_When_Division_By_Zero()
    {
        // arrange
        const string template = "Result: @(1 / 0)";
        
        // act
        Result<string> result = TemplateEngine.Render(template);
        
        // assert
        Assert.False(result.IsOk);
        IError error = result.Error!;
        Assert.Equal("DivisionByZero", error.Code);
    }
}