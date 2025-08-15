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

    [Fact]
    public void Should_Render_For_Block_With_Index_Bound_As_Number()
    {
        Template template = new([
            Node.FromText("X", TextSpan.At(0, 1)),
            Node.FromFor(
                "item",
                "i",
                Expr.FromIdent("items"),
                [
                    Node.FromInterpolateIdent("i", TextSpan.At(0, 1)),
                    Node.FromText(":", TextSpan.At(0, 1)),
                    Node.FromInterpolateIdent("item", TextSpan.At(0, 4)),
                    Node.FromText(";", TextSpan.At(0, 1)),
                ],
                TextSpan.At(1, 23)
            ),
            Node.FromText("Y", TextSpan.At(0, 1))
        ]);

        MapAccessor globals = MapAccessor.With(
            ("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b")
            ))
        );

        RendererAssert.Render(template, globals, "X0:a;1:b;Y");
    }

    [Fact]
    public void Should_Render_Else_When_Sequence_Is_Empty()
    {
        Template template = new([
            Node.FromText("X", TextSpan.At(0, 1)),
            Node.FromFor(
                "item",
                Expr.FromIdent("items"),
                [
                    Node.FromInterpolateIdent("item", TextSpan.At(0, 4))
                ],
                [
                    Node.FromText("EMPTY", TextSpan.At(0, 5))
                ],
                TextSpan.At(1, 19)
            ),
            Node.FromText("Y", TextSpan.At(0, 1))
        ]);

        MapAccessor globals = MapAccessor.With(("items", Value.FromSequence()));

        RendererAssert.Render(template, globals, "XEMPTYY");
    }

    [Fact]
    public void Should_Ignore_Else_When_Sequence_Is_Not_Empty()
    {
        Template template = new([
            Node.FromText("X", TextSpan.At(0, 1)),
            Node.FromFor(
                "item",
                Expr.FromIdent("items"),
                [
                    Node.FromInterpolateIdent("item", TextSpan.At(0, 4))
                ],
                [
                    Node.FromText("EMPTY", TextSpan.At(0, 5))
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