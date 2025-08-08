using System.Globalization;

namespace dotRenderer.Tests;

public class RendererTests
{
    [Fact]
    public void Renderer_Generic_Should_Render_Nested_Path_From_Model()
    {
        TestDictModel model = TestDictModel.With(("User.Name", "Alice"));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new TextNode("Hello "),
            new EvalNode(["Model", "User", "Name"]),
            new TextNode("!")
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("Hello Alice!", html);
    }

    [Fact]
    public void Renderer_Generic_Should_Throw_When_Nested_Path_Missing()
    {
        TestDictModel model = TestDictModel.With(("User", "Alice"));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new TextNode("Hello "),
            new EvalNode(["Model", "User", "Name"]),
            new TextNode("!")
        ]);

        KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() =>
            Renderer.Render(ast, model, accessor)
        );

        Assert.Contains("User.Name", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Generic_Should_Render_IfNode_With_Flag_True()
    {
        TestDictModel model = TestDictModel.With(("Flag", "true"));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new TextNode("X"),
            new IfNode(
                new PropertyExpr(["Model", "Flag"]),
                new SequenceNode([new TextNode("YES")])
            ),
            new TextNode("Y")
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("XYESY", html);
    }

    [Fact]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Flag_False()
    {
        TestDictModel model = TestDictModel.With(("Flag", "false"));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new TextNode("X"),
            new IfNode(
                new PropertyExpr(["Model", "Flag"]),
                new SequenceNode([new TextNode("YES")])
            ),
            new TextNode("Y")
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("XY", html);
    }

    [Fact]
    public void Renderer_Generic_Should_Throw_If_Accessor_Returns_NonBoolean_String()
    {
        TestDictModel model = TestDictModel.With(("OtherFlag", "yes"));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new IfNode(
                new PropertyExpr(["Model", "OtherFlag"]),
                new SequenceNode([new TextNode("YES")])
            )
        ]);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            Renderer.Render(ast, model, accessor)
        );

        Assert.Contains("must resolve to \"true\" or \"false\"", ex.Message, StringComparison.Ordinal);
        Assert.Contains("yes", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Renderer_Generic_Should_Throw_If_Accessor_Returns_Null_For_IfNode()
    {
        TestDictModel model = TestDictModel.Empty;
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new IfNode(
                new PropertyExpr(["Model", "YetAnotherFlag"]),
                new SequenceNode([new TextNode("YES")])
            )
        ]);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            Renderer.Render(ast, model, accessor)
        );

        Assert.Contains("returned null", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Flag", ex.Message, StringComparison.Ordinal);
        Assert.Contains("true", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("false", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Renderer_Generic_Should_Render_IfNode_With_Literal_True()
    {
        TestDictModel model = TestDictModel.Empty;
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new TextNode("A"),
            new IfNode(
                new LiteralExpr<bool>(true),
                new SequenceNode([new TextNode("YES")])
            ),
            new TextNode("B")
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("AYESB", html);
    }

    [Fact]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Literal_False()
    {
        TestDictModel model = TestDictModel.Empty;
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new TextNode("A"),
            new IfNode(
                new LiteralExpr<bool>(false),
                new SequenceNode([new TextNode("NO")])
            ),
            new TextNode("B")
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("AB", html);
    }

    [Fact]
    public void Renderer_Generic_Should_Render_IfNode_With_Unary_Not_Condition_True()
    {
        TestDictModel model = TestDictModel.With(("Flag", "false"));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new IfNode(
                new UnaryExpr("!", new PropertyExpr(["Model", "Flag"])),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("OK", html);
    }

    [Fact]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Unary_Not_Condition_False()
    {
        TestDictModel model = TestDictModel.With(("Flag", "true"));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new IfNode(
                new UnaryExpr("!", new PropertyExpr(["Model", "Flag"])),
                new SequenceNode([new TextNode("NOPE")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("", html);
    }

    [Fact]
    public void Renderer_Generic_Should_Render_IfNode_With_Binary_And_Condition_True()
    {
        TestDictModel model = TestDictModel.With(("A", "true"), ("B", "true"));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    "&&",
                    new PropertyExpr(["Model", "A"]),
                    new PropertyExpr(["Model", "B"])
                ),
                new SequenceNode([new TextNode("YES")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("YES", html);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Binary_And_Condition_False(bool a, bool b)
    {
        TestDictModel model = TestDictModel.With(("A", a.ToString()), ("B", b.ToString()));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    "&&",
                    new PropertyExpr(["Model", "A"]),
                    new PropertyExpr(["Model", "B"])
                ),
                new SequenceNode([new TextNode("NO")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("", html);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Renderer_Generic_Should_Render_IfNode_With_Binary_Or_Condition_True(bool a, bool b)
    {
        TestDictModel model = TestDictModel.With(("A", a.ToString()), ("B", b.ToString()));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    "||",
                    new PropertyExpr(["Model", "A"]),
                    new PropertyExpr(["Model", "B"])
                ),
                new SequenceNode([new TextNode("YES")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("YES", html);
    }

    [Fact]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Binary_Or_Condition_False()
    {
        TestDictModel model = TestDictModel.With(("A", "false"), ("B", "false"));
        TestDictAccessor accessor = new();

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    "||",
                    new PropertyExpr(["Model", "A"]),
                    new PropertyExpr(["Model", "B"])
                ),
                new SequenceNode([new TextNode("NO")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal("", html);
    }

    [Theory]
    [InlineData(42, 42, "==", "OK")]
    [InlineData(42, 42, "!=", "")]
    [InlineData(42, 43, "==", "")]
    [InlineData(42, 43, "!=", "OK")]
    [InlineData(5, 3, ">", "OK")]
    [InlineData(2, 3, ">", "")]
    [InlineData(2, 3, "<", "OK")]
    [InlineData(3, 3, "<=", "OK")]
    [InlineData(2, 3, "<=", "OK")]
    [InlineData(5, 3, ">=", "OK")]
    [InlineData(3, 5, ">=", "")]
    public void Renderer_Generic_Should_Compare_Int_Values(int left, int right, string op, string expected)
    {
        TestDictModel model = TestDictModel.With(("Value", left.ToString(CultureInfo.InvariantCulture)));
        TestDictAccessor accessor = new();

        ExprNode leftExpr = new PropertyExpr(["Model", "Value"]);
        ExprNode rightExpr = new LiteralExpr<int>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData(42.0, 42.0, "==", "OK")]
    [InlineData(42.0, 42.0, "!=", "")]
    [InlineData(42.0, 43.0, "==", "")]
    [InlineData(42.0, 43.0, "!=", "OK")]
    [InlineData(5.0, 3.0, ">", "OK")]
    [InlineData(2.0, 3.0, ">", "")]
    [InlineData(2.0, 3.0, "<", "OK")]
    [InlineData(3.0, 3.0, "<=", "OK")]
    [InlineData(2.0, 3.0, "<=", "OK")]
    [InlineData(5.0, 3.0, ">=", "OK")]
    [InlineData(3.0, 5.0, ">=", "")]
    public void Renderer_Generic_Should_Compare_Double_Values(double left, double right, string op, string expected)
    {
        TestDictModel model = TestDictModel.With(("Value", left.ToString(CultureInfo.InvariantCulture)));
        TestDictAccessor accessor = new();

        ExprNode leftExpr = new PropertyExpr(["Model", "Value"]);
        ExprNode rightExpr = new LiteralExpr<double>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData(42, 42.0, "==", "OK")]
    [InlineData(42, 42.0, "!=", "")]
    [InlineData(42, 43.0, "==", "")]
    [InlineData(42, 43.0, "!=", "OK")]
    [InlineData(5, 3.0, ">", "OK")]
    [InlineData(2, 3.0, ">", "")]
    [InlineData(2, 3.0, "<", "OK")]
    [InlineData(3, 3.0, "<=", "OK")]
    [InlineData(2, 3.0, "<=", "OK")]
    [InlineData(5, 3.0, ">=", "OK")]
    [InlineData(3, 5.0, ">=", "")]
    public void Renderer_Generic_Should_Compare_Int_And_Double_Values(int left, double right, string op,
        string expected)
    {
        TestDictModel model = TestDictModel.With(("Value", left.ToString(CultureInfo.InvariantCulture)));
        TestDictAccessor accessor = new();

        ExprNode leftExpr = new PropertyExpr(["Model", "Value"]);
        ExprNode rightExpr = new LiteralExpr<double>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData(42.0, 42, "==", "OK")]
    [InlineData(42.0, 42, "!=", "")]
    [InlineData(42.0, 43, "==", "")]
    [InlineData(42.0, 43, "!=", "OK")]
    [InlineData(5.0, 3, ">", "OK")]
    [InlineData(2.0, 3, ">", "")]
    [InlineData(2.0, 3, "<", "OK")]
    [InlineData(3.0, 3, "<=", "OK")]
    [InlineData(2.0, 3, "<=", "OK")]
    [InlineData(5.0, 3, ">=", "OK")]
    [InlineData(3.0, 5, ">=", "")]
    public void Renderer_Generic_Should_Compare_Double_And_Int_Values(
        double left,
        int right,
        string op,
        string expected)
    {
        TestDictModel model = TestDictModel.With(("Value", left.ToString(CultureInfo.InvariantCulture)));

        TestDictAccessor accessor = new();

        ExprNode leftExpr = new PropertyExpr(["Model", "Value"]);
        ExprNode rightExpr = new LiteralExpr<int>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData(true, true, "==", "OK")]
    [InlineData(true, true, "!=", "")]
    [InlineData(true, false, "==", "")]
    [InlineData(true, false, "!=", "OK")]
    [InlineData(false, false, "==", "OK")]
    [InlineData(false, false, "!=", "")]
    [InlineData(false, true, "==", "")]
    [InlineData(false, true, "!=", "OK")]
    public void Renderer_Generic_Should_Compare_Bool_Values(bool left, bool right, string op, string expected)
    {
        TestDictModel model = TestDictModel.With(("Value", left.ToString()));
        TestDictAccessor accessor = new();

        ExprNode leftExpr = new PropertyExpr(["Model", "Value"]);
        ExprNode rightExpr = new LiteralExpr<bool>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData("abc", "abc", "==", "OK")]
    [InlineData("abc", "abc", "!=", "")]
    [InlineData("abc", "def", "==", "")]
    [InlineData("abc", "def", "!=", "OK")]
    public void Renderer_Generic_Should_Compare_String_Values(string left, string right, string op, string expected)
    {
        TestDictModel model = TestDictModel.With(("Value", left));
        TestDictAccessor accessor = new();

        ExprNode leftExpr = new PropertyExpr(["Model", "Value"]);
        ExprNode rightExpr = new LiteralExpr<string>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, model, accessor);

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData("abc", "3", "==")]
    [InlineData("abc", "1.5", "==")]
    [InlineData("true", "1", "==")]
    [InlineData("true", "abc", "==")]
    [InlineData("1", "true", "==")]
    public void Renderer_Generic_Should_Throw_On_Comparison_With_Incompatible_Types(string left, string right,
        string op)
    {
        ExprNode leftExpr =
            int.TryParse(left, out var li)
                ? new LiteralExpr<int>(li)
                : double.TryParse(left, NumberStyles.Float, CultureInfo.InvariantCulture, out var ld)
                    ? new LiteralExpr<double>(ld)
                    : bool.TryParse(left, out var lb)
                        ? new LiteralExpr<bool>(lb)
                        : new LiteralExpr<string>(left);
        ExprNode rightExpr =
            int.TryParse(right, out var ri)
                ? new LiteralExpr<int>(ri)
                : double.TryParse(right, NumberStyles.Float, CultureInfo.InvariantCulture, out var rd)
                    ? new LiteralExpr<double>(rd)
                    : bool.TryParse(right, out var rb)
                        ? new LiteralExpr<bool>(rb)
                        : new LiteralExpr<string>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([new TextNode("err")])
            )
        ]);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            Renderer.Render(ast, TestDictModel.Empty, new TestDictAccessor())
        );

        Assert.Contains("Cannot compare values of types", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("abc", "def", ">")]
    [InlineData("abc", "def", "<")]
    [InlineData("true", "false", ">")]
    [InlineData("false", "true", "<=")]
    public void Renderer_Generic_Should_Throw_On_Unsupported_Operator_For_Type(string left, string right, string op)
    {
        ExprNode leftExpr =
            bool.TryParse(left, out var lb) ? new LiteralExpr<bool>(lb) : new LiteralExpr<string>(left);

        ExprNode rightExpr =
            bool.TryParse(right, out var rb) ? new LiteralExpr<bool>(rb) : new LiteralExpr<string>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([new TextNode("err")])
            )
        ]);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            Renderer.Render(ast, TestDictModel.Empty, new TestDictAccessor())
        );

        Assert.Contains("Only == and != are supported", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("@if (true)\r\n{\r\n- Hello there!\n- General @Model.Name\r\n}")]
    [InlineData("@if (true){\r\n- Hello there!\n- General @Model.Name\r\n}")]
    [InlineData("@if (true)\r\n{- Hello there!\n- General @Model.Name\r\n}")]
    [InlineData("@if (true)\r\n{\r\n- Hello there!\n- General @Model.Name}")]
    [InlineData("@if (true)\n{\n- Hello there!\n- General @Model.Name\n}")]
    [InlineData("@if (true){\n- Hello there!\n- General @Model.Name\n}")]
    [InlineData("@if (true)\n{- Hello there!\n- General @Model.Name\n}")]
    [InlineData("@if (true)\n{\n- Hello there!\n- General @Model.Name}")]
    public void Renderer_Should_Render_If_Block_With_Text_Multiline_And_Interpolation(string template)
    {
        TestDictAccessor accessor = new();
        ITemplate<TestDictModel> compiled = TemplateCompiler.Compile(template, accessor);

        string html = compiled.Render(TestDictModel.With(("Name", "Kenobi")));

        Assert.Equal("""
                     - Hello there!
                     - General Kenobi
                     """, html);
    }

    private sealed record TestDictModel(Dictionary<string, string> Dict)
    {
        public static TestDictModel Empty => new([]);

        public static TestDictModel With((string Key, string Value) pair, params (string Key, string Value)[] pairs)
            => new(pairs.Prepend(pair).ToDictionary());
    }

    private sealed class TestDictAccessor : IValueAccessor<TestDictModel>
    {
        public string? AccessValue(string path, TestDictModel model) => model.Dict.GetValueOrDefault(path);
    }
}