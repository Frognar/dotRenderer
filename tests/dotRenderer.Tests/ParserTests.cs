namespace dotRenderer.Tests;

public class ParserTests
{
    [Fact]
    public void Parser_Should_Build_Sequence_AST()
    {
        IEnumerable<object> tokens =
        [
            new TextToken("Hello, "),
            new InterpolationToken(["Model", "Name"]),
            new TextToken("!")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        ParserAssert.AstEquals(ast,
            new SequenceNode([
                new TextNode("Hello, "),
                new EvalNode(["Model", "Name"]),
                new TextNode("!")
            ]));
    }

    [Fact]
    public void Parser_Should_Parse_If_Block_With_Text_And_Interpolation()
    {
        IEnumerable<object> tokens =
        [
            new TextToken("Hello "),
            new IfToken(
                "Model.IsAdmin",
                [
                    new TextToken("ADMIN "),
                    new InterpolationToken(["Model", "Name"])
                ]),
            new TextToken("!")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        ParserAssert.AstEquals(ast,
            new SequenceNode([
                new TextNode("Hello "),
                new IfNode(
                    new PropertyExpr(["Model", "IsAdmin"]),
                    new SequenceNode([
                        new TextNode("ADMIN "),
                        new EvalNode(["Model", "Name"])
                    ])),
                new TextNode("!")
            ]));
    }

    [Fact]
    public void Parser_Should_Parse_If_Block_With_Complex_Condition()
    {
        IEnumerable<object> tokens =
        [
            new TextToken("Access: "),
            new IfToken(
                "Model.Age > 18 && Model.IsAdmin",
                [
                    new TextToken("granted"),
                ]),
            new TextToken(".")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        ParserAssert.AstEquals(ast,
            new SequenceNode([
                new TextNode("Access: "),
                new IfNode(
                    new BinaryExpr(
                        "&&",
                        new BinaryExpr(
                            ">",
                            new PropertyExpr(["Model", "Age"]),
                            new LiteralExpr<int>(18)),
                        new PropertyExpr(["Model", "IsAdmin"])),
                    new SequenceNode([new TextNode("granted")])),
                new TextNode("."),
            ]));
    }

    [Fact]
    public void Parser_Should_Parse_If_Block_With_Parenthesized_Complex_Condition()
    {
        IEnumerable<object> tokens =
        [
            new TextToken("Access: "),
            new IfToken(
                "(Model.Age > 18 || Model.IsAdmin) && Model.IsMod",
                [
                    new TextToken("superuser"),
                ]),
            new TextToken(".")
        ];

        SequenceNode ast = Parser.Parse(tokens);


        ParserAssert.AstEquals(ast,
            new SequenceNode([
                new TextNode("Access: "),
                new IfNode(
                    new BinaryExpr(
                        "&&",
                        new BinaryExpr(
                            "||",
                            new BinaryExpr(
                                ">",
                                new PropertyExpr(["Model", "Age"]),
                                new LiteralExpr<int>(18)),
                            new PropertyExpr(["Model", "IsAdmin"])
                        ),
                        new PropertyExpr(["Model", "IsMod"])),
                    new SequenceNode([new TextNode("superuser")])),
                new TextNode(".")
            ]));
    }

    [Fact]
    public void Parser_Should_Handle_Nested_If_Blocks()
    {
        IEnumerable<object> tokens =
        [
            new TextToken("A "),
            new IfToken(
                "Model.x",
                [
                    new TextToken("B "),
                    new IfToken(
                        "Model.y",
                        [new TextToken("C")]),
                    new InterpolationToken(["Model", "Z"]),
                ]),
            new TextToken("D")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        ParserAssert.AstEquals(ast,
            new SequenceNode([
                new TextNode("A "),
                new IfNode(
                    new PropertyExpr(["Model", "x"]),
                    new SequenceNode([
                        new TextNode("B "),
                        new IfNode(
                            new PropertyExpr(["Model", "y"]),
                            new SequenceNode([new TextNode("C")])
                        ),
                        new EvalNode(["Model", "Z"])
                    ])
                ),
                new TextNode("D")
            ])
        );
    }

    [Fact]
    public void Parser_Should_Throw_On_Unknown_Token_Type()
    {
        object[] tokens = [42];

        ParserAssert.Throws<InvalidOperationException>(tokens, "Unknown token of type Int32");
    }
    
    [Fact]
    public void Parser_Should_Parse_OutputExpression_Node()
    {
        IEnumerable<object> tokens =
        [
            new TextToken("Ans: "),
            new OutExprToken("1 + 2 * 3"),
            new TextToken(".")
        ];

        SequenceNode ast = Parser.Parse(tokens);

        ParserAssert.AstEquals(ast,
            new SequenceNode([
                new TextNode("Ans: "),
                new OutExprNode(
                    new BinaryExpr("+",
                        new LiteralExpr<int>(1),
                        new BinaryExpr("*",
                            new LiteralExpr<int>(2),
                            new LiteralExpr<int>(3)))),
                new TextNode(".")
            ]));
    }

}