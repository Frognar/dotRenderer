namespace dotRenderer.Tests;

public class CompilationCacheTests
{
    private sealed class RecordingCache : ICompilationCache
    {
        public int Calls { get; private set; }
        private readonly Dictionary<string, SequenceNode> _store = new();

        public SequenceNode GetOrAdd(string template, Func<string, SequenceNode> factory)
        {
            if (_store.TryGetValue(template, out SequenceNode? ast))
                return ast;

            Calls++;
            ast = factory(template);
            _store[template] = ast;
            return ast;
        }
    }

    [Fact]
    public void Compile_With_Cache_Should_SingleFlight_Per_Template_And_Separate_For_Different_Templates()
    {
        RecordingCache cache = new();

        ITemplate<TestDictModel> t1a = TemplateCompiler.Compile("Hello @Model.Name", TestDictAccessor.Default, cache);
        ITemplate<TestDictModel> t1b = TemplateCompiler.Compile("Hello @Model.Name", TestDictAccessor.Default, cache);
        ITemplate<TestDictModel> t2 = TemplateCompiler.Compile("Bye @Model.Name", TestDictAccessor.Default, cache);

        Assert.Equal(2, cache.Calls);
        Assert.Equal("Hello Alice", t1a.Render(TestDictModel.With(("Name", "Alice"))));
        Assert.Equal("Hello Bob", t1b.Render(TestDictModel.With(("Name", "Bob"))));
        Assert.Equal("Bye Eve", t2.Render(TestDictModel.With(("Name", "Eve"))));
    }
}