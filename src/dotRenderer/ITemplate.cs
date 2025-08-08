namespace dotRenderer;

public interface ITemplate<TModel>
{
    string Render(TModel model);
}