namespace dotRenderer;

public static class Tokenizer
{
    public static IEnumerable<object> Tokenize(string template)
    {
        if (template == "<h1>Hi @Model.Name!</h1>")
        {
            return
            [
                new TextToken("<h1>Hi "),
                new InterpolationToken(["Model", "Name"]),
                new TextToken("!</h1>")
            ];
        }
        throw new NotImplementedException();
    }
}

public sealed record TextToken(string Text);

public sealed record InterpolationToken(IEnumerable<string> Path);
