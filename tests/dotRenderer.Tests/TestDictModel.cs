namespace dotRenderer.Tests;

internal sealed record TestDictModel(Dictionary<string, string> Dict)
{
    public static TestDictModel Empty { get; } = new([]);

    public static TestDictModel With((string Key, string Value) pair, params (string Key, string Value)[] pairs)
        => new(pairs.Prepend(pair).ToDictionary());
}

internal sealed class TestDictAccessor : IValueAccessor<TestDictModel>
{
    public static TestDictAccessor Default { get; } = new();
    public string? AccessValue(string path, TestDictModel model) => model.Dict.GetValueOrDefault(path);
}