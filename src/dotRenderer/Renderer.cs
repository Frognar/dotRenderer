using System.Globalization;
using System.Text;

namespace dotRenderer;

public static class Renderer
{
    private enum PieceKind { Text, Eval, If }

    private sealed class Piece(string s, PieceKind k)
    {
        public string S { get; set; } = s;
        public PieceKind K { get; } = k;
    }

    public static string Render<TModel>(SequenceNode ast, TModel model, IValueAccessor<TModel> accessor)
    {
        ArgumentNullException.ThrowIfNull(ast);
        ArgumentNullException.ThrowIfNull(accessor);
        
        List<Piece> pieces = new (ast.Children.Count);
        foreach (Node node in ast.Children)
        {
            pieces.Add(node switch
            {
                TextNode t => new Piece(t.Text, PieceKind.Text),
                EvalNode e => new Piece(RenderEval(e, model, accessor), PieceKind.Eval),
                IfNode i => new Piece(EvalIfCondition(i.Condition, model, accessor)
                    ? Render(i.Body, model, accessor)
                    : string.Empty, PieceKind.If),
                _ => new Piece(string.Empty, PieceKind.Text)
            });
        }
        
        for (int i = 0; i < pieces.Count; i++)
        {
            if (pieces[i].S.Length == 0 && i > 0 && i + 1 < pieces.Count)
            {
                if (EndsWithNewline(pieces[i - 1].S) && StartsWithNewline(pieces[i + 1].S, out int nlLen))
                {
                    pieces[i + 1].S = pieces[i + 1].S.Substring(nlLen);
                }
            }
        }

        for (int i = 0; i + 1 < pieces.Count; i++)
        {
            bool atIfBoundary = pieces[i].K == PieceKind.If || pieces[i + 1].K == PieceKind.If;
            if (atIfBoundary && EndsWithNewline(pieces[i].S) && StartsWithNewline(pieces[i + 1].S, out int nlLen))
            {
                pieces[i + 1].S = pieces[i + 1].S.Substring(nlLen);
            }
        }
        
        var sb = new StringBuilder(pieces.Sum(p => p.S.Length));
        foreach (var p in pieces) sb.Append(p.S);
        return sb.ToString();
    }

    private static string RenderEval<TModel>(EvalNode e, TModel model, IValueAccessor<TModel> accessor)
    {
        string joinedPath = JoinModelPath(e.Path);
        return accessor.AccessValue(joinedPath, model)
               ?? throw new KeyNotFoundException(
                   $"Missing key '{joinedPath}' in model (path: {joinedPath})");
    }

    private static bool EndsWithNewline(string s)
        => s.EndsWith("\r\n", StringComparison.Ordinal) || s.EndsWith('\n');

    private static bool StartsWithNewline(string s, out int newlineLength)
    {
        if (s.StartsWith("\r\n", StringComparison.Ordinal)) { newlineLength = 2; return true; }
        if (s.StartsWith('\n'))   { newlineLength = 1; return true; }
        newlineLength = 0; return false;
    }

    private static string JoinModelPath(IEnumerable<string> pathSegments) => string.Join('.', pathSegments.Skip(1));

    private static bool EvalIfCondition<TModel>(ExprNode cond, TModel model, IValueAccessor<TModel> accessor)
    {
        return cond switch
        {
            LiteralExpr<bool> lit => lit.Value,
            PropertyExpr prop => TryGetBool(prop, model, accessor),
            UnaryExpr { Operator: "!" } unary => !EvalIfCondition(unary.Operand, model, accessor),
            BinaryExpr { Operator: "&&" } binary => EvalIfCondition(binary.Left, model, accessor) &&
                                                    EvalIfCondition(binary.Right, model, accessor),
            BinaryExpr { Operator: "||" } binary => EvalIfCondition(binary.Left, model, accessor) ||
                                                    EvalIfCondition(binary.Right, model, accessor),
            BinaryExpr { Operator: "==" or "!=" or ">" or "<" or ">=" or "<=" } binary =>
                CompareOperands(binary.Left, binary.Right, model, accessor, binary.Operator),
            _ => throw new InvalidOperationException(
                $"Unsupported expression in if condition: {cond.GetType().Name}")
        };
    }

