using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

internal static class LexerAssert
{
    public static void Lex(string input, params Token[] expected)
    {
        Result<ImmutableArray<Token>> result = Lexer.Lex(input);
        
        Assert.True(result.IsOk);
        ImmutableArray<Token> tokens = result.Value;
        Assert.Equal(expected.Length, tokens.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], tokens[i]);
        }
    }
}