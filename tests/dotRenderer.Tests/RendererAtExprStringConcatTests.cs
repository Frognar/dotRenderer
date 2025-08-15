using DotRenderer;

namespace dotRenderer.Tests;

public class RendererAtExprStringConcatTests
{
    [Fact]
    public void Should_Render_String_Concatenation()
    {
        Template template = new([
            Node.FromText("X", TextSpan.At(0, 1)),
            Node.FromInterpolateExpr(
                Expr.FromBinaryAdd(
                    Expr.FromString("A"),
                    Expr.FromString("B")
                ),
                TextSpan.At(1, 10)
            ),
            Node.FromText("Y", TextSpan.At(0, 1))
        ]);

        RendererAssert.Render(template, MapAccessor.Empty, "XABY");
    }
}