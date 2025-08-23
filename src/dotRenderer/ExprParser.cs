using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;

namespace DotRenderer;

public static class ExprParser
{
    [Pure]
    public static Result<IExpr> Parse(string text)
    {
        State s = State.Of(text).SkipWs();
        Result<(IExpr expr, State rest)> parsed = ParseOr(s);
        if (!parsed.IsOk)
        {
            return Result<IExpr>.Err(parsed.Error!);
        }

        (IExpr expr, State rest) = parsed.Value;
        rest = rest.SkipWs();
        if (!rest.Eof)
        {
            return Result<IExpr>.Err(new ParseError(
                "ExprTrailing",
                TextSpan.At(rest.Pos, rest.Remaining),
                "Unexpected trailing input in expression."));
        }

        return Result<IExpr>.Ok(expr);
    }

    private readonly record struct State(string Text, int Pos)
    {
        public string Text { get; } = Text;
        public int Length => Text.Length;
        public int Remaining => Length - Pos;
        public bool Eof => Pos >= Length;

        public State Advance(int delta = 1) => this with { Pos = Pos + delta };

        public State SkipWs()
        {
            int i = Pos;
            while (i < Length && char.IsWhiteSpace(Text[i]))
            {
                i++;
            }

            return this with { Pos = i };
        }

        public bool StartsWith(ReadOnlySpan<char> s) =>
            Remaining >= s.Length && Text.AsSpan(Pos, s.Length).SequenceEqual(s);

        public static State Of(string text) => new(text, 0);
    }

    private static Result<(T value, State rest)> Ok<T>(T v, State rest)
        => Result<(T, State)>.Ok((v, rest));

    private static Result<(T value, State rest)> Err<T>(string code, TextSpan span, string message)
        => Result<(T, State)>.Err(new ParseError(code, span, message));

    private static Result<(IExpr expr, State rest)> ParseOr(State s)
    {
        Result<(IExpr expr, State rest)> leftRes = ParseAnd(s);
        if (!leftRes.IsOk)
        {
            return leftRes;
        }

        (IExpr left, State rest) = leftRes.Value;

        while (true)
        {
            State save = rest;
            rest = rest.SkipWs();
            if (rest.Remaining >= 2 && rest.Text[rest.Pos] == '|' && rest.Text[rest.Pos + 1] == '|')
            {
                rest = rest.Advance(2).SkipWs();
                Result<(IExpr expr, State rest)> rightRes = ParseAnd(rest);
                if (!rightRes.IsOk)
                {
                    return rightRes;
                }

                (IExpr right, State after) = rightRes.Value;
                left = Expr.FromBinaryOr(left, right);
                rest = after;
                continue;
            }

            rest = save;
            break;
        }

        return Ok(left, rest);
    }

    private static Result<(IExpr expr, State rest)> ParseAnd(State s)
    {
        Result<(IExpr expr, State rest)> leftRes = ParseEquality(s);
        if (!leftRes.IsOk)
        {
            return leftRes;
        }

        (IExpr left, State rest) = leftRes.Value;

        while (true)
        {
            State save = rest;
            rest = rest.SkipWs();
            if (rest.Remaining >= 2 && rest.Text[rest.Pos] == '&' && rest.Text[rest.Pos + 1] == '&')
            {
                rest = rest.Advance(2).SkipWs();
                Result<(IExpr expr, State rest)> rightRes = ParseEquality(rest);
                if (!rightRes.IsOk)
                {
                    return rightRes;
                }

                (IExpr right, State after) = rightRes.Value;
                left = Expr.FromBinaryAnd(left, right);
                rest = after;
                continue;
            }

            rest = save;
            break;
        }

        return Ok(left, rest);
    }

    private static Result<(IExpr expr, State rest)> ParseEquality(State s)
    {
        Result<(IExpr expr, State rest)> leftRes = ParseRelational(s);
        if (!leftRes.IsOk)
        {
            return leftRes;
        }

        (IExpr left, State rest) = leftRes.Value;

        while (true)
        {
            State save = rest;
            rest = rest.SkipWs();
            if (rest.Remaining >= 2 && rest.Text[rest.Pos] == '=' && rest.Text[rest.Pos + 1] == '=')
            {
                rest = rest.Advance(2).SkipWs();
                Result<(IExpr expr, State rest)> rightRes = ParseRelational(rest);
                if (!rightRes.IsOk)
                {
                    return rightRes;
                }

                (IExpr right, State after) = rightRes.Value;
                left = Expr.FromBinaryEq(left, right);
                rest = after;
                continue;
            }

            rest = save;
            break;
        }

        return Ok(left, rest);
    }

