namespace dotRenderer.Tests;

internal static class TokenizerAssert
{
    public static void TokenSequence(object[] tokens, params object[] expected)
    {
        Assert.Equal(expected.Length, tokens.Length);
        foreach ((object act, object exp) in tokens.Zip(expected))
        {
            switch (exp)
            {
                case InterpolationToken interp:
                    InterpolationToken actualInter = Assert.IsType<InterpolationToken>(act);
                    Assert.Equal(interp.Path, actualInter.Path);
                    break;

                case TextToken textTok:
                    TextToken actualText = Assert.IsType<TextToken>(act);
                    Assert.Equal(textTok.Text, actualText.Text);
                    break;

                case IfToken ifTok:
                    IfToken actualIf = Assert.IsType<IfToken>(act);
                    Assert.Equal(ifTok.Condition, actualIf.Condition);
                    TokenSequence([.. actualIf.Body], [.. ifTok.Body]);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported expected type: {exp.GetType()}");
            }
        }
    }

    public static void Throws<TException>(string template, string expectedMessageFragment) where TException : Exception
    {
        TException ex = Assert.Throws<TException>(() => Tokenizer.Tokenize(template));
        Assert.Contains(expectedMessageFragment, ex.Message, StringComparison.Ordinal);
    }
}