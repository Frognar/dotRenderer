namespace dotRenderer.Tests;

public class TokenizerTests
{
    [Fact]
    public void Tokenizer_Should_Split_Text_And_Model_Interpolation()
    {
        string template = "<h1>Hi @Model.Name!</h1>";
        object[] tokens = Tokenizer.Tokenize(template).ToArray();
        
        Assert.Equal(3, tokens.Length);
        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("<h1>Hi ", ((TextToken)tokens[0]).Text);

        Assert.IsType<InterpolationToken>(tokens[1]);
        InterpolationToken interp = (InterpolationToken)tokens[1];
        Assert.Equal(["Model", "Name"], interp.Path);

        Assert.IsType<TextToken>(tokens[2]);
        Assert.Equal("!</h1>", ((TextToken)tokens[2]).Text);
    }
    
    [Fact]
    public void Tokenizer_Should_Return_Single_TextToken_When_No_Interpolation()
    {
        string template = "plain text only";
        object[] tokens = Tokenizer.Tokenize(template).ToArray();

        Assert.Single(tokens);
        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("plain text only", ((TextToken)tokens[0]).Text);
    }
}