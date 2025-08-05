using DotMaybe;

namespace dotRenderer;

public interface IValueAccessor<TModel>
{
    Maybe<string> AccessValue(string path, TModel model);
}