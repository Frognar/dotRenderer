using DotRenderer;

namespace dotRenderer.Tests;

public class RendererNegativeTests
{
    [Fact]
    public void Should_Error_When_Interpolated_Ident_Not_Found()
    {
        Template template = new([
            Node.FromInterpolateIdent("name", TextSpan.At(0, 5))
        ]);

        Result<string> res = Renderer.Render(template, null);
        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("MissingIdent", e.Code);
        Assert.Equal(TextSpan.At(0, 5), e.Range);
    }

    [Fact]
    public void Should_Error_When_Interpolated_Ident_Is_Not_Scalar()
    {
        Template template = new([
            Node.FromInterpolateIdent("name", TextSpan.At(0, 5))
        ]);

        MapAccessor globals = MapAccessor.With(("name",
            Value.FromMap(new Dictionary<string, Value>())));

        Result<string> res = Renderer.Render(template, globals);
        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("TypeMismatch", e.Code);
        Assert.Equal(TextSpan.At(0, 5), e.Range);
        Assert.Equal("Identifier 'name' is not a scalar value.", e.Message);
    }

    [Fact]
    public void Should_Error_When_Interpolated_Expr_Evaluates_To_NonScalar()
    {
        Template template = new([
            Node.FromInterpolateExpr(
                Expr.FromIdent("u"),
                TextSpan.At(0, 3))
        ]);

        MapAccessor globals = MapAccessor.With(("u",
            Value.FromMap(new Dictionary<string, Value>())));

        Result<string> res = Renderer.Render(template, globals);
        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("TypeMismatch", e.Code);
        Assert.Equal(TextSpan.At(0, 3), e.Range);
        Assert.Equal("Expression did not evaluate to a scalar value.", e.Message);
    }

    [Fact]
    public void Should_Error_If_Condition_Is_Not_Boolean()
    {
        Template template = new([
            Node.FromIf(
                Expr.FromNumber(1),
                [Node.FromText("T", TextSpan.At(0, 1))],
                TextSpan.At(5, 4))
        ]);

        Result<string> res = Renderer.Render(template, MapAccessor.Empty);
        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("TypeMismatch", e.Code);
        Assert.Equal(TextSpan.At(5, 4), e.Range);
        Assert.Equal("Condition of @if must be boolean.", e.Message);
    }

    [Fact]
    public void Should_Error_When_Member_Is_Missing()
    {
        Template template = new([
            Node.FromInterpolateExpr(
                Expr.FromMember(Expr.FromIdent("u"), "x"),
                TextSpan.At(0, 5))
        ]);

        MapAccessor globals = MapAccessor.With(("u",
            Value.FromMap(new Dictionary<string, Value>())));

        Result<string> res = Renderer.Render(template, globals);
        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("MissingMember", e.Code);
        Assert.Equal(TextSpan.At(0, 5), e.Range);
    }

    [Fact]
    public void Should_Error_When_Unary_Not_On_Number()
    {
        Template template = new([
            Node.FromInterpolateExpr(
                Expr.FromUnaryNot(Expr.FromNumber(1)),
                TextSpan.At(0, 2))
        ]);

        Result<string> res = Renderer.Render(template, MapAccessor.Empty);
        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("TypeMismatch", e.Code);
        Assert.Equal(TextSpan.At(0, 2), e.Range);
        Assert.Equal("Operator '!' expects boolean.", e.Message);
    }
}