using DotRenderer;

namespace dotRenderer.Tests;

public class ParserTests
{
    [Fact]
    public void Should_Parse_Plain_Text_Into_Single_TextNode_In_Template() =>
        ParserAssert.Parse(
            [
                Token.FromText("Hello, World!", TextSpan.At(0, 13))
            ],
            Template.With(
                Node.FromText("Hello, World!", TextSpan.At(0, 13))
            ));

    [Fact]
    public void Should_Parse_AtIdent_Token_Into_InterpolateIdentNode_Between_Text_Nodes() =>
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtIdent("name", TextSpan.At(6, 5)),
                Token.FromText("1", TextSpan.At(11, 1))
            ],
            Template.With(
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateIdent("name", TextSpan.At(6, 5)),
                Node.FromText("1", TextSpan.At(11, 1))
            ));

    [Fact]
    public void Should_Parse_AtExpr_Token_Into_InterpolateExprNode_Between_Text_Nodes() =>
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtExpr("1+2", TextSpan.At(6, 6)), // spans "@(1+2)"
                Token.FromText("!", TextSpan.At(12, 1))
            ],
            Template.With(
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateExpr(Expr.FromBinaryAdd(Expr.FromNumber(1), Expr.FromNumber(2)), TextSpan.At(6, 6)),
                Node.FromText("!", TextSpan.At(12, 1))
            ));

    [Fact]
    public void Should_Parse_AtExpr_With_String_Concat() =>
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtExpr("\"A\" + \"B\"", TextSpan.At(6, 12)), // "@(\"A\" + \"B\")"
                Token.FromText("!", TextSpan.At(18, 1))
            ],
            Template.With(
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateExpr(
                    Expr.FromBinaryAdd(
                        Expr.FromString("A"),
                        Expr.FromString("B")
                    ),
                    TextSpan.At(6, 12)
                ),
                Node.FromText("!", TextSpan.At(18, 1))
            )
        );

    [Fact]
    public void Should_Parse_AtExpr_Member_Access_Between_Text_Nodes() =>
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtExpr("u.name", TextSpan.At(6, 9)), // spans "@(u.name)"
                Token.FromText("!", TextSpan.At(15, 1))
            ],
            Template.With(
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateExpr(
                    Expr.FromMember(Expr.FromIdent("u"), "name"),
                    TextSpan.At(6, 9)
                ),
                Node.FromText("!", TextSpan.At(15, 1))
            )
        );

    [Fact]
    public void Should_Parse_AtIf_With_Single_Block_No_Else() =>
        ParserAssert.Parse(
            [
                Token.FromText("X", TextSpan.At(0, 1)),
                Token.FromAtIf("true", TextSpan.At(1, 9)), // "@if(true)"
                Token.FromLBrace(TextSpan.At(10, 1)), // "{"
                Token.FromText("ok", TextSpan.At(11, 2)),
                Token.FromRBrace(TextSpan.At(13, 1)), // "}"
                Token.FromText("Y", TextSpan.At(14, 1)),
            ],
            Template.With(
                Node.FromText("X", TextSpan.At(0, 1)),
                Node.FromIf(
                    Expr.FromBoolean(true),
                    [
                        Node.FromText("ok", TextSpan.At(11, 2))
                    ],
                    TextSpan.At(1, 9) // range of "@if(true)"
                ),
                Node.FromText("Y", TextSpan.At(14, 1))
            )
        );

    [Fact]
    public void Should_Parse_AtIf_With_Then_And_Else_Blocks() =>
        ParserAssert.Parse(
            [
                Token.FromText("A", TextSpan.At(0, 1)),
                Token.FromAtIf("true", TextSpan.At(1, 9)),
                Token.FromLBrace(TextSpan.At(10, 1)),
                Token.FromText("T", TextSpan.At(11, 1)),
                Token.FromRBrace(TextSpan.At(12, 1)),
                Token.FromElse(TextSpan.At(13, 4)),
                Token.FromLBrace(TextSpan.At(17, 1)),
                Token.FromText("E", TextSpan.At(18, 1)),
                Token.FromRBrace(TextSpan.At(19, 1)),
                Token.FromText("B", TextSpan.At(20, 1)),
            ],
            Template.With(
                Node.FromText("A", TextSpan.At(0, 1)),
                Node.FromIf(
                    Expr.FromBoolean(true),
                    [
                        Node.FromText("T", TextSpan.At(11, 1))
                    ],
                    [
                        Node.FromText("E", TextSpan.At(18, 1))
                    ],
                    TextSpan.At(1, 9) // range of "@if(true)"
                ),
                Node.FromText("B", TextSpan.At(20, 1))
            )
        );

    [Fact]
    public void Should_Parse_AtFor_Header_And_Block() =>
        ParserAssert.Parse(
            [
                Token.FromText("A", TextSpan.At(0, 1)),
                Token.FromAtFor("item in items", TextSpan.At(1, 19)),
                Token.FromLBrace(TextSpan.At(20, 1)),
                Token.FromText("x", TextSpan.At(21, 1)),
                Token.FromRBrace(TextSpan.At(22, 1)),
                Token.FromText("B", TextSpan.At(23, 1)),
            ],
            Template.With(
                Node.FromText("A", TextSpan.At(0, 1)),
                Node.FromFor(
                    "item",
                    Expr.FromIdent("items"),
                    [
                        Node.FromText("x", TextSpan.At(21, 1))
                    ],
                    TextSpan.At(1, 19)
                ),
                Node.FromText("B", TextSpan.At(23, 1))
            )
        );

    [Fact]
    public void Should_Parse_AtFor_Header_With_Item_And_Index() =>
        ParserAssert.Parse(
            [
                Token.FromText("A", TextSpan.At(0, 1)),
                Token.FromAtFor("item, i in items", TextSpan.At(1, 21)),
                Token.FromLBrace(TextSpan.At(22, 1)),
                Token.FromText("x", TextSpan.At(23, 1)),
                Token.FromRBrace(TextSpan.At(24, 1)),
                Token.FromText("B", TextSpan.At(25, 1)),
            ],
            Template.With(
                Node.FromText("A", TextSpan.At(0, 1)),
                Node.FromFor(
                    "item",
                    "i",
                    Expr.FromIdent("items"),
                    [
                        Node.FromText("x", TextSpan.At(23, 1))
                    ],
                    TextSpan.At(1, 21)
                ),
                Node.FromText("B", TextSpan.At(25, 1))
            )
        );

    [Fact]
    public void Should_Parse_For_With_Else_Block() =>
        ParserAssert.Parse(
            [
                Token.FromText("A", TextSpan.At(0, 1)),
                Token.FromAtFor("item in items", TextSpan.At(1, 19)),
                Token.FromLBrace(TextSpan.At(20, 1)),
                Token.FromText("x", TextSpan.At(21, 1)),
                Token.FromRBrace(TextSpan.At(22, 1)),
                Token.FromElse(TextSpan.At(23, 4)),
                Token.FromLBrace(TextSpan.At(27, 1)),
                Token.FromText("e", TextSpan.At(28, 1)),
                Token.FromRBrace(TextSpan.At(29, 1)),
                Token.FromText("B", TextSpan.At(30, 1)),
            ],
            Template.With(
                Node.FromText("A", TextSpan.At(0, 1)),
                Node.FromFor(
                    "item",
                    Expr.FromIdent("items"),
                    [
                        Node.FromText("x", TextSpan.At(21, 1))
                    ],
                    [
                        Node.FromText("e", TextSpan.At(28, 1))
                    ],
                    TextSpan.At(1, 19)
                ),
                Node.FromText("B", TextSpan.At(30, 1))
            )
        );

    [Fact]
    public void Should_Parse_If_Elif_Else_Chain() =>
        ParserAssert.Parse(
            [
                Token.FromText("A", TextSpan.At(0, 1)),
                Token.FromAtIf("false", TextSpan.At(1, 10)),
                Token.FromLBrace(TextSpan.At(11, 1)),
                Token.FromText("T", TextSpan.At(12, 1)),
                Token.FromRBrace(TextSpan.At(13, 1)),
                Token.FromElse(TextSpan.At(14, 4)),
                Token.FromAtIf("true", TextSpan.At(14, 11)),
                Token.FromLBrace(TextSpan.At(25, 1)),
                Token.FromText("U", TextSpan.At(26, 1)),
                Token.FromRBrace(TextSpan.At(27, 1)),
                Token.FromElse(TextSpan.At(28, 4)),
                Token.FromLBrace(TextSpan.At(32, 1)),
                Token.FromText("E", TextSpan.At(33, 1)),
                Token.FromRBrace(TextSpan.At(34, 1)),
                Token.FromText("B", TextSpan.At(35, 1)),
            ],
            Template.With(
                Node.FromText("A", TextSpan.At(0, 1)),
                Node.FromIf(
                    Expr.FromBoolean(false),
                    [
                        Node.FromText("T", TextSpan.At(12, 1))
                    ],
                    [
                        Node.FromIf(
                            Expr.FromBoolean(true),
                            [
                                Node.FromText("U", TextSpan.At(26, 1))
                            ],
                            [
                                Node.FromText("E", TextSpan.At(33, 1))
                            ],
                            TextSpan.At(14, 11)
                        )
                    ],
                    TextSpan.At(1, 10)
                ),
                Node.FromText("B", TextSpan.At(35, 1))
            ));

    [Fact]
    public void Should_Error_When_AtExpr_Contains_Invalid_Expr() =>
        ParserAssert.FailsToParse([Token.FromAtExpr("1 2", TextSpan.At(0, 6))],
            "ExprTrailing",
            TextSpan.At(0, 6));

    [Fact]
    public void Should_Error_IfMissingLBrace_After_If() =>
        ParserAssert.FailsToParse([
                Token.FromAtIf("true", TextSpan.At(0, 9)),
                Token.FromText("x", TextSpan.At(9, 1))
            ],
            "IfMissingLBrace",
            TextSpan.At(0, 9));

    [Fact]
    public void Should_Error_IfMissingRBrace_In_If() =>
        ParserAssert.FailsToParse([
                Token.FromAtIf("true", TextSpan.At(0, 9)),
                Token.FromLBrace(TextSpan.At(9, 1)),
                Token.FromText("x", TextSpan.At(10, 1))
            ],
            "IfMissingRBrace",
            TextSpan.At(0, 9));

    [Fact]
    public void Should_Error_ElseMissingLBrace_After_If_Block() =>
        ParserAssert.FailsToParse([
                Token.FromAtIf("true", TextSpan.At(0, 9)),
                Token.FromLBrace(TextSpan.At(9, 1)),
                Token.FromText("x", TextSpan.At(10, 1)),
                Token.FromRBrace(TextSpan.At(11, 1)),
                Token.FromElse(TextSpan.At(12, 4))
            ],
            "ElseMissingLBrace",
            TextSpan.At(0, 9));

    [Fact]
    public void Should_Error_ElseMissingRBrace_In_If_Else_Block() =>
        ParserAssert.FailsToParse([
                Token.FromAtIf("true", TextSpan.At(0, 9)),
                Token.FromLBrace(TextSpan.At(9, 1)),
                Token.FromText("x", TextSpan.At(10, 1)),
                Token.FromRBrace(TextSpan.At(11, 1)),
                Token.FromElse(TextSpan.At(12, 4)),
                Token.FromLBrace(TextSpan.At(16, 1)),
                Token.FromText("y", TextSpan.At(17, 1))
            ],
            "ElseMissingRBrace",
            TextSpan.At(0, 9));

    [Fact]
    public void Should_Error_ForMissingLBrace() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item in items", TextSpan.At(0, 19)),
                Token.FromText("x", TextSpan.At(19, 1))
            ],
            "ForMissingLBrace",
            TextSpan.At(0, 19));

    [Fact]
    public void Should_Error_ForMissingRBrace() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item in items", TextSpan.At(0, 19)),
                Token.FromLBrace(TextSpan.At(19, 1)),
                Token.FromText("x", TextSpan.At(20, 1))
            ],
            "ForMissingRBrace",
            TextSpan.At(0, 19));

    [Fact]
    public void Should_Error_ElseMissingLBrace_In_For() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item in items", TextSpan.At(0, 19)),
                Token.FromLBrace(TextSpan.At(19, 1)),
                Token.FromText("x", TextSpan.At(20, 1)),
                Token.FromRBrace(TextSpan.At(21, 1)),
                Token.FromElse(TextSpan.At(22, 4))
            ],
            "ElseMissingLBrace",
            TextSpan.At(0, 19));

    [Fact]
    public void Should_Error_ElseMissingRBrace_In_For() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item in items", TextSpan.At(0, 19)),
                Token.FromLBrace(TextSpan.At(19, 1)),
                Token.FromText("x", TextSpan.At(20, 1)),
                Token.FromRBrace(TextSpan.At(21, 1)),
                Token.FromElse(TextSpan.At(22, 4)),
                Token.FromLBrace(TextSpan.At(26, 1)),
                Token.FromText("e", TextSpan.At(27, 1))
            ],
            "ElseMissingRBrace",
            TextSpan.At(0, 19));

    [Fact]
    public void Should_Error_ForHeader_Item_Ident() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("1 in items", TextSpan.At(0, 12))
            ],
            "ForItemIdent",
            TextSpan.At(0, 12));

    [Fact]
    public void Should_Error_ForHeader_Index_Ident() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item, 1 in items", TextSpan.At(0, 17))
            ],
            "ForIndexIdent",
            TextSpan.At(0, 17));

    [Fact]
    public void Should_Error_ForHeader_Missing_In() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item of items", TextSpan.At(0, 15))
            ],
            "ForMissingIn",
            TextSpan.At(0, 15));

    [Fact]
    public void Should_Error_ForHeader_Missing_Expr() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item in   ", TextSpan.At(0, 10))
            ],
            "ForMissingExpr",
            TextSpan.At(0, 10));

    [Fact]
    public void Should_Error_ForHeader_Expr_Parse_Error_Mapped_To_Token_Range() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item in 1 2", TextSpan.At(0, 12))
            ],
            "ExprTrailing",
            TextSpan.At(0, 12));

    [Fact]
    public void Should_Error_ForBody_Expr_Parse_Error_In_For() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item in items", TextSpan.At(0, 19)),
                Token.FromLBrace(TextSpan.At(19, 1)),
                Token.FromAtExpr("1 2", TextSpan.At(20, 6))
            ],
            "ExprTrailing",
            TextSpan.At(20, 6));

    [Fact]
    public void Should_Error_ForElseBody_Expr_Parse_Error_In_For() =>
        ParserAssert.FailsToParse([
                Token.FromAtFor("item in items", TextSpan.At(0, 19)),
                Token.FromLBrace(TextSpan.At(19, 1)),
                Token.FromText("x", TextSpan.At(20, 1)),
                Token.FromRBrace(TextSpan.At(21, 1)),
                Token.FromElse(TextSpan.At(22, 4)),
                Token.FromLBrace(TextSpan.At(26, 1)),
                Token.FromAtExpr("1 2", TextSpan.At(27, 6))
            ],
            "ExprTrailing",
            TextSpan.At(27, 6));
}