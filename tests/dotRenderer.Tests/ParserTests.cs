namespace dotRenderer.Tests;

public class ParserTests
{
    [Fact]
    public void Parser_Should_Build_Sequence_AST()
    {
        object[] tokens =
        [
            new TextToken("Hello, "),
            new InterpolationToken(["Model", "Name"]),
            new TextToken("!")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        SequenceNode seq = Assert.IsType<SequenceNode>(ast);
        Assert.Equal(3, seq.Children.Count);
        Assert.IsType<TextNode>(seq.Children[0]);
        Assert.Equal("Hello, ", ((TextNode)seq.Children[0]).Text);
        Assert.IsType<EvalNode>(seq.Children[1]);
        Assert.Equal(["Model", "Name"], ((EvalNode)seq.Children[1]).Path);
        Assert.IsType<TextNode>(seq.Children[2]);
        Assert.Equal("!", ((TextNode)seq.Children[2]).Text);
    }

    [Fact]
    public void Parser_Should_Parse_If_Block_With_Text_And_Interpolation()
    {
        object[] tokens =
        [
            new TextToken("Hello "),
            new IfToken("Model.IsAdmin",
                [
                    new TextToken("ADMIN "),
                    new InterpolationToken(["Model", "Name"])
                ]
            ),
            new TextToken("!")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        SequenceNode seq = Assert.IsType<SequenceNode>(ast);
        Assert.Equal(3, seq.Children.Count);
        Assert.IsType<TextNode>(seq.Children[0]);
        Assert.Equal("Hello ", ((TextNode)seq.Children[0]).Text);
        IfNode ifNode = Assert.IsType<IfNode>(seq.Children[1]);
        PropertyExpr condition = Assert.IsType<PropertyExpr>(ifNode.Condition);
        Assert.Equal(["Model", "IsAdmin"], condition.Path);
        SequenceNode body = Assert.IsType<SequenceNode>(ifNode.Body);
        Assert.Equal(2, body.Children.Count);
        Assert.IsType<TextNode>(body.Children[0]);
        Assert.Equal("ADMIN ", ((TextNode)body.Children[0]).Text);
        Assert.IsType<EvalNode>(body.Children[1]);
        Assert.Equal(["Model", "Name"], ((EvalNode)body.Children[1]).Path);
        Assert.IsType<TextNode>(seq.Children[2]);
        Assert.Equal("!", ((TextNode)seq.Children[2]).Text);
    }

    [Fact]
    public void Parser_Should_Parse_If_Block_With_ExprNode_Condition()
    {
        object[] tokens =
        [
            new TextToken("Hi "),
            new IfToken("Model.IsAdmin",
                [
                    new TextToken("ADMIN "),
                    new InterpolationToken(["Model", "Name"])
                ]
            ),
            new TextToken("!")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        SequenceNode seq = Assert.IsType<SequenceNode>(ast);
        Assert.Equal(3, seq.Children.Count);
        Assert.IsType<TextNode>(seq.Children[0]);
        Assert.Equal("Hi ", ((TextNode)seq.Children[0]).Text);
        IfNode ifNode = Assert.IsType<IfNode>(seq.Children[1]);
        PropertyExpr cond = Assert.IsType<PropertyExpr>(ifNode.Condition);
        Assert.Equal(["Model", "IsAdmin"], cond.Path);
        SequenceNode body = Assert.IsType<SequenceNode>(ifNode.Body);
        Assert.Equal(2, body.Children.Count);
        Assert.IsType<TextNode>(body.Children[0]);
        Assert.Equal("ADMIN ", ((TextNode)body.Children[0]).Text);
        Assert.IsType<EvalNode>(body.Children[1]);
        Assert.Equal(["Model", "Name"], ((EvalNode)body.Children[1]).Path);
        Assert.IsType<TextNode>(seq.Children[2]);
        Assert.Equal("!", ((TextNode)seq.Children[2]).Text);
    }[Fact]
    public void Parser_Should_Parse_If_Block_With_Complex_Condition()
    {
        object[] tokens =
        [
            new TextToken("Access: "),
            new IfToken("Model.Age > 18 && Model.IsAdmin",
                [
                    new TextToken("granted")
                ]
            ),
            new TextToken(".")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        SequenceNode seq = Assert.IsType<SequenceNode>(ast);
        Assert.Equal(3, seq.Children.Count);
        Assert.IsType<TextNode>(seq.Children[0]);
        Assert.Equal("Access: ", ((TextNode)seq.Children[0]).Text);
        IfNode ifNode = Assert.IsType<IfNode>(seq.Children[1]);
        BinaryExpr cond = Assert.IsType<BinaryExpr>(ifNode.Condition);
        Assert.Equal("&&", cond.Operator);
        BinaryExpr left = Assert.IsType<BinaryExpr>(cond.Left);
        Assert.Equal(">", left.Operator);
        PropertyExpr leftProp = Assert.IsType<PropertyExpr>(left.Left);
        Assert.Equal(["Model", "Age"], leftProp.Path);
        LiteralExpr<int> leftVal = Assert.IsType<LiteralExpr<int>>(left.Right);
        Assert.Equal(18, leftVal.Value);
        PropertyExpr rightProp = Assert.IsType<PropertyExpr>(cond.Right);
        Assert.Equal(["Model", "IsAdmin"], rightProp.Path);
        SequenceNode body = Assert.IsType<SequenceNode>(ifNode.Body);
        Assert.Single(body.Children);
        Assert.IsType<TextNode>(body.Children[0]);
        Assert.Equal("granted", ((TextNode)body.Children[0]).Text);
        Assert.IsType<TextNode>(seq.Children[2]);
        Assert.Equal(".", ((TextNode)seq.Children[2]).Text);
    }
    [Fact]
    public void Parser_Should_Parse_If_Block_With_Parenthesized_Complex_Condition()
    {
        object[] tokens =
        [
            new TextToken("Access: "),
            new IfToken("(Model.Age > 18 || Model.IsAdmin) && Model.IsMod",
                [
                    new TextToken("superuser")
                ]
            ),
            new TextToken(".")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        SequenceNode seq = Assert.IsType<SequenceNode>(ast);
        Assert.Equal(3, seq.Children.Count);
        Assert.IsType<TextNode>(seq.Children[0]);
        Assert.Equal("Access: ", ((TextNode)seq.Children[0]).Text);
        IfNode ifNode = Assert.IsType<IfNode>(seq.Children[1]);
        BinaryExpr and = Assert.IsType<BinaryExpr>(ifNode.Condition);
        Assert.Equal("&&", and.Operator);
        BinaryExpr or = Assert.IsType<BinaryExpr>(and.Left);
        Assert.Equal("||", or.Operator);
        BinaryExpr gt = Assert.IsType<BinaryExpr>(or.Left);
        Assert.Equal(">", gt.Operator);
        PropertyExpr gtLeft = Assert.IsType<PropertyExpr>(gt.Left);
        Assert.Equal(["Model", "Age"], gtLeft.Path);
        LiteralExpr<int> gtRight = Assert.IsType<LiteralExpr<int>>(gt.Right);
        Assert.Equal(18, gtRight.Value);
        PropertyExpr orRight = Assert.IsType<PropertyExpr>(or.Right);
        Assert.Equal(["Model", "IsAdmin"], orRight.Path);
        PropertyExpr andRight = Assert.IsType<PropertyExpr>(and.Right);
        Assert.Equal(["Model", "IsMod"], andRight.Path);
        SequenceNode body = Assert.IsType<SequenceNode>(ifNode.Body);
        Assert.Single(body.Children);
        Assert.IsType<TextNode>(body.Children[0]);
        Assert.Equal("superuser", ((TextNode)body.Children[0]).Text);
        Assert.IsType<TextNode>(seq.Children[2]);
        Assert.Equal(".", ((TextNode)seq.Children[2]).Text);
    }

    [Fact]
    public void Parser_Should_Throw_On_Unknown_Token_Type()
    {
        object[] tokens = [42];

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => Parser.Parse(tokens));
        Assert.Contains("Unknown token of type", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Int32", ex.Message, StringComparison.Ordinal);
    }
}