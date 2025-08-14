using DotRenderer;

namespace dotRenderer.Tests;

public class TemplateEngineNestedMemberTests
{
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