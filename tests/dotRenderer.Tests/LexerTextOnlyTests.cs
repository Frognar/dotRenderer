using System.Collections.Immutable;
using DotRenderer;
using Range = DotRenderer.Range;

namespace dotRenderer.Tests;

public class LexerTextOnlyTests
{
    [Fact]
    public void Should_Tokenize_Plain_Text_As_Single_Text_Token()
    {
        LexerAssert.Lex("Hello, world!", [
            new Token(TokenKind.Text, "Hello, world!", new Range(0, 13)),
        ]);
    }
}