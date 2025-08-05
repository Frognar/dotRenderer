namespace dotRenderer;

public static class Tokenizer
{
    public static IEnumerable<object> Tokenize(string template)
        => throw new NotImplementedException();
}

public sealed record TextToken(string Text);
public sealed record InterpolationToken(IEnumerable<string> Path);