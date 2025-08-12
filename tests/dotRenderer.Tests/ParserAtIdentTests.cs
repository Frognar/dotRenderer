using System.Collections.Immutable;
using DotRenderer;

namespace dotRenderer.Tests;

public class ParserAtIdentTests
{
    [Fact]
    public void Should_Parse_AtIdent_Token_Into_InterpolateIdentNode_Between_Text_Nodes()
    {
        ParserAssert.Parse(
            [
                Token.FromText("Hello ", TextSpan.At(0, 6)),
                Token.FromAtIdent("name", TextSpan.At(6, 5)),
                Token.FromText("1", TextSpan.At(11, 1))
            ],
            new Template([
                Node.FromText("Hello ", TextSpan.At(0, 6)),
                Node.FromInterpolateIdent("name", TextSpan.At(6, 5)),
                Node.FromText("1", TextSpan.At(11, 1))
            ]));
    }
}