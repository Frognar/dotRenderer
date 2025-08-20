using DotRenderer;

namespace dotRenderer.Tests;

public class RendererTests
{
    [Fact]
    public void Should_Render_Single_TextNode_Verbatim() =>
        RendererAssert.Render(
            Template.With(
                Node.FromText("Hello, renderer!", TextSpan.At(0, "Hello, renderer!".Length))
            ),
            MapAccessor.Empty,
            "Hello, renderer!"
        );

    [Fact]
    public void Should_Render_Interpolated_Identifier_Using_Accessor() =>
        RendererAssert.Render(
            Template.With(
                Node.FromText("Hello ", TextSpan.At(0, "Hello ".Length)),
                Node.FromInterpolateIdent("name", TextSpan.At("Hello ".Length, 1 + "name".Length)),
                Node.FromText("!", TextSpan.At("Hello ".Length + 1 + "name".Length, "!".Length))
            ),
            MapAccessor.With(("name", Value.FromString("Alice"))),
            "Hello Alice!"
        );

    [Fact]
    public void Should_Render_InterpolateExpr_Number_Addition() =>
        RendererAssert.Render(
            Template.With(
                Node.FromInterpolateExpr(
                    Expr.FromBinaryAdd(
                        Expr.FromNumber(1),
                        Expr.FromNumber(2)
                    ),
                    TextSpan.At(0, 6) // spans "@(1+2)" in source
                )
            ),
            MapAccessor.Empty,
            "3"
        );

    [Fact]
    public void Should_Render_String_Concatenation() =>
        RendererAssert.Render(
            Template.With(
                Node.FromText("X", TextSpan.At(0, 1)),
                Node.FromInterpolateExpr(
                    Expr.FromBinaryAdd(
                        Expr.FromString("A"),
                        Expr.FromString("B")
                    ),
                    TextSpan.At(1, 10)
                ),
                Node.FromText("Y", TextSpan.At(0, 1))
            ),
            MapAccessor.Empty,
            "XABY");

    [Fact]
    public void Should_Render_If_Then_Block_When_Condition_Is_True() =>
        RendererAssert.Render(
            Template.With(
                Node.FromText("X", TextSpan.At(0, 1)),
                Node.FromIf(
                    Expr.FromBoolean(true),
                    [
                        Node.FromText("ok", TextSpan.At(0, 2))
                    ],
                    TextSpan.At(1, 9)
                ),
                Node.FromText("Y", TextSpan.At(0, 1))
            ),
            MapAccessor.Empty,
            "XokY"
        );

    [Fact]
    public void Should_Render_Else_Block_When_Condition_Is_False() =>
        RendererAssert.Render(
            Template.With(
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
            ),
            MapAccessor.Empty,
            "AEB");


    [Fact]
    public void Should_Render_For_Block_By_Iterating_Sequence_And_Binding_Item() =>
        RendererAssert.Render(
            Template.With(
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
            ),
            MapAccessor.With(
                ("items", Value.FromSequence(
                    Value.FromString("a"),
                    Value.FromString("b")
                ))
            ), "XabY");

    [Fact]
    public void Should_Render_For_Block_With_Index_Bound_As_Number() =>
        RendererAssert.Render(
            Template.With(
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
            ),
            MapAccessor.With(
                ("items", Value.FromSequence(
                    Value.FromString("a"),
                    Value.FromString("b")
                ))
            ), "X0:a;1:b;Y");

    [Fact]
    public void Should_Render_Else_When_Sequence_Is_Empty() =>
        RendererAssert.Render(
            Template.With(
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
            ),
            MapAccessor.With(("items", Value.FromSequence())),
            "XEMPTYY");

    [Fact]
    public void Should_Ignore_Else_When_Sequence_Is_Not_Empty() =>
        RendererAssert.Render(
            Template.With(
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
            ),
            MapAccessor.With(
                ("items", Value.FromSequence(
                    Value.FromString("a"),
                    Value.FromString("b")
                ))
            ), "XabY");

