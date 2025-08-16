using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace DotRenderer;

public static class Parser
{
    [Pure]
    public static Result<Template> Parse(ImmutableArray<Token> tokens)
    {
        return ParseSequence(new TokenReader(tokens), stopAtRBrace: false)
            .Map(nodes => new Template(nodes));
    }

    private sealed class TokenReader(ImmutableArray<Token> tokens)
    {
        private readonly ImmutableArray<Token> _tokens = tokens;
        private int _i;

        public bool Eof => _i >= _tokens.Length;
        public Token Current => _tokens[_i];
        public TokenKind? Kind => Eof ? null : Current.Kind;

        public Token Take()
        {
            Token t = Current;
            _i++;
            return t;
        }

        public bool Match(TokenKind kind)
        {
            if (Kind == kind)
            {
                _i++;
                return true;
            }

            return false;
        }
    }

    private static Result<ImmutableArray<INode>> ParseSequence(
        TokenReader reader,
        bool stopAtRBrace)
    {
        ImmutableArray<INode>.Builder nodes = ImmutableArray.CreateBuilder<INode>();
        while (!reader.Eof)
        {
            if (stopAtRBrace && reader.Kind == TokenKind.RBrace)
            {
                break;
            }

            switch (reader.Kind)
            {
                case TokenKind.Text:
                    nodes.Add(ParseText(reader));
                    continue;

                case TokenKind.AtIdent:
                    nodes.Add(ParseAtIdent(reader));
                    continue;

                case TokenKind.AtExpr:
                {
                    Result<INode> res = ParseAtExpr(reader);
                    if (!res.IsOk)
                    {
                        return Result<ImmutableArray<INode>>.Err(res.Error!);
                    }

                    nodes.Add(res.Value);
                    continue;
                }

                case TokenKind.AtIf:
                {
                    Result<INode> res = ParseIf(reader);
                    if (!res.IsOk)
                    {
                        return Result<ImmutableArray<INode>>.Err(res.Error!);
                    }

                    nodes.Add(res.Value);
                    continue;
                }

                case TokenKind.AtFor:
                {
                    Result<INode> res = ParseFor(reader);
                    if (!res.IsOk)
                    {
                        return Result<ImmutableArray<INode>>.Err(res.Error!);
                    }

                    nodes.Add(res.Value);
                    continue;
                }

                default:
                    reader.Take();
                    continue;
            }
        }

        return Result<ImmutableArray<INode>>.Ok(nodes.ToImmutable());
    }

    private static TextNode ParseText(TokenReader reader)
    {
        Token t = reader.Take();
        return Node.FromText(t.Text, t.Range);
    }

    private static InterpolateIdentNode ParseAtIdent(TokenReader reader)
    {
        Token t = reader.Take();
        return Node.FromInterpolateIdent(t.Text, t.Range);
    }

    private static Result<INode> ParseAtExpr(TokenReader reader)
    {
        Token t = reader.Take();
        Result<IExpr> expr = ExprParser.Parse(t.Text);
        if (!expr.IsOk)
        {
            IError e = expr.Error!;
            return Result<INode>.Err(new ParseError(e.Code, t.Range, e.Message));
        }

        return Result<INode>.Ok(Node.FromInterpolateExpr(expr.Value, t.Range));
    }

    private static Result<INode> ParseIf(TokenReader reader)
    {
        Token atIf = reader.Take();
        Result<IExpr> condRes = ExprParser.Parse(atIf.Text);
        if (!condRes.IsOk)
        {
            IError e = condRes.Error!;
            return Result<INode>.Err(new ParseError(e.Code, atIf.Range, e.Message));
        }

        if (!reader.Match(TokenKind.LBrace))
        {
            return Result<INode>.Err(new ParseError("IfMissingLBrace", atIf.Range, "Expected '{' after @if(...)."));
        }

        Result<ImmutableArray<INode>> thenRes = ParseSequence(reader, stopAtRBrace: true);
        if (!thenRes.IsOk)
        {
            return Result<INode>.Err(thenRes.Error!);
        }

        if (!reader.Match(TokenKind.RBrace))
        {
            return Result<INode>.Err(new ParseError("IfMissingRBrace", atIf.Range, "Expected '}' to close @if block."));
        }

        ImmutableArray<INode> elseNodes = ImmutableArray<INode>.Empty;
        if (reader.Kind == TokenKind.Else)
        {
            reader.Take();
            if (!reader.Match(TokenKind.LBrace))
            {
                return Result<INode>.Err(new ParseError("ElseMissingLBrace", atIf.Range, "Expected '{' after else."));
            }

            Result<ImmutableArray<INode>> elseRes = ParseSequence(reader, stopAtRBrace: true);
            if (!elseRes.IsOk)
            {
                return Result<INode>.Err(elseRes.Error!);
            }

            if (!reader.Match(TokenKind.RBrace))
            {
                return Result<INode>.Err(new ParseError("ElseMissingRBrace", atIf.Range, "Expected '}' to close else block."));
            }

            elseNodes = elseRes.Value;
        }

        IfNode node = elseNodes.IsDefaultOrEmpty
            ? Node.FromIf(condRes.Value, thenRes.Value, atIf.Range)
            : Node.FromIf(condRes.Value, thenRes.Value, elseNodes, atIf.Range);

        return Result<INode>.Ok(node);
    }

