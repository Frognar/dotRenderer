using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class LexerEscapeAtTests
{
    [Fact]
    public void Should_Tokenize_Double_At_As_Single_Literal_At_In_Text()
    {
        // arrange
        const string input = "Hello @@world";

        // act
        Result<ImmutableArray<Token>> result = Lexer.Lex(input);

        // assert
        Assert.True(result.IsOk);
        ImmutableArray<Token> tokens = result.Value;
        Assert.Equal(3, tokens.Length);

        Token t0 = tokens[0];
        Assert.Equal(TokenKind.Text, t0.Kind);
        Assert.Equal("Hello ", t0.Text);
        Assert.Equal(0, t0.Range.Offset);
        Assert.Equal(6, t0.Range.Length);

        Token t1 = tokens[1];
        Assert.Equal(TokenKind.Text, t1.Kind);
        Assert.Equal("@", t1.Text);
        Assert.Equal(6, t1.Range.Offset);
        Assert.Equal(2, t1.Range.Length); // spans '@@' in source

        Token t2 = tokens[2];
        Assert.Equal(TokenKind.Text, t2.Kind);
        Assert.Equal("world", t2.Text);
        Assert.Equal(8, t2.Range.Offset);
        Assert.Equal(5, t2.Range.Length);
    }
}