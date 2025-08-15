using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineForTests
{
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
}