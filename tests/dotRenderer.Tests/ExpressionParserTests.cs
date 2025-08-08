using System.Runtime.CompilerServices;

namespace dotRenderer.Tests;

public class ExpressionParserTests
{
    [Fact]
    public void ExpressionParser_Should_Parse_Bool_Property()
    {
        string expr = "Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(node, new PropertyExpr(["Model", "IsAdmin"]));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Nested_Property()
    {
        string expr = "Model.User.Name";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(node, new PropertyExpr(["Model", "User", "Name"]));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Bool_Literal_True()
    {
        string expr = "true";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(node, new LiteralExpr<bool>(true));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Bool_Literal_False()
    {
        string expr = "false";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(node, new LiteralExpr<bool>(false));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Unary_Not_On_Property()
    {
        string expr = "!Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new UnaryExpr("!", new PropertyExpr(["Model", "IsAdmin"])));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Equality_Expression()
    {
        string expr = "Model.IsAdmin == true";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "==",
                new PropertyExpr(["Model", "IsAdmin"]),
                new LiteralExpr<bool>(true)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_GreaterEqual_Expression()
    {
        string expr = "Model.Age >= 18";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(">=",
                new PropertyExpr(["Model", "Age"]),
                new LiteralExpr<int>(18)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_LessThanOrEqual()
    {
        string expr = "Model.Age <= 18";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr("<=",
                new PropertyExpr(["Model", "Age"]),
                new LiteralExpr<int>(18)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_And_Expression()
    {
        string expr = "Model.Age >= 18 && Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "&&",
                new BinaryExpr(">=",
                    new PropertyExpr(["Model", "Age"]),
                    new LiteralExpr<int>(18)),
                new PropertyExpr(["Model", "IsAdmin"])));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Or_Expression()
    {
        string expr = "Model.Age >= 18 || Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "||",
                new BinaryExpr(">=",
                    new PropertyExpr(["Model", "Age"]),
                    new LiteralExpr<int>(18)),
                new PropertyExpr(["Model", "IsAdmin"])));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_And_With_Unary_Not()
    {
        string expr = "Model.Age >= 18 && !Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "&&",
                new BinaryExpr(">=",
                    new PropertyExpr(["Model", "Age"]),
                    new LiteralExpr<int>(18)),
                new UnaryExpr("!", new PropertyExpr(["Model", "IsAdmin"]))));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Equality_With_Double_Literal()
    {
        string expr = "Model.Ratio == 1.5";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "==",
                new PropertyExpr(["Model", "Ratio"]),
                new LiteralExpr<double>(1.5)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Equality_With_String_Literal()
    {
        string expr = "Model.Name == \"Alice\"";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "==",
                new PropertyExpr(["Model", "Name"]),
                new LiteralExpr<string>("Alice")));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Inequality()
    {
        string expr = "Model.Name != \"Alice\"";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "!=",
                new PropertyExpr(["Model", "Name"]),
                new LiteralExpr<string>("Alice")));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_LessThan()
    {
        string expr = "Model.Count < 10";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "<",
                new PropertyExpr(["Model", "Count"]),
                new LiteralExpr<int>(10)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_GreaterThan()
    {
        string expr = "Model.Count > 1";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                ">",
                new PropertyExpr(["Model", "Count"]),
                new LiteralExpr<int>(1)));
    }


    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Addition()
    {
        string expr = "Model.Count + 1";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "+",
                new PropertyExpr(["Model", "Count"]),
                new LiteralExpr<int>(1)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Subtraction()
    {
        string expr = "Model.Count - 1";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "-",
                new PropertyExpr(["Model", "Count"]),
                new LiteralExpr<int>(1)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Parenthesized_Expression()
    {
        string expr = "(Model.Age > 18) && Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "&&",
                new BinaryExpr(
                    ">",
                    new PropertyExpr(["Model", "Age"]),
                    new LiteralExpr<int>(18)
                ),
                new PropertyExpr(["Model", "IsAdmin"])));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Nested_Parentheses()
    {
        string expr = "Model.Age > 18 && (Model.IsAdmin || Model.IsMod)";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "&&",
                new BinaryExpr(
                    ">",
                    new PropertyExpr(["Model", "Age"]),
                    new LiteralExpr<int>(18)
                ),
                new BinaryExpr(
                    "||",
                    new PropertyExpr(["Model", "IsAdmin"]),
                    new PropertyExpr(["Model", "IsMod"]))));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Multiplication()
    {
        string expr = "Model.Count * 2";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "*",
                new PropertyExpr(["Model", "Count"]),
                new LiteralExpr<int>(2)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Division()
    {
        string expr = "Model.Count / 2";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "/",
                new PropertyExpr(["Model", "Count"]),
                new LiteralExpr<int>(2)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Modulo()
    {
        string expr = "Model.Count % 2";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "%",
                new PropertyExpr(["Model", "Count"]),
                new LiteralExpr<int>(2)));
    }

    [Fact]
    public void ExpressionParser_Should_Respect_Operator_Precedence_Multiplication_Addition()
    {
        string expr = "Model.Count + 2 * 3";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new BinaryExpr(
                "+",
                new PropertyExpr(["Model", "Count"]),
                new BinaryExpr(
                    "*",
                    new LiteralExpr<int>(2),
                    new LiteralExpr<int>(3))));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Unary_Minus_Int()
    {
        string expr = "-5";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(node, new UnaryExpr("-", new LiteralExpr<int>(5)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Unary_Minus_Double()
    {
        string expr = "-1.5";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(node, new UnaryExpr("-", new LiteralExpr<double>(1.5)));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Unary_Minus_On_Property()
    {
        string expr = "-Model.Val";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(
            node,
            new UnaryExpr("-", new PropertyExpr(["Model", "Val"])));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Double_Scientific_Literal()
    {
        string expr = "1e3";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(node, new LiteralExpr<double>(1000.0));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Double_Scientific_Literal_With_Negative_Exponent()
    {
        string expr = "1e-3";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(node, new LiteralExpr<double>(0.001));
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Double_Scientific_Literal_With_Plus_Exponent()
    {
        string expr = "2E+2";

        ExprNode node = ExpressionParser.Parse(expr);

        ExpressionAssert.AstEquals(node, new LiteralExpr<double>(200.0));
    }

    [Theory]
    [InlineData("1e")]
    [InlineData("1e+")]
    [InlineData("1E-")]
    public void ExpressionParser_Should_Throw_On_Invalid_Scientific_Notation(string expr)
    {
        ExpressionAssert.Throws<InvalidOperationException>(expr, "Invalid scientific notation");
    }

    [Fact]
    public void ExpressionParser_Should_Throw_On_Unknown_Token()
    {
        ExpressionAssert.Throws<InvalidOperationException>("???", "Unknown token near: '???'");
    }

    [Theory]
    [InlineData("Model.")]
    [InlineData("Model.User.")]
    public void ExpressionParser_Should_Throw_On_Missing_Property_Segment(string expr)
    {
        ExpressionAssert.Throws<InvalidOperationException>(expr, "Expected property segment after '.'");
    }

    [Fact]
    public void ExpressionParser_Should_Throw_On_Multiple_Dots_In_Number()
    {
        ExpressionAssert.Throws<InvalidOperationException>(
            "Model.Val == 1.2.3",
            "Multiple dots in number");
    }

    [Fact]
    public void ExpressionParser_Should_Throw_On_Unclosed_String_Literal()
    {
        ExpressionAssert.Throws<InvalidOperationException>(
            "Model.Name == \"Alice",
            "Unclosed string literal");
    }

    [Fact]
    public void ExpressionParser_Should_Throw_On_Unclosed_Parenthesis()
    {
        ExpressionAssert.Throws<InvalidOperationException>(
            "(Model.Age > 18",
            "Unclosed parenthesis");
    }
}