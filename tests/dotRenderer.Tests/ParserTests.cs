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
            new Template([
                Node.FromText("Hello, World!", TextSpan.At(0, 13))
            ]));

    [Fact]
    public void Should_Parse_AtIdent_Token_Into_InterpolateIdentNode_Between_Text_Nodes() =>
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtIdent("name", TextSpan.At(6, 5)),
                Token.FromText("1", TextSpan.At(11, 1))
            ],
            new Template([
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateIdent("name", TextSpan.At(6, 5)),
                Node.FromText("1", TextSpan.At(11, 1))
            ]));

    [Fact]
    public void Should_Parse_AtExpr_Token_Into_InterpolateExprNode_Between_Text_Nodes() =>
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtExpr("1+2", TextSpan.At(6, 6)), // spans "@(1+2)"
                Token.FromText("!", TextSpan.At(12, 1))
            ],
            new Template([
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateExpr(Expr.FromBinaryAdd(Expr.FromNumber(1), Expr.FromNumber(2)), TextSpan.At(6, 6)),
                Node.FromText("!", TextSpan.At(12, 1))
            ]));

    [Fact]
    public void Should_Parse_AtExpr_With_String_Concat() =>
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtExpr("\"A\" + \"B\"", TextSpan.At(6, 12)), // "@(\"A\" + \"B\")"
                Token.FromText("!", TextSpan.At(18, 1))
            ],
            new Template([
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateExpr(
                    Expr.FromBinaryAdd(
                        Expr.FromString("A"),
                        Expr.FromString("B")
                    ),
                    TextSpan.At(6, 12)
                ),
                Node.FromText("!", TextSpan.At(18, 1))
            ])
        );

    [Fact]
    public void Should_Parse_AtExpr_Member_Access_Between_Text_Nodes() =>
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtExpr("u.name", TextSpan.At(6, 9)), // spans "@(u.name)"
                Token.FromText("!", TextSpan.At(15, 1))
            ],
            new Template([
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateExpr(
                    Expr.FromMember(Expr.FromIdent("u"), "name"),
                    TextSpan.At(6, 9)
                ),
                Node.FromText("!", TextSpan.At(15, 1))
            ])
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
            new Template([
                Node.FromText("X", TextSpan.At(0, 1)),
                Node.FromIf(
                    Expr.FromBoolean(true),
                    [
                        Node.FromText("ok", TextSpan.At(11, 2))
                    ],
                    TextSpan.At(1, 9) // range of "@if(true)"
                ),
                Node.FromText("Y", TextSpan.At(14, 1)),
            ])
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
            new Template([
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
                Node.FromText("B", TextSpan.At(20, 1)),
            ])
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
            new Template([
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
            ])
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
            new Template([
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
            ])
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
            new Template([
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
            ])
        );

    [Fact]
    public void Should_Error_When_AtExpr_Contains_Invalid_Expr()
    {
        Result<Template> parsed = Parser.Parse([
            Token.FromAtExpr("1 2", TextSpan.At(0, 6))
        ]);

        Assert.False(parsed.IsOk);
        IError e = parsed.Error!;
        Assert.Equal("ExprTrailing", e.Code);
        Assert.Equal(TextSpan.At(0, 6), e.Range);
    }

    [Fact]
    public void Should_Error_IfMissingLBrace_After_If()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtIf("true", TextSpan.At(0, 9)),
            Token.FromText("x", TextSpan.At(9, 1))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("IfMissingLBrace", e.Code);
        Assert.Equal(TextSpan.At(0, 9), e.Range);
    }

    [Fact]
    public void Should_Error_IfMissingRBrace_In_If()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtIf("true", TextSpan.At(0, 9)),
            Token.FromLBrace(TextSpan.At(9, 1)),
            Token.FromText("x", TextSpan.At(10, 1))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("IfMissingRBrace", e.Code);
        Assert.Equal(TextSpan.At(0, 9), e.Range);
    }

    [Fact]
    public void Should_Error_ElseMissingLBrace_After_If_Block()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtIf("true", TextSpan.At(0, 9)),
            Token.FromLBrace(TextSpan.At(9, 1)),
            Token.FromText("x", TextSpan.At(10, 1)),
            Token.FromRBrace(TextSpan.At(11, 1)),
            Token.FromElse(TextSpan.At(12, 4))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ElseMissingLBrace", e.Code);
        Assert.Equal(TextSpan.At(0, 9), e.Range);
    }

    [Fact]
    public void Should_Error_ElseMissingRBrace_In_If_Else_Block()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtIf("true", TextSpan.At(0, 9)),
            Token.FromLBrace(TextSpan.At(9, 1)),
            Token.FromText("x", TextSpan.At(10, 1)),
            Token.FromRBrace(TextSpan.At(11, 1)),
            Token.FromElse(TextSpan.At(12, 4)),
            Token.FromLBrace(TextSpan.At(16, 1)),
            Token.FromText("y", TextSpan.At(17, 1))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ElseMissingRBrace", e.Code);
        Assert.Equal(TextSpan.At(0, 9), e.Range);
    }

    [Fact]
    public void Should_Error_ForMissingLBrace()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtFor("item in items", TextSpan.At(0, 19)),
            Token.FromText("x", TextSpan.At(19, 1))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ForMissingLBrace", e.Code);
        Assert.Equal(TextSpan.At(0, 19), e.Range);
    }

    [Fact]
    public void Should_Error_ForMissingRBrace()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtFor("item in items", TextSpan.At(0, 19)),
            Token.FromLBrace(TextSpan.At(19, 1)),
            Token.FromText("x", TextSpan.At(20, 1))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ForMissingRBrace", e.Code);
        Assert.Equal(TextSpan.At(0, 19), e.Range);
    }

    [Fact]
    public void Should_Error_ElseMissingLBrace_In_For()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtFor("item in items", TextSpan.At(0, 19)),
            Token.FromLBrace(TextSpan.At(19, 1)),
            Token.FromText("x", TextSpan.At(20, 1)),
            Token.FromRBrace(TextSpan.At(21, 1)),
            Token.FromElse(TextSpan.At(22, 4))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ElseMissingLBrace", e.Code);
        Assert.Equal(TextSpan.At(0, 19), e.Range);
    }

    [Fact]
    public void Should_Error_ElseMissingRBrace_In_For()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtFor("item in items", TextSpan.At(0, 19)),
            Token.FromLBrace(TextSpan.At(19, 1)),
            Token.FromText("x", TextSpan.At(20, 1)),
            Token.FromRBrace(TextSpan.At(21, 1)),
            Token.FromElse(TextSpan.At(22, 4)),
            Token.FromLBrace(TextSpan.At(26, 1)),
            Token.FromText("e", TextSpan.At(27, 1))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ElseMissingRBrace", e.Code);
        Assert.Equal(TextSpan.At(0, 19), e.Range);
    }

    [Fact]
    public void Should_Error_ForHeader_Item_Ident()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtFor("1 in items", TextSpan.At(0, 12))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ForItemIdent", e.Code);
        Assert.Equal(TextSpan.At(0, 12), e.Range);
    }

    [Fact]
    public void Should_Error_ForHeader_Index_Ident()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtFor("item, 1 in items", TextSpan.At(0, 17))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ForIndexIdent", e.Code);
        Assert.Equal(TextSpan.At(0, 17), e.Range);
    }

    [Fact]
    public void Should_Error_ForHeader_Missing_In()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtFor("item of items", TextSpan.At(0, 15))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ForMissingIn", e.Code);
        Assert.Equal(TextSpan.At(0, 15), e.Range);
    }

    [Fact]
    public void Should_Error_ForHeader_Missing_Expr()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtFor("item in   ", TextSpan.At(0, 10))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ForMissingExpr", e.Code);
        Assert.Equal(TextSpan.At(0, 10), e.Range);
    }

    [Fact]
    public void Should_Error_ForHeader_Expr_Parse_Error_Mapped_To_Token_Range()
    {
        Result<Template> res = Parser.Parse([
            Token.FromAtFor("item in 1 2", TextSpan.At(0, 12))
        ]);

        Assert.False(res.IsOk);
        IError e = res.Error!;
        Assert.Equal("ExprTrailing", e.Code);
        Assert.Equal(TextSpan.At(0, 12), e.Range);
    }
}