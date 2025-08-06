
namespace dotRenderer;

public interface IValueAccessor<TModel>
{
    string? AccessValue(string path, TModel model);
}