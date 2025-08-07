namespace dotRenderer.Tests;

internal static class ParserAssert
{
    public static void AstEquals(Node actual, Node expected)
    {
        switch (expected)
        {
            case TextNode text:
                TextNode actText = Assert.IsType<TextNode>(actual);
                Assert.Equal(text.Text, actText.Text);
                break;

            case EvalNode eval:
                EvalNode actEval = Assert.IsType<EvalNode>(actual);
                Assert.Equal(eval.Path, actEval.Path);
                break;

            case IfNode ifNode:
                IfNode actIf = Assert.IsType<IfNode>(actual);
                AssertExprEquals(actIf.Condition, ifNode.Condition);
                AstEquals(actIf.Body, ifNode.Body);
                break;

            case SequenceNode seq:
                SequenceNode actSeq = Assert.IsType<SequenceNode>(actual);
                Assert.Equal(seq.Children.Count, actSeq.Children.Count);
                foreach ((Node a, Node e) in actSeq.Children.Zip(seq.Children))
                {
                    AstEquals(a, e);
                }

                break;

            default:
                throw new InvalidOperationException($"Unsupported node type: {expected.GetType()}");
        }
    }

    public static void AstSequence(Node actual, params Node[] expected)
    {
        SequenceNode seq = Assert.IsType<SequenceNode>(actual);
        Assert.Equal(expected.Length, seq.Children.Count);
        foreach ((Node a, Node e) in seq.Children.Zip(expected))
        {
            AstEquals(a, e);
        }
    }

    public static void AssertExprEquals(ExprNode? actual, ExprNode? expected)
    {
        if (actual == null || expected == null)
        {
            Assert.Equal(expected, actual);
            return;
        }

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
                AssertExprEquals(actUnary.Operand, unary.Operand);
                break;

            case BinaryExpr bin:
                BinaryExpr actBin = Assert.IsType<BinaryExpr>(actual);
                Assert.Equal(bin.Operator, actBin.Operator);
                AssertExprEquals(actBin.Left, bin.Left);
                AssertExprEquals(actBin.Right, bin.Right);
                break;

            default:
                throw new InvalidOperationException($"Unsupported ExprNode type: {expected.GetType()}");
        }
    }

    public static void Throws<TException>(object[] tokens, string? expectedMessageFragment = null)
        where TException : Exception
    {
        TException ex = Assert.Throws<TException>(() => Parser.Parse(tokens));
        if (expectedMessageFragment != null)
        {
            Assert.Contains(expectedMessageFragment, ex.Message, StringComparison.Ordinal);
        }
    }
}