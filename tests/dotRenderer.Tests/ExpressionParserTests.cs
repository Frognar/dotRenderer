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
}