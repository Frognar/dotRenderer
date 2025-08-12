using System.Diagnostics.Contracts;
using System.Globalization;

namespace DotRenderer;

public static class ExprParser
{
    [Pure]
    public static Result<IExpr> Parse(string text)
    {
        text ??= "";
        int i = 0;
        int n = text.Length;

        SkipWs(text, ref i, n);

        Result<IExpr> first = ParsePrimary(text, ref i, n);
        if (!first.IsOk)
        {
            return first;
        }

        IExpr left = first.Value;

        while (true)
        {
            SkipWs(text, ref i, n);
            if (i >= n || text[i] != '+')
            {
                break;
            }

            i++;
            SkipWs(text, ref i, n);

            Result<IExpr> rightRes = ParsePrimary(text, ref i, n);
            if (!rightRes.IsOk)
            {
                return rightRes;
            }

            IExpr right = rightRes.Value;
            left = Expr.FromBinaryAdd(left, right);
        }

        SkipWs(text, ref i, n);
        if (i != n)
        {
            return Result<IExpr>.Err(new ParseError("ExprTrailing", TextSpan.At(i, n - i),
                "Unexpected trailing input in expression."));
        }

        return Result<IExpr>.Ok(left);
    }

    private static void SkipWs(string text, ref int i, int n)
    {
        while (i < n && char.IsWhiteSpace(text[i]))
        {
            i++;
        }
    }

    private static Result<IExpr> ParsePrimary(string text, ref int i, int n)
    {
        if (i >= n)
        {
            return Result<IExpr>.Err(new ParseError("ExprEmpty", TextSpan.At(i, 0), "Expected expression."));
        }

        char c = text[i];

        if (char.IsDigit(c))
        {
            int start = i;
            i++;
            while (i < n && char.IsDigit(text[i]))
            {
                i++;
            }

            if (i < n && text[i] == '.')
            {
                i++;
                while (i < n && char.IsDigit(text[i]))
                {
                    i++;
                }
            }

            string slice = text[start..i];
            if (!double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return Result<IExpr>.Err(new ParseError("NumberFormat", TextSpan.At(start, i - start),
                    "Invalid number literal."));
            }

            return Result<IExpr>.Ok(Expr.FromNumber(value));
        }

        if (IsIdentStart(c))
        {
            int start = i;
            i++;
            while (i < n && IsIdentPart(text[i]))
            {
                i++;
            }

            string name = text[start..i];
            return Result<IExpr>.Ok(Expr.FromIdent(name));
        }

        return Result<IExpr>.Err(new ParseError("UnexpectedChar", TextSpan.At(i, 1), $"Unexpected character '{c}'."));
    }

    private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_';
    private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_';
}