using DotRenderer;
using Range = DotRenderer.Range;

namespace dotRenderer.Tests;

public class LexerEscapeAtTests
{
    [Fact]
    public void Should_Tokenize_Double_At_As_Single_Literal_At_In_Text()
    {
        LexerAssert.Lex("Hello @@world", [
            new Token(TokenKind.Text, "Hello ", new Range(0, 6)),
            new Token(TokenKind.Text, "@", new Range(6, 2)),
            new Token(TokenKind.Text, "world", new Range(8, 5))
        ]);
    }
}