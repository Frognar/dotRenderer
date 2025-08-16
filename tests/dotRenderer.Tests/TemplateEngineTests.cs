using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineTests
{
    [Fact]
    public void Should_Return_Plain_Text_Unchanged()
    {
        // arrange
        const string template = "Plain text only.";

        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(template, result.Value);
    }
    
    [Fact]
    public void Should_Render_Template_With_Interpolated_Identifier()
    {
        // arrange
        const string template = "Hello @name!";

        // act
        Result<string> result = TemplateEngine.Render(template, MapAccessor.With(("name", Value.FromString("Alice"))));

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("Hello Alice!", result.Value);
    }
    
    [Fact]
    public void Should_Render_AtExpr_Number_Addition()
    {
        // arrange
        const string template = "Result: @(1+2)!";

        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("Result: 3!", result.Value);
    }
    
    [Fact]
    public void Should_Render_AtExpr_String_Concatenation()
    {
        const string template = "Hello @(\"A\" + \"B\")!";
        Result<string> result = TemplateEngine.Render(template);
        Assert.True(result.IsOk);
        Assert.Equal("Hello AB!", result.Value);
    }

    [Theory]
    [InlineData("Result: @(1 + 2 * (3 - 1))", "Result: 5")]
    [InlineData("Result: @(-1 + 2 * (3 - 1))", "Result: 3")]
    [InlineData("Result: @(2 / (3 - 1))", "Result: 1")]
    [InlineData("Result: @(2 % 2)", "Result: 0")]
    public void Should_Render_Add_Multiply_With_Grouping_Precedence(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Should_Report_Error_When_Division_By_Zero()
    {
        // arrange
        const string template = "Result: @(1 / 0)";
        
        // act
        Result<string> result = TemplateEngine.Render(template);
        
        // assert
        Assert.False(result.IsOk);
        IError error = result.Error!;
        Assert.Equal("DivisionByZero", error.Code);
    }
    
    [Theory]
    [InlineData("A@if(true){T}else{E}B","ATB")]
    [InlineData("A@if(false){T}else{E}B", "AEB")]
    public void Should_Render_If_Else_Using_TemplateEngine(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Should_Render_Then_When_Equality_Is_True()
    {
        // arrange
        const string template = "A@if(1==1){T}else{E}B";

        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("ATB", result.Value);
    }

    [Theory]
    [InlineData("A@if(2>1){T}else{E}B", "ATB")]
    [InlineData("A@if(1>2){T}else{E}B", "AEB")]
    [InlineData("A@if(2>=1){T}else{E}B", "ATB")]
    [InlineData("A@if(1>=2){T}else{E}B", "AEB")]
    [InlineData("A@if(1<2){T}else{E}B", "ATB")]
    [InlineData("A@if(2<1){T}else{E}B", "AEB")]
    [InlineData("A@if(1<=2){T}else{E}B", "ATB")]
    [InlineData("A@if(2<=1){T}else{E}B", "AEB")]
    public void Should_Render_Correct_Block_For_Comparison_Operators(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("A@if(2>1 && !false || false){T}else{E}B", "ATB")]
    [InlineData("A@if(0>1 && !false || false){T}else{E}B", "AEB")]
    [InlineData("A@if(0>1 && !true || false){T}else{E}B", "AEB")]
    [InlineData("A@if(2>1 && !true || false){T}else{E}B", "AEB")]
    [InlineData("A@if(2>1 && !false || true){T}else{E}B", "ATB")]
    [InlineData("A@if(0>1 && !false || true){T}else{E}B", "ATB")]
    [InlineData("A@if(0>1 && !true || true){T}else{E}B", "ATB")]
    [InlineData("A@if(2>1 && !true || true){T}else{E}B", "ATB")]
    public void Should_Render_Correct_Block_For_And_Or_And_Not_Operators(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);
        
        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Should_Report_Error_Even_When_Left_ShortCircuits()
    {
        // arrange
        const string template = "A@if(false && 1 + true > 0){T}else{E}B";

        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.False(result.IsOk);
        IError error = result.Error!;
        Assert.Equal("TypeMismatch", error.Code);
        Assert.Equal(TextSpan.At(1, 26), error.Range); // spans "@if(false && 1 + true > 0)"
        Assert.Equal("Operator '+' expects numbers.", error.Message);
    }

    [Theory]
    [InlineData("A@if((false || true) && true){T}else{E}B", "ATB")]
    [InlineData("A@if((true || true) && true){T}else{E}B", "ATB")]
    [InlineData("A@if((true || false) && true){T}else{E}B", "ATB")]
    [InlineData("A@if((false || false) && true){T}else{E}B", "AEB")]
    public void Should_Render_Then_When_Grouped_Condition_Is_True(string template, string expected)
    {
        // act
        Result<string> result = TemplateEngine.Render(template);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal(expected, result.Value);
    }
    
    [Fact]
    public void Should_Render_For_Loop_In_TemplateEngine()
    {
        // arrange
        const string template = "X@for(item in items){@item}Y";
        MapAccessor globals = MapAccessor.With(
            ("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b")
            ))
        );

        // act
        Result<string> result = TemplateEngine.Render(template, globals);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("XabY", result.Value);
    }

    [Fact]
    public void Should_Render_For_Loop_With_DotAccess()
    {
        // arrange
        const string template = "X@for(u in users){@(u.name)}Y";

        MapAccessor globals = MapAccessor.With(
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
        );

        // act
        Result<string> result = TemplateEngine.Render(template, globals);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("XabY", result.Value);
    }

    [Fact]
    public void Should_Fail_For_When_Expression_Is_Not_A_Sequence()
    {
        // arrange
        const string template = "X@for(item in num){@item}Y";
        MapAccessor globals = MapAccessor.With(
            ("num", Value.FromNumber(123))
        );

        // act
        Result<string> result = TemplateEngine.Render(template, globals);

        // assert
        Assert.False(result.IsOk);
        IError error = result.Error!;
        Assert.Equal("TypeMismatch", error.Code);
        Assert.Equal(TextSpan.At(1, 17), error.Range); // "@for(item in num)"
        Assert.Equal("Expression of @for must evaluate to a sequence, but got Number.", error.Message);
    }

    [Fact]
    public void Should_Render_For_Loop_With_Index_In_TemplateEngine()
    {
        // arrange
        const string template = "X@for(item, i in items){@i:@item;}Y";
        MapAccessor globals = MapAccessor.With(
            ("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b")
            ))
        );

        // act
        Result<string> result = TemplateEngine.Render(template, globals);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("X0:a;1:b;Y", result.Value);
    }

    [Fact]
    public void Should_Render_Else_For_Empty_Sequence()
    {
        // arrange
        const string template = "X@for(item in items){@item}else{E}Y";
        MapAccessor globals = MapAccessor.With(("items", Value.FromSequence()));
        
        // act
        Result<string> result = TemplateEngine.Render(template, globals);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("XEY", result.Value);
    }

    [Fact]
    public void Should_Render_Body_For_NonEmpty_Sequence()
    {
        // arrange
        const string template = "X@for(item in items){@item}else{E}Y";
        MapAccessor globals = MapAccessor.With(
            ("items", Value.FromSequence(
                Value.FromString("a"),
                Value.FromString("b")
            ))
        );

        // act
        Result<string> result = TemplateEngine.Render(template, globals);
        
        // assert
        Assert.True(result.IsOk);
        Assert.Equal("XabY", result.Value);
    }
    
    [Fact]
    public void Should_Render_For_Loop_With_Nested_DotAccess()
    {
        // arrange
        const string template = "X@for(u in users){@(u.address.city)}Y";

        MapAccessor globals = MapAccessor.With(
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
            ))
        );

        // act
        Result<string> result = TemplateEngine.Render(template, globals);

        // assert
        Assert.True(result.IsOk);
        Assert.Equal("XabY", result.Value);
    }
}