    private static bool CompareOperands<TModel>(
        ExprNode left,
        ExprNode right,
        TModel model,
        IValueAccessor<TModel> accessor,
        string op)
    {
        if (IsArithmetic(left) || IsArithmetic(right))
        {
            double lnum = EvalNumber(left, model, accessor);
            double rnum = EvalNumber(right, model, accessor);
            return op switch
            {
                "==" => Math.Abs(lnum - rnum) < 0.000000001,
                "!=" => Math.Abs(lnum - rnum) > 0.000000001,
                ">" => lnum > rnum,
                "<" => lnum < rnum,
                ">=" => lnum >= rnum,
                "<=" => lnum <= rnum,
                _ => throw new InvalidOperationException($"Unknown operator '{op}'")
            };
        }

        object l = EvalOperand(left, model, accessor);
        object r = EvalOperand(right, model, accessor);

        return (l, r, op) switch
        {
            (int li, int ri, "==") => li == ri,
            (int li, int ri, "!=") => li != ri,
            (int li, int ri, ">") => li > ri,
            (int li, int ri, "<") => li < ri,
            (int li, int ri, ">=") => li >= ri,
            (int li, int ri, "<=") => li <= ri,
            (double ld, double rd, "==") => Math.Abs(ld - rd) < 0.000000001,
            (double ld, double rd, "!=") => Math.Abs(ld - rd) > 0.000000001,
            (double ld, double rd, ">") => ld > rd,
            (double ld, double rd, "<") => ld < rd,
            (double ld, double rd, ">=") => ld >= rd,
            (double ld, double rd, "<=") => ld <= rd,
            (int li, double rd, "==") => Math.Abs(li - rd) < 0.000000001,
            (int li, double rd, "!=") => Math.Abs(li - rd) > 0.000000001,
            (int li, double rd, ">") => li > rd,
            (int li, double rd, "<") => li < rd,
            (int li, double rd, ">=") => li >= rd,
            (int li, double rd, "<=") => li <= rd,
            (double ld, int ri, "==") => Math.Abs(ld - ri) < 0.000000001,
            (double ld, int ri, "!=") => Math.Abs(ld - ri) > 0.000000001,
            (double ld, int ri, ">") => ld > ri,
            (double ld, int ri, "<") => ld < ri,
            (double ld, int ri, ">=") => ld >= ri,
            (double ld, int ri, "<=") => ld <= ri,
            (bool lb, bool rb, "==") => lb == rb,
            (bool lb, bool rb, "!=") => lb != rb,
            (string ls, string rs, "==") => ls == rs,
            (string ls, string rs, "!=") => ls != rs,
            (string, string, ">" or "<" or ">=" or "<=") => throw new InvalidOperationException(
                "Only == and != are supported for string"),
            (bool, bool, ">" or "<" or ">=" or "<=") => throw new InvalidOperationException(
                "Only == and != are supported for bool"),
            _ => throw new InvalidOperationException(
                $"Cannot compare values of types '{l.GetType().Name}' and '{r.GetType().Name}'.")
        };
    }

    private static object EvalOperand<TModel>(ExprNode expr, TModel model, IValueAccessor<TModel> accessor)
    {
        return expr switch
        {
            LiteralExpr<int> litInt => litInt.Value,
            LiteralExpr<double> litDouble => litDouble.Value,
            LiteralExpr<string> litStr => litStr.Value,
            LiteralExpr<bool> litBool => litBool.Value,
            PropertyExpr prop => TryParseFromAccessor(prop, model, accessor),
            _ => throw new InvalidOperationException($"Unsupported operand type: {expr.GetType().Name}")
        };
    }

    private static object TryParseFromAccessor<TModel>(PropertyExpr prop, TModel model, IValueAccessor<TModel> accessor)
    {
        string joinedPath = JoinModelPath(prop.Path);
        string value = accessor.AccessValue(joinedPath, model)
                       ?? throw new KeyNotFoundException($"Missing key '{joinedPath}' in model (path: {joinedPath})");

        if (bool.TryParse(value, out bool boolVal))
        {
            return boolVal;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleVal))
        {
            return doubleVal;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intVal))
        {
            return intVal;
        }

        return value;
    }

    private static bool TryGetBool<TModel>(PropertyExpr prop, TModel model, IValueAccessor<TModel> accessor)
    {
        string joinedPath = JoinModelPath(prop.Path);
        string? str = accessor.AccessValue(joinedPath, model);
        if (str is null)
        {
            throw new InvalidOperationException(
                $"If condition path '{joinedPath}' returned null (expected \"true\" or \"false\")");
        }

        if (string.Equals(str, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(str, "false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        throw new InvalidOperationException(
            $"If condition path '{joinedPath}' must resolve to \"true\" or \"false\", got '{str}'.");
    }

    private static bool IsArithmetic(ExprNode expr) => expr switch
    {
        UnaryExpr { Operator: "-" } => true,
        BinaryExpr { Operator: "+" or "-" or "*" or "/" or "%" } => true,
        _ => false
    };

    private static double EvalNumber<TModel>(ExprNode expr, TModel model, IValueAccessor<TModel> accessor) =>
        expr switch
        {
            LiteralExpr<int> li => li.Value,
            LiteralExpr<double> ld => ld.Value,
            UnaryExpr { Operator: "-" } u => -EvalNumber(u.Operand, model, accessor),
            BinaryExpr { Operator: "+" } b =>
                EvalNumber(b.Left, model, accessor) + EvalNumber(b.Right, model, accessor),
            BinaryExpr { Operator: "-" } b =>
                EvalNumber(b.Left, model, accessor) - EvalNumber(b.Right, model, accessor),
            BinaryExpr { Operator: "*" } b =>
                EvalNumber(b.Left, model, accessor) * EvalNumber(b.Right, model, accessor),
            BinaryExpr { Operator: "/" } b =>
                EvalNumber(b.Left, model, accessor) / EvalNumber(b.Right, model, accessor),
            BinaryExpr { Operator: "%" } b =>
                EvalNumber(b.Left, model, accessor) % EvalNumber(b.Right, model, accessor),
            PropertyExpr p => TryParseNumberFromAccessor(p, model, accessor),
            _ => throw new InvalidOperationException($"Expected numeric expression, got {expr.GetType().Name}")
        };

    private static double TryParseNumberFromAccessor<TModel>(PropertyExpr prop, TModel model,
        IValueAccessor<TModel> accessor)
    {
        string joinedPath = JoinModelPath(prop.Path);
        string value = accessor.AccessValue(joinedPath, model)
                       ?? throw new KeyNotFoundException($"Missing key '{joinedPath}' in model (path: {joinedPath})");

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
        {
            return d;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
        {
            return i;
        }

        throw new InvalidOperationException($"Value at '{joinedPath}' is not numeric: '{value}'");
    }
}