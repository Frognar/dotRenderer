namespace dotRenderer.Tests;

public class ParserTests
{
    private static IEnumerable<object> Tokenize(string template) => Tokenizer.Tokenize(template);

    [Fact]
    public void Parser_Should_Build_Sequence_AST()
    {
        IEnumerable<object> tokens = Tokenize("Hello, @Model.Name!");

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
        IEnumerable<object> tokens = Tokenize("Hello @if (Model.IsAdmin) {ADMIN @Model.Name}!");

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
        IEnumerable<object> tokens = Tokenize("Access: @if (Model.Age > 18 && Model.IsAdmin) {granted}.");

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
        IEnumerable<object> tokens = Tokenize(
            "Access: @if ((Model.Age > 18 || Model.IsAdmin) && Model.IsMod) {superuser}.");

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
    public void Parser_Should_Throw_On_Unknown_Token_Type()
    {
        object[] tokens = [42];

        ParserAssert.Throws<InvalidOperationException>(tokens, "Unknown token of type Int32");
    }
}