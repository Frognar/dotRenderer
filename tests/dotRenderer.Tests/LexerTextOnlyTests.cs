using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class LexerTextOnlyTests
{
    [Fact]
    public void Should_Tokenize_Plain_Text_As_Single_Text_Token()
    {
        // arrange
        string input = "Hello, world!";

        // act
        Result<ImmutableArray<Token>> result = Lexer.Lex(input);

        // assert
        Assert.True(result.IsOk);
        ImmutableArray<Token> tokens = result.Value;
        Assert.Single(tokens);

        Token t = tokens[0];
        Assert.Equal(TokenKind.Text, t.Kind);
        Assert.Equal(input, t.Text);
        Assert.Equal(0, t.Range.Offset);
        Assert.Equal(input.Length, t.Range.Length);
    }
}