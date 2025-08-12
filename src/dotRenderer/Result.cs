namespace DotRenderer;

public readonly record struct Result<T>
{
    public bool IsOk { get; }
    public T Value { get; }
    public IError? Error { get; }

    private Result(bool isOk, T value, IError? error)
        => (IsOk, Value, Error) = (isOk, value, error);

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Err(IError error) => new(false, default!, error);
}

public interface IError
{
    TextSpan Range { get; }
    string Code { get; }
    string Message { get; }
}

public readonly record struct TextSpan(int Offset, int Length)
{
    public static TextSpan At(int offset, int length) => new(offset, length);
}

public sealed record LexError(string Code, TextSpan Range, string Message) : IError;

public sealed record EvalError(string Code, TextSpan Range, string Message) : IError;