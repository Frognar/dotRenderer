using DotRenderer;

namespace dotRenderer.Tests;

public class ExprParserTests
{
    [Fact]
    public void Should_Parse_Number_Addition_Into_BinaryExpr() =>
        ExprParserAssert.Parse(
            "1+2",
            Expr.FromBinaryAdd(Expr.FromNumber(1), Expr.FromNumber(2)));

    [Fact]
    public void Should_Parse_Simple_Member_Access() =>
        ExprParserAssert.Parse(
            "u.name",
            Expr.FromMember(Expr.FromIdent("u"), "name"));

    [Fact]
    public void Should_Parse_Nested_Member_Access() =>
        ExprParserAssert.Parse(
            "u.address.city",
            Expr.FromMember(
                Expr.FromMember(
                    Expr.FromIdent("u"),
                    "address"
                ),
                "city"
            ));

    [Fact]
    public void Should_Parse_Simple_String_Literal() =>
        ExprParserAssert.Parse(
            "\"A\"",
            Expr.FromString("A"));

    [Fact]
    public void Should_Parse_String_With_Escaped_Quote_And_Backslash() =>
        ExprParserAssert.Parse(
            "\"\\\"\\\\\"",
            Expr.FromString("\"\\"));

    [Fact]
    public void Should_Parse_String_With_Escaped_Newline_And_Tab() =>
        ExprParserAssert.Parse(
            "\"A\\nB\\tC\"",
            Expr.FromString("A\nB\tC"));

    [Fact]
    public void Should_Error_When_Expression_Is_Empty() =>
        ExprParserAssert.FailsToParse(
            "",
            "ExprEmpty",
            TextSpan.At(0, 0));

    [Fact]
    public void Should_Error_When_Trailing_Input_After_Expr() =>
        ExprParserAssert.FailsToParse(
            "1 2",
            "ExprTrailing",
            TextSpan.At(2, 1));

    [Fact]
    public void Should_Error_On_Unexpected_Char() =>
        ExprParserAssert.FailsToParse(
            "@",
            "UnexpectedChar",
            TextSpan.At(0, 1));

    [Fact]
    public void Should_Error_When_Missing_RParen() =>
        ExprParserAssert.FailsToParse(
            "(1+2",
            "MissingRParen",
            TextSpan.At(4, 0));

    [Fact]
    public void Should_Error_On_Number_Format() =>
        ExprParserAssert.FailsToParse(
            "1.2.",
            "NumberFormat",
            TextSpan.At(0, 4));

    [Fact]
    public void Should_Error_On_Unterminated_String() =>
        ExprParserAssert.FailsToParse(
            "\"abc",
            "StringUnterminated",
            TextSpan.At(0, 4));

    [Fact]
    public void Should_Error_On_Unsupported_String_Escape() =>
        ExprParserAssert.FailsToParse(
            "\"\\x\"",
            "StringEscape",
            TextSpan.At(1, 2));

    [Fact]
    public void Should_Error_On_Member_Without_Name() =>
        ExprParserAssert.FailsToParse(
            "a.",
            "MemberName",
            TextSpan.At(2, 0));

    [Fact]
    public void Should_Error_When_Addition_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "1+",
            "ExprEmpty",
            TextSpan.At(2, 0));

    [Fact]
    public void Should_Error_When_Subtraction_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "1-",
            "ExprEmpty",
            TextSpan.At(2, 0));
}