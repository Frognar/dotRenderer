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
}