    private static Result<(IExpr expr, State rest)> ParseRelational(State s)
    {
        Result<(IExpr expr, State rest)> leftRes = ParseAdditive(s);
        if (!leftRes.IsOk)
        {
            return leftRes;
        }

        (IExpr left, State rest) = leftRes.Value;
        while (true)
        {
            State save = rest;
            rest = rest.SkipWs();
            if (!rest.Eof)
            {
                if (rest.StartsWith("<="))
                {
                    rest = rest.Advance(2).SkipWs();
                    Result<(IExpr expr, State rest)> r = ParseAdditive(rest);
                    if (!r.IsOk)
                    {
                        return r;
                    }

                    (IExpr right, State after) = r.Value;
                    left = Expr.FromBinaryLe(left, right);
                    rest = after;
                    continue;
                }

                if (rest.StartsWith(">="))
                {
                    rest = rest.Advance(2).SkipWs();
                    Result<(IExpr expr, State rest)> r = ParseAdditive(rest);
                    if (!r.IsOk)
                    {
                        return r;
                    }

                    (IExpr right, State after) = r.Value;
                    left = Expr.FromBinaryGe(left, right);
                    rest = after;
                    continue;
                }

                if (rest.Text[rest.Pos] == '<')
                {
                    rest = rest.Advance().SkipWs();
                    Result<(IExpr expr, State rest)> r = ParseAdditive(rest);
                    if (!r.IsOk)
                    {
                        return r;
                    }

                    (IExpr right, State after) = r.Value;
                    left = Expr.FromBinaryLt(left, right);
                    rest = after;
                    continue;
                }

                if (rest.Text[rest.Pos] == '>')
                {
                    rest = rest.Advance().SkipWs();
                    Result<(IExpr expr, State rest)> r = ParseAdditive(rest);
                    if (!r.IsOk)
                    {
                        return r;
                    }

                    (IExpr right, State after) = r.Value;
                    left = Expr.FromBinaryGt(left, right);
                    rest = after;
                    continue;
                }
            }

            rest = save;
            break;
        }

        return Ok(left, rest);
    }

    private static Result<(IExpr expr, State rest)> ParseAdditive(State s)
    {
        Result<(IExpr expr, State rest)> first = ParseMultiplicative(s);
        if (!first.IsOk)
        {
            return first;
        }

        (IExpr left, State rest) = first.Value;

        while (true)
        {
            State save = rest;
            rest = rest.SkipWs();
            if (!rest.Eof && (rest.Text[rest.Pos] == '+' || rest.Text[rest.Pos] == '-'))
            {
                char op = rest.Text[rest.Pos];
                rest = rest.Advance().SkipWs();
                Result<(IExpr expr, State rest)> r = ParseMultiplicative(rest);
                if (!r.IsOk)
                {
                    return r;
                }

                (IExpr right, State after) = r.Value;
                left = op == '+' ? Expr.FromBinaryAdd(left, right) : Expr.FromBinarySub(left, right);
                rest = after;
                continue;
            }

            rest = save;
            break;
        }

        return Ok(left, rest);
    }

    private static Result<(IExpr expr, State rest)> ParseMultiplicative(State s)
    {
        Result<(IExpr expr, State rest)> first = ParseUnary(s);
        if (!first.IsOk)
        {
            return first;
        }

        (IExpr left, State rest) = first.Value;

        while (true)
        {
            State save = rest;
            rest = rest.SkipWs();
            if (!rest.Eof && (rest.Text[rest.Pos] == '*' || rest.Text[rest.Pos] == '/' || rest.Text[rest.Pos] == '%'))
            {
                char op = rest.Text[rest.Pos];
                rest = rest.Advance().SkipWs();
                Result<(IExpr expr, State rest)> r = ParseUnary(rest);
                if (!r.IsOk)
                {
                    return r;
                }

                (IExpr right, State after) = r.Value;
                left = op switch
                {
                    '*' => Expr.FromBinaryMul(left, right),
                    '/' => Expr.FromBinaryDiv(left, right),
                    _ => Expr.FromBinaryMod(left, right)
                };
                rest = after;
                continue;
            }

            rest = save;
            break;
        }

        return Ok(left, rest);
    }

