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

public static class Result
{
    public static Result<TResult> Map<T, TResult>(this Result<T> source, Func<T, TResult> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return !source.IsOk
            ? Result<TResult>.Err(source.Error!)
            : Result<TResult>.Ok(map(source.Value));
    }

    public static Result<TResult> Bind<T, TResult>(this Result<T> source, Func<T, Result<TResult>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);
        return !source.IsOk
            ? Result<TResult>.Err(source.Error!)
            : bind(source.Value);
    }

    public static Result<TResult> Bind2<T, T2, TResult>(
        this Result<T> source,
        Func<Result<T2>> otherFactory,
        Func<T, T2, Result<TResult>> bind)
    {
        ArgumentNullException.ThrowIfNull(otherFactory);
        ArgumentNullException.ThrowIfNull(bind);
        if (!source.IsOk)
        {
            return Result<TResult>.Err(source.Error!);
        }

        Result<T2> other = otherFactory();
        return !other.IsOk
            ? Result<TResult>.Err(other.Error!)
            : bind(source.Value, other.Value);
    }
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

public sealed record ParseError(string Code, TextSpan Range, string Message) : IError;