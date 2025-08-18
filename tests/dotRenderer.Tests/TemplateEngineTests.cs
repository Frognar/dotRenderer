using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineTests
{
    [Fact]
    public void Should_Return_Plain_Text_Unchanged() =>
        TemplateEngineAssert.Render(
            "Plain text only.",
            MapAccessor.Empty,
            "Plain text only.");

    [Fact]
    public void Should_Render_Template_With_Interpolated_Identifier() =>
        TemplateEngineAssert.Render(
            "Hello @name!",
            MapAccessor.With(("name", Value.FromString("Alice"))),
            "Hello Alice!");

    [Fact]
    public void Should_Render_AtExpr_Number_Addition() =>
        TemplateEngineAssert.Render(
            "Result: @(1+2)!",
            MapAccessor.Empty,
            "Result: 3!");

    [Fact]
    public void Should_Render_AtExpr_String_Concatenation() =>
        TemplateEngineAssert.Render(
            "Hello @(\"A\" + \"B\")!",
            MapAccessor.Empty,
            "Hello AB!");

    [Theory]
    [InlineData("Result: @(1 + 2 * (3 - 1))", "Result: 5")]
    [InlineData("Result: @(-1 + 2 * (3 - 1))", "Result: 3")]
    [InlineData("Result: @(2 / (3 - 1))", "Result: 1")]
    [InlineData("Result: @(2 % 2)", "Result: 0")]
    public void Should_Render_Add_Multiply_With_Grouping_Precedence(string template, string expected) =>
        TemplateEngineAssert.Render(
            template,
            MapAccessor.Empty,
            expected);

    [Theory]
    [InlineData("A@if(true){T}else{E}B", "ATB")]
    [InlineData("A@if(false){T}else{E}B", "AEB")]
    public void Should_Render_If_Else_Using_TemplateEngine(string template, string expected) =>
        TemplateEngineAssert.Render(
            template,
            MapAccessor.Empty,
            expected);

    [Fact]
    public void Should_Render_Then_When_Equality_Is_True() =>
        TemplateEngineAssert.Render(
            "A@if(1==1){T}else{E}B",
            MapAccessor.Empty,
            "ATB");

    [Theory]
    [InlineData("A@if(2>1){T}else{E}B", "ATB")]
    [InlineData("A@if(1>2){T}else{E}B", "AEB")]
    [InlineData("A@if(2>=1){T}else{E}B", "ATB")]
    [InlineData("A@if(1>=2){T}else{E}B", "AEB")]
    [InlineData("A@if(1<2){T}else{E}B", "ATB")]
    [InlineData("A@if(2<1){T}else{E}B", "AEB")]
    [InlineData("A@if(1<=2){T}else{E}B", "ATB")]
    [InlineData("A@if(2<=1){T}else{E}B", "AEB")]
    public void Should_Render_Correct_Block_For_Comparison_Operators(string template, string expected) =>
        TemplateEngineAssert.Render(
            template,
            MapAccessor.Empty,
            expected);

    [Theory]
    [InlineData("A@if(2>1 && !false || false){T}else{E}B", "ATB")]
    [InlineData("A@if(0>1 && !false || false){T}else{E}B", "AEB")]
    [InlineData("A@if(0>1 && !true || false){T}else{E}B", "AEB")]
    [InlineData("A@if(2>1 && !true || false){T}else{E}B", "AEB")]
    [InlineData("A@if(2>1 && !false || true){T}else{E}B", "ATB")]
    [InlineData("A@if(0>1 && !false || true){T}else{E}B", "ATB")]
    [InlineData("A@if(0>1 && !true || true){T}else{E}B", "ATB")]
    [InlineData("A@if(2>1 && !true || true){T}else{E}B", "ATB")]
    public void Should_Render_Correct_Block_For_And_Or_And_Not_Operators(string template, string expected) =>
        TemplateEngineAssert.Render(
            template,
            MapAccessor.Empty,
            expected);

    [Theory]
    [InlineData("A@if((false || true) && true){T}else{E}B", "ATB")]
    [InlineData("A@if((true || true) && true){T}else{E}B", "ATB")]
    [InlineData("A@if((true || false) && true){T}else{E}B", "ATB")]
    [InlineData("A@if((false || false) && true){T}else{E}B", "AEB")]
    public void Should_Render_Then_When_Grouped_Condition_Is_True(string template, string expected) =>
        TemplateEngineAssert.Render(
            template,
            MapAccessor.Empty,
            expected);

    [Fact]
    public void Should_Render_For_Loop_In_TemplateEngine() =>
        TemplateEngineAssert.Render(
            "X@for(item in items){@item}Y",
            MapAccessor.With(
                ("items", Value.FromSequence(
                    Value.FromString("a"),
                    Value.FromString("b")
                ))
            ),
            "XabY");

    [Fact]
    public void Should_Render_For_Loop_With_DotAccess() =>
        TemplateEngineAssert.Render(
            "X@for(u in users){@(u.name)}Y",
            MapAccessor.With(
                ("users", Value.FromSequence(
                    Value.FromMap(new Dictionary<string, Value>
                    {
                        ["name"] = Value.FromString("a")
                    }),
                    Value.FromMap(new Dictionary<string, Value>
                    {
                        ["name"] = Value.FromString("b")
                    })
                ))
            ),
            "XabY");

    [Fact]
    public void Should_Render_For_Loop_With_Index_In_TemplateEngine() =>
        TemplateEngineAssert.Render(
            "X@for(item, i in items){@i:@item;}Y",
            MapAccessor.With(
                ("items", Value.FromSequence(
                    Value.FromString("a"),
                    Value.FromString("b")
                ))
            ),
            "X0:a;1:b;Y");

    [Fact]
    public void Should_Render_Else_For_Empty_Sequence() =>
        TemplateEngineAssert.Render(
            "X@for(item in items){@item}else{E}Y",
            MapAccessor.With(("items", Value.FromSequence())),
            "XEY");

    [Fact]
    public void Should_Render_Body_For_NonEmpty_Sequence() =>
        TemplateEngineAssert.Render(
            "X@for(item in items){@item}else{E}Y",
            MapAccessor.With(
                ("items", Value.FromSequence(
                    Value.FromString("a"),
                    Value.FromString("b")
                ))),
            "XabY");

    [Fact]
    public void Should_Render_For_Loop_With_Nested_DotAccess() =>
        TemplateEngineAssert.Render(
            "X@for(u in users){@(u.address.city)}Y",
            MapAccessor.With(
                ("users", Value.FromSequence(
                    Value.FromMap(new Dictionary<string, Value>
                    {
                        ["address"] = Value.FromMap(new Dictionary<string, Value>
                        {
                            ["city"] = Value.FromString("a")
                        })
                    }),
                    Value.FromMap(new Dictionary<string, Value>
                    {
                        ["address"] = Value.FromMap(new Dictionary<string, Value>
                        {
                            ["city"] = Value.FromString("b")
                        })
                    })
                ))),
            "XabY");

    [Fact]
    public void Should_Render_For_With_Loop_Index_And_Item() =>
        TemplateEngineAssert.Render(
            "X@for(item in items){@(loop.index):@item;}Y",
            MapAccessor.With(("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b"),
                Value.FromString("c")
            ))),
            "X0:a;1:b;2:c;Y");

    [Fact]
    public void Should_Render_For_With_Loop_First_And_Last_Flags() =>
        TemplateEngineAssert.Render(
            "X@for(item in items){@if(loop.isFirst){^}@if(loop.isLast){$}@item}Y",
            MapAccessor.With(("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b"),
                Value.FromString("c")
            ))),
            "X^ab$cY");

    [Fact]
    public void Should_Render_For_With_Loop_Even_Flags() =>
        TemplateEngineAssert.Render(
            "X@for(item in items){@if(loop.isEven){E}else{O}}Y",
            MapAccessor.With(("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b"),
                Value.FromString("c")
            ))),
            "XEOEY");

    [Fact]
    public void Should_Render_For_With_Loop_Odd_Flags() =>
        TemplateEngineAssert.Render(
            "X@for(item in items){@if(loop.isOdd){O}else{E}}Y",
            MapAccessor.With(("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b"),
                Value.FromString("c")
            ))),
            "XEOEY");

    [Fact]
    public void Should_Trim_One_Outer_Newline_Inside_If_Block() =>
        TemplateEngineAssert.Render(
            """
            @if(true){
            <style>
            </style>
            }else{}
            """,
            MapAccessor.Empty,
            """
            <style>
            </style>
            """);
    
    [Fact]
    public void Should_Allow_LBrace_On_Next_Line_After_If() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @if(true)
            {abc}
            xyz
            """,
            MapAccessor.Empty,
            """
            xyz
            abc
            xyz
            """);

    [Fact]
    public void Should_Allow_LBrace_On_Next_Line_After_For() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @for(item in items)
            {@item}
            xyz
            """,
            MapAccessor.With(("items", Value.FromSequence(Value.FromNumber(1)))),
            """
            xyz
            1
            xyz
            """);

    [Fact]
    public void Should_Report_Error_When_Division_By_Zero() =>
        TemplateEngineAssert.FailsToRender(
            "Result: @(1 / 0)",
            MapAccessor.Empty,
            "DivisionByZero",
            TextSpan.At(8, 8));

    [Fact]
    public void Should_Report_Error_Even_When_Left_ShortCircuits() =>
        TemplateEngineAssert.FailsToRender(
            "A@if(false && 1 + true > 0){T}else{E}B",
            MapAccessor.Empty,
            "TypeMismatch",
            TextSpan.At(1, 26), // spans "@if(false && 1 + true > 0)"
            "Operator '+' expects numbers.");

    [Fact]
    public void Should_Fail_For_When_Expression_Is_Not_A_Sequence() =>
        TemplateEngineAssert.FailsToRender(
            "X@for(item in num){@item}Y",
            MapAccessor.With(
                ("num", Value.FromNumber(123))
            ),
            "TypeMismatch",
            TextSpan.At(1, 17), // spans "@for(item in num)"
            "Expression of @for must evaluate to a sequence, but got Number.");
}