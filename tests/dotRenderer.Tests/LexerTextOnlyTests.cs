using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class LexerTextOnlyTests
{
    [Fact]
    public void Should_Tokenize_Plain_Text_As_Single_Text_Token()
    {
        LexerAssert.Lex("Hello, world!", [
            Token.FromText("Hello, world!", new TextSpan(0, 13)),
        ]);
    }
}