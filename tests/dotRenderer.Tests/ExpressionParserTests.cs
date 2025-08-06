namespace dotRenderer.Tests;

public class ExpressionParserTests
{
    [Fact]
    public void ExpressionParser_Should_Throw_On_Unknown_Token()
    {
        string expr = "???";

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => ExpressionParser.Parse(expr));
        Assert.Contains("Unknown token near:", ex.Message, StringComparison.Ordinal);
        Assert.Contains("???", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Bool_Property()
    {
        string expr = "Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        PropertyExpr prop = Assert.IsType<PropertyExpr>(node);
        Assert.Equal(["Model", "IsAdmin"], prop.Path);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Nested_Property()
    {
        string expr = "Model.User.Name";

        ExprNode node = ExpressionParser.Parse(expr);

        PropertyExpr prop = Assert.IsType<PropertyExpr>(node);
        Assert.Equal(["Model", "User", "Name"], prop.Path);
    }

    [Theory]
    [InlineData("Model.")]
    [InlineData("Model.User.")]
    public void ExpressionParser_Should_Throw_On_Missing_Property_Segment(string expr)
    {
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => ExpressionParser.Parse(expr));
        Assert.Contains("Expected property segment after '.'", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Bool_Literal_True()
    {
        string expr = "true";

        ExprNode node = ExpressionParser.Parse(expr);

        LiteralExpr<bool> lit = Assert.IsType<LiteralExpr<bool>>(node);
        Assert.True(lit.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Bool_Literal_False()
    {
        string expr = "false";

        ExprNode node = ExpressionParser.Parse(expr);

        LiteralExpr<bool> lit = Assert.IsType<LiteralExpr<bool>>(node);
        Assert.False(lit.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Unary_Not_On_Property()
    {
        string expr = "!Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        UnaryExpr not = Assert.IsType<UnaryExpr>(node);
        Assert.Equal("!", not.Operator);
        PropertyExpr arg = Assert.IsType<PropertyExpr>(not.Operand);
        Assert.Equal(["Model", "IsAdmin"], arg.Path);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Equality_Expression()
    {
        string expr = "Model.IsAdmin == true";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("==", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "IsAdmin"], left.Path);
        LiteralExpr<bool> right = Assert.IsType<LiteralExpr<bool>>(bin.Right);
        Assert.True(right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_GreaterEqual_Expression()
    {
        string expr = "Model.Age >= 18";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal(">=", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Age"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(bin.Right);
        Assert.Equal(18, right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_LessThanOrEqual()
    {
        string expr = "Model.Age <= 18";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("<=", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Age"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(bin.Right);
        Assert.Equal(18, right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_And_Expression()
    {
        string expr = "Model.Age >= 18 && Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("&&", bin.Operator);
        BinaryExpr left = Assert.IsType<BinaryExpr>(bin.Left);
        Assert.Equal(">=", left.Operator);
        PropertyExpr leftLeft = Assert.IsType<PropertyExpr>(left.Left);
        Assert.Equal(["Model", "Age"], leftLeft.Path);
        LiteralExpr<int> leftRight = Assert.IsType<LiteralExpr<int>>(left.Right);
        Assert.Equal(18, leftRight.Value);
        PropertyExpr right = Assert.IsType<PropertyExpr>(bin.Right);
        Assert.Equal(["Model", "IsAdmin"], right.Path);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Or_Expression()
    {
        string expr = "Model.Age >= 18 || Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("||", bin.Operator);
        BinaryExpr left = Assert.IsType<BinaryExpr>(bin.Left);
        Assert.Equal(">=", left.Operator);
        PropertyExpr leftLeft = Assert.IsType<PropertyExpr>(left.Left);
        Assert.Equal(["Model", "Age"], leftLeft.Path);
        LiteralExpr<int> leftRight = Assert.IsType<LiteralExpr<int>>(left.Right);
        Assert.Equal(18, leftRight.Value);
        PropertyExpr right = Assert.IsType<PropertyExpr>(bin.Right);
        Assert.Equal(["Model", "IsAdmin"], right.Path);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_And_With_Unary_Not()
    {
        string expr = "Model.Age >= 18 && !Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("&&", bin.Operator);
        BinaryExpr left = Assert.IsType<BinaryExpr>(bin.Left);
        Assert.Equal(">=", left.Operator);
        PropertyExpr leftLeft = Assert.IsType<PropertyExpr>(left.Left);
        Assert.Equal(["Model", "Age"], leftLeft.Path);
        LiteralExpr<int> leftRight = Assert.IsType<LiteralExpr<int>>(left.Right);
        Assert.Equal(18, leftRight.Value);
        UnaryExpr right = Assert.IsType<UnaryExpr>(bin.Right);
        Assert.Equal("!", right.Operator);
        PropertyExpr arg = Assert.IsType<PropertyExpr>(right.Operand);
        Assert.Equal(["Model", "IsAdmin"], arg.Path);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Equality_With_Double_Literal()
    {
        string expr = "Model.Ratio == 1.5";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("==", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Ratio"], left.Path);
        LiteralExpr<double> right = Assert.IsType<LiteralExpr<double>>(bin.Right);
        Assert.Equal(1.5, right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Throw_On_Multiple_Dots_In_Number()
    {
        string expr = "Model.Val == 1.2.3";

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => ExpressionParser.Parse(expr));
        Assert.Contains("Multiple dots in number", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Equality_With_String_Literal()
    {
        string expr = "Model.Name == \"Alice\"";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("==", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Name"], left.Path);
        LiteralExpr<string> right = Assert.IsType<LiteralExpr<string>>(bin.Right);
        Assert.Equal("Alice", right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Throw_On_Unclosed_String_Literal()
    {
        string expr = "Model.Name == \"Alice";

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => ExpressionParser.Parse(expr));
        Assert.Contains("Unclosed string literal", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Inequality()
    {
        string expr = "Model.Name != \"Alice\"";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("!=", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Name"], left.Path);
        LiteralExpr<string> right = Assert.IsType<LiteralExpr<string>>(bin.Right);
        Assert.Equal("Alice", right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_LessThan()
    {
        string expr = "Model.Count < 10";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("<", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Count"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(bin.Right);
        Assert.Equal(10, right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_GreaterThan()
    {
        string expr = "Model.Count > 1";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal(">", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Count"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(bin.Right);
        Assert.Equal(1, right.Value);
    }


    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Addition()
    {
        string expr = "Model.Count + 1";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("+", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Count"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(bin.Right);
        Assert.Equal(1, right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Subtraction()
    {
        string expr = "Model.Count - 1";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("-", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Count"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(bin.Right);
        Assert.Equal(1, right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Parenthesized_Expression()
    {
        string expr = "(Model.Age > 18) && Model.IsAdmin";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr and = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("&&", and.Operator);
        BinaryExpr paren = Assert.IsType<BinaryExpr>(and.Left);
        Assert.Equal(">", paren.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(paren.Left);
        Assert.Equal(["Model", "Age"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(paren.Right);
        Assert.Equal(18, right.Value);
        PropertyExpr rightAnd = Assert.IsType<PropertyExpr>(and.Right);
        Assert.Equal(["Model", "IsAdmin"], rightAnd.Path);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Nested_Parentheses()
    {
        string expr = "Model.Age > 18 && (Model.IsAdmin || Model.IsMod)";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr and = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("&&", and.Operator);
        BinaryExpr gt = Assert.IsType<BinaryExpr>(and.Left);
        Assert.Equal(">", gt.Operator);
        PropertyExpr age = Assert.IsType<PropertyExpr>(gt.Left);
        Assert.Equal(["Model", "Age"], age.Path);
        LiteralExpr<int> gtRight = Assert.IsType<LiteralExpr<int>>(gt.Right);
        Assert.Equal(18, gtRight.Value);
        BinaryExpr or = Assert.IsType<BinaryExpr>(and.Right);
        Assert.Equal("||", or.Operator);
        PropertyExpr leftOr = Assert.IsType<PropertyExpr>(or.Left);
        Assert.Equal(["Model", "IsAdmin"], leftOr.Path);
        PropertyExpr rightOr = Assert.IsType<PropertyExpr>(or.Right);
        Assert.Equal(["Model", "IsMod"], rightOr.Path);
    }

    [Fact]
    public void ExpressionParser_Should_Throw_On_Unclosed_Parenthesis()
    {
        string expr = "(Model.Age > 18";

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => ExpressionParser.Parse(expr));
        Assert.Contains("Unclosed parenthesis", ex.Message, StringComparison.Ordinal);
    }
    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Multiplication()
    {
        string expr = "Model.Count * 2";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("*", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Count"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(bin.Right);
        Assert.Equal(2, right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Division()
    {
        string expr = "Model.Count / 2";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("/", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Count"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(bin.Right);
        Assert.Equal(2, right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Binary_Modulo()
    {
        string expr = "Model.Count % 2";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr bin = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("%", bin.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(bin.Left);
        Assert.Equal(["Model", "Count"], left.Path);
        LiteralExpr<int> right = Assert.IsType<LiteralExpr<int>>(bin.Right);
        Assert.Equal(2, right.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Respect_Operator_Precedence_Multiplication_Addition()
    {
        string expr = "Model.Count + 2 * 3";

        ExprNode node = ExpressionParser.Parse(expr);

        BinaryExpr add = Assert.IsType<BinaryExpr>(node);
        Assert.Equal("+", add.Operator);
        PropertyExpr left = Assert.IsType<PropertyExpr>(add.Left);
        Assert.Equal(["Model", "Count"], left.Path);
        BinaryExpr mul = Assert.IsType<BinaryExpr>(add.Right);
        Assert.Equal("*", mul.Operator);
        LiteralExpr<int> mulLeft = Assert.IsType<LiteralExpr<int>>(mul.Left);
        Assert.Equal(2, mulLeft.Value);
        LiteralExpr<int> mulRight = Assert.IsType<LiteralExpr<int>>(mul.Right);
        Assert.Equal(3, mulRight.Value);
    }
    [Fact]
    public void ExpressionParser_Should_Parse_Unary_Minus_Int()
    {
        string expr = "-5";

        ExprNode node = ExpressionParser.Parse(expr);

        UnaryExpr unary = Assert.IsType<UnaryExpr>(node);
        Assert.Equal("-", unary.Operator);
        LiteralExpr<int> val = Assert.IsType<LiteralExpr<int>>(unary.Operand);
        Assert.Equal(5, val.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Unary_Minus_Double()
    {
        string expr = "-1.5";

        ExprNode node = ExpressionParser.Parse(expr);

        UnaryExpr unary = Assert.IsType<UnaryExpr>(node);
        Assert.Equal("-", unary.Operator);
        LiteralExpr<double> val = Assert.IsType<LiteralExpr<double>>(unary.Operand);
        Assert.Equal(1.5, val.Value);
    }

    [Fact]
    public void ExpressionParser_Should_Parse_Unary_Minus_On_Property()
    {
        string expr = "-Model.Val";

        ExprNode node = ExpressionParser.Parse(expr);

        UnaryExpr unary = Assert.IsType<UnaryExpr>(node);
        Assert.Equal("-", unary.Operator);
        PropertyExpr prop = Assert.IsType<PropertyExpr>(unary.Operand);
        Assert.Equal(["Model", "Val"], prop.Path);
    }
}