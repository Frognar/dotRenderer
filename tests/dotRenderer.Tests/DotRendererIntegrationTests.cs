namespace dotRenderer.Tests;

public class DotRendererIntegrationTests
{
    [Fact]
    public void Compile_And_Render_Should_Produce_Expected_Html()
    {
        string template = "<h1>Hi @Model.Name!</h1>";
        ITemplate compiled = TemplateCompiler.Compile(template);

        Dictionary<string, object> model = new() { { "Name", "Alice" } };
        string html = compiled.Render(model);

        Assert.Equal("<h1>Hi Alice!</h1>", html);
    }

    private sealed record User(string Name);

    private sealed class UserAccessor : IValueAccessor<User>
    {
        public string? AccessValue(string path, User model)
        {
            return path == "Name" ? model.Name : null;
        }
    }

    [Fact]
    public void Compile_Generic_Should_Render_Model_With_Custom_Accessor()
    {
        string template = "<h1>Hi @Model.Name!</h1>";
        UserAccessor accessor = new();

        ITemplate<User> compiled = TemplateCompiler.Compile(template, accessor);

        User user = new("Alice");
        string html = compiled.Render(user);

        Assert.Equal("<h1>Hi Alice!</h1>", html);
    }
}