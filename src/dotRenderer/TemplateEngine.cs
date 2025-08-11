using System.Collections.Immutable;

namespace DotRenderer;

public static class TemplateEngine
{
    public static Result<string> Render(string template, IValueAccessor? globals = null)
    {
        Result<ImmutableArray<Token>> lex = Lexer.Lex(template);
        if (!lex.IsOk)
        {
            return Result<string>.Err(lex.Error!);
        }

        Result<Template> parse = Parser.Parse(lex.Value);
        if (!parse.IsOk)
        {
            return Result<string>.Err(parse.Error!);
        }

        Result<string> render = Renderer.Render(parse.Value);
        return render;
    }
}