namespace DotRenderer;

public interface IValueAccessor
{
    (bool ok, Value value) Get(string name);
}