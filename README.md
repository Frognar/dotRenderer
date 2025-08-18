# dotRenderer

A lightweight template engine for .NET with a Razor‑like `@` syntax: `@if (...) { ... }`, `@for (...) { ... }`, and inline interpolation via `@ident` / `@(expr)`.

## Table of contents

* [Features](#features)
* [Requirements](#requirements)
* [Install / use in your project](#install--use-in-your-project)
* [Quick start](#quick-start)
* [Template syntax](#template-syntax)
* [Data model](#data-model)
* [API](#api)
* [Whitespace rules](#whitespace-rules)
* [Running tests](#running-tests)
* [Example: invoice HTML](#example-invoice-html)
* [Roadmap](#roadmap)
* [License](#license)

## Features

* Conditions: `@if (...) { ... } else { ... }`.
* Loops over sequences:

  * `@for (item in items) { ... }`
  * `@for (item, i in items) { ... }` (zero‑based index)
  * **`else` for empty sequences:**

    ```
    @for (item in items) {
      ...
    } else {
      <div class="muted">No items</div>
    }
    ```
* Interpolation: `@ident` and `@(expression)` (arithmetic, property access, etc.).
* Loop metadata via `loop`:

  * `loop.index`, `loop.count`, `loop.isFirst`, `loop.isLast`, `loop.isOdd`, `loop.isEven`.
* Reasonable trimming/collapsing of newlines around empty blocks for clean HTML output.
* Escape `@` by typing `@@`.
* Detailed errors with ranges: `LexError`, `ParseError`, `EvalError`.

## Requirements

* **.NET SDK 8+**, **C# 12** or newer recommended.
* Solution includes **`.slnx`** (Visual Studio 2022 17.10+/17.14+). On older VS versions, reference the `.csproj` directly via CLI.

## Install / use in your project

No NuGet package (yet). Use a project reference:

```bash
# as a git submodule (optional)
git submodule add https://github.com/Frognar/dotRenderer externals/dotRenderer

# add the library project to your solution
dotnet sln add externals/dotRenderer/src/dotRenderer/dotRenderer.csproj

# add the reference from your app
dotnet add <YourProject>.csproj reference externals/dotRenderer/src/dotRenderer/dotRenderer.csproj
```

## Quick start

```csharp
// using dotRenderer; // adjust the namespace if different

string tpl = "Hello @name! @if(itemsCount > 0){You have @(itemsCount) item(s).}else{The list is empty.}";

var data = MapAccessor.With(
    ("name",       Value.FromString("Frognar")),
    ("itemsCount", Value.FromNumber(2))
);

var res = TemplateEngine.Render(tpl, data);

if (res.IsOk)
{
    Console.WriteLine(res.Value);
}
else
{
    Console.Error.WriteLine($"{res.Error.Code} at {res.Error.Range.Offset}:{res.Error.Range.Length} — {res.Error.Message}");
}
```

### Loop with metadata and index

```csharp
string tpl = "X@for(item, i in items){@(loop.index):@item;}Y";

var data = MapAccessor.With(("items", Value.FromSequence(
    Value.FromString("a"),
    Value.FromString("b"),
    Value.FromString("c")
)));

Console.WriteLine(TemplateEngine.Render(tpl, data).Value);
// X0:a;1:b;2:c;Y
```

### `for` … `else` (empty sequence fallback)

```csharp
string tpl = @"@for(item in items){<li>@item</li>}else{<li class=\"muted\">No items</li>}";

var data = MapAccessor.With(("items", Value.FromSequence()));
Console.WriteLine(TemplateEngine.Render(tpl, data).Value);
// <li class="muted">No items</li>
```

### HTML/CSS + literal `@`

```csharp
string tpl = """
<style>
body { margin:0 }
@@media (min-width: 600px) { .grid { display: grid } }
</style>
@if(show){<p>OK</p>}else{}
""";

var data = MapAccessor.With(("show", Value.FromBool(true)));
Console.WriteLine(TemplateEngine.Render(tpl, data).Value);
```

## Template syntax

* **Identifier interpolation:** `@name`

* **Expression interpolation:** `@(price * qty)` or `@(user.name)`

* **Condition:**

  ```
  @if (cond) {
    ...
  } else {
    ...
  }
  ```

* **Loop:**

  ```
  @for (item in items) { ... }
  @for (item, i in items) { ... }     // i = 0..N-1

  // Optional else-branch for empty sequences
  @for (item in items) {
    ...
  } else {
    ...
  }
  ```

  Loop metadata via `loop` inside the body: `loop.index`, `loop.count`, `loop.isFirst`, `loop.isLast`, `loop.isOdd`, `loop.isEven`.

* **Escape `@`:** type `@@` to render a literal `@`.

## Data model

The engine operates on `Value` and `IValueAccessor`:

* **Scalars:** `Value.FromString(...)`, `Value.FromNumber(...)`, `Value.FromBool(...)`
* **Sequences:** `Value.FromSequence(...)`
* **Maps (objects):** `Value.FromMap(...)`
* **Access:** simplest via `MapAccessor.With(("key", Value...))`.
  Nested access and shadowing within blocks are supported via chained accessors.

Example structure:

```csharp
var model = MapAccessor.With(
  ("invoice", Value.FromMap(
      ("number", Value.FromString("INV-2025/08/001")),
      ("date",   Value.FromString("2025-08-01"))
  )),
  ("items", Value.FromSequence(
      Value.FromMap(("description", Value.FromString("Widget A")),
                    ("qty",        Value.FromNumber(2)),
                    ("unitPrice",  Value.FromNumber(100))),
      Value.FromMap(("description", Value.FromString("Widget B")),
                    ("qty",        Value.FromNumber(1)),
                    ("unitPrice",  Value.FromNumber(50.5)))
  ))
);
```

## API

```csharp
public static class TemplateEngine
{
    public static Result<string> Render(string template, IValueAccessor? globals = null);
}
```

Returns `Result<string>`:

* `IsOk == true` → `Value` contains the rendered text.
* `IsOk == false` → `Error` is `LexError` / `ParseError` / `EvalError` (with code, range and message).

## Whitespace rules

* Empty `@if`/`else` blocks and empty `@for` iterations won’t leave stray blank lines.
* The opening `{` may be placed on the same or the next line after `@if` / `@for`.

## Running tests

```bash
dotnet restore
dotnet build
dotnet test
```
