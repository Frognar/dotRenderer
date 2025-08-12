using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class RendererAtIdentTests
{
    [Fact]
    public void should_render_interpolated_identifier_using_accessor()
    {
        // arrange
        const string left = "Hello ";
        const string ident = "name";
        const string right = "!";
        ImmutableArray<INode> children =
        [
            new TextNode(left, TextSpan.At(0, left.Length)),
            new InterpolateIdentNode(ident, TextSpan.At(left.Length, 1 + ident.Length)),
            new TextNode(right, TextSpan.At(left.Length + 1 + ident.Length, right.Length))
        ];

        Template template = new(children);
        MapAccessor accessor = new(new Dictionary<string, Value>
        {
            ["name"] = Value.FromString("Alice")
        });

        // act
        Result<string> result = Renderer.Render(template, accessor);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("Hello Alice!", result.Value);
    }
}