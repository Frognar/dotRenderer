using System.Globalization;

namespace dotRenderer;

public static class ExpressionParser
{
    public static ExprNode Parse(string expr)
    {
        ArgumentNullException.ThrowIfNull(expr);
        Parser parser = new(expr);
        ExprNode node = parser.ParseExpression();
        parser.SkipWhitespace();
        return !parser.End
            ? throw new InvalidOperationException($"Unexpected token at end: {parser.Remaining}'")
            : node;
    }

    private sealed class Parser(string expr)
    {
        private readonly string _expr = expr;
        private int _pos;

        public bool End => _pos >= _expr.Length;
        public string Remaining => _expr[_pos..];

        public void SkipWhitespace()
        {
            while (!End && char.IsWhiteSpace(_expr[_pos]))
            {
                _pos++;
            }
        }

        public ExprNode ParseExpression()
        {
            ExprNode left = ParseAnd();
            SkipWhitespace();
            while (Match("||"))
            {
                SkipWhitespace();
                ExprNode right = ParseAnd();
                left = new BinaryExpr("||", left, right);
                SkipWhitespace();
            }

            return left;
        }

        private ExprNode ParseAnd()
        {
            ExprNode left = ParseEquality();
            SkipWhitespace();
            while (Match("&&"))
            {
                SkipWhitespace();
                ExprNode right = ParseEquality();
                left = new BinaryExpr("&&", left, right);
                SkipWhitespace();
            }

            return left;
        }

        private ExprNode ParseEquality()
        {
            ExprNode left = ParseUnary();
            SkipWhitespace();
            while (true)
            {
                if (Match("=="))
                {
                    SkipWhitespace();
                    ExprNode right = ParseUnary();
                    left = new BinaryExpr("==", left, right);
                }
                else if (Match(">="))
                {
                    SkipWhitespace();
                    ExprNode right = ParseUnary();
                    left = new BinaryExpr(">=", left, right);
                }
                else
                {
                    break;
                }

                SkipWhitespace();
            }

            return left;
        }

        private ExprNode ParseUnary()
        {
            SkipWhitespace();
            if (Match("!"))
            {
                SkipWhitespace();
                ExprNode operand = ParseUnary();
                return new UnaryExpr("!", operand);
            }

            return ParsePrimary();
        }

        private ExprNode ParsePrimary()
        {
            SkipWhitespace();
            if (Match("true"))
            {
                return new LiteralExpr<bool>(true);
            }

            if (Match("false"))
            {
                return new LiteralExpr<bool>(false);
            }

            if (Peek() == '"')
            {
                _pos++;
                int start = _pos;
                while (!End && Peek() != '"')
                {
                    _pos++;
                }

                if (End)
                {
                    throw new InvalidOperationException("Unclosed string literal");
                }

                string lit = _expr[start.._pos];
                _pos++;
                return new LiteralExpr<string>(lit);
            }

            if (char.IsDigit(Peek()))
            {
                int start = _pos;
                bool hasDot = false;
                while (!End && (char.IsDigit(Peek()) || Peek() == '.'))
                {
                    if (Peek() == '.')
                    {
                        if (hasDot)
                        {
                            throw new InvalidOperationException("Multiple dots in number");
                        }

                        hasDot = true;
                    }

                    _pos++;
                }

                string lit = _expr[start.._pos];
                return hasDot
                    ? new LiteralExpr<double>(double.Parse(lit, CultureInfo.InvariantCulture))
                    : new LiteralExpr<int>(int.Parse(lit, CultureInfo.InvariantCulture));
            }

            if (Match("Model."))
            {
                List<string> segments = ["Model"];
                while (true)
                {
                    int start = _pos;
                    while (!End && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
                    {
                        _pos++;
                    }

                    if (start == _pos)
                    {
                        throw new InvalidOperationException("Expected property segment after '.'");
                    }

                    segments.Add(_expr[start.._pos]);

                    if (Match("."))
                    {
                        continue;
                    }

                    break;
                }

                return new PropertyExpr(segments);
            }

            throw new InvalidOperationException($"Unknown token near: '{Remaining}");
        }

        private bool Match(string s)
        {
            SkipWhitespace();
            if (_expr.AsSpan(_pos).StartsWith(s.AsSpan(), StringComparison.Ordinal))
            {
                _pos += s.Length;
                return true;
            }

            return false;
        }

        private char Peek() => !End ? _expr[_pos] : '\0';
    }
}

public abstract record ExprNode;

public sealed record PropertyExpr(IReadOnlyList<string> Path) : ExprNode;

public sealed record LiteralExpr<T>(T Value) : ExprNode;

public sealed record UnaryExpr(string Operator, ExprNode Operand) : ExprNode;

public sealed record BinaryExpr(string Operator, ExprNode Left, ExprNode Right) : ExprNode;