using DotRenderer;
using Range = DotRenderer.Range;

namespace dotRenderer.Tests;

public class LexerAtIdentTests
{
    [Fact]
    public void Should_Tokenize_At_Ident_Between_Text_Fragments()
    {
        LexerAssert.Lex("Hello @name!", [
            new Token(TokenKind.Text, "Hello ", new Range(0, 6)),
            new Token(TokenKind.AtIdent, "name", new Range(6, 5)), // includes '@'
            new Token(TokenKind.Text, "!", new Range(11, 1))
        ]);
    }
}