    private static Result<(IExpr expr, State rest)> ParseUnary(State s)
    {
        State save = s.SkipWs();
        if (!save.Eof && save.Text[save.Pos] == '!')
        {
            State afterBang = save.Advance();
            Result<(IExpr expr, State rest)> inner = ParseUnary(afterBang);
            if (!inner.IsOk)
            {
                return inner;
            }

            return Result<(IExpr, State)>.Ok((Expr.FromUnaryNot(inner.Value.expr), inner.Value.rest));
        }

        save = s.SkipWs();
        if (!save.Eof && save.Text[save.Pos] == '-')
        {
            State afterMinus = save.Advance();
            Result<(IExpr expr, State rest)> inner = ParseUnary(afterMinus);
            if (!inner.IsOk)
            {
                return inner;
            }

            return Result<(IExpr, State)>.Ok((Expr.FromUnaryNeg(inner.Value.expr), inner.Value.rest));
        }

        return ParsePrimaryWithMemberChain(s);
    }

    private static Result<(IExpr expr, State rest)> ParsePrimaryWithMemberChain(State s)
    {
        Result<(IExpr expr, State rest)> atom = ParsePrimary(s);
        if (!atom.IsOk)
        {
            return atom;
        }

        (IExpr current, State rest) = atom.Value;

        while (true)
        {
            State dotPos = rest;
            rest = rest.SkipWs();
            if (!rest.Eof && rest.Text[rest.Pos] == '.')
            {
                rest = rest.Advance();
                (bool ok, string name, State rest) ident = ReadIdent(rest);
                if (!ident.ok)
                {
                    if (current is not NumberExpr)
                    {
                        return Err<IExpr>(
                            "MemberName",
                            TextSpan.At(rest.Pos, 0),
                            "Expected member name after '.'.");
                    }

                    int numberStart = FindNumberStart(dotPos.Text, dotPos.Pos);
                    return Err<IExpr>(
                        "NumberFormat",
                        TextSpan.At(numberStart, dotPos.Pos + 1 - numberStart),
                        "Invalid number literal.");
                }

                current = Expr.FromMember(current, ident.name);
                rest = ident.rest;
                continue;
            }

            rest = dotPos;
            break;
        }

        return Ok(current, rest);
    }

    private static Result<(IExpr expr, State rest)> ParsePrimary(State s)
    {
        State rest = s.SkipWs();
        if (rest.Eof)
        {
            return Err<IExpr>("ExprEmpty", TextSpan.At(rest.Pos, 0), "Expected expression.");
        }

        char c = rest.Text[rest.Pos];

        if (c == '(')
        {
            rest = rest.Advance().SkipWs();
            Result<(IExpr expr, State rest)> inner = ParseOr(rest);
            if (!inner.IsOk)
            {
                return inner;
            }

            rest = inner.Value.rest.SkipWs();
            if (rest.Eof || rest.Text[rest.Pos] != ')')
            {
                return Err<IExpr>("MissingRParen", TextSpan.At(rest.Pos, rest.Eof ? 0 : 1),
                    "Expected ')' to close parenthesized expression.");
            }

            return Ok(inner.Value.expr, rest.Advance());
        }

        if (c == '"')
        {
            Result<(string value, State rest)> sLit = ParseStringLiteral(rest);
            if (!sLit.IsOk)
            {
                return Err<IExpr>(sLit.Error!.Code, sLit.Error!.Range, sLit.Error!.Message);
            }

            return Result<(IExpr, State)>.Ok((Expr.FromString(sLit.Value.value), sLit.Value.rest));
        }

        if (rest.StartsWith("true") && (rest.Pos + 4 == rest.Length || !IsIdentPart(rest.Text[rest.Pos + 4])))
        {
            return Ok<IExpr>(Expr.FromBoolean(true), rest.Advance(4));
        }

        if (rest.StartsWith("false") && (rest.Pos + 5 == rest.Length || !IsIdentPart(rest.Text[rest.Pos + 5])))
        {
            return Ok<IExpr>(Expr.FromBoolean(false), rest.Advance(5));
        }

        if (char.IsDigit(c))
        {
            Result<(double value, State rest)> num = ParseNumber(rest);
            if (!num.IsOk)
            {
                return Err<IExpr>(num.Error!.Code, num.Error!.Range, num.Error!.Message);
            }

            return Ok<IExpr>(Expr.FromNumber(num.Value.value), num.Value.rest);
        }

        if (IsIdentStart(c))
        {
            (bool ok, string name, State rest) ident = ReadIdent(rest);
            if (!ident.ok)
            {
                return Err<IExpr>("UnexpectedChar", TextSpan.At(rest.Pos, 1), $"Unexpected character '{c}'.");
            }

            return Ok<IExpr>(Expr.FromIdent(ident.name), ident.rest);
        }

        return Err<IExpr>("UnexpectedChar", TextSpan.At(rest.Pos, 1), $"Unexpected character '{c}'.");
    }

