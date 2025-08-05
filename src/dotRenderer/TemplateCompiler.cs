namespace dotRenderer;

public static class TemplateCompiler
{
    public static ITemplate Compile(string template)
    {
        IEnumerable<object> tokens = Tokenizer.Tokenize(template);
        SequenceNode ast = Parser.Parse(tokens);
        return new CompiledTemplate(ast);
    }

    private sealed class CompiledTemplate(SequenceNode ast) : ITemplate
    {
        private readonly SequenceNode _ast = ast;

        public string Render(IReadOnlyDictionary<string, object> model)
            => Renderer.Render(_ast, model);
    }
}