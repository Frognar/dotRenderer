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
        Assert.Equal("Model.IsAdmin", ifNode.Condition);

        SequenceNode body = Assert.IsType<SequenceNode>(ifNode.Body);
        Assert.Equal(2, body.Children.Count);
        Assert.IsType<TextNode>(body.Children[0]);
        Assert.Equal("ADMIN ", ((TextNode)body.Children[0]).Text);
        Assert.IsType<EvalNode>(body.Children[1]);
        Assert.Equal(["Model", "Name"], ((EvalNode)body.Children[1]).Path);

        Assert.IsType<TextNode>(seq.Children[2]);
        Assert.Equal("!", ((TextNode)seq.Children[2]).Text);
    }
}