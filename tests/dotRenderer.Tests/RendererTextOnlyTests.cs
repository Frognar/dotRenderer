using DotRenderer;

namespace dotRenderer.Tests;

public class RendererTextOnlyTests
{
    [Fact]
    public void Should_Render_Single_TextNode_Verbatim()
    {
        // arrange
        const string input = "Hello, renderer!";
        Template template = new([Node.FromText(input, TextSpan.At(0, input.Length))]);

        // act
        Result<string> result = Renderer.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(input, result.Value);
    }
}