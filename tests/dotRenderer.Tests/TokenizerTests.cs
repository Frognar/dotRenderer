namespace dotRenderer.Tests;

public class TokenizerTests
{
    [Fact]
    public void Tokenizer_Should_Split_Text_And_Model_Interpolation()
    {
        string template = "<h1>Hi @Model.Name!</h1>";
        object[] tokens = [.. Tokenizer.Tokenize(template)];
        
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
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        Assert.Single(tokens);
        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("plain text only", ((TextToken)tokens[0]).Text);
    }
    
    [Fact]
    public void Tokenizer_Should_Handle_Interpolation_At_Start()
    {
        string template = "@Model.Name!";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        Assert.Equal(2, tokens.Length);

        Assert.IsType<InterpolationToken>(tokens[0]);
        InterpolationToken interp = (InterpolationToken)tokens[0];
        Assert.Equal(["Model", "Name"], interp.Path);

        Assert.IsType<TextToken>(tokens[1]);
        Assert.Equal("!", ((TextToken)tokens[1]).Text);
    }

    [Fact]
    public void Tokenizer_Should_Handle_Multiple_Interpolations()
    {
        string template = "Hello @Model.Foo and @Model.Bar!";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        Assert.Equal(5, tokens.Length);

        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("Hello ", ((TextToken)tokens[0]).Text);

        Assert.IsType<InterpolationToken>(tokens[1]);
        Assert.Equal(["Model", "Foo"], ((InterpolationToken)tokens[1]).Path);

        Assert.IsType<TextToken>(tokens[2]);
        Assert.Equal(" and ", ((TextToken)tokens[2]).Text);

        Assert.IsType<InterpolationToken>(tokens[3]);
        Assert.Equal(["Model", "Bar"], ((InterpolationToken)tokens[3]).Path);

        Assert.IsType<TextToken>(tokens[4]);
        Assert.Equal("!", ((TextToken)tokens[4]).Text);
    }
    
    
    [Fact]
    public void Tokenizer_Should_Handle_Interpolation_At_End()
    {
        string template = "Say hi to @Model.User";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        Assert.Equal(2, tokens.Length);

        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("Say hi to ", ((TextToken)tokens[0]).Text);

        Assert.IsType<InterpolationToken>(tokens[1]);
        Assert.Equal(["Model", "User"], ((InterpolationToken)tokens[1]).Path);
    }

    [Fact]
    public void Tokenizer_Should_Throw_If_Interpolation_Missing_Identifier()
    {
        string template = "Hello @Model.";
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => Tokenizer.Tokenize(template).ToArray());
        Assert.Contains("@Model.", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Tokenizer_Should_Handle_Escaping_At_Sign()
    {
        string template = "Price: @@Model.Price";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        Assert.Single(tokens);
        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("Price: @Model.Price", ((TextToken)tokens[0]).Text);
    }

    [Fact]
    public void Tokenizer_Should_Handle_Mixed_Escaped_At_And_Interpolation()
    {
        string template = "@@ @Model.Price @@@";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        Assert.Equal(3, tokens.Length);

        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("@ ", ((TextToken)tokens[0]).Text);

        Assert.IsType<InterpolationToken>(tokens[1]);
        Assert.Equal(["Model", "Price"], ((InterpolationToken)tokens[1]).Path);

        Assert.IsType<TextToken>(tokens[2]);
        Assert.Equal(" @@", ((TextToken)tokens[2]).Text);
    }

    [Fact]
    public void Tokenizer_Should_Handle_Dot_Separated_Path()
    {
        string template = "Hello @Model.User.Name!";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        Assert.Equal(3, tokens.Length);

        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("Hello ", ((TextToken)tokens[0]).Text);

        Assert.IsType<InterpolationToken>(tokens[1]);
        Assert.Equal(["Model", "User", "Name"], ((InterpolationToken)tokens[1]).Path);

        Assert.IsType<TextToken>(tokens[2]);
        Assert.Equal("!", ((TextToken)tokens[2]).Text);
    }

    [Fact]
    public void Tokenizer_Should_Terminate_Interpolation_On_Whitespace()
    {
        string template = "Hello @Model.Full Name!";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        Assert.Equal(3, tokens.Length);

        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("Hello ", ((TextToken)tokens[0]).Text);

        Assert.IsType<InterpolationToken>(tokens[1]);
        Assert.Equal(["Model", "Full"], ((InterpolationToken)tokens[1]).Path);

        Assert.IsType<TextToken>(tokens[2]);
        Assert.Equal(" Name!", ((TextToken)tokens[2]).Text);
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_Block_With_Text_And_Interpolation()
    {
        string template = "Hello @if (Model.IsAdmin) {ADMIN @Model.Name}!";
        object[] tokens = [.. Tokenizer.Tokenize(template)];Assert.Equal(3, tokens.Length);

        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("Hello ", ((TextToken)tokens[0]).Text);

        IfToken ifToken = Assert.IsType<IfToken>(tokens[1]);
        Assert.Equal("Model.IsAdmin", ifToken.Condition);

        var bodyTokens = ifToken.Body.ToArray();
        Assert.Equal(2, bodyTokens.Length);
        Assert.IsType<TextToken>(bodyTokens[0]);
        Assert.Equal("ADMIN ", ((TextToken)bodyTokens[0]).Text);

        Assert.IsType<InterpolationToken>(bodyTokens[1]);
        Assert.Equal(["Model", "Name"], ((InterpolationToken)bodyTokens[1]).Path);

        Assert.IsType<TextToken>(tokens[2]);
        Assert.Equal("!", ((TextToken)tokens[2]).Text);
    }
    
    [Fact]
    public void Tokenizer_Should_Tokenize_If_Block_With_Empty_Body()
    {
        string template = "Hi@if (Model.X) {}!";
        object[] tokens = [.. Tokenizer.Tokenize(template)];
        Assert.Equal(3, tokens.Length);
        Assert.IsType<TextToken>(tokens[0]);
        Assert.Equal("Hi", ((TextToken)tokens[0]).Text);

        IfToken ifToken = Assert.IsType<IfToken>(tokens[1]);
        Assert.Equal("Model.X", ifToken.Condition);
        Assert.Empty(ifToken.Body);

        Assert.IsType<TextToken>(tokens[2]);
        Assert.Equal("!", ((TextToken)tokens[2]).Text);
    }
}