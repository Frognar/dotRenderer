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
    public void Should_Parse_String_With_Escaped_Carriage_Return() =>
        ExprParserAssert.Parse(
            "\"A\\rB\"",
            Expr.FromString("A\rB"));

    [Fact]
    public void Should_Parse_True_Literal_Into_BooleanExpr() =>
        ExprParserAssert.Parse(
            "true",
            Expr.FromBoolean(true));

    [Fact]
    public void Should_Parse_False_Literal_Into_BooleanExpr() =>
        ExprParserAssert.Parse(
            "false",
            Expr.FromBoolean(false));

    [Fact]
    public void Should_Parse_Chained_Or_As_LeftAssociative()
        => ExprParserAssert.Parse(
            "false || true || false",
            Expr.FromBinaryOr(
                Expr.FromBinaryOr(
                    Expr.FromBoolean(false),
                    Expr.FromBoolean(true)),
                Expr.FromBoolean(false)));

    [Fact]
    public void Should_Parse_Chained_And_As_LeftAssociative() =>
        ExprParserAssert.Parse(
            "true && false && true",
            Expr.FromBinaryAnd(
                Expr.FromBinaryAnd(
                    Expr.FromBoolean(true),
                    Expr.FromBoolean(false)),
                Expr.FromBoolean(true)));

    [Fact]
    public void Should_Parse_Simple_Subtraction() =>
        ExprParserAssert.Parse(
            "3-2",
            Expr.FromBinarySub(Expr.FromNumber(3), Expr.FromNumber(2)));

    [Fact]
    public void Should_Parse_Simple_Multiplication() =>
        ExprParserAssert.Parse(
            "2*3",
            Expr.FromBinaryMul(Expr.FromNumber(2), Expr.FromNumber(3)));

    [Fact]
    public void Should_Parse_Simple_Division() =>
        ExprParserAssert.Parse(
            "6/2",
            Expr.FromBinaryDiv(Expr.FromNumber(6), Expr.FromNumber(2)));

    [Fact]
    public void Should_Parse_Simple_Modulo() =>
        ExprParserAssert.Parse(
            "7%3",
            Expr.FromBinaryMod(Expr.FromNumber(7), Expr.FromNumber(3)));

    [Fact]
    public void Should_Parse_Chained_Multiplicative_As_LeftAssociative() =>
        ExprParserAssert.Parse(
            "2*3/4%5",
            Expr.FromBinaryMod(
                Expr.FromBinaryDiv(
                    Expr.FromBinaryMul(
                        Expr.FromNumber(2),
                        Expr.FromNumber(3)),
                    Expr.FromNumber(4)),
                Expr.FromNumber(5)));

    [Fact]
    public void Should_Parse_Chained_Equality_As_LeftAssociative() =>
        ExprParserAssert.Parse(
            "1 == 2 == 3",
            Expr.FromBinaryEq(
                Expr.FromBinaryEq(
                    Expr.FromNumber(1),
                    Expr.FromNumber(2)),
                Expr.FromNumber(3)));

    [Fact]
    public void Should_Parse_Simple_LessThan() =>
        ExprParserAssert.Parse(
            "1<2",
            Expr.FromBinaryLt(Expr.FromNumber(1), Expr.FromNumber(2)));

    [Fact]
    public void Should_Parse_Simple_GreaterThan() =>
        ExprParserAssert.Parse(
            "1>2",
            Expr.FromBinaryGt(Expr.FromNumber(1), Expr.FromNumber(2)));

    [Fact]
    public void Should_Parse_Simple_LessOrEqual() =>
        ExprParserAssert.Parse(
            "1<=2",
            Expr.FromBinaryLe(Expr.FromNumber(1), Expr.FromNumber(2)));

    [Fact]
    public void Should_Parse_Simple_GreaterOrEqual() =>
        ExprParserAssert.Parse(
            "1>=2",
            Expr.FromBinaryGe(Expr.FromNumber(1), Expr.FromNumber(2)));

    [Fact]
    public void Should_Parse_Chained_Relation_And_Stay_LeftAssociative() =>
        ExprParserAssert.Parse(
            "1 < 2 <= 3 > 1 >= 0",
            Expr.FromBinaryGe(
                Expr.FromBinaryGt(
                    Expr.FromBinaryLe(
                        Expr.FromBinaryLt(
                            Expr.FromNumber(1),
                            Expr.FromNumber(2)),
                        Expr.FromNumber(3)),
                    Expr.FromNumber(1)),
                Expr.FromNumber(0)));

    [Fact]
    public void Should_Parse_Unary_Not() =>
        ExprParserAssert.Parse(
            "!false",
            Expr.FromUnaryNot(Expr.FromBoolean(false)));

    [Fact]
    public void Should_Parse_Unary_Negation() =>
        ExprParserAssert.Parse(
            "-1",
            Expr.FromUnaryNeg(Expr.FromNumber(1)));

    [Fact]
    public void Should_Parse_Parenthesized_Expression()
        => ExprParserAssert.Parse(
            "(1)",
            Expr.FromNumber(1));

    [Fact]
    public void Should_Error_When_UnaryNot_Missing_Operand() =>
        ExprParserAssert.FailsToParse(
            "!",
            "ExprEmpty",
            TextSpan.At(1, 0));

    [Fact]
    public void Should_Error_When_UnaryNeg_Missing_Operand() =>
        ExprParserAssert.FailsToParse(
            "-",
            "ExprEmpty",
            TextSpan.At(1, 0));

    [Fact]
    public void Should_Error_When_LessThan_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "1<",
            "ExprEmpty",
            TextSpan.At(2, 0));

    [Fact]
    public void Should_Error_When_GreaterThan_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "1>",
            "ExprEmpty",
            TextSpan.At(2, 0));

    [Fact]
    public void Should_Error_When_LessOrEqual_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "1<=",
            "ExprEmpty",
            TextSpan.At(3, 0));

    [Fact]
    public void Should_Error_When_GreaterOrEqual_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "1>=",
            "ExprEmpty",
            TextSpan.At(3, 0));

    [Fact]
    public void Should_Error_When_Multiplication_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "2*",
            "ExprEmpty",
            TextSpan.At(2, 0));

    [Fact]
    public void Should_Error_When_Division_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "2/",
            "ExprEmpty",
            TextSpan.At(2, 0));

    [Fact]
    public void Should_Error_When_Modulo_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "2%",
            "ExprEmpty",
            TextSpan.At(2, 0));

    [Fact]
    public void Should_Error_When_Equality_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "1==",
            "ExprEmpty",
            TextSpan.At(3, 0));

    [Fact]
    public void Should_Error_When_And_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "true&&",
            "ExprEmpty",
            TextSpan.At(6, 0));

    [Fact]
    public void Should_Error_In_Paren_When_Expression_Is_Empty()
        => ExprParserAssert.FailsToParse(
            "(",
            "ExprEmpty",
            TextSpan.At(1, 0));

    [Fact]
    public void Should_Error_In_Paren_On_Unexpected_Char()
        => ExprParserAssert.FailsToParse(
            "( )",
            "UnexpectedChar",
            TextSpan.At(2, 1));

    [Fact]
    public void Should_Error_When_Or_Missing_Right_Operand() =>
        ExprParserAssert.FailsToParse(
            "true||",
            "ExprEmpty",
            TextSpan.At(6, 0));

    [Fact]
    public void Should_Error_On_Number_With_Dot_And_No_Fraction()
        => ExprParserAssert.FailsToParse(
            "1.",
            "NumberFormat",
            TextSpan.At(0, 2));

    [Fact]
    public void Should_Error_On_Number_With_NonAscii_Digits()
        => ExprParserAssert.FailsToParse(
            "١",
            "NumberFormat",
            TextSpan.At(0, 1));

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