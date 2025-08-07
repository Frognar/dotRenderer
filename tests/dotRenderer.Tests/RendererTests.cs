namespace dotRenderer.Tests;

public class RendererTests
{
    [Fact]
    public void Renderer_Should_Render_Text_And_Eval_From_Model()
    {
        SequenceNode ast = new([
            new TextNode("<h1>Hello, "),
            new EvalNode(["Model", "Name"]),
            new TextNode("!</h1>")
        ]);

        Dictionary<string, object> model = new()
        {
            { "Name", "Alice" }
        };

        string html = Renderer.Render(ast, model);

        Assert.Equal("<h1>Hello, Alice!</h1>", html);
    }

    [Fact]
    public void Renderer_Should_Render_Nested_Path_From_Model()
    {
        Dictionary<string, string> user = new()
        {
            { "Name", "Bob" }
        };
        Dictionary<string, object> model = new()
        {
            { "User", user }
        };

        SequenceNode ast = new([
            new TextNode("Hello, "),
            new EvalNode(["Model", "User", "Name"]),
            new TextNode("!")
        ]);

        string html = Renderer.Render(ast, model);

        Assert.Equal("Hello, Bob!", html);
    }

    [Fact]
    public void Renderer_Should_Throw_When_Nested_Path_Missing()
    {
        Dictionary<string, object> model = [];

        SequenceNode ast = new([
            new TextNode("Hello, "),
            new EvalNode(["Model", "User", "Name"]),
            new TextNode("!")
        ]);

        KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => Renderer.Render(ast, model));
        Assert.Contains("User", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Should_Throw_When_Leaf_Dictionary_String_Missing_Key()
    {
        Dictionary<string, object> model = new()
        {
            { "User", new Dictionary<string, string>() }
        };

        SequenceNode ast = new([
            new EvalNode(["Model", "User", "Name"])
        ]);

        KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => Renderer.Render(ast, model));
        Assert.Contains("Name", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Should_Throw_When_Model_Is_Not_Dictionary()
    {
        Dictionary<string, object> model = new()
        {
            { "User", new object() }
        };

        SequenceNode ast = new([
            new EvalNode(["Model", "User", "Name"])
        ]);

        KeyNotFoundException ex = Assert.Throws<KeyNotFoundException>(() => Renderer.Render(ast, model));
        Assert.Contains("Name", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Should_Render_IfNode_With_True_Literal()
    {
        SequenceNode ast = new([
            new TextNode("A"),
            new IfNode(new LiteralExpr<bool>(true),
                new SequenceNode([new TextNode("Yes")])
            ),
            new TextNode("B")
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object>());
        Assert.Equal("AYesB", html);
    }

    [Fact]
    public void Renderer_Should_Not_Render_IfNode_With_False_Literal()
    {
        SequenceNode ast = new([
            new TextNode("A"),
            new IfNode(new LiteralExpr<bool>(false),
                new SequenceNode([new TextNode("No")])
            ),
            new TextNode("B")
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object>());
        Assert.Equal("AB", html);
    }

    [Fact]
    public void Renderer_Should_Render_IfNode_With_PropertyExpr_Bool_True()
    {
        SequenceNode ast = new([
            new TextNode("X"),
            new IfNode(
                new PropertyExpr(["Model", "Flag"]),
                new SequenceNode([new TextNode("ON")])
            ),
            new TextNode("Y")
        ]);
        Dictionary<string, object> model = new() { { "Flag", true } };

        string html = Renderer.Render(ast, model);
        Assert.Equal("XONY", html);
    }

    [Fact]
    public void Renderer_Should_Not_Render_IfNode_With_PropertyExpr_Bool_False()
    {
        SequenceNode ast = new([
            new TextNode("X"),
            new IfNode(
                new PropertyExpr(["Model", "Flag"]),
                new SequenceNode([new TextNode("OFF")])
            ),
            new TextNode("Y")
        ]);
        Dictionary<string, object> model = new() { { "Flag", false } };

        string html = Renderer.Render(ast, model);
        Assert.Equal("XY", html);
    }

    [Fact]
    public void Renderer_Should_Throw_If_PropertyExpr_Value_Is_String()
    {
        SequenceNode ast = new([
            new IfNode(
                new PropertyExpr(["Model", "Flag"]),
                new SequenceNode([new TextNode("ON")])
            )
        ]);
        Dictionary<string, object> model = new() { { "Flag", "true" } };

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => Renderer.Render(ast, model));
        Assert.Contains("bool", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Flag", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Should_Throw_If_PropertyExpr_Value_Is_Int()
    {
        SequenceNode ast = new([
            new IfNode(
                new PropertyExpr(["Model", "Flag"]),
                new SequenceNode([new TextNode("ON")])
            )
        ]);
        Dictionary<string, object> model = new() { { "Flag", 1 } };

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => Renderer.Render(ast, model));
        Assert.Contains("bool", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Flag", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Should_Throw_If_PropertyExpr_Value_Is_Null()
    {
        SequenceNode ast = new([
            new IfNode(
                new PropertyExpr(["Model", "Flag"]),
                new SequenceNode([new TextNode("ON")])
            )
        ]);

        Dictionary<string, object> model = new() { { "Flag", null! } };

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => Renderer.Render(ast, model));
        Assert.Contains("bool", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Flag", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Should_Render_IfNode_With_Binary_And_Condition()
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    "&&",
                    new PropertyExpr(["Model", "A"]),
                    new PropertyExpr(["Model", "B"])
                ),
                new SequenceNode([new TextNode("T")])
            )
        ]);

        Dictionary<string, object> model = new()
        {
            { "A", true },
            { "B", true }
        };

        string html = Renderer.Render(ast, model);

        Assert.Equal("T", html);
    }

    [Fact]
    public void Renderer_Should_Not_Render_IfNode_With_Binary_And_Condition_False()
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    "&&",
                    new PropertyExpr(["Model", "A"]),
                    new PropertyExpr(["Model", "B"])
                ),
                new SequenceNode([new TextNode("T")])
            )
        ]);

