namespace dotRenderer.Tests;

public class TokenizerTests
{
    [Fact]
    public void Tokenizer_Should_Split_Text_And_Model_Interpolation()
    {
        string template = "<h1>Hi @Model.Name!</h1>";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("<h1>Hi "),
            new InterpolationToken(["Model", "Name"]),
            new TextToken("!</h1>"));
    }

    [Fact]
    public void Tokenizer_Should_Return_Single_TextToken_When_No_Interpolation()
    {
        string template = "plain text only";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens, new TextToken("plain text only"));
    }

    [Fact]
    public void Tokenizer_Should_Handle_Interpolation_At_Start()
    {
        string template = "@Model.Name!";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new InterpolationToken(["Model", "Name"]),
            new TextToken("!"));
    }

    [Fact]
    public void Tokenizer_Should_Handle_Multiple_Interpolations()
    {
        string template = "Hello @Model.Foo and @Model.Bar!";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("Hello "),
            new InterpolationToken(["Model", "Foo"]),
            new TextToken(" and "),
            new InterpolationToken(["Model", "Bar"]),
            new TextToken("!"));
    }

    [Fact]
    public void Tokenizer_Should_Handle_Interpolation_At_End()
    {
        string template = "Say hi to @Model.User";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("Say hi to "),
            new InterpolationToken(["Model", "User"]));
    }

    [Fact]
    public void Tokenizer_Should_Handle_Escaping_At_Sign()
    {
        string template = "Price: @@Model.Price";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens, new TextToken("Price: @Model.Price"));
    }

    [Fact]
    public void Tokenizer_Should_Handle_Mixed_Escaped_At_And_Interpolation()
    {
        string template = "@@ @Model.Price @@@";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("@ "),
            new InterpolationToken(["Model", "Price"]),
            new TextToken(" @@"));
    }

    [Fact]
    public void Tokenizer_Should_Handle_Dot_Separated_Path()
    {
        string template = "Hello @Model.User.Name!";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("Hello "),
            new InterpolationToken(["Model", "User", "Name"]),
            new TextToken("!"));
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_Block_With_Text_And_Interpolation()
    {
        string template = "Hello @if (Model.IsAdmin) {ADMIN @Model.Name}!";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("Hello "),
            new IfToken("Model.IsAdmin", [
                new TextToken("ADMIN "),
                new InterpolationToken(["Model", "Name"])
            ]),
            new TextToken("!"));
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_Block_With_Empty_Body()
    {
        string template = "Hi @if (Model.X) {}!";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("Hi "),
            new IfToken("Model.X", []),
            new TextToken("!"));
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_Block_With_Arbitrary_Condition_And_Body()
    {
        string template = "A @if (fooBar) {abc}B";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("A "),
            new IfToken("fooBar", [new TextToken("abc")]),
            new TextToken("B"));
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_Block_With_Parenthesized_Condition()
    {
        string template = "A @if ((Model.X && Model.Y)) {abc}B";

        object[] tokens = [..Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("A "),
            new IfToken("(Model.X && Model.Y)", [new TextToken("abc")]),
            new TextToken("B"));
    }

    [Fact]
    public void Tokenizer_Should_Handle_Nested_If_Blocks()
    {
        string template = "A @if (x) {B @if (y) {C}@Model.Z}D";

        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("A "),
            new IfToken("x", [
                new TextToken("B "),
                new IfToken("y", [
                    new TextToken("C")
                ]),
                new InterpolationToken(["Model", "Z"])
            ]),
            new TextToken("D")
        );
    }

    [Fact]
    public void Tokenizer_Should_Treat_Inline_If_As_Text()
    {
        string template = "X@if (Model.Foo) {YES}";

        object[] tokens = [..Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("X@if (Model.Foo) {YES}")
        );
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_After_Whitespace()
    {
        string template = "X @if (Model.Foo) {YES}";

        object[] tokens = [..Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("X "),
            new IfToken("Model.Foo", [new TextToken("YES")])
        );
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_At_Start()
    {
        string template = "@if (Model.Foo) {YES}";

        object[] tokens = [..Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new IfToken("Model.Foo", [new TextToken("YES")])
        );
    }

    [Fact]
    public void Tokenizer_Should_Treat_If_Embedded_In_Word_As_Text()
    {
        string template = "foo@if (Model.Foo) {YES}bar";

        object[] tokens = [..Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("foo@if (Model.Foo) {YES}bar")
        );
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_After_Newline()
    {
        string template = "abc\n@if (Model.Foo) {YES}";

        object[] tokens = [..Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new TextToken("abc\n"),
            new IfToken("Model.Foo", [new TextToken("YES")])
        );
    }

    [Fact]
    public void Tokenizer_Should_Throw_If_Interpolation_Missing_Identifier()
    {
        string template = "Hello @Model.";

        TokenizerAssert.Throws<InvalidOperationException>(template, expectedMessageFragment: "@Model.");
    }

    [Fact]
    public void Tokenizer_Should_Throw_On_Unclosed_If_Block()
    {
        string template = "Hi @if (Model.X) { ...";

        TokenizerAssert.Throws<InvalidOperationException>(
            template,
            expectedMessageFragment: "Unclosed @if block: missing '}'");
    }

    [Fact]
    public void Tokenizer_Should_Throw_On_If_Missing_Closing_Paren()
    {
        string template = "Hello @if (Model.X { body }";

        TokenizerAssert.Throws<InvalidOperationException>(template,
            expectedMessageFragment: "Unclosed @if condition: missing ')'");
    }

    [Fact]
    public void Tokenizer_Should_Throw_On_If_Missing_Brace()
    {
        string template = "Hello @if (Model.X) body }";

        TokenizerAssert.Throws<InvalidOperationException>(
            template,
            expectedMessageFragment: "Expected '{' after @if condition");
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_Block_Without_Space_Before_Paren()
    {
        string template = "@if(Model.Foo){YES}";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new IfToken("Model.Foo", [new TextToken("YES")]));
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_Block_With_Multiple_Spaces_Before_Paren()
    {
        string template = "@if   (Model.Foo){YES}";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new IfToken("Model.Foo", [new TextToken("YES")]));
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_Condition_With_String_Containing_Closing_Paren()
    {
        string template = "@if (Model.S == \")\") {OK}";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new IfToken("Model.S == \")\"", [new TextToken("OK")]));
    }

    [Fact]
    public void Tokenizer_Should_Tokenize_If_Body_With_String_Containing_Closing_Brace()
    {
        string template = "@if (true) {before \"}\" after}";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new IfToken("true", [new TextToken("before \"}\" after")]));
    }

    [Fact]
    public void Tokenizer_Should_Handle_Escaped_Quote_Inside_Condition_String()
    {
        string template = "@if (Model.S == \"\\\")\\\"\") {OK}";
        object[] tokens = [.. Tokenizer.Tokenize(template)];

        TokenizerAssert.TokenSequence(tokens,
            new IfToken("Model.S == \"\\\")\\\"\"", [new TextToken("OK")]));
    }

    [Fact]
    public void Tokenizer_Should_Throw_On_If_Missing_Open_Paren()
    {
        string template = "@if  Model.Flag) {X}";

        TokenizerAssert.Throws<InvalidOperationException>(template, "Expected '(' after @if");
    }
    
    [Theory]
    [InlineData("@if (true) {\nX}")]
    [InlineData("@if (true) {\r\nX}")]
    [InlineData("@if (true) {X\n}")]
    [InlineData("@if (true) {X\r\n}")]
    [InlineData("@if (true) {\r\nX\r\n}")]
    public void Tokenizer_Should_Trim_Single_Newlines_Around_If_Body_Leaving_Content(string template)
    {
        object[] tokens = [.. Tokenizer.Tokenize(template)];
        TokenizerAssert.TokenSequence(tokens, new IfToken("true", [ new TextToken("X") ]));
    }

    [Theory]
    [InlineData("@if (true) {\n}")]
    [InlineData("@if (true) {\r\n}")]
    public void Tokenizer_Should_Trim_Single_Newlines_To_Empty_If_Body(string template)
    {
        object[] tokens = [.. Tokenizer.Tokenize(template)];
        TokenizerAssert.TokenSequence(tokens, new IfToken("true", Array.Empty<object>()));
    }
}