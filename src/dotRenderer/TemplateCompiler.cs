using System.Security.AccessControl;

namespace dotRenderer;

public static class TemplateCompiler
{
    public static ITemplate Compile(string template)
    {
        IEnumerable<object> tokens = Tokenizer.Tokenize(template);
        SequenceNode ast = Parser.Parse(tokens);
        return new CompiledTemplate(ast);
    }

    public static ITemplate<TModel> Compile<TModel>(string template, IValueAccessor<TModel> valueAccessor)
    {
        IEnumerable<object> tokens = Tokenizer.Tokenize(template);
        SequenceNode ast = Parser.Parse(tokens);
        return new CompiledTemplate<TModel>(ast, valueAccessor);
    }

    private sealed class CompiledTemplate(SequenceNode ast) : ITemplate
    {
        private readonly SequenceNode _ast = ast;

        public string Render(IReadOnlyDictionary<string, object> model)
            => Renderer.Render(_ast, model);
    }
    
    private sealed class CompiledTemplate<TModel>(
        SequenceNode ast,
        IValueAccessor<TModel> accessor) : ITemplate<TModel>
    {
        private readonly SequenceNode _ast = ast;
        private readonly IValueAccessor<TModel> _accessor = accessor;

        public string Render(TModel model)
            => Renderer.Render(_ast, model, _accessor);
    }
}