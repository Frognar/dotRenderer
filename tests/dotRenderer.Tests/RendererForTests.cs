using DotRenderer;

namespace dotRenderer.Tests;

public class RendererForTests
{
    [Fact]
    public void Should_Render_For_Block_By_Iterating_Sequence_And_Binding_Item()
    {
        Template template = new([
            Node.FromText("X", TextSpan.At(0, 1)),
            Node.FromFor(
                "item",
                Expr.FromIdent("items"),
                [
                    Node.FromInterpolateIdent("item", TextSpan.At(0, 4))
                ],
                TextSpan.At(1, 19)
            ),
            Node.FromText("Y", TextSpan.At(0, 1))
        ]);

        MapAccessor globals = MapAccessor.With(
            ("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b")
            ))
        );

        RendererAssert.Render(template, globals, "XabY");
    }
}