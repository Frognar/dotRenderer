namespace dotRenderer;

public interface ICompilationCache
{
    SequenceNode GetOrAdd(string template, Func<string, SequenceNode> factory);
}