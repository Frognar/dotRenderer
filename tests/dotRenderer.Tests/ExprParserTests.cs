using DotRenderer;

namespace dotRenderer.Tests;

public class ExprParserTests
{
    [Fact]
    public void Should_Parse_Number_Addition_Into_BinaryExpr()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("1+2");

        // assert
        Assert.True(result.IsOk);
        IExpr expected = Expr.FromBinaryAdd(Expr.FromNumber(1), Expr.FromNumber(2));
        Assert.Equal(expected, result.Value);
    }
    
    [Fact]
    public void Should_Parse_Simple_Member_Access()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("u.name");

        // assert
        Assert.True(result.IsOk);
        IExpr expected = Expr.FromMember(Expr.FromIdent("u"), "name");
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Should_Parse_Nested_Member_Access()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("u.address.city");

        // assert
        Assert.True(result.IsOk);
        IExpr expected =
            Expr.FromMember(
                Expr.FromMember(
                    Expr.FromIdent("u"),
                    "address"
                ),
                "city"
            );
        Assert.Equal(expected, result.Value);
    }
    
    [Fact]
    public void Should_Parse_Simple_String_Literal()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("\"A\"");
        
        // assert
        Assert.True(result.IsOk);
        Assert.Equal(Expr.FromString("A"), result.Value);
    }

    [Fact]
    public void Should_Parse_String_With_Escaped_Quote_And_Backslash()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("\"\\\"\\\\\"");
       
        // assert
        Assert.True(result.IsOk);
        Assert.Equal(Expr.FromString("\"\\"), result.Value);
    }

    [Fact]
    public void Should_Parse_String_With_Escaped_Newline_And_Tab()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("\"A\\nB\\tC\"");
        
        // assert
        Assert.True(result.IsOk);
        Assert.Equal(Expr.FromString("A\nB\tC"), result.Value);
    }
    
    [Fact]
    public void Should_Error_When_Expression_Is_Empty()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("");
        
        // assert
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("ExprEmpty", e.Code);
        Assert.Equal(TextSpan.At(0, 0), e.Range);
    }

    [Fact]
    public void Should_Error_When_Trailing_Input_After_Expr()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("1 2");
        
        // assert
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("ExprTrailing", e.Code);
        Assert.Equal(TextSpan.At(2, 1), e.Range);
    }

    [Fact]
    public void Should_Error_On_Unexpected_Char()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("@");
       
        // assert
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("UnexpectedChar", e.Code);
        Assert.Equal(TextSpan.At(0, 1), e.Range);
    }

    [Fact]
    public void Should_Error_When_Missing_RParen()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("(1+2");
        
        // assert
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("MissingRParen", e.Code);
        Assert.Equal(TextSpan.At(4, 0), e.Range);
    }

    [Fact]
    public void Should_Error_On_Number_Format()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("1.2.");
        
        // assert
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("NumberFormat", e.Code);
        Assert.Equal(TextSpan.At(0, 4), e.Range);
    }

    [Fact]
    public void Should_Error_On_Unterminated_String()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("\"abc");
        
        // assert
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("StringUnterminated", e.Code);
        Assert.Equal(TextSpan.At(0, 4), e.Range);
    }

    [Fact]
    public void Should_Error_On_Unsupported_String_Escape()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("\"\\x\"");
        
        // assert
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("StringEscape", e.Code);
        Assert.Equal(TextSpan.At(1, 2), e.Range);
    }

    [Fact]
    public void Should_Error_On_Member_Without_Name()
    {
        // act
        Result<IExpr> result = ExprParser.Parse("a.");
        
        // assert
        Assert.False(result.IsOk);
        IError e = result.Error!;
        Assert.Equal("MemberName", e.Code);
        Assert.Equal(TextSpan.At(2, 0), e.Range);
    }
}