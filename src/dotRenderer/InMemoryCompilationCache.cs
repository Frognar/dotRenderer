namespace dotRenderer;

public sealed class InMemoryCompilationCache : ICompilationCache
{
    private readonly Dictionary<string, SequenceNode> _store = [];

    public SequenceNode GetOrAdd(string template, Func<string, SequenceNode> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        if (_store.TryGetValue(template, out SequenceNode? ast))
        {
            return ast;
        }

        ast = factory(template);
        _store[template] = ast;
        return ast;
    }
}