namespace dotRenderer.Tests;

internal static class TokenizerAssert
{
    public static void TokenSequence(object[] tokens, params object[] expected)
    {
        Assert.Equal(expected.Length, tokens.Length);
        for (int i = 0; i < tokens.Length; i++)
        {
            switch (expected[i])
            {
                case InterpolationToken interp:
                    InterpolationToken actualInter = Assert.IsType<InterpolationToken>(tokens[i]);
                    Assert.Equal(interp.Path, actualInter.Path);
                    break;

                case TextToken textTok:
                    TextToken actualText = Assert.IsType<TextToken>(tokens[i]);
                    Assert.Equal(textTok.Text, actualText.Text);
                    break;

                case IfToken ifTok:
                    IfToken actualIf = Assert.IsType<IfToken>(tokens[i]);
                    Assert.Equal(ifTok.Condition, actualIf.Condition);
                    TokenSequence([.. actualIf.Body], [.. ifTok.Body]);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported expected type: {expected[i].GetType()}");
            }
        }
    }

    public static void Throws<TException>(string template, string expectedMessageFragment) where TException : Exception
    {
        TException ex = Assert.Throws<TException>(() => Tokenizer.Tokenize(template));
        Assert.Contains(expectedMessageFragment, ex.Message, StringComparison.Ordinal);
    }
}