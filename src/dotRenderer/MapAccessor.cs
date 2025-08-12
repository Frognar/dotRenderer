namespace DotRenderer;

public sealed class MapAccessor(IReadOnlyDictionary<string, Value> map) : IValueAccessor
{
    private readonly IReadOnlyDictionary<string, Value> _map = map;

    public (bool ok, Value value) Get(string name) =>
        _map.TryGetValue(name, out Value value)
            ? (true, value)
            : (false, default);

    public static MapAccessor Empty { get; } = new(new Dictionary<string, Value>(0));

    public static MapAccessor With((string name, Value value) value, params (string name, Value value)[] values)
        => new(values.Prepend(value).ToDictionary());
}