using System.Globalization;

namespace dotRenderer.Tests;

public class RendererTests
{
    [Fact]
    public void Renderer_Generic_Should_Render_Nested_Path_From_Model()
    {
        RendererAssert.Renders(
            new SequenceNode([
                new TextNode("Hello "),
                new EvalNode(["Model", "User", "Name"]),
                new TextNode("!")
            ]),
            TestDictModel.With(("User.Name", "Alice")),
            "Hello Alice!");
    }

    [Fact]
    public void Renderer_Generic_Should_Render_IfNode_With_Flag_True()
    {
        RendererAssert.Renders(
            new SequenceNode([
                new TextNode("X"),
                new IfNode(
                    new PropertyExpr(["Model", "Flag"]),
                    new SequenceNode([new TextNode("YES")])
                ),
                new TextNode("Y")
            ]),
            TestDictModel.With(("Flag", "true")),
            "XYESY");
    }

    [Fact]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Flag_False()
    {
        RendererAssert.Renders(
            new SequenceNode([
                new TextNode("X"),
                new IfNode(
                    new PropertyExpr(["Model", "Flag"]),
                    new SequenceNode([new TextNode("YES")])
                ),
                new TextNode("Y")
            ]),
            TestDictModel.With(("Flag", "false")),
            "XY");
    }

    [Fact]
    public void Renderer_Generic_Should_Render_IfNode_With_Literal_True()
    {
        RendererAssert.Renders(
            new SequenceNode([
                new TextNode("A"),
                new IfNode(
                    new LiteralExpr<bool>(true),
                    new SequenceNode([new TextNode("YES")])
                ),
                new TextNode("B")
            ]),
            TestDictModel.Empty,
            "AYESB");
    }

    [Fact]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Literal_False()
    {
        RendererAssert.Renders(
            new SequenceNode([
                new TextNode("A"),
                new IfNode(
                    new LiteralExpr<bool>(false),
                    new SequenceNode([new TextNode("NO")])
                ),
                new TextNode("B")
            ]),
            TestDictModel.Empty,
            "AB");
    }

