namespace dotRenderer;

public interface ITemplate
{
    string Render(IReadOnlyDictionary<string, object> model);
}