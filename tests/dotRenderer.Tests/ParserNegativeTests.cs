using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class ParserNegativeTests
{
    [Fact]
    public void Should_Error_When_AtExpr_Contains_Invalid_Expr()
    {
        Result<ImmutableArray<Token>> lex = Lexer.Lex("@(1 2)");
        Assert.True(lex.IsOk);

        Result<Template> parsed = Parser.Parse(lex.Value);
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