        Dictionary<string, object> model = new()
        {
            { "A", true },
            { "B", false }
        };

        string html = Renderer.Render(ast, model);

        Assert.Equal("", html);
    }

    [Fact]
    public void Renderer_Should_Render_IfNode_With_Binary_Or_Condition()
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    "||",
                    new PropertyExpr(["Model", "A"]),
                    new PropertyExpr(["Model", "B"])
                ),
                new SequenceNode([new TextNode("T")])
            )
        ]);

        Dictionary<string, object> model = new()
        {
            { "A", false },
            { "B", true }
        };

        string html = Renderer.Render(ast, model);

        Assert.Equal("T", html);
    }

    [Fact]
    public void Renderer_Should_Render_IfNode_With_Unary_Not_Condition()
    {
        SequenceNode ast = new([
            new IfNode(
                new UnaryExpr(
                    "!",
                    new PropertyExpr(["Model", "IsGuest"])
                ),
                new SequenceNode([new TextNode("Hi!")])
            )
        ]);

        Dictionary<string, object> model = new()
        {
            { "IsGuest", false }
        };

        string html = Renderer.Render(ast, model);

        Assert.Equal("Hi!", html);
    }

    [Fact]
    public void Renderer_Should_Not_Render_IfNode_With_Unary_Not_Condition_False()
    {
        SequenceNode ast = new([
            new IfNode(
                new UnaryExpr(
                    "!",
                    new PropertyExpr(["Model", "IsGuest"])
                ),
                new SequenceNode([new TextNode("Hi!")])
            )
        ]);

        Dictionary<string, object> model = new()
        {
            { "IsGuest", true }
        };

        string html = Renderer.Render(ast, model);

        Assert.Equal("", html);
    }

    [Theory]
    [InlineData(5, 5, "==", "OK")]
    [InlineData(5, 3, "==", "")]
    [InlineData(5, 3, "!=", "OK")]
    [InlineData(5, 5, "!=", "")]
    [InlineData(3, 5, "<", "OK")]
    [InlineData(5, 3, "<", "")]
    [InlineData(3, 3, "<=", "OK")]
    [InlineData(5, 3, "<=", "")]
    [InlineData(5, 3, ">", "OK")]
    [InlineData(3, 5, ">", "")]
    [InlineData(3, 3, ">=", "OK")]
    [InlineData(3, 5, ">=", "")]
    public void Renderer_Should_Compare_Ints(int left, int right, string op, string expected)
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, new LiteralExpr<int>(left), new LiteralExpr<int>(right)),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object>());

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData(5.0, 5.0, "==", "OK")]
    [InlineData(5.1, 5.0, "==", "")]
    [InlineData(2.0, 3.5, "!=", "OK")]
    [InlineData(3.5, 3.5, "!=", "")]
    [InlineData(2.0, 3.5, "<", "OK")]
    [InlineData(4.0, 2.0, "<", "")]
    [InlineData(3.0, 3.0, "<=", "OK")]
    [InlineData(4.0, 3.0, "<=", "")]
    [InlineData(4.0, 3.0, ">", "OK")]
    [InlineData(2.0, 3.0, ">", "")]
    [InlineData(3.0, 3.0, ">=", "OK")]
    [InlineData(2.0, 3.0, ">=", "")]
    public void Renderer_Should_Compare_Doubles(double left, double right, string op, string expected)
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, new LiteralExpr<double>(left), new LiteralExpr<double>(right)),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object>());

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData("abc", "abc", "==", "OK")]
    [InlineData("abc", "def", "==", "")]
    [InlineData("abc", "def", "!=", "OK")]
    [InlineData("abc", "abc", "!=", "")]
    public void Renderer_Should_Compare_Strings(string left, string right, string op, string expected)
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, new LiteralExpr<string>(left), new LiteralExpr<string>(right)),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object>());

        Assert.Equal(expected, html);
    }

    [Fact]
    public void Renderer_Should_Compare_Properties()
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr("==", new PropertyExpr(["Model", "Name"]), new LiteralExpr<string>("Alice")),
                new SequenceNode([new TextNode("Alice")])
            )
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object> { { "Name", "Alice" } });

        Assert.Equal("Alice", html);
    }

    [Theory]
    [InlineData(true, true, "==", "OK")]
    [InlineData(true, false, "==", "")]
    [InlineData(true, false, "!=", "OK")]
    [InlineData(false, false, "!=", "")]
    public void Renderer_Should_Compare_Bools(bool left, bool right, string op, string expected)
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, new LiteralExpr<bool>(left), new LiteralExpr<bool>(right)),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object>());

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData(2, 2.0, "==", "OK")]
    [InlineData(2, 3.1, "==", "")]
    [InlineData(2.0, 2, "==", "OK")]
    [InlineData(2.0, 4, "!=", "OK")]
    [InlineData(2.0, 4, ">", "")]
    public void Renderer_Should_Compare_Mixed_Int_Double(object left, object right, string op, string expected)
    {
        ExprNode leftExpr = left is int li ? new LiteralExpr<int>(li) : new LiteralExpr<double>((double)left);
        ExprNode rightExpr = right is int ri ? new LiteralExpr<int>(ri) : new LiteralExpr<double>((double)right);

        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(op, leftExpr, rightExpr),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        string html = Renderer.Render(ast, new Dictionary<string, object>());

        Assert.Equal(expected, html);
    }

    [Theory]
    [InlineData("<", typeof(string), typeof(string), "Only == and != are supported for string")]
    [InlineData(">", typeof(bool), typeof(bool), "Only == and != are supported for bool")]
    [InlineData("==", typeof(int), typeof(bool), "Cannot compare values of types")]
    [InlineData("!=", typeof(int), typeof(string), "Cannot compare values of types")]
    [InlineData("<=", typeof(string), typeof(int), "Cannot compare values of types")]
    public void Renderer_Should_Throw_On_Comparison_With_Incompatible_Types(string op, Type leftType, Type rightType,
        string expectedMsg)
    {
        ExprNode left = leftType == typeof(int)
            ? new LiteralExpr<int>(1)
            : leftType == typeof(bool)
                ? new LiteralExpr<bool>(true)
                : new LiteralExpr<string>("abc");

        ExprNode right = rightType == typeof(int)
            ? new LiteralExpr<int>(1)
            : rightType == typeof(bool)
                ? new LiteralExpr<bool>(true)
                : new LiteralExpr<string>("def");

        SequenceNode ast = new([
            new IfNode(new BinaryExpr(op, left, right), new SequenceNode([new TextNode("err")]))
        ]);

        InvalidOperationException ex =
            Assert.Throws<InvalidOperationException>(() => Renderer.Render(ast, new Dictionary<string, object>()));
        Assert.Contains(expectedMsg, ex.Message, StringComparison.Ordinal);
    }

    private sealed record DummyExpr : ExprNode;

    [Fact]
    public void Renderer_Should_Throw_On_Unsupported_Expression_Node()
    {
        SequenceNode ast = new([
            new IfNode(new DummyExpr(), new SequenceNode([new TextNode("err")]))
        ]);

        InvalidOperationException ex =
            Assert.Throws<InvalidOperationException>(() => Renderer.Render(ast, new Dictionary<string, object>()));
        Assert.Contains("Unsupported expression node", ex.Message, StringComparison.Ordinal);
        Assert.Contains("DummyExpr", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Renderer_Should_Throw_On_Unsupported_Operand_Type()
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr("==", new DummyExpr(), new DummyExpr()),
                new SequenceNode([new TextNode("err")]))
        ]);

        InvalidOperationException ex =
            Assert.Throws<InvalidOperationException>(() => Renderer.Render(ast, new Dictionary<string, object>()));
        Assert.Contains("Unsupported operand type", ex.Message, StringComparison.Ordinal);
        Assert.Contains("DummyExpr", ex.Message, StringComparison.Ordinal);
    }

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