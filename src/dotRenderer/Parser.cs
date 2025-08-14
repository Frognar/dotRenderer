using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace DotRenderer;

public static class Parser
{
    [Pure]
    public static Result<Template> Parse(ImmutableArray<Token> tokens)
    {
        ImmutableArray<INode>.Builder nodesBuilder = ImmutableArray.CreateBuilder<INode>();
        int i = 0;
        int n = tokens.Length;

        while (i < n)
        {
            Token t = tokens[i];
            switch (t.Kind)
            {
                case TokenKind.Text:
                    nodesBuilder.Add(Node.FromText(t.Text, t.Range));
                    i++;
                    break;
                
                case TokenKind.AtIdent:
                    nodesBuilder.Add(Node.FromInterpolateIdent(t.Text, t.Range));
                    i++;
                    break;
                    
                case TokenKind.AtExpr:
                    Result<IExpr> parsed = ExprParser.Parse(t.Text);
                    if (!parsed.IsOk)
                    {
                        IError err = parsed.Error!;
                        return Result<Template>.Err(new ParseError(err.Code, t.Range, err.Message));
                    }

                    nodesBuilder.Add(Node.FromInterpolateExpr(parsed.Value, t.Range));
                    i++;
                    break;

                case TokenKind.AtIf:
                    Result<(int newi, INode node)> ifNode = ParseIf(tokens, i);
                    if (!ifNode.IsOk)
                    {
                        return Result<Template>.Err(ifNode.Error!);
                    }

                    nodesBuilder.Add(ifNode.Value.node);
                    i = ifNode.Value.newi;;
                    break;

                case TokenKind.AtFor:
                    Result<(int newi, INode node)> forNode = ParseFor(tokens, i);
                    if (!forNode.IsOk)
                    {
                        return Result<Template>.Err(forNode.Error!);
                    }

                    nodesBuilder.Add(forNode.Value.node);
                    i = forNode.Value.newi;
                    break;
            }
        }

        Template template = new(nodesBuilder.ToImmutable());
        return Result<Template>.Ok(template);
    }

