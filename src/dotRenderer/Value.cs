using System.Collections.Immutable;
using System.Globalization;

namespace DotRenderer;

public enum ValueKind
{
    Text,
    Boolean,
    Number,
    Sequence,
    Map
}

public readonly record struct Value
{
    public ValueKind Kind { get; }
    public string Text { get; }
    public double Number { get; }
    public bool Boolean { get; }
    public ImmutableArray<Value> Sequence { get; }
    public ImmutableDictionary<string, Value> Map { get; }

    private Value(
        ValueKind kind,
        string text,
        double number,
        bool boolean,
        ImmutableArray<Value> sequence,
        ImmutableDictionary<string, Value> map)
    {
        Kind = kind;
        Text = text;
        Number = number;
        Boolean = boolean;
        Sequence = sequence;
        Map = map;
    }

    public static Value FromString(string value) =>
        new(ValueKind.Text, value, 0d, false, [], ImmutableDictionary<string, Value>.Empty);

    public static Value FromBool(bool value) =>
        new(ValueKind.Boolean, "", 0d, value, [], ImmutableDictionary<string, Value>.Empty);

    public static Value FromNumber(double value) =>
        new(ValueKind.Number, "", value, false, [], ImmutableDictionary<string, Value>.Empty);

    public static Value FromSequence(ImmutableArray<Value> items) =>
        new(ValueKind.Sequence, "", 0d, false, items, ImmutableDictionary<string, Value>.Empty);

    public static Value FromSequence(params Value[] items) =>
        new(ValueKind.Sequence, "", 0d, false, [..items], ImmutableDictionary<string, Value>.Empty);

    public static Value FromMap(params IEnumerable<(string key, Value value)> map) =>
        new(ValueKind.Map, "", 0d, false, [], map.ToImmutableDictionary(kv => kv.key, kv => kv.value));
    
    public static Value FromMap(ImmutableDictionary<string, Value> map) =>
        new(ValueKind.Map, "", 0d, false, [], map);

    public static Value FromMap(IReadOnlyDictionary<string, Value> map) =>
        new(ValueKind.Map, "", 0d, false, [], map.ToImmutableDictionary());


    public (bool ok, string value) AsString() =>
        Kind == ValueKind.Text && Text is not null
            ? (true, Text)
            : (false, "");

    public (bool ok, double value) AsNumber() =>
        Kind == ValueKind.Number
            ? (true, Number)
            : (false, 0d);

    public (bool ok, bool value) AsBool() =>
        Kind == ValueKind.Boolean
            ? (true, Boolean)
            : (false, false);

    public (bool ok, ImmutableArray<Value> value) AsSequence() =>
        Kind == ValueKind.Sequence
            ? (true, Sequence)
            : (false, []);

    public (bool ok, ImmutableDictionary<string, Value> value) AsMap() =>
        Kind == ValueKind.Map
            ? (true, Map)
            : (false, ImmutableDictionary<string, Value>.Empty);

    public string ToInvariantString() =>
        Kind switch
        {
            ValueKind.Text => Text ?? string.Empty,
            ValueKind.Number => Number.ToString(CultureInfo.InvariantCulture),
            ValueKind.Boolean => Boolean.ToString(),
            _ => string.Empty
        };
}