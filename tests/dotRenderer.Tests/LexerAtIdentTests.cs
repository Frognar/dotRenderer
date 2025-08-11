using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class LexerAtIdentTests
{
    [Fact]
    public void Should_Tokenize_At_Ident_Between_Text_Fragments()
    {
        // arrange
        const string input = "Hello @name!";

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
        Assert.Equal(TokenKind.AtIdent, t1.Kind);
        Assert.Equal("name", t1.Text);
        Assert.Equal(6, t1.Range.Offset);   // includes '@'
        Assert.Equal(5, t1.Range.Length);   // "@name"

        Token t2 = tokens[2];
        Assert.Equal(TokenKind.Text, t2.Kind);
        Assert.Equal("!", t2.Text);
        Assert.Equal(11, t2.Range.Offset);
        Assert.Equal(1, t2.Range.Length);
    }
}