    private static Result<INode> ParseFor(TokenReader reader)
    {
        Token atFor = reader.Take();
        Result<(string item, string? index, IExpr seq)> header = ParseForHeader(atFor.Text, atFor.Range);
        if (!header.IsOk)
        {
            return Result<INode>.Err(header.Error!);
        }

        if (!reader.Match(TokenKind.LBrace))
        {
            return Result<INode>.Err(new ParseError("ForMissingLBrace", atFor.Range, "Expected '{' after @for(...)."));
        }

        Result<ImmutableArray<INode>> bodyRes = ParseSequence(reader, stopAtRBrace: true);
        if (!bodyRes.IsOk)
        {
            return Result<INode>.Err(bodyRes.Error!);
        }

        if (!reader.Match(TokenKind.RBrace))
        {
            return Result<INode>.Err(new ParseError("ForMissingRBrace", atFor.Range, "Expected '}' to close @for block."));
        }

        ImmutableArray<INode> elseNodes = ImmutableArray<INode>.Empty;
        if (reader.Kind == TokenKind.Else)
        {
            reader.Take();
            if (!reader.Match(TokenKind.LBrace))
            {
                return Result<INode>.Err(new ParseError("ElseMissingLBrace", atFor.Range, "Expected '{' after else."));
            }

            Result<ImmutableArray<INode>> elseRes = ParseSequence(reader, stopAtRBrace: true);
            if (!elseRes.IsOk)
            {
                return Result<INode>.Err(elseRes.Error!);
            }

            if (!reader.Match(TokenKind.RBrace))
            {
                return Result<INode>.Err(new ParseError("ElseMissingRBrace", atFor.Range, "Expected '}' to close else block."));
            }

            elseNodes = elseRes.Value;
        }

        (string item, string? index, IExpr seq) = header.Value;
        ForNode node = index is null
            ? Node.FromFor(item, seq, bodyRes.Value, elseNodes, atFor.Range)
            : Node.FromFor(item, index, seq, bodyRes.Value, elseNodes, atFor.Range);

        return Result<INode>.Ok(node);
    }

    private static Result<(string item, string? index, IExpr seq)> ParseForHeader(string header, TextSpan range)
    {
        int i = 0;
        int n = header.Length;
        i = SkipWs(header, i, n);
        if (i >= n || !(char.IsLetter(header[i]) || header[i] == '_'))
        {
            return Err("ForItemIdent", "Expected loop variable identifier.");
        }

        (string item, i) = ReadIdent(header, i, n);
        i = SkipWs(header, i, n);
        string? index = null;
        if (i < n && header[i] == ',')
        {
            i++;
            i = SkipWs(header, i, n);
            if (i >= n || !(char.IsLetter(header[i]) || header[i] == '_'))
            {
                return Err("ForIndexIdent", "Expected index identifier after ','.");
            }

            (index, i) = ReadIdent(header, i, n);
            i = SkipWs(header, i, n);
        }

        if (!(i + 2 <= n && header.AsSpan(i, 2).SequenceEqual("in".AsSpan())))
        {
            return Err("ForMissingIn", "Expected 'in' in @for header.");
        }

        i += 2;
        i = SkipWs(header, i, n);
        if (i >= n)
        {
            return Err("ForMissingExpr", "Expected expression after 'in'.");
        }

        string exprText = header[i..].Trim();
        Result<IExpr> expr = ExprParser.Parse(exprText);
        if (!expr.IsOk)
        {
            IError e = expr.Error!;
            return Result<(string, string?, IExpr)>.Err(new ParseError(e.Code, range, e.Message));
        }

        return Result<(string, string?, IExpr)>.Ok((item, index, expr.Value));

        static int SkipWs(string s, int i, int n)
        {
            while (i < n && char.IsWhiteSpace(s[i]))
            {
                i++;
            }

            return i;
        }

        static (string ident, int newI) ReadIdent(string s, int i, int n)
        {
            int start = i;
            i++;
            while (i < n && (char.IsLetterOrDigit(s[i]) || s[i] == '_'))
            {
                i++;
            }

            return (s[start..i], i);
        }

        Result<(string, string?, IExpr)> Err(string code, string message)
            => Result<(string, string?, IExpr)>.Err(new ParseError(code, range, message));
    }
}
