using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class ParserTextOnlyTests
{
    [Fact]
    public void Should_Parse_Plain_Text_Into_Single_TextNode_In_Template()
    {
        // arrange
        const string input = "Hello, World!"; 
        ImmutableArray<Token> tokens = [Token.FromText(input, TextSpan.At(0, input.Length))];

        // act
        Result<Template> parse = Parser.Parse(tokens);

        // assert
        Assert.True(parse.IsOk);
        Template template = parse.Value;
        Assert.Single(template.Children);

        INode node = template.Children[0];
        Assert.IsType<TextNode>(node);

        TextNode tn = (TextNode)node;
        Assert.Equal(input, tn.Text);
        Assert.Equal(0, tn.Range.Offset);
        Assert.Equal(input.Length, tn.Range.Length);
    }
}