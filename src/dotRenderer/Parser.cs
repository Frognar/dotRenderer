using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace DotRenderer;

public static class Parser
{
    [Pure]
    public static Result<Template> Parse(ImmutableArray<Token> tokens)
    {
        State r = State.Of(tokens);
        Result<(ImmutableArray<INode> nodes, State rest)> seq = ParseSequence(r, stopAtRBrace: false);
        return !seq.IsOk
            ? Result<Template>.Err(seq.Error!)
            : Result<Template>.Ok(Template.With(seq.Value.nodes));
    }

    private readonly record struct State(ImmutableArray<Token> Tokens, int Index)
    {
        public bool Eof => Index >= Tokens.Length;
        public Token Current => Tokens[Index];
        public TokenKind? Kind => Eof ? null : Current.Kind;

        public State Advance(int delta = 1) => this with { Index = Index + delta };

        public (Token tok, State rest) Take()
        {
            Token t = Current;
            return (t, Advance());
        }

        public (bool matched, State rest) Match(TokenKind kind)
        {
            if (!Eof && Current.Kind == kind)
            {
                return (true, Advance());
            }

            return (false, this);
        }

        public static State Of(ImmutableArray<Token> tokens) => new(tokens, 0);
    }

    private static Result<(ImmutableArray<INode> nodes, State rest)> ParseSequence(State r, bool stopAtRBrace)
    {
        ImmutableArray<INode>.Builder nodes = ImmutableArray.CreateBuilder<INode>();
        int braceDepth = 0;
        while (!r.Eof)
        {
            switch (r.Kind)
            {
                case TokenKind.Text:
                {
                    (Token t, State rest) = r.Take();
                    nodes.Add(Node.FromText(t.Text, t.Range));
                    r = rest;
                    continue;
                }
                case TokenKind.AtIdent:
                {
                    (Token t, State rest) = r.Take();
                    nodes.Add(Node.FromInterpolateIdent(t.Text, t.Range));
                    r = rest;
                    continue;
                }
                case TokenKind.AtExpr:
                {
                    Result<(INode node, State rest)> res = ParseAtExpr(r);
                    if (!res.IsOk)
                    {
                        return Result<(ImmutableArray<INode>, State)>.Err(res.Error!);
                    }

                    nodes.Add(res.Value.node);
                    r = res.Value.rest;
                    continue;
                }
                case TokenKind.AtIf:
                {
                    Result<(INode node, State rest)> res = ParseIf(r);
                    if (!res.IsOk)
                    {
                        return Result<(ImmutableArray<INode>, State)>.Err(res.Error!);
                    }

                    nodes.Add(res.Value.node);
                    r = res.Value.rest;
                    continue;
                }
                case TokenKind.AtFor:
                {
                    Result<(INode node, State rest)> res = ParseFor(r);
                    if (!res.IsOk)
                    {
                        return Result<(ImmutableArray<INode>, State)>.Err(res.Error!);
                    }

                    nodes.Add(res.Value.node);
                    r = res.Value.rest;
                    continue;
                }
                case TokenKind.LBrace:
                {
                    (Token t, State rest) = r.Take();
                    nodes.Add(Node.FromText("{", t.Range));
                    braceDepth++;
                    r = rest;
                    continue;
                }
                case TokenKind.RBrace:
                {
                    if (stopAtRBrace && braceDepth == 0)
                    {
                        return Result<(ImmutableArray<INode>, State)>.Ok((nodes.ToImmutable(), r));
                    }

                    (Token t, State rest) = r.Take();
                    nodes.Add(Node.FromText("}", t.Range));
                    if (braceDepth > 0)
                    {
                        braceDepth--;
                    }

                    r = rest;
                    continue;
                }
                case TokenKind.Else:
                {
                    (Token t, State rest) = r.Take();
                    nodes.Add(Node.FromText(t.Text, t.Range));
                    r = rest;
                    continue;
                }
                default:
                {
                    (_, r) = r.Take();
                    continue;
                }
            }
        }

        return Result<(ImmutableArray<INode>, State)>.Ok((nodes.ToImmutable(), r));
    }