    private static (bool ok, string name, State rest) ReadIdent(State s)
    {
        if (s.Eof || !IsIdentStart(s.Text[s.Pos]))
        {
            return (false, string.Empty, s);
        }

        int start = s.Pos;
        int i = s.Pos + 1;
        while (i < s.Length && IsIdentPart(s.Text[i]))
        {
            i++;
        }

        string name = s.Text.Substring(start, i - start);
        return (true, name, s with { Pos = i });
    }

    private static Result<(string value, State rest)> ParseStringLiteral(State s)
    {
        int start = s.Pos;
        if (s.Eof || s.Text[s.Pos] != '"')
        {
            return Result<(string, State)>.Err(new ParseError("StringStart", TextSpan.At(s.Pos, 0), "Expected '\"'."));
        }

        State rest = s.Advance();
        StringBuilder sb = new();
        bool escape = false;
        while (!rest.Eof)
        {
            char ch = rest.Text[rest.Pos];
            if (escape)
            {
                switch (ch)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    default:
                        return Result<(string, State)>.Err(new ParseError(
                            "StringEscape",
                            TextSpan.At(rest.Pos - 1, 2),
                            "Unsupported escape sequence."));
                }

                escape = false;
                rest = rest.Advance();
                continue;
            }

            if (ch == '\\')
            {
                escape = true;
                rest = rest.Advance();
                continue;
            }

            if (ch == '"')
            {
                return Ok(sb.ToString(), rest.Advance());
            }

            sb.Append(ch);
            rest = rest.Advance();
        }

        return Result<(string, State)>.Err(new ParseError(
            "StringUnterminated",
            TextSpan.At(start, s.Length - start),
            "Unterminated string literal."));
    }

    private static Result<(double value, State rest)> ParseNumber(State s)
    {
        int start = s.Pos;
        int i = s.Pos;
        while (i < s.Length && char.IsDigit(s.Text[i]))
        {
            i++;
        }

        if (i < s.Length && s.Text[i] == '.')
        {
            int dot = i;
            i++;
            while (i < s.Length && char.IsDigit(s.Text[i]))
            {
                i++;
            }

            if (i == dot + 1)
            {
                return Result<(double, State)>.Err(new ParseError(
                    "NumberFormat",
                    TextSpan.At(start, i - start),
                    "Invalid number literal."));
            }
        }

        string slice = s.Text.Substring(start, i - start);
        if (!double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return Result<(double, State)>.Err(new ParseError(
                "NumberFormat",
                TextSpan.At(start, i - start),
                "Invalid number literal."));
        }

        return Ok(value, s with { Pos = i });
    }

    private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_';
    private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static int FindNumberStart(string text, int dotIndex)
    {
        int k = dotIndex - 1;
        while (k >= 0 && (char.IsDigit(text[k]) || text[k] == '.'))
        {
            k--;
        }

        return k + 1;
    }
}