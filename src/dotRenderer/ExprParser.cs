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

        Result<IExpr> first = ParseEquality(text, ref i, n);
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

            Result<IExpr> rightRes = ParseEquality(text, ref i, n);
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

    private static Result<IExpr> ParseEquality(string text, ref int i, int n)
    {
        Result<IExpr> leftRes = ParseRelational(text, ref i, n);
        if (!leftRes.IsOk)
        {
            return leftRes;
        }

        IExpr left = leftRes.Value;

        while (true)
        {
            int save = i;
            SkipWs(text, ref i, n);
            if (i + 1 < n && text[i] == '=' && text[i + 1] == '=')
            {
                i += 2;
                SkipWs(text, ref i, n);

                Result<IExpr> rightRes = ParseRelational(text, ref i, n);
                if (!rightRes.IsOk)
                {
                    return rightRes;
                }

                IExpr right = rightRes.Value;
                left = Expr.FromBinaryEq(left, right);
                continue;
            }

            i = save;
            break;
        }

        return Result<IExpr>.Ok(left);
    }
    
    private static Result<IExpr> ParseRelational(string text, ref int i, int n)
    {
        Result<IExpr> leftRes = ParseAdditive(text, ref i, n);
        if (!leftRes.IsOk)
        {
            return leftRes;
        }

        IExpr left = leftRes.Value;

        while (true)
        {
            int save = i;
            SkipWs(text, ref i, n);

            if (i < n)
            {
                if (i + 1 < n && text[i] == '<' && text[i + 1] == '=')
                {
                    i += 2;
                    SkipWs(text, ref i, n);
                    Result<IExpr> rightLe = ParseAdditive(text, ref i, n);
                    if (!rightLe.IsOk)
                    {
                        return rightLe;
                    }

                    left = Expr.FromBinaryLe(left, rightLe.Value);
                    continue;
                }
                if (i + 1 < n && text[i] == '>' && text[i + 1] == '=')
                {
                    i += 2;
                    SkipWs(text, ref i, n);
                    Result<IExpr> rightGe = ParseAdditive(text, ref i, n);
                    if (!rightGe.IsOk)
                    {
                        return rightGe;
                    }

                    left = Expr.FromBinaryGe(left, rightGe.Value);
                    continue;
                }
                if (text[i] == '<')
                {
                    i++;
                    SkipWs(text, ref i, n);
                    Result<IExpr> rightLt = ParseAdditive(text, ref i, n);
                    if (!rightLt.IsOk)
                    {
                        return rightLt;
                    }

                    left = Expr.FromBinaryLt(left, rightLt.Value);
                    continue;
                }
                if (text[i] == '>')
                {
                    i++;
                    SkipWs(text, ref i, n);
                    Result<IExpr> rightGt = ParseAdditive(text, ref i, n);
                    if (!rightGt.IsOk)
                    {
                        return rightGt;
                    }

                    left = Expr.FromBinaryGt(left, rightGt.Value);
                    continue;
                }
            }

            i = save;
            break;
        }

        return Result<IExpr>.Ok(left);
    }

    private static Result<IExpr> ParseAdditive(string text, ref int i, int n)
    {
        Result<IExpr> first = ParsePrimaryWithMemberChain(text, ref i, n);
        if (!first.IsOk)
        {
            return first;
        }

        IExpr left = first.Value;

        while (true)
        {
            int save = i;
            SkipWs(text, ref i, n);
            if (i < n && text[i] == '+')
            {
                i++;
                SkipWs(text, ref i, n);

                Result<IExpr> rightRes = ParsePrimaryWithMemberChain(text, ref i, n);
                if (!rightRes.IsOk)
                {
                    return rightRes;
                }

                IExpr right = rightRes.Value;
                left = Expr.FromBinaryAdd(left, right);
                continue;
            }

            i = save;
            break;
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

    private static Result<IExpr> ParsePrimaryWithMemberChain(string text, ref int i, int n)
    {
        Result<IExpr> atom = ParsePrimary(text, ref i, n);
        if (!atom.IsOk)
        {
            return atom;
        }

        IExpr current = atom.Value;
        while (true)
        {
            int save = i;
            SkipWs(text, ref i, n);
            if (i < n && text[i] == '.')
            {
                i++;
                if (i >= n || !IsIdentStart(text[i]))
                {
                    return Result<IExpr>.Err(new ParseError("MemberName", TextSpan.At(i, 0),
                        "Expected member name after '.'."));
                }

                int start = i;
                i++;
                while (i < n && IsIdentPart(text[i]))
                {
                    i++;
                }

                string name = text[start..i];
                current = Expr.FromMember(current, name);
                continue;
            }

            i = save;
            break;
        }

        return Result<IExpr>.Ok(current);
    }

    private static Result<IExpr> ParsePrimary(string text, ref int i, int n)
    {
        if (i >= n)
        {
            return Result<IExpr>.Err(new ParseError("ExprEmpty", TextSpan.At(i, 0), "Expected expression."));
        }

        char c = text[i];

        if (i + 4 <= n && text.Substring(i, 4) == "true" && (i + 4 == n || !IsIdentPart(text[i + 4])))
        {
            i += 4;
            return Result<IExpr>.Ok(Expr.FromBoolean(true));
        }

        if (i + 5 <= n && text.Substring(i, 5) == "false" && (i + 5 == n || !IsIdentPart(text[i + 5])))
        {
            i += 5;
            return Result<IExpr>.Ok(Expr.FromBoolean(false));
        }

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