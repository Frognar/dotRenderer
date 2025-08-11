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
    public string? Text { get; }
    public double Number { get; }
    public bool Boolean { get; }

    private Value(ValueKind kind, string? text, double number, bool boolean)
    {
        Kind = kind;
        Text = text;
        Number = number;
        Boolean = boolean;
    }

    public static Value FromString(string value) => new(ValueKind.Text, value, 0d, false);
    public static Value FromBool(bool value) => new(ValueKind.Boolean, null, 0d, value);
    public static Value FromNumber(double value) => new(ValueKind.Number, null, value, false);

    public (bool ok, string value) AsString() =>
        Kind == ValueKind.Text && Text is not null
            ? (true, Text)
            : (false, string.Empty);

    public (bool ok, double value) AsNumber() =>
        Kind == ValueKind.Number
            ? (true, Number)
            : (false, 0d);

    public (bool ok, bool value) AsBool() =>
        Kind == ValueKind.Boolean
            ? (true, Boolean)
            : (false, false);

    public string ToInvariantString() =>
        Kind switch
        {
            ValueKind.Text => Text ?? string.Empty,
            ValueKind.Number => Number.ToString(CultureInfo.InvariantCulture),
            ValueKind.Boolean => Boolean.ToString(),
            _ => string.Empty
        };
}