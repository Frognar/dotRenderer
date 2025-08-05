namespace dotRenderer;

public interface ITemplate
{
    string Render(IReadOnlyDictionary<string, object> model);
}
public interface ITemplate<TModel>
{
    string Render(TModel model);
}