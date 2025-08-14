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
}