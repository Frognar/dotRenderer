using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

internal static class ParserAssert
{
    public static void Parse(
        ImmutableArray<Token> tokens,
        Template expected)
    {
        Result<Template> result = Parser.Parse(tokens);

        Assert.True(result.IsOk);
        Template template = result.Value;
        Assert.Equal(expected.Children.Length, template.Children.Length);
        foreach ((INode a, INode e) in template.Children.Zip(expected.Children))
        {
            AssertEqual(e, a);
        }
    }

    private static void AssertEqual(INode expected, INode actual)
    {
        switch (expected)
        {
            case TextNode:
            case InterpolateIdentNode:
            case InterpolateExprNode:
                Assert.Equal(expected, actual);
                break;

            case IfNode ifNode:
                IfNode actIf = Assert.IsType<IfNode>(actual);;
                Assert.Equal(ifNode.Condition, actIf.Condition);
                Assert.Equal(ifNode.Range, actIf.Range);
                foreach ((INode a, INode e) in actIf.Then.Zip(ifNode.Then))
                {
                    AssertEqual(e, a);
                }
                
                Assert.Equal(ifNode.Else.Length, actIf.Else.Length);
                foreach ((INode a, INode e)  in actIf.Else.Zip(ifNode.Else))
                {
                    AssertEqual(e, a);
                }

                break;

            case ForNode forExp:
                ForNode forAct = Assert.IsType<ForNode>(actual);
                Assert.Equal(forExp.Item, forAct.Item);
                Assert.Equal(forExp.Index, forAct.Index);
                Assert.Equal(forExp.Seq, forAct.Seq);
                Assert.Equal(forExp.Range, forAct.Range);
                Assert.Equal(forExp.Body.Length, forAct.Body.Length);
                foreach ((INode a, INode e) in forAct.Body.Zip(forExp.Body))
                {
                    AssertEqual(e, a);
                }

                break;
        }
    }
}