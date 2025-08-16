namespace DotRenderer;

public static class TemplateEngine
{
    public static Result<string> Render(string template, IValueAccessor? globals = null) =>
        Lexer.Lex(template)
            .Bind(Parser.Parse)
            .Bind(Renderer.RenderWithAccessor(globals));
}