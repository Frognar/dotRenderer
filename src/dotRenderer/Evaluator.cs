using System.Diagnostics.Contracts;

namespace DotRenderer;

public static class Evaluator
{
    [Pure]
    public static Result<Value> EvaluateIdent(IValueAccessor accessor, string name, Range range)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        (bool ok, Value value) = accessor.Get(name);
        return !ok
            ? Result<Value>.Err(new EvalError("MissingIdent", range, $"Identifier '{name}' was not found."))
            : Result<Value>.Ok(value);
    }
}