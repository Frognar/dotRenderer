using DotRenderer;

namespace dotRenderer.Tests;

public class LexerTests
{
    [Fact]
    public void Should_Tokenize_Plain_Text_As_Single_Text_Token() =>
        LexerAssert.Lex("Hello, world!",
            Token.FromText("Hello, world!", TextSpan.At(0, 13)));

    [Fact]
    public void Should_Tokenize_At_Ident_Between_Text_Fragments() =>
        LexerAssert.Lex("Hello @name!",
            Token.FromText("Hello ", TextSpan.At(0, 6)),
            Token.FromAtIdent("name", TextSpan.At(6, 5)),
            Token.FromText("!", TextSpan.At(11, 1)));

    [Fact]
    public void Should_Tokenize_Double_At_As_Single_Literal_At_In_Text() =>
        LexerAssert.Lex("Hello @@world",
            Token.FromText("Hello ", TextSpan.At(0, 6)),
            Token.FromText("@", TextSpan.At(6, 2)),
            Token.FromText("world", TextSpan.At(8, 5)));

    [Fact]
    public void Should_Tokenize_At_Expr_Between_Text_Fragments() =>
        LexerAssert.Lex("Hello @(1+2)!",
            Token.FromText("Hello ", TextSpan.At(0, 6)),
            Token.FromAtExpr("1+2", TextSpan.At(6, 6)),
            Token.FromText("!", TextSpan.At(12, 1)));

    [Fact]
    public void Should_Tokenize_At_Expr_With_String_Concat() =>
        LexerAssert.Lex("Hello @(\"A\" + \"B\")!",
            Token.FromText("Hello ", TextSpan.At(0, 6)),
            Token.FromAtExpr("\"A\" + \"B\"", TextSpan.At(6, 12)),
            Token.FromText("!", TextSpan.At(18, 1)));

    [Fact]
    public void Should_Tokenize_At_Expr_With_Paren_Inside_String() =>
        LexerAssert.Lex("A@(\"(\")B",
            Token.FromText("A", TextSpan.At(0, 1)),
            Token.FromAtExpr("\"(\"", TextSpan.At(1, 6)),
            Token.FromText("B", TextSpan.At(7, 1)));

    [Fact]
    public void Should_Tokenize_AtIf_With_Single_Block_No_Else() =>
        LexerAssert.Lex("X@if(true){ok}Y",
            Token.FromText("X", TextSpan.At(0, 1)),
            Token.FromAtIf("true", TextSpan.At(1, 9)),
            Token.FromLBrace(TextSpan.At(10, 1)),
            Token.FromText("ok", TextSpan.At(11, 2)),
            Token.FromRBrace(TextSpan.At(13, 1)),
            Token.FromText("Y", TextSpan.At(14, 1)));

    [Fact]
    public void Should_Tokenize_Else_After_If_Block() =>
        LexerAssert.Lex("A@if(true){T}else{E}B",
            Token.FromText("A", TextSpan.At(0, 1)),
            Token.FromAtIf("true", TextSpan.At(1, 9)),
            Token.FromLBrace(TextSpan.At(10, 1)),
            Token.FromText("T", TextSpan.At(11, 1)),
            Token.FromRBrace(TextSpan.At(12, 1)),
            Token.FromElse(TextSpan.At(13, 4)),
            Token.FromLBrace(TextSpan.At(17, 1)),
            Token.FromText("E", TextSpan.At(18, 1)),
            Token.FromRBrace(TextSpan.At(19, 1)),
            Token.FromText("B", TextSpan.At(20, 1)));

    [Fact]
    public void Should_Not_Tokenize_Else_When_Part_Of_A_Bigger_Word() =>
        LexerAssert.Lex("xelse{y}",
            Token.FromText("xelse", TextSpan.At(0, 5)),
            Token.FromLBrace(TextSpan.At(5, 1)),
            Token.FromText("y", TextSpan.At(6, 1)),
            Token.FromRBrace(TextSpan.At(7, 1)));

    [Fact]
    public void Should_Not_Tokenize_Else_Without_Following_LBrace() =>
        LexerAssert.Lex("A else B",
            Token.FromText("A else B", TextSpan.At(0, 8)));

    [Fact]
    public void Should_Tokenize_AtFor_With_Single_Block_No_Else() =>
        LexerAssert.Lex("A@for(item in items){x}B",
            Token.FromText("A", TextSpan.At(0, 1)),
            Token.FromAtFor("item in items", TextSpan.At(1, 19)),
            Token.FromLBrace(TextSpan.At(20, 1)),
            Token.FromText("x", TextSpan.At(21, 1)),
            Token.FromRBrace(TextSpan.At(22, 1)),
            Token.FromText("B", TextSpan.At(23, 1)));

    [Fact]
    public void Should_Tokenize_Else_After_For_Block() =>
        LexerAssert.Lex("A@for(item in items){x}else{e}B",
            Token.FromText("A", TextSpan.At(0, 1)),
            Token.FromAtFor("item in items", TextSpan.At(1, 19)),
            Token.FromLBrace(TextSpan.At(20, 1)),
            Token.FromText("x", TextSpan.At(21, 1)),
            Token.FromRBrace(TextSpan.At(22, 1)),
            Token.FromElse(TextSpan.At(23, 4)),
            Token.FromLBrace(TextSpan.At(27, 1)),
            Token.FromText("e", TextSpan.At(28, 1)),
            Token.FromRBrace(TextSpan.At(29, 1)),
            Token.FromText("B", TextSpan.At(30, 1)));

    [Fact]
    public void Should_Tokenize_If_Elif_Else_Chain()
        => LexerAssert.Lex("A@if(false){T}@elif(true){U}else{E}B",
            Token.FromText("A", TextSpan.At(0, 1)),
            Token.FromAtIf("false", TextSpan.At(1, 10)), // "@if(false)"
            Token.FromLBrace(TextSpan.At(11, 1)),
            Token.FromText("T", TextSpan.At(12, 1)),
            Token.FromRBrace(TextSpan.At(13, 1)),
            Token.FromElse(TextSpan.At(14, 4)),
            Token.FromAtIf("true", TextSpan.At(14, 11)),
            Token.FromLBrace(TextSpan.At(25, 1)),
            Token.FromText("U", TextSpan.At(26, 1)),
            Token.FromRBrace(TextSpan.At(27, 1)),
            Token.FromElse(TextSpan.At(28, 4)),
            Token.FromLBrace(TextSpan.At(32, 1)),
            Token.FromText("E", TextSpan.At(33, 1)),
            Token.FromRBrace(TextSpan.At(34, 1)),
            Token.FromText("B", TextSpan.At(35, 1)));
}