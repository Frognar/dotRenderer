using DotRenderer;

namespace dotRenderer.Tests;

public class RendererTextOnlyTests
{
    [Fact]
    public void Should_Render_Single_TextNode_Verbatim()
    {
        RendererAssert.Render(
            new Template([
                Node.FromText("Hello, renderer!", TextSpan.At(0, "Hello, renderer!".Length))
            ]),
            MapAccessor.Empty,
            "Hello, renderer!"
        );
    }
}