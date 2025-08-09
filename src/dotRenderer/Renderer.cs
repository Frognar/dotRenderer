using System.Globalization;
using System.Text;

namespace dotRenderer;

public static class Renderer
{
    private enum SegmentKind
    {
        Text,
        Eval,
        IfBoundary
    }

    private readonly record struct Segment(string Text, SegmentKind Kind);

    public static string Render<TModel>(SequenceNode ast, TModel model, IValueAccessor<TModel> accessor)
    {
        ArgumentNullException.ThrowIfNull(ast);
        ArgumentNullException.ThrowIfNull(accessor);
        IEnumerable<Segment> segments = ast.Children.Select(n => RenderNode(n, model, accessor));
        return CombineSegments(segments);
    }

    private static Segment RenderNode<TModel>(Node node, TModel model, IValueAccessor<TModel> accessor) =>
        node switch
        {
            TextNode t => new Segment(t.Text, SegmentKind.Text),
            EvalNode e => new Segment(RenderEval(e, model, accessor), SegmentKind.Eval),
            IfNode i => RenderIf(i, model, accessor),
            _ => new Segment("", SegmentKind.Text)
        };

    private static Segment RenderIf<TModel>(IfNode i, TModel model, IValueAccessor<TModel> accessor)
    {
        bool cond = EvalIfCondition(i.Condition, model, accessor);
        string body = cond ? TrimOneOuterNewline(Render(i.Body, model, accessor)) : "";
        return new Segment(body, SegmentKind.IfBoundary);
    }

    private static string CombineSegments(IEnumerable<Segment> segments)
    {
        StringBuilder sb = new();
        bool prevEndsWithNewLine = false;
        bool prevWasIfBoundary = false;

        foreach (Segment segment in segments)
        {
            string current = segment.Text;
            if ((prevWasIfBoundary || segment.Kind == SegmentKind.IfBoundary) &&
                prevEndsWithNewLine &&
                HasLeadingNewline(current) is int leadNlLen and > 0)
            {
                current = current[leadNlLen..];
            }

            sb.Append(current);
            if (current.Length > 0)
            {
                prevEndsWithNewLine = HasTrailingNewline(current) > 0;
            }

            prevWasIfBoundary = segment.Kind == SegmentKind.IfBoundary;
        }

        return sb.ToString();
    }

    private static string TrimOneOuterNewline(string s)
    {
        int lead = HasLeadingNewline(s);
        if (lead > 0)
        {
            s = s[lead..];
        }

        int trail = HasTrailingNewline(s);
        if (trail > 0)
        {
            s = s[..^trail];
        }

        return s;
    }

    private static int HasLeadingNewline(string s) =>
        s.Length switch
        {
            >= 2 when s[0] == '\r' && s[1] == '\n' => 2,
            >= 1 when s[0] == '\n' => 1,
            _ => 0
        };

    private static int HasTrailingNewline(string s) =>
        s.Length switch
        {
            >= 2 when s[^2] == '\r' && s[^1] == '\n' => 2,
            >= 1 when s[^1] == '\n' => 1,
            _ => 0
        };

    private static string RenderEval<TModel>(EvalNode e, TModel model, IValueAccessor<TModel> accessor)
    {
        string joinedPath = JoinModelPath(e.Path);
        return accessor.AccessValue(joinedPath, model)
               ?? throw new KeyNotFoundException(
                   $"Missing key '{joinedPath}' in model (path: {joinedPath})");
    }

    private static string JoinModelPath(IEnumerable<string> pathSegments) => string.Join('.', pathSegments.Skip(1));

    private static bool EvalIfCondition<TModel>(ExprNode cond, TModel model, IValueAccessor<TModel> accessor) =>
        cond switch
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
                "==" => Math.Abs(lnum - rnum) < 1e-9,
                "!=" => Math.Abs(lnum - rnum) > 1e-9,
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

            (double ld, double rd, "==") => Math.Abs(ld - rd) < 1e-9,
            (double ld, double rd, "!=") => Math.Abs(ld - rd) > 1e-9,
            (double ld, double rd, ">") => ld > rd,
            (double ld, double rd, "<") => ld < rd,
            (double ld, double rd, ">=") => ld >= rd,
            (double ld, double rd, "<=") => ld <= rd,

            (int li, double rd, "==") => Math.Abs(li - rd) < 1e-9,
            (int li, double rd, "!=") => Math.Abs(li - rd) > 1e-9,
            (int li, double rd, ">") => li > rd,
            (int li, double rd, "<") => li < rd,
            (int li, double rd, ">=") => li >= rd,
            (int li, double rd, "<=") => li <= rd,

            (double ld, int ri, "==") => Math.Abs(ld - ri) < 1e-9,
            (double ld, int ri, "!=") => Math.Abs(ld - ri) > 1e-9,
            (double ld, int ri, ">") => ld > ri,
            (double ld, int ri, "<") => ld < ri,
            (double ld, int ri, ">=") => ld >= ri,
            (double ld, int ri, "<=") => ld <= ri,

            (bool lb, bool rb, "==") => lb == rb,
            (bool lb, bool rb, "!=") => lb != rb,

            (string ls, string rs, "==") => ls == rs,
            (string ls, string rs, "!=") => ls != rs,
            (string, string, ">" or "<" or ">=" or "<=") =>
                throw new InvalidOperationException("Only == and != are supported for string"),
            (bool, bool, ">" or "<" or ">=" or "<=") =>
                throw new InvalidOperationException("Only == and != are supported for bool"),

            _ => throw new InvalidOperationException(
                $"Cannot compare values of types '{l.GetType().Name}' and '{r.GetType().Name}'.")
        };
    }

    private static object EvalOperand<TModel>(ExprNode expr, TModel model, IValueAccessor<TModel> accessor) =>
        expr switch
        {
            LiteralExpr<int> litInt => litInt.Value,
            LiteralExpr<double> litDouble => litDouble.Value,
            LiteralExpr<string> litStr => litStr.Value,
            LiteralExpr<bool> litBool => litBool.Value,
            PropertyExpr prop => ParseFromAccessor(prop, model, accessor),
            _ => throw new InvalidOperationException($"Unsupported operand type: {expr.GetType().Name}")
        };

    private static object ParseFromAccessor<TModel>(PropertyExpr prop, TModel model, IValueAccessor<TModel> accessor)
    {
        string joinedPath = JoinModelPath(prop.Path);
        string value = accessor.AccessValue(joinedPath, model)
                       ?? throw new KeyNotFoundException($"Missing key '{joinedPath}' in model (path: {joinedPath})");

        if (bool.TryParse(value, out bool boolVal))
        {
            return boolVal;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intVal))
        {
            return intVal;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double dblVal))
        {
            return dblVal;
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

    private static bool IsArithmetic(ExprNode expr) =>
        expr switch
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
            PropertyExpr p => ParseNumberFromAccessor(p, model, accessor),
            _ => throw new InvalidOperationException($"Expected numeric expression, got {expr.GetType().Name}")
        };

    private static double ParseNumberFromAccessor<TModel>(
        PropertyExpr prop,
        TModel model,
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