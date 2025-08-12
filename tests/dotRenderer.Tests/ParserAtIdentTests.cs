using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class ParserAtIdentTests
{
    [Fact]
    public void Should_Parse_AtIdent_Token_Into_InterpolateIdentNode_Between_Text_Nodes()
    {
        // arrange
        const string left = "Hello ";
        const string name = "name";
        const string right = "!";
        ImmutableArray<Token> tokens =
        [
            Token.FromText(left, TextSpan.At(0, left.Length)),
            Token.FromAtIdent(name, TextSpan.At(left.Length, 1 + name.Length)),
            Token.FromText(right, TextSpan.At(left.Length + 1 + name.Length, right.Length))
        ];

        // act
        Result<Template> parse = Parser.Parse(tokens);

        // assert
        Assert.True(parse.IsOk);
        Template template = parse.Value;
        Assert.Equal(3, template.Children.Length);

        Assert.IsType<TextNode>(template.Children[0]);
        TextNode n0 = (TextNode)template.Children[0];
        Assert.Equal(left, n0.Text);

        Assert.IsType<InterpolateIdentNode>(template.Children[1]);
        InterpolateIdentNode n1 = (InterpolateIdentNode)template.Children[1];
        Assert.Equal(name, n1.Name);
        Assert.Equal(Token.FromAtIdent(name, TextSpan.At(left.Length, 1 + name.Length)).Range.Offset, n1.Range.Offset);
        Assert.Equal(Token.FromAtIdent(name, TextSpan.At(left.Length, 1 + name.Length)).Range.Length, n1.Range.Length);

        Assert.IsType<TextNode>(template.Children[2]);
        TextNode n2 = (TextNode)template.Children[2];
        Assert.Equal(right, n2.Text);
    }
}