using DotRenderer;

namespace dotRenderer.Tests;

public class RendererAtExprAdditionTests
{
    [Fact]
    public void Should_Render_InterpolateExpr_Number_Addition()
    {
        RendererAssert.Render(
            new Template([
                Node.FromInterpolateExpr(
                    Expr.FromBinaryAdd(
                        Expr.FromNumber(1),
                        Expr.FromNumber(2)
                    ),
                    TextSpan.At(0, 6) // spans "@(1+2)" in source
                )
            ]),
            MapAccessor.Empty,
            "3"
        );
    }
}