    private static Result<(INode node, State rest)> ParseAtExpr(State r)
    {
        (Token t, State rest) = r.Take();
        Result<IExpr> expr = ExprParser.Parse(t.Text);
        if (!expr.IsOk)
        {
            IError e = expr.Error!;
            return Result<(INode, State)>.Err(new ParseError(e.Code, t.Range, e.Message));
        }

        return Result<(INode, State)>.Ok((Node.FromInterpolateExpr(expr.Value, t.Range), rest));
    }

    private static Result<(INode node, State rest)> ParseIf(State r)
    {
        (Token atIf, State afterIf0) = r.Take();
        (State afterIf, bool hadNlBeforeL) = SkipWhitespaceTextTokensWithNewlineInfo(afterIf0);
        Result<IExpr> condRes = ExprParser.Parse(atIf.Text);
        if (!condRes.IsOk)
        {
            IError e = condRes.Error!;
            return Result<(INode, State)>.Err(new ParseError(e.Code, atIf.Range, e.Message));
        }

        (bool hasL, State afterL) = afterIf.Match(TokenKind.LBrace);
        if (!hasL)
        {
            return Result<(INode, State)>.Err(new ParseError("IfMissingLBrace", atIf.Range,
                "Expected '{' after @if(...)."));
        }

        Result<(ImmutableArray<INode> thenNodes, State rest)> thenSeq = ParseSequence(afterL, stopAtRBrace: true);
        if (!thenSeq.IsOk)
        {
            return Result<(INode, State)>.Err(thenSeq.Error!);
        }

        (ImmutableArray<INode> thenNodes, State afterThen) = thenSeq.Value;
        (bool hasR, State afterR) = afterThen.Match(TokenKind.RBrace);
        if (!hasR)
        {
            return Result<(INode, State)>.Err(
                new ParseError("IfMissingRBrace", atIf.Range, "Expected '}' to close @if block."));
        }

        ImmutableArray<INode> elseNodes = [];
        State rest = SkipWhitespaceTextTokens(afterR);
        if (rest.Kind == TokenKind.Else)
        {
            (_, rest) = rest.Take();
            rest = SkipWhitespaceTextTokens(rest);
            if (rest.Kind == TokenKind.AtIf)
            {
                Result<(INode node, State rest)> elifRes = ParseIf(rest);
                if (!elifRes.IsOk)
                {
                    return Result<(INode, State)>.Err(elifRes.Error!);
                }

                elseNodes = [elifRes.Value.node];
                rest = elifRes.Value.rest;
            }
            else
            {
                (bool hasElseL, State afterElseL) = rest.Match(TokenKind.LBrace);
                if (!hasElseL)
                {
                    return Result<(INode, State)>.Err(
                        new ParseError("ElseMissingLBrace", atIf.Range, "Expected '{' after else."));
                }

                Result<(ImmutableArray<INode> nodes, State rest)> elseSeq = ParseSequence(afterElseL, stopAtRBrace: true);
                if (!elseSeq.IsOk)
                {
                    return Result<(INode, State)>.Err(elseSeq.Error!);
                }

                (ImmutableArray<INode> elseParsed, State afterElseBody) = elseSeq.Value;
                (bool hasElseR, State afterElseR) = afterElseBody.Match(TokenKind.RBrace);
                if (!hasElseR)
                {
                    return Result<(INode, State)>.Err(
                        new ParseError("ElseMissingRBrace", atIf.Range, "Expected '}' to close else block."));
                }

                elseNodes = elseParsed;
                rest = afterElseR;
            }
        }

        IfNode node = elseNodes.IsDefaultOrEmpty
            ? Node.FromIf(condRes.Value, thenNodes, hadNlBeforeL, atIf.Range)
            : Node.FromIf(condRes.Value, thenNodes, elseNodes, hadNlBeforeL, atIf.Range);

        return Result<(INode, State)>.Ok((node, rest));
    }

