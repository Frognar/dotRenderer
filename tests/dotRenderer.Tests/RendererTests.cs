using System.Globalization;

namespace dotRenderer.Tests;

public class RendererTests
{
    [Fact]
    public void Renderer_Generic_Should_Render_Nested_Path_From_Model()
    {
        UserHolder model = new(new User("Alice"));
        UserHolderAccessor accessor = new();

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
        UserHolder model = new(new User("Alice"));
        UserHolderAccessorReturnsNull accessor = new();

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
        TestModel model = new(true);
        TestModelAccessor accessor = new();

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
        TestModel model = new(false);
        TestModelAccessor accessor = new();

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
        TestModel model = new(true);
        TestModelAccessor accessor = new();

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
        TestModel model = new(false);
        TestModelAccessor accessor = new();

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
        Dummy model = new();
        DummyAccessor accessor = new();

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
        Dummy model = new();
        DummyAccessor accessor = new();

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
        TestModel model = new(false);
        TestModelAccessor accessor = new();

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
        TestModel model = new(true);
        TestModelAccessor accessor = new();

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
        DoubleFlagModel model = new(true, true);
        DoubleFlagAccessor accessor = new();

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
        DoubleFlagModel model = new(a, b);
        DoubleFlagAccessor accessor = new();

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
        DoubleFlagModel model = new(a, b);
        DoubleFlagAccessor accessor = new();

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
        DoubleFlagModel model = new(false, false);
        DoubleFlagAccessor accessor = new();

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
        CompareIntModel model = new(left);
        CompareIntAccessor accessor = new();

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
        CompareDoubleModel model = new(left);
        CompareDoubleAccessor accessor = new();

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
    public void Renderer_Generic_Should_Compare_Int_And_Double_Values(int left, double right, string op, string expected)
    {
        CompareIntModel model = new(left);
        CompareIntAccessor accessor = new();

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
    public void Renderer_Generic_Should_Compare_Double_And_Int_Values(double left, int right, string op, string expected)
    {
        CompareDoubleModel model = new(left);
        CompareDoubleAccessor accessor = new();

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
        CompareBoolModel model = new(left);
        CompareBoolAccessor accessor = new();

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
        CompareStringModel model = new(left);
        CompareStringAccessor accessor = new();

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
    public void Renderer_Generic_Should_Throw_On_Comparison_With_Incompatible_Types(string left, string right, string op)
    {
        ExprNode leftExpr =
            int.TryParse(left, out var li) ? new LiteralExpr<int>(li) :
                double.TryParse(left, NumberStyles.Float, CultureInfo.InvariantCulture, out var ld) ? new LiteralExpr<double>(ld) :
                bool.TryParse(left, out var lb) ? new LiteralExpr<bool>(lb) :
                new LiteralExpr<string>(left);
        ExprNode rightExpr =
            int.TryParse(right, out var ri) ? new LiteralExpr<int>(ri) :
            double.TryParse(right, NumberStyles.Float, CultureInfo.InvariantCulture, out var rd) ? new LiteralExpr<double>(rd) :
            bool.TryParse(right, out var rb) ? new LiteralExpr<bool>(rb) :
            new LiteralExpr<string>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([ new TextNode("err") ])
            )
        ]);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            Renderer.Render(ast, new Dummy(), new DummyAccessor())
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
            bool.TryParse(left, out var lb) ? new LiteralExpr<bool>(lb) :
                new LiteralExpr<string>(left);

        ExprNode rightExpr =
            bool.TryParse(right, out var rb) ? new LiteralExpr<bool>(rb) :
                new LiteralExpr<string>(right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([ new TextNode("err") ])
            )
        ]);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            Renderer.Render(ast, new Dummy(), new DummyAccessor())
        );

        Assert.Contains("Only == and != are supported", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record CompareDoubleModel(double Value);

    private sealed class CompareDoubleAccessor : IValueAccessor<CompareDoubleModel>
    {
        public string? AccessValue(string path, CompareDoubleModel model)
            => path switch
            {
                "Value" => model.Value.ToString(CultureInfo.InvariantCulture) switch
                {
                    { } s when !s.Contains('.', StringComparison.Ordinal) => s + ".0",
                    { } s => s
                },
                _ => null
            };
    }

    [Fact]
    public void Renderer_Should_Render_If_Block_With_Text_Multiline_And_Interpolation()
    {
        string template = """
                          @if (true)
                          {
                          - Hello there!
                          - General @Model.Name
                          }
                          """;

        TestNameAccessor accessor = new();
        ITemplate<TestName> compiled = TemplateCompiler.Compile(template, accessor);

        string html = compiled.Render(new TestName("Kenobi"));

        Assert.Equal("""
                     - Hello there!
                     - General Kenobi
                     """, html);
    }

    private sealed record TestName(string Name);
    private sealed class TestNameAccessor : IValueAccessor<TestName>
    {
        public string? AccessValue(string path, TestName model)
            => path switch
            {
                "Name" => model.Name,
                _ => null
            };
    }

    private sealed record CompareIntModel(int Value);

    private sealed class CompareIntAccessor : IValueAccessor<CompareIntModel>
    {
        public string? AccessValue(string path, CompareIntModel model)
            => path switch
            {
                "Value" => model.Value.ToString(CultureInfo.InvariantCulture),
                _ => null
            };
    }

    private sealed record CompareBoolModel(bool Value);

    private sealed class CompareBoolAccessor : IValueAccessor<CompareBoolModel>
    {
        public string? AccessValue(string path, CompareBoolModel model)
            => path switch
            {
                "Value" => model.Value.ToString(CultureInfo.InvariantCulture),
                _ => null
            };
    }

    private sealed record CompareStringModel(string Value);

    private sealed class CompareStringAccessor : IValueAccessor<CompareStringModel>
    {
        public string? AccessValue(string path, CompareStringModel model)
            => path switch
            {
                "Value" => model.Value,
                _ => null
            };
    }

    private sealed record DoubleFlagModel(bool A, bool B);

    private sealed class DoubleFlagAccessor : IValueAccessor<DoubleFlagModel>
    {
        public string? AccessValue(string path, DoubleFlagModel model)
            => path switch
            {
                "A" => model.A.ToString(),
                "B" => model.B.ToString(),
                _ => null
            };
    }

    private sealed record Dummy;

    private sealed class DummyAccessor : IValueAccessor<Dummy>
    {
        public string? AccessValue(string path, Dummy model) => null;
    }

    private sealed record TestModel(bool Flag);

    private sealed class TestModelAccessor : IValueAccessor<TestModel>
    {
        public string? AccessValue(string path, TestModel model)
            => path switch
            {
                "Flag" => model.Flag.ToString(),
                "OtherFlag" => "yes",
                _ => null
            };
    }

    private sealed record UserHolder(User User);

    private sealed record User(string Name);

    private sealed class UserHolderAccessor : IValueAccessor<UserHolder>
    {
        public string? AccessValue(string path, UserHolder model)
            => path == "User.Name" && model.User is { Name: var name } ? name : null;
    }

    private sealed class UserHolderAccessorReturnsNull : IValueAccessor<UserHolder>
    {
        public string? AccessValue(string path, UserHolder model) => null;
    }
}