using System.Text;

namespace dotRenderer;

public static class Tokenizer
{
    public static IEnumerable<object> Tokenize(string template)
        => Tokenize(template ?? throw new ArgumentNullException(nameof(template)), 0, template.Length);

    private static List<object> Tokenize(string template, int start, int end)
    {
        List<object> tokens = [];
        StringBuilder sb = new();
        int pos = start;

        while (pos < end)
        {
            if (template.IndexOf("@if (", pos, StringComparison.Ordinal) == pos)
            {
                if (sb.Length > 0)
                {
                    tokens.Add(new TextToken(sb.ToString()));
                    sb.Clear();
                }
                
                pos += "@if (".Length;
                (string condition, pos) = ReadCondition(template, pos, end);
                while (pos < end && char.IsWhiteSpace(template[pos]))
                {
                    pos++;
                }

                if (pos >= end || template[pos] != '{')
                {
                    throw new InvalidOperationException("Expected '{' after @if condition");
                }

                pos++;
                
                int bodyStart = pos;
                int braces = 1;
                while (pos < end && braces > 0)
                {
                    if (template[pos] == '{')
                    {
                        braces++;
                    }
                    else if (template[pos] == '}')
                    {
                        braces--;
                    }

                    pos++;
                }
                
                if (braces != 0)
                {
                    throw new InvalidOperationException("Unclosed @if block: missing '}'");
                }
                
                int bodyEnd = pos - 1;
                IEnumerable<object> bodyTokens = Tokenize(template, bodyStart, bodyEnd);
                tokens.Add(new IfToken(condition, bodyTokens));
                continue;
            }

            if (template[pos] == '@' && pos + 1 < end && template[pos + 1] == '@')
            {
                sb.Append('@');
                pos += 2;
                continue;
            }

            if (template.IndexOf("@Model.", pos, StringComparison.Ordinal) == pos)
            {
                if (sb.Length > 0)
                {
                    tokens.Add(new TextToken(sb.ToString()));
                    sb.Clear();
                }
                
                int pathStart = pos + "@Model.".Length;
                int pathEnd = pathStart;
                List<string> segments = ["Model"];
                while (true)
                {
                    int segStart = pathEnd;
                    while (pathEnd < end && IsIdentifierChar(template[pathEnd]))
                    {
                        pathEnd++;
                    }
                    
                    string name = template[segStart..pathEnd];
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new InvalidOperationException($"No identifier after @Model. at position {pos}");
                    }
                    
                    segments.Add(name);

                    if (pathEnd < end && template[pathEnd] == '.')
                    {
                        pathEnd++;
                        continue;
                    }
                    
                    break;
                }
                
                tokens.Add(new InterpolationToken(segments));
                pos = pathEnd;
                continue;
            }
            
            sb.Append(template[pos]);
            pos++;
        }

        if (sb.Length > 0)
        {
            tokens.Add(new TextToken(sb.ToString()));
        }

        return tokens;
    }

    private static (string condition, int nextPos) ReadCondition(string template, int pos, int end)
    {
        int condStart = pos;
        int parens = 1;
        while (pos < end && parens > 0)
        {
            if (template[pos] == '(')
            {
                parens++;
            }
            else if (template[pos] == ')')
            {
                parens--;
            }

            pos++;
        }
        if (parens != 0)
        {
            throw new InvalidOperationException("Unclosed @if condition: missing ')'");
        }

        int condEnd = pos - 1;
        return (template.Substring(condStart, condEnd - condStart).Trim(), pos);
    }

    private static bool IsIdentifierChar(char c)
        => char.IsLetterOrDigit(c) || c == '_';
}

public sealed record TextToken(string Text);

public sealed record InterpolationToken(IEnumerable<string> Path);

public sealed record IfToken(string Condition, IEnumerable<object> Body);