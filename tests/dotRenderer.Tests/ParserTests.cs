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
}