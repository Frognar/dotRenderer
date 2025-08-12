using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class ParserTextOnlyTests
{
    [Fact]
    public void Should_Parse_Plain_Text_Into_Single_TextNode_In_Template()
    {
        ParserAssert.Parse(
            [
                Token.FromText("Hello, World!", TextSpan.At(0, 13))
            ],
            new Template([
                Node.FromText("Hello, World!", TextSpan.At(0, 13))
            ]));
    }
}