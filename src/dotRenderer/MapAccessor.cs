namespace DotRenderer;

public sealed class MapAccessor(IReadOnlyDictionary<string, Value> map) : IValueAccessor
{
    private readonly IReadOnlyDictionary<string, Value> _map = map;

    public (bool ok, Value value) Get(string name) =>
        _map.TryGetValue(name, out Value value)
            ? (true, value)
            : (false, default);
}