    [Fact]
    public void Renderer_Generic_Should_Render_IfNode_With_Unary_Not_Condition_True()
    {
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new UnaryExpr("!", new PropertyExpr(["Model", "Flag"])),
                    new SequenceNode([new TextNode("OK")])
                )
            ]),
            TestDictModel.With(("Flag", "false")),
            "OK");
    }

    [Fact]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Unary_Not_Condition_False()
    {
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new UnaryExpr("!", new PropertyExpr(["Model", "Flag"])),
                    new SequenceNode([new TextNode("NOPE")])
                )
            ]),
            TestDictModel.With(("Flag", "true")),
            "");
    }

    [Fact]
    public void Renderer_Generic_Should_Render_IfNode_With_Binary_And_Condition_True()
    {
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        "&&",
                        new PropertyExpr(["Model", "A"]),
                        new PropertyExpr(["Model", "B"])
                    ),
                    new SequenceNode([new TextNode("YES")])
                )
            ]),
            TestDictModel.With(("A", "true"), ("B", "true")),
            "YES");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Binary_And_Condition_False(bool a, bool b)
    {
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        "&&",
                        new PropertyExpr(["Model", "A"]),
                        new PropertyExpr(["Model", "B"])
                    ),
                    new SequenceNode([new TextNode("NO")])
                )
            ]),
            TestDictModel.With(("A", a.ToString()), ("B", b.ToString())),
            "");
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Renderer_Generic_Should_Render_IfNode_With_Binary_Or_Condition_True(bool a, bool b)
    {
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        "||",
                        new PropertyExpr(["Model", "A"]),
                        new PropertyExpr(["Model", "B"])
                    ),
                    new SequenceNode([new TextNode("YES")])
                )
            ]),
            TestDictModel.With(("A", a.ToString()), ("B", b.ToString())),
            "YES");
    }

    [Fact]
    public void Renderer_Generic_Should_Not_Render_IfNode_With_Binary_Or_Condition_False()
    {
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        "||",
                        new PropertyExpr(["Model", "A"]),
                        new PropertyExpr(["Model", "B"])
                    ),
                    new SequenceNode([new TextNode("NO")])
                )
            ]),
            TestDictModel.With(("A", "false"), ("B", "false")),
            "");
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
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        op,
                        new PropertyExpr(["Model", "Value"]),
                        new LiteralExpr<int>(right)),
                    new SequenceNode([new TextNode("OK")])
                )
            ]),
            TestDictModel.With(("Value", left.ToString(CultureInfo.InvariantCulture))),
            expected);
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
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        op,
                        new PropertyExpr(["Model", "Value"]),
                        new LiteralExpr<double>(right)),
                    new SequenceNode([new TextNode("OK")])
                )
            ]),
            TestDictModel.With(("Value", left.ToString(CultureInfo.InvariantCulture))),
            expected);
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
    public void Renderer_Generic_Should_Compare_Int_And_Double_Values(
        int left,
        double right,
        string op,
        string expected)
    {
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        op,
                        new PropertyExpr(["Model", "Value"]),
                        new LiteralExpr<double>(right)),
                    new SequenceNode([new TextNode("OK")])
                )
            ]),
            TestDictModel.With(("Value", left.ToString(CultureInfo.InvariantCulture))),
            expected);
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
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        op,
                        new PropertyExpr(["Model", "Value"]),
                        new LiteralExpr<int>(right)),
                    new SequenceNode([new TextNode("OK")])
                )
            ]),
            TestDictModel.With(("Value", left.ToString(CultureInfo.InvariantCulture))),
            expected);
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
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        op,
                        new PropertyExpr(["Model", "Value"]),
                        new LiteralExpr<bool>(right)),
                    new SequenceNode([new TextNode("OK")])
                )
            ]),
            TestDictModel.With(("Value", left.ToString())),
            expected);
    }

    [Theory]
    [InlineData("abc", "abc", "==", "OK")]
    [InlineData("abc", "abc", "!=", "")]
    [InlineData("abc", "def", "==", "")]
    [InlineData("abc", "def", "!=", "OK")]
    public void Renderer_Generic_Should_Compare_String_Values(string left, string right, string op, string expected)
    {
        RendererAssert.Renders(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(
                        op,
                        new PropertyExpr(["Model", "Value"]),
                        new LiteralExpr<string>(right)),
                    new SequenceNode([new TextNode("OK")])
                )
            ]),
            TestDictModel.With(("Value", left)),
            expected);
    }

    [Theory]
    [InlineData("""
                @if (true)
                {
                - Hello there!
                - General @Model.Name
                }
                """)]
    [InlineData("""
                @if (true){
                - Hello there!
                - General @Model.Name
                }
                """)]
    [InlineData("""
                @if (true)
                {- Hello there!
                - General @Model.Name
                }
                """)]
    [InlineData("""
                @if (true)
                {
                - Hello there!
                - General @Model.Name}
                """)]
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

    [Fact]
    public void Renderer_Generic_Should_Throw_When_Nested_Path_Missing()
    {
        RendererAssert.Throws<KeyNotFoundException>(
            new SequenceNode([
                new TextNode("Hello "),
                new EvalNode(["Model", "User", "Name"]),
                new TextNode("!")
            ]),
            TestDictModel.With(("User", "Alice")),
            "User.Name");
    }

    [Fact]
    public void Renderer_Generic_Should_Throw_If_Accessor_Returns_NonBoolean_String()
    {
        RendererAssert.Throws<InvalidOperationException>(
            new SequenceNode([
                new IfNode(
                    new PropertyExpr(["Model", "OtherFlag"]),
                    new SequenceNode([new TextNode("YES")])
                )
            ]),
            TestDictModel.With(("OtherFlag", "yes")),
            "must resolve to \"true\" or \"false\"",
            "yes");
    }

    [Fact]
    public void Renderer_Generic_Should_Throw_If_Accessor_Returns_Null_For_IfNode()
    {
        RendererAssert.Throws<InvalidOperationException>(
            new SequenceNode([
                new IfNode(
                    new PropertyExpr(["Model", "YetAnotherFlag"]),
                    new SequenceNode([new TextNode("YES")])
                )
            ]),
            TestDictModel.Empty,
            "returned null",
            "Flag",
            "true",
            "false");
    }

    [Theory]
    [InlineData("abc", "3", "==")]
    [InlineData("abc", "1.5", "==")]
    [InlineData("true", "1", "==")]
    [InlineData("true", "abc", "==")]
    [InlineData("1", "true", "==")]
    public void Renderer_Generic_Should_Throw_On_Comparison_With_Incompatible_Types(
        string left,
        string right,
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

        RendererAssert.Throws<InvalidOperationException>(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(op, leftExpr, rightExpr),
                    new SequenceNode([new TextNode("err")])
                )
            ]),
            TestDictModel.Empty,
            "Cannot compare values of types");
    }

    [Theory]
    [InlineData("abc", "def", ">")]
    [InlineData("abc", "def", "<")]
    [InlineData("true", "false", ">")]
    [InlineData("false", "true", "<=")]
    public void Renderer_Generic_Should_Throw_On_Unsupported_Operator_For_Type(string left, string right, string op)
    {
        ExprNode leftExpr =
            bool.TryParse(left, out var lb)
                ? new LiteralExpr<bool>(lb)
                : new LiteralExpr<string>(left);

        ExprNode rightExpr =
            bool.TryParse(right, out var rb)
                ? new LiteralExpr<bool>(rb)
                : new LiteralExpr<string>(right);

        RendererAssert.Throws<InvalidOperationException>(
            new SequenceNode([
                new IfNode(
                    new BinaryExpr(op, leftExpr, rightExpr),
                    new SequenceNode([new TextNode("err")])
                )
            ]),
            TestDictModel.Empty,
            "Only == and != are supported");
    }

    [Fact]
    public void Renderer_Generic_Should_Evaluate_Arithmetic_Addition_In_If_Condition()
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    ">",
                    new BinaryExpr(
                        "+",
                        new PropertyExpr(["Model", "Age"]),
                        new LiteralExpr<int>(1)),
                    new LiteralExpr<int>(18)),
                new SequenceNode([new TextNode("YES")])
            )
        ]);

        RendererAssert.Renders(ast, TestDictModel.With(("Age", "18")), "YES");
        RendererAssert.Renders(ast, TestDictModel.With(("Age", "17")), "");
    }

    [Fact]
    public void Renderer_Generic_Should_Evaluate_Arithmetic_UnaryMinus_In_If_Condition()
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    "<",
                    new UnaryExpr("-", new PropertyExpr(["Model", "Debt"])),
                    new LiteralExpr<int>(0)),
                new SequenceNode([new TextNode("OK")])
            )
        ]);

        RendererAssert.Renders(ast, TestDictModel.With(("Debt", "5")), "OK");
        RendererAssert.Renders(ast, TestDictModel.With(("Debt", "-5")), "");
    }

    [Fact]
    public void Renderer_Generic_Should_Evaluate_Arithmetic_Complex_Expression_In_If_Condition()
    {
        SequenceNode ast = new([
            new IfNode(
                new BinaryExpr(
                    "<=",
                    new BinaryExpr("+",
                        new BinaryExpr("*",
                            new PropertyExpr(["Model", "X"]),
                            new LiteralExpr<int>(2)),
                        new LiteralExpr<int>(3)),
                    new LiteralExpr<int>(11)), new SequenceNode([new TextNode("OK")]))
        ]);

        RendererAssert.Renders(ast, TestDictModel.With(("X", "4")), "OK");
        RendererAssert.Renders(ast, TestDictModel.With(("X", "5")), "");
    }

    [Fact]
    public void Renderer_Should_Trim_Single_Newlines_Inside_If_Body()
    {
        RendererAssert.Renders(new SequenceNode([
            new IfNode(
                new LiteralExpr<bool>(true),
                new SequenceNode([new TextNode("\nX\n")])
            )
        ]), TestDictModel.Empty, "X");
    }
}