namespace dotRenderer.Tests;

public class RendererTests
{
    [Fact]
    public void Renderer_Should_Render_Text_And_Eval_From_Model()
    {
        SequenceNode ast = new([
            new TextNode("<h1>Hello, "),
            new EvalNode(["Model", "Name"]),
            new TextNode("!</h1>")
        ]);

        Dictionary<string, object> model = new()
        {
            { "Name", "Alice" }
        };

        string html = Renderer.Render(ast, model);

        Assert.Equal("<h1>Hello, Alice!</h1>", html);
    }

    [Fact]
    public void Renderer_Should_Render_Nested_Path_From_Model()
    {
        Dictionary<string, string> user = new()
        {
            { "Name", "Bob" }
        };
        Dictionary<string, object> model = new()
        {
            { "User", user }
        };

        SequenceNode ast = new([
            new TextNode("Hello, "),
            new EvalNode(["Model", "User", "Name"]),
            new TextNode("!")
        ]);

        string html = Renderer.Render(ast, model);

        Assert.Equal("Hello, Bob!", html);
    }

    [Fact]
    public void Renderer_Should_Throw_When_Nested_Path_Missing()
    {
        Dictionary<string, object> model = [];

        SequenceNode ast = new([
            new TextNode("Hello, "),
            new EvalNode(["Model", "User", "Name"]),
            new TextNode("!")
        ]);

        KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => Renderer.Render(ast, model));
        Assert.Contains("User", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Should_Throw_When_Leaf_Dictionary_String_Missing_Key()
    {
        Dictionary<string, object> model = new Dictionary<string, object>
        {
            { "User", new Dictionary<string, string>() }
        };

        SequenceNode ast = new([
            new EvalNode(["Model", "User", "Name"])
        ]);

        KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => Renderer.Render(ast, model));
        Assert.Contains("Name", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Should_Throw_When_Model_Is_Not_Dictionary()
    {
        Dictionary<string, object> model = new()
        {
            { "User", new object() }
        };

        SequenceNode ast = new([
            new EvalNode(["Model", "User", "Name"])
        ]);
        
        KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => Renderer.Render(ast, model));
        Assert.Contains("Name", ex.Message, StringComparison.Ordinal);
    }
    [Fact]
    public void Renderer_Should_Render_IfNode_With_True_Literal()
    {
        SequenceNode ast = new([
            new TextNode("A"),
            new IfNode(new LiteralExpr<bool>(true),
                new SequenceNode([ new TextNode("Yes") ])
            ),
            new TextNode("B")
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object>());
        Assert.Equal("AYesB", html);
    }

    [Fact]
    public void Renderer_Should_Not_Render_IfNode_With_False_Literal()
    {
        SequenceNode ast = new([
            new TextNode("A"),
            new IfNode(new LiteralExpr<bool>(false),
                new SequenceNode([ new TextNode("No") ])
            ),
            new TextNode("B")
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object>());
        Assert.Equal("AB", html);
    }
}