    [Fact]
    public void Should_Expose_Loop_Index_And_Count() =>
        RendererAssert.Render(
            Template.With(
                Node.FromFor(
                    "item",
                    Expr.FromIdent("items"),
                    [
                        Node.FromInterpolateExpr(Expr.FromMember(Expr.FromIdent("loop"), "index"), TextSpan.At(0, 1)),
                        Node.FromText(":", TextSpan.At(0, 1)),
                        Node.FromInterpolateIdent("item", TextSpan.At(0, 1)),
                        Node.FromText(";", TextSpan.At(0, 1)),
                    ],
                    TextSpan.At(0, 1)
                )
            ),
            MapAccessor.With(("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b")
            ))),
            "0:a;1:b;");

    [Fact]
    public void Should_Expose_Loop_Even_Flags() =>
        RendererAssert.Render(
            Template.With(
                Node.FromFor(
                    "item",
                    Expr.FromIdent("items"),
                    [
                        Node.FromIf(
                            Expr.FromMember(Expr.FromIdent("loop"), "isEven"),
                            [Node.FromText("E", TextSpan.At(0, 1))],
                            [Node.FromText("O", TextSpan.At(0, 1))],
                            TextSpan.At(0, 1)
                        )
                    ],
                    TextSpan.At(0, 1)
                )
            ),
            MapAccessor.With(("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b"),
                Value.FromString("c")
            ))),
            "EOE");

    [Fact]
    public void Should_Expose_Loop_Odd_Flags() =>
        RendererAssert.Render(
            Template.With(
                Node.FromFor(
                    "item",
                    Expr.FromIdent("items"),
                    [
                        Node.FromIf(
                            Expr.FromMember(Expr.FromIdent("loop"), "isOdd"),
                            [Node.FromText("O", TextSpan.At(0, 1))],
                            [Node.FromText("E", TextSpan.At(0, 1))],
                            TextSpan.At(0, 1)
                        )
                    ],
                    TextSpan.At(0, 1)
                )
            ),
            MapAccessor.With(("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b"),
                Value.FromString("c")
            ))),
            "EOE");

    [Fact]
    public void Should_Shadow_Global_Loop_Identifier() =>
        RendererAssert.Render(
            Template.With(
                Node.FromFor(
                    "item",
                    Expr.FromIdent("items"),
                    [
                        Node.FromInterpolateExpr(
                            Expr.FromMember(Expr.FromIdent("loop"), "index"),
                            TextSpan.At(0, 1)),
                    ],
                    TextSpan.At(0, 1)
                )
            ),
            MapAccessor.With(
                ("loop", Value.FromString("should_be_shadowed")),
                ("items", Value.FromSequence(Value.FromString("x")))
            ),
            "0");

    [Fact]
    public void Should_Error_When_Interpolated_Ident_Not_Found() =>
        RendererAssert.FailsToRender(
            Template.With(
                Node.FromInterpolateIdent("name", TextSpan.At(0, 5))
            ),
            MapAccessor.Empty,
            "MissingIdent",
            TextSpan.At(0, 5));

    [Fact]
    public void Should_Error_When_Interpolated_Ident_Is_Not_Scalar() =>
        RendererAssert.FailsToRender(
            Template.With(
                Node.FromInterpolateIdent("name", TextSpan.At(0, 5))
            ),
            MapAccessor.With(("name", Value.FromMap(new Dictionary<string, Value>()))),
            "TypeMismatch",
            TextSpan.At(0, 5),
            "Identifier 'name' is not a scalar value.");

    [Fact]
    public void Should_Error_When_Interpolated_Expr_Evaluates_To_NonScalar() =>
        RendererAssert.FailsToRender(
            Template.With(
                Node.FromInterpolateExpr(
                    Expr.FromIdent("u"), TextSpan.At(0, 3))
            ),
            MapAccessor.With(("u", Value.FromMap(new Dictionary<string, Value>()))),
            "TypeMismatch",
            TextSpan.At(0, 3),
            "Expression did not evaluate to a scalar value.");

    [Fact]
    public void Should_Error_If_Condition_Is_Not_Boolean() =>
        RendererAssert.FailsToRender(
            Template.With(
                Node.FromIf(
                    Expr.FromNumber(1),
                    [Node.FromText("T", TextSpan.At(0, 1))], TextSpan.At(5, 4))
            ),
            MapAccessor.Empty,
            "TypeMismatch",
            TextSpan.At(5, 4),
            "Condition of @if must be boolean.");

    [Fact]
    public void Should_Error_When_Member_Is_Missing() =>
        RendererAssert.FailsToRender(
            Template.With(
                Node.FromInterpolateExpr(
                    Expr.FromMember(Expr.FromIdent("u"), "x"), TextSpan.At(0, 5))
            ),
            MapAccessor.With(("u", Value.FromMap(new Dictionary<string, Value>()))),
            "MissingMember",
            TextSpan.At(0, 5));

    [Fact]
    public void Should_Error_When_Unary_Not_On_Number() =>
        RendererAssert.FailsToRender(
            Template.With(
                Node.FromInterpolateExpr(
                    Expr.FromUnaryNot(Expr.FromNumber(1)), TextSpan.At(0, 2))
            ),
            MapAccessor.Empty,
            "TypeMismatch",
            TextSpan.At(0, 2),
            "Operator '!' expects boolean.");
}