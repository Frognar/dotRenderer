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
}