    private static Result<(INode node, State rest)> ParseFor(State r)
    {
        (Token atFor, State afterFor0) = r.Take();
        (State afterFor, bool hadNlBeforeL) = SkipWhitespaceTextTokensWithNewlineInfo(afterFor0);
        Result<(string item, string? index, IExpr seq)> header = ParseForHeader(atFor.Text, atFor.Range);
        if (!header.IsOk)
        {
            return Result<(INode, State)>.Err(header.Error!);
        }

        (bool hasL, State afterL) = afterFor.Match(TokenKind.LBrace);
        if (!hasL)
        {
            return Result<(INode, State)>.Err(new ParseError("ForMissingLBrace", atFor.Range,
                "Expected '{' after @for(...)."));
        }

        Result<(ImmutableArray<INode> body, State rest)> bodySeq = ParseSequence(afterL, stopAtRBrace: true);
        if (!bodySeq.IsOk)
        {
            return Result<(INode, State)>.Err(bodySeq.Error!);
        }

        (ImmutableArray<INode> body, State afterBody) = bodySeq.Value;
        (bool hasR, State afterR) = afterBody.Match(TokenKind.RBrace);
        if (!hasR)
        {
            return Result<(INode, State)>.Err(new ParseError("ForMissingRBrace", atFor.Range,
                "Expected '}' to close @for block."));
        }

        ImmutableArray<INode> elseNodes = [];
        State rest = SkipWhitespaceTextTokens(afterR);
        if (rest.Kind == TokenKind.Else)
        {
            (_, rest) = rest.Take();
            rest = SkipWhitespaceTextTokens(rest);
            (bool hasElseL, State afterElseL) = rest.Match(TokenKind.LBrace);
            if (!hasElseL)
            {
                return Result<(INode, State)>.Err(new ParseError("ElseMissingLBrace", atFor.Range,
                    "Expected '{' after else."));
            }

            Result<(ImmutableArray<INode> nodes, State rest)> elseSeq = ParseSequence(afterElseL, stopAtRBrace: true);
            if (!elseSeq.IsOk)
            {
                return Result<(INode, State)>.Err(elseSeq.Error!);
            }

            (ImmutableArray<INode> elseParsed, State afterElseBody) = elseSeq.Value;
            (bool hasElseR, State afterElseR) = afterElseBody.Match(TokenKind.RBrace);
            if (!hasElseR)
            {
                return Result<(INode, State)>.Err(new ParseError("ElseMissingRBrace", atFor.Range,
                    "Expected '}' to close else block."));
            }

            elseNodes = elseParsed;
            rest = afterElseR;
        }

        (string item, string? index, IExpr seq) = header.Value;
        ForNode node = index is null
            ? Node.FromFor(item, seq, body, elseNodes, hadNlBeforeL, atFor.Range)
            : Node.FromFor(item, index, seq, body, elseNodes, hadNlBeforeL, atFor.Range);

        return Result<(INode, State)>.Ok((node, rest));
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

        (string item, int i2) = ReadIdent(header, i, n);
        i = i2;
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

    private static State SkipWhitespaceTextTokens(State s)
    {
        while (s is { Eof: false, Kind: TokenKind.Text })
        {
            string t = s.Current.Text;
            if (!t.All(char.IsWhiteSpace))
            {
                break;
            }

            s = s.Advance();
        }

        return s;
    }

    private static (State rest, bool hadNewline) SkipWhitespaceTextTokensWithNewlineInfo(State s)
    {
        bool hadNl = false;
        while (s is { Eof: false, Kind: TokenKind.Text } && s.Current.Text.All(char.IsWhiteSpace))
        {
            if (!hadNl && s.Current.Text.AsSpan().IndexOfAny('\n', '\r') >= 0)
            {
                hadNl = true;
            }

            s = s.Advance();
        }

        return (s, hadNl);
    }
}