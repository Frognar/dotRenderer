namespace DotRenderer;

public sealed class ChainAccessor(IValueAccessor parent, string name, Value value) : IValueAccessor
{
    private readonly IValueAccessor _parent = parent;
    private readonly string _name = name;
    private readonly Value _value = value;

    public (bool ok, Value value) Get(string name)
        => string.Equals(name, _name, StringComparison.Ordinal)
            ? (true, _value)
            : _parent.Get(name);
}