    private static Result<(int, INode)> ParseIf(ImmutableArray<Token> tokens, int i)
    {
        int n = tokens.Length;
        Token atIf = tokens[i];
        Result<IExpr> cond = ExprParser.Parse(atIf.Text);
        if (!cond.IsOk)
        {
            IError err = cond.Error!;
            return Result<(int, INode)>.Err(new ParseError(err.Code, atIf.Range, err.Message));
        }

        i++;
        if (i >= n || tokens[i].Kind != TokenKind.LBrace)
        {
            return Result<(int, INode)>.Err(new ParseError("IfMissingLBrace", atIf.Range, "Expected '{' after @if(...)."));
        }

        i++;
        ImmutableArray<INode>.Builder thenBuilder = ImmutableArray.CreateBuilder<INode>();
        while (i < n && tokens[i].Kind != TokenKind.RBrace)
        {
            Token t = tokens[i];
            switch (t.Kind)
            {
                case TokenKind.Text:
                    thenBuilder.Add(Node.FromText(t.Text, t.Range));
                    i++;
                    break;

                case TokenKind.AtIdent:
                    thenBuilder.Add(Node.FromInterpolateIdent(t.Text, t.Range));
                    i++;
                    break;

                case TokenKind.AtExpr:
                {
                    Result<IExpr> parsed = ExprParser.Parse(t.Text);
                    if (!parsed.IsOk)
                    {
                        IError err = parsed.Error!;
                        return Result<(int, INode)>.Err(new ParseError(err.Code, t.Range, err.Message));
                    }

                    thenBuilder.Add(Node.FromInterpolateExpr(parsed.Value, t.Range));
                    i++;
                    break;
                }

                case TokenKind.AtIf:
                {
                    Result<(int i, INode node)> inner = ParseIf(tokens, i);
                    if (!inner.IsOk)
                    {
                        return inner;
                    }
                    
                    i = inner.Value.i;
                    thenBuilder.Add(inner.Value.node);
                    break;
                }

                case TokenKind.LBrace:
                    return Result<(int, INode)>.Err(new ParseError("UnexpectedLBrace", t.Range, "Unexpected '{' inside @if block."));
            }
        }

        if (i >= n || tokens[i].Kind != TokenKind.RBrace)
        {
            return Result<(int, INode)>.Err(new ParseError("IfMissingRBrace", atIf.Range, "Expected '}' to close @if block."));
        }

        i++;
        
        ImmutableArray<INode>.Builder elseBuilder = ImmutableArray.CreateBuilder<INode>();
        if (i < n && tokens[i].Kind == TokenKind.Else)
        {
            i++;
            if (i >= n || tokens[i].Kind != TokenKind.LBrace)
            {
                return Result<(int, INode)>.Err(new ParseError("ElseMissingLBrace", atIf.Range, "Expected '{' after else."));
            }

            i++;
            while (i < n && tokens[i].Kind != TokenKind.RBrace)
            {
                Token t = tokens[i];
                switch (t.Kind)
                {
                    case TokenKind.Text:
                        elseBuilder.Add(Node.FromText(t.Text, t.Range));
                        i++;
                        break;

                    case TokenKind.AtIdent:
                        elseBuilder.Add(Node.FromInterpolateIdent(t.Text, t.Range));
                        i++;
                        break;

                    case TokenKind.AtExpr:
                    {
                        Result<IExpr> parsed = ExprParser.Parse(t.Text);
                        if (!parsed.IsOk)
                        {
                            IError err = parsed.Error!;
                            return Result<(int, INode)>.Err(new ParseError(err.Code, t.Range, err.Message));
                        }

                        elseBuilder.Add(Node.FromInterpolateExpr(parsed.Value, t.Range));
                        i++;
                        break;
                    }

                    case TokenKind.AtIf:
                    {
                        Result<(int i2, INode node)> inner = ParseIf(tokens, i);
                        if (!inner.IsOk)
                        {
                            return inner;
                        }
                        i = inner.Value.i2;
                        elseBuilder.Add(inner.Value.node);
                        break;
                    }

                    case TokenKind.LBrace:
                        return Result<(int, INode)>.Err(new ParseError("UnexpectedLBrace", t.Range, "Unexpected '{' inside else block."));

                    case TokenKind.RBrace:
                        break;

                    default:
                        i++;
                        break;
                }
            }

            if (i >= n || tokens[i].Kind != TokenKind.RBrace)
            {
                return Result<(int, INode)>.Err(new ParseError("ElseMissingRBrace", atIf.Range, "Expected '}' to close else block."));
            }

            i++;
        }

        IfNode node = elseBuilder.Count > 0
            ? Node.FromIf(cond.Value, thenBuilder.ToImmutable(), elseBuilder.ToImmutable(), atIf.Range)
            : Node.FromIf(cond.Value, thenBuilder.ToImmutable(), atIf.Range);

        return Result<(int, INode)>.Ok((i, node));
    }private static Result<(int, INode)> ParseFor(ImmutableArray<Token> tokens, int i)
    {
        int n = tokens.Length;
        Token atFor = tokens[i];
        Result<(string item, IExpr seq)> header = ParseForHeader(atFor.Text, atFor.Range);
        if (!header.IsOk)
        {
            return Result<(int, INode)>.Err(header.Error!);
        }

        i++;
        if (i >= n || tokens[i].Kind != TokenKind.LBrace)
        {
            return Result<(int, INode)>.Err(new ParseError("ForMissingLBrace", atFor.Range, "Expected '{' after @for(...)."));
        }

        i++;
        ImmutableArray<INode>.Builder bodyBuilder = ImmutableArray.CreateBuilder<INode>();
        while (i < n && tokens[i].Kind != TokenKind.RBrace)
        {
            Token t = tokens[i];
            switch (t.Kind)
            {
                case TokenKind.Text:
                    bodyBuilder.Add(Node.FromText(t.Text, t.Range));
                    i++;
                    break;

                case TokenKind.AtIdent:
                    bodyBuilder.Add(Node.FromInterpolateIdent(t.Text, t.Range));
                    i++;
                    break;

                case TokenKind.AtExpr:
                {
                    Result<IExpr> parsed = ExprParser.Parse(t.Text);
                    if (!parsed.IsOk)
                    {
                        IError err = parsed.Error!;
                        return Result<(int, INode)>.Err(new ParseError(err.Code, t.Range, err.Message));
                    }

                    bodyBuilder.Add(Node.FromInterpolateExpr(parsed.Value, t.Range));
                    i++;
                    break;
                }

                case TokenKind.AtIf:
                {
                    Result<(int i2, INode node)> innerIf = ParseIf(tokens, i);
                    if (!innerIf.IsOk)
                    {
                        return innerIf;
                    }
                    
                    i = innerIf.Value.i2;
                    bodyBuilder.Add(innerIf.Value.node);
                    break;
                }

                case TokenKind.AtFor:
                {
                    Result<(int i2, INode node)> innerFor = ParseFor(tokens, i);
                    if (!innerFor.IsOk)
                    {
                        return innerFor;
                    }

                    i = innerFor.Value.i2;
                    bodyBuilder.Add(innerFor.Value.node);
                    break;
                }

                case TokenKind.LBrace:
                    return Result<(int, INode)>.Err(new ParseError("UnexpectedLBrace", t.Range, "Unexpected '{' inside @for block."));
            }
        }

        if (i >= n || tokens[i].Kind != TokenKind.RBrace)
        {
            return Result<(int, INode)>.Err(new ParseError("ForMissingRBrace", atFor.Range, "Expected '}' to close @for block."));
        }

        i++;
        ForNode node = Node.FromFor(header.Value.item, header.Value.seq, bodyBuilder.ToImmutable(), atFor.Range);
        return Result<(int, INode)>.Ok((i, node));
    }

    private static Result<(string item, IExpr seq)> ParseForHeader(string header, TextSpan range)
    {
        int i = 0;
        int n = header.Length;

        while (i < n && char.IsWhiteSpace(header[i]))
        {
            i++;
        }

        if (i >= n || !(char.IsLetter(header[i]) || header[i] == '_'))
        {
            return Result<(string, IExpr)>.Err(new ParseError("ForItemIdent", range, "Expected loop variable identifier."));
        }

        int startIdent = i;
        i++;
        while (i < n && (char.IsLetterOrDigit(header[i]) || header[i] == '_'))
        {
            i++;
        }

        string item = header[startIdent..i];

        while (i < n && char.IsWhiteSpace(header[i]))
        {
            i++;
        }

        if (!(i + 2 <= n && header.AsSpan(i, 2).SequenceEqual("in".AsSpan())))
        {
            return Result<(string, IExpr)>.Err(new ParseError("ForMissingIn", range, "Expected 'in' in @for header."));
        }
        i += 2;

        while (i < n && char.IsWhiteSpace(header[i]))
        {
            i++;
        }

        if (i >= n)
        {
            return Result<(string, IExpr)>.Err(new ParseError("ForMissingExpr", range, "Expected expression after 'in'."));
        }

        string exprText = header[i..].Trim();
        Result<IExpr> expr = ExprParser.Parse(exprText);
        if (!expr.IsOk)
        {
            IError err = expr.Error!;
            return Result<(string, IExpr)>.Err(new ParseError(err.Code, range, err.Message));
        }

        return Result<(string, IExpr)>.Ok((item, expr.Value));
    }
}