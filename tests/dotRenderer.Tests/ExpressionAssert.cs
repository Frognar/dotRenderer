namespace dotRenderer.Tests;

internal static class ExpressionAssert
{
    public static void AstEquals(ExprNode actual, ExprNode expected)
    {
        Assert.Equal(expected.GetType(), actual.GetType());
        switch (expected)
        {
            case PropertyExpr prop:
                PropertyExpr actProp = Assert.IsType<PropertyExpr>(actual);
                Assert.Equal(prop.Path, actProp.Path);
                break;

            case LiteralExpr<int> litInt:
                LiteralExpr<int> actLitInt = Assert.IsType<LiteralExpr<int>>(actual);
                Assert.Equal(litInt.Value, actLitInt.Value);
                break;

            case LiteralExpr<double> litDouble:
                LiteralExpr<double> actLitDouble = Assert.IsType<LiteralExpr<double>>(actual);
                Assert.Equal(litDouble.Value, actLitDouble.Value);
                break;

            case LiteralExpr<string> litStr:
                LiteralExpr<string> actLitStr = Assert.IsType<LiteralExpr<string>>(actual);
                Assert.Equal(litStr.Value, actLitStr.Value);
                break;

            case LiteralExpr<bool> litBool:
                LiteralExpr<bool> actLitBool = Assert.IsType<LiteralExpr<bool>>(actual);
                Assert.Equal(litBool.Value, actLitBool.Value);
                break;

            case UnaryExpr unary:
                UnaryExpr actUnary = Assert.IsType<UnaryExpr>(actual);
                Assert.Equal(unary.Operator, actUnary.Operator);
                AstEquals(actUnary.Operand, unary.Operand);
                break;

            case BinaryExpr bin:
                BinaryExpr actBin = Assert.IsType<BinaryExpr>(actual);
                Assert.Equal(bin.Operator, actBin.Operator);
                AstEquals(actBin.Left, bin.Left);
                AstEquals(actBin.Right, bin.Right);
                break;

            default:
                throw new InvalidOperationException($"Unsupported ExprNode type: {expected.GetType()}");
        }
    }

    public static void Throws<TException>(string expr, string? messageFragment = null)
        where TException : Exception
    {
        TException ex = Assert.Throws<TException>(() => ExpressionParser.Parse(expr));
        if (messageFragment is not null)
        {
            Assert.Contains(messageFragment, ex.Message, StringComparison.Ordinal);
        }
    }
}