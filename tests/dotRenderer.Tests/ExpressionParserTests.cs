namespace dotRenderer.Tests;

public class ExpressionParserTests
{
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
}