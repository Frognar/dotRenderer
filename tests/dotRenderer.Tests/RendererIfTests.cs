using DotRenderer;

namespace dotRenderer.Tests;

public class RendererIfTests
{
    [Fact]
    public void Should_Render_If_Then_Block_When_Condition_Is_True()
    {
        RendererAssert.Render(
            new Template([
                Node.FromText("X", TextSpan.At(0, 1)),
                Node.FromIf(
                    Expr.FromBoolean(true),
                    [
                        Node.FromText("ok", TextSpan.At(0, 2))
                    ],
                    TextSpan.At(1, 9)
                ),
                Node.FromText("Y", TextSpan.At(0, 1))
            ]),
            MapAccessor.Empty,
            "XokY"
        );
    }

    [Fact]
    public void Should_Render_Else_Block_When_Condition_Is_False()
    {
        Template template = new([
            Node.FromText("A", TextSpan.At(0, 1)),
            Node.FromIf(
                Expr.FromBoolean(false),
                [
                    Node.FromText("T", TextSpan.At(0, 1))
                ],
                [
                    Node.FromText("E", TextSpan.At(0, 1))
                ],
                TextSpan.At(1, 14)
            ),
            Node.FromText("B", TextSpan.At(0, 1))
        ]);

        RendererAssert.Render(template, MapAccessor.Empty, "AEB");
    }
}