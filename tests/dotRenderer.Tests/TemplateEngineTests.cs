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
    public void Should_Collapse_Surrounding_Newlines_When_If_False_Renders_Empty() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @if(false)
            {abc}
            xyz
            """,
            MapAccessor.Empty,
            """
            xyz
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
    public void Else_Should_Trim_Outer_Newlines_SameLine_Braces() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @if(false){abc}else{def}
            xyz
            """,
            MapAccessor.Empty,
            """
            xyz
            def
            xyz
            """);

    [Fact]
    public void Else_Should_Trim_Outer_Newlines_NextLine_Braces() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @if(false)
            {
            abc
            }
            else
            {
            def
            }
            xyz
            """,
            MapAccessor.Empty,
            """
            xyz
            def
            xyz
            """);

    [Fact]
    public void For_Else_Should_Render_When_Sequence_Is_Empty_And_Trim_Newlines() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @for(x in arr)
            {
            <li>@x</li>
            }
            else
            {
            <p>empty</p>
            }
            xyz
            """,
            MapAccessor.With(("arr", Value.FromSequence())),
            """
            xyz
            <p>empty</p>
            xyz
            """);

    [Fact]
    public void For_Should_Ignore_Else_When_Sequence_NotEmpty_And_Trim_Outer_Newlines() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @for(x in arr)
            {
            <li>@x</li>
            }
            else
            {
            <p>empty</p>
            }
            xyz
            """,
            MapAccessor.With(("arr", Value.FromSequence(Value.FromNumber(1), Value.FromNumber(2)))),
            """
            xyz
            <li>1</li>
            <li>2</li>
            xyz
            """);

    [Theory]
    [InlineData("xyz@if(false){abc}\nxyz", "xyz\nxyz")]
    [InlineData("xyz@if(true)\n{abc}\nxyz", "xyz\nabc\nxyz")]
    [InlineData("xyz\n@if(false){abc}xyz", "xyz\nxyz")]
    [InlineData("xyz\n@if(false)\n{abc}xyz", "xyz\nxyz")]
    public void If_Should_Handle_Inline_And_NextLine_Whitespaces(string template, string expected)
        => TemplateEngineAssert.Render(template, MapAccessor.Empty, expected);

    [Theory]
    [InlineData("xyz\n@for(x in xs){<i>@x</i>}xyz", "xyz\nxyz")]
    [InlineData("xyz\n@for(x in xs)\n{<i>@x</i>}xyz", "xyz\nxyz")]
    [InlineData("xyz@for(x in xs){<i>@x</i>}\nxyz", "xyz\nxyz")]
    public void For_Empty_Inline_After_Block_Should_Leave_Single_Newline(string template, string expected)
        => TemplateEngineAssert.Render(
            template,
            MapAccessor.With(("xs", Value.FromSequence())),
            expected);

    [Theory]
    [InlineData("xyz@for(x in xs)\n{<i>@x</i>}\nxyz", "xyz\n<i>1</i>\nxyz")]
    [InlineData("xyz\n@for(x in xs){<i>@x</i>}\nxyz", "xyz\n<i>1</i>\nxyz")]
    [InlineData("xyz\n@for(x in xs)\n{<i>@x</i>}\nxyz", "xyz\n<i>1</i>\nxyz")]
    public void For_Should_Handle_Inline_And_NextLine_Whitespaces(string template, string expected)
        => TemplateEngineAssert.Render(
            template,
            MapAccessor.With(("xs", Value.FromSequence(Value.FromNumber(1)))),
            expected);

    [Fact]
    public void If_With_Empty_Else_Should_Collapse_Newlines() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @if(true)
            {
            abc
            }
            else
            {
            }
            xyz
            """,
            MapAccessor.Empty,
            """
            xyz
            abc
            xyz
            """);

    [Fact]
    public void For_With_Empty_Else_Should_Collapse_Newlines_When_Empty_Sequence() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @for(x in arr)
            {
            <li>@x</li>
            }
            else
            {
            }
            xyz
            """,
            MapAccessor.With(("arr", Value.FromSequence())),
            """
            xyz
            xyz
            """);

    [Fact]
    public void For_With_Empty_Else_Should_Collapse_Newlines_When_NotEmpty_Sequence() =>
        TemplateEngineAssert.Render(
            """
            xyz
            @for(x in arr)
            {
            <li>@x</li>
            }
            else
            {
            }
            xyz
            """,
            MapAccessor.With(("arr", Value.FromSequence(Value.FromNumber(1)))),
            """
            xyz
            <li>1</li>
            xyz
            """);

    [Fact]
    public void Should_Render_Inline_CSS_As_Is() =>
        TemplateEngineAssert.Render(
            """
            <style>
            body { margin:0 }
            @@media (min-width: 600px) { .grid { display: grid } }
            .else { color: red }
            </style>
            """,
            MapAccessor.Empty,
            """
            <style>
            body { margin:0 }
            @media (min-width: 600px) { .grid { display: grid } }
            .else { color: red }
            </style>
            """);

    [Fact]
    public void Should_Render_CSS_Inside_If_Block() =>
        TemplateEngineAssert.Render(
            """
            @if(true){
            <style>
            .card { border:1px solid #000 }
            .card .title { font-weight: bold }
            </style>
            }else{}
            """,
            MapAccessor.Empty,
            """
            <style>
            .card { border:1px solid #000 }
            .card .title { font-weight: bold }
            </style>
            """);

    [Fact]
    public void Should_Render_If_Elif_Else_Chain()
        => TemplateEngineAssert.Render(
            "A@if(false){T}@elif(true){U}else{E}B",
            MapAccessor.Empty,
            "AUB");

    [Fact]
    public void Should_Render_Unclosed_AtExpr_As_Plain_Text()
        => TemplateEngineAssert.Render(
            "A@(",
            MapAccessor.Empty,
            "A@(");

    [Fact]
    public void Should_Render_Unclosed_AtIf_As_Identifier_And_Text()
        => TemplateEngineAssert.Render(
            "A@if(",
            MapAccessor.With(("if", Value.FromString("B"))),
            "AB(");

    [Fact]
    public void Should_Render_Unclosed_AtElif_As_Identifier_And_Text()
        => TemplateEngineAssert.Render(
            "A@elif(",
            MapAccessor.With(("elif", Value.FromString("B"))),
            "AB(");

    [Fact]
    public void Should_Render_Unclosed_AtFor_As_Identifier_And_Text()
        => TemplateEngineAssert.Render(
            "A@for(",
            MapAccessor.With(("for", Value.FromString("B"))),
            "AB(");

    [Fact]
    public void Should_Render_Else_As_Plain_Text_When_Not_Preceded_By_At()
        => TemplateEngineAssert.Render(
            "X else{y}",
            MapAccessor.Empty,
            "X else{y}");

    [Theory]
    [InlineData("A@if(false){T}@elif(false){U}else{E}B", "AEB")]
    [InlineData("A@if(true){T}@elif(true){U}else{E}B",  "ATB")]
    public void Should_Pick_First_Matching_Branch_In_Chain(string template, string expected)
        => TemplateEngineAssert.Render(template, MapAccessor.Empty, expected);

    [Fact]
    public void Should_Render_Html_Invoice_EndToEnd() =>
        TemplateEngineAssert.Render("""
                                    <!doctype html>
                                    <html>
                                    <head>
                                    <meta charset="utf-8">
                                    <title>Invoice @(invoice.number)</title>
                                    <style>
                                    body { font-family: Arial, sans-serif; margin: 40px; }
                                    h1 { color: #333; }
                                    table { border-collapse: collapse; width: 100%; margin-top: 20px; }
                                    th, td { border: 1px solid #ddd; padding: 8px; text-align: right; }
                                    th { background-color: #f2f2f2; }
                                    td:first-child, th:first-child { text-align: center; }
                                    tfoot td { font-weight: bold; }
                                    </style>
                                    </head>
                                    <body>
                                    <h1>Invoice @(invoice.number)</h1>
                                    <p>Date: @(invoice.date)</p>
                                    <p>Seller: @(seller.name)</p>
                                    <p>Buyer: @(buyer.name)</p>
                                    @if(buyer.vat == ""){<p>Buyer VAT: N/A</p>}else{<p>Buyer VAT: @(buyer.vat)</p>}
                                    <table>
                                    <thead><tr><th>#</th><th>Description</th><th>Qty</th><th>Unit</th><th>Unit Price</th><th>Line Total</th></tr></thead>
                                    <tbody>
                                    @for (item, i in items) {<tr>
                                    <td>@(i+1)</td><td style="text-align:left">@(item.description)</td><td>@(item.qty)</td><td>pcs</td><td>@(item.unitPrice)</td><td>@(item.qty * item.unitPrice)</td>
                                    </tr>}
                                    </tbody>
                                    <tfoot>
                                    <tr><td colspan="5">Subtotal</td><td>@subtotal</td></tr>
                                    <tr><td colspan="5">Tax (@(taxRate*100)%)</td><td>@tax</td></tr>
                                    <tr><td colspan="5"><strong>Total</strong></td><td><strong>@total</strong></td></tr>
                                    </tfoot>
                                    </table>
                                    <p>Notes: @notes</p>
                                    </body>
                                    </html>
                                    """,
            MapAccessor.With(
                ("invoice", Value.FromMap(
                    ("number", Value.FromString("FV-2025/08/001")),
                    ("date", Value.FromString("2025-08-01")))),
                ("seller", Value.FromMap(("name", Value.FromString("Acme Sp. z o.o.")))),
                ("buyer", Value.FromMap(
                    ("name", Value.FromString("Globex S.A.")),
                    ("vat", Value.FromString("")))),
                ("items", Value.FromSequence(
                    Value.FromMap(
                        ("description", Value.FromString("Widget A")),
                        ("qty", Value.FromNumber(2)),
                        ("unitPrice", Value.FromNumber(100))),
                    Value.FromMap(
                        ("description", Value.FromString("Widget B")),
                        ("qty", Value.FromNumber(1)),
                        ("unitPrice", Value.FromNumber(50.5))))),
                ("taxRate", Value.FromNumber(0.23)),
                ("subtotal", Value.FromNumber(250.5)),
                ("tax", Value.FromNumber(57.615)),
                ("total", Value.FromNumber(308.115)),
                ("notes", Value.FromString("Thank you for your business!"))),
            """
            <!doctype html>
            <html>
            <head>
            <meta charset="utf-8">
            <title>Invoice FV-2025/08/001</title>
            <style>
            body { font-family: Arial, sans-serif; margin: 40px; }
            h1 { color: #333; }
            table { border-collapse: collapse; width: 100%; margin-top: 20px; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: right; }
            th { background-color: #f2f2f2; }
            td:first-child, th:first-child { text-align: center; }
            tfoot td { font-weight: bold; }
            </style>
            </head>
            <body>
            <h1>Invoice FV-2025/08/001</h1>
            <p>Date: 2025-08-01</p>
            <p>Seller: Acme Sp. z o.o.</p>
            <p>Buyer: Globex S.A.</p>
            <p>Buyer VAT: N/A</p>
            <table>
            <thead><tr><th>#</th><th>Description</th><th>Qty</th><th>Unit</th><th>Unit Price</th><th>Line Total</th></tr></thead>
            <tbody>
            <tr>
            <td>1</td><td style="text-align:left">Widget A</td><td>2</td><td>pcs</td><td>100</td><td>200</td>
            </tr><tr>
            <td>2</td><td style="text-align:left">Widget B</td><td>1</td><td>pcs</td><td>50.5</td><td>50.5</td>
            </tr>
            </tbody>
            <tfoot>
            <tr><td colspan="5">Subtotal</td><td>250.5</td></tr>
            <tr><td colspan="5">Tax (23%)</td><td>57.615</td></tr>
            <tr><td colspan="5"><strong>Total</strong></td><td><strong>308.115</strong></td></tr>
            </tfoot>
            </table>
            <p>Notes: Thank you for your business!</p>
            </body>
            </html>
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