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
            }
        }

        Template template = new(nodesBuilder.ToImmutable());
        return Result<Template>.Ok(template);
    }

    private static Result<(int, INode)> ParseIf(ImmutableArray<Token> tokens, int i)
    {
        int n = tokens.Length;
        Token atIf = tokens[i]; // Kind == AtIf
        Result<IExpr> cond = ExprParser.Parse(atIf.Text);
        if (!cond.IsOk)
        {
            IError err = cond.Error!;
            return Result<(int, INode)>.Err(new ParseError(err.Code, atIf.Range, err.Message));
        }

        i++; // move past @if
        if (i >= n || tokens[i].Kind != TokenKind.LBrace)
        {
            return Result<(int, INode)>.Err(new ParseError("IfMissingLBrace", atIf.Range, "Expected '{' after @if(...)."));
        }

        i++; // consume '{'
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

        i++; // consume '}'
        IfNode node = Node.FromIf(cond.Value, thenBuilder.ToImmutable(), atIf.Range);
        return Result<(int, INode)>.Ok((i, node));
    }
}