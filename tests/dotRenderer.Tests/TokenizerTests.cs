namespace dotRenderer.Tests;

public class TokenizerTests
{
    [Fact]
    public void Tokenizer_Should_Split_Text_And_Model_Interpolation()
    {
        string template = "<h1>Hi @Model.Name!</h1>";
        var tokens = Tokenizer.Tokenize(template);

        Assert.Collection(tokens,
            t => Assert.Equal(new TextToken("<h1>Hi "), t),
            t => Assert.Equal(new InterpolationToken(["Model", "Name"]), t),
            t => Assert.Equal(new TextToken("!</h1>"), t)
        );
    }
}