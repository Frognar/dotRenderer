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
        for (int i = 0; i < expected.Children.Length; i++)
        {
            Assert.Equal(expected.Children[i], template.Children[i]);
        }
    }
}