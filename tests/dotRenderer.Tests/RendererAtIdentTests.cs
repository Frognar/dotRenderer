using DotRenderer;

namespace dotRenderer.Tests;

public class RendererAtIdentTests
{
    [Fact]
    public void Should_Render_Interpolated_Identifier_Using_Accessor()
    {
        RendererAssert.Render(
            new Template([
                Node.FromText("Hello ", TextSpan.At(0, "Hello ".Length)),
                Node.FromInterpolateIdent("name", TextSpan.At("Hello ".Length, 1 + "name".Length)),
                Node.FromText("!", TextSpan.At("Hello ".Length + 1 + "name".Length, "!".Length))
            ]),
            MapAccessor.With(("name", Value.FromString("Alice"))),
            "Hello Alice!"
        );
    }
}