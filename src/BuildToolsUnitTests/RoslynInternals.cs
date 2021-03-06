﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace BuildToolsUnitTests
{
    // these types borrowed from Roslyn's internal implementations of the abstract types
    internal sealed class InMemoryAdditionalText : AdditionalText
    {
        private readonly SourceText _content;

        public InMemoryAdditionalText(string path, string content)
        {
            Path = path;
            _content = SourceText.From(content, Encoding.UTF8);
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default) => _content;
    }

    internal sealed class TestAnalyzeConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly ImmutableDictionary<object, AnalyzerConfigOptions> _treeDict;

        public static TestAnalyzeConfigOptionsProvider Empty { get; }
            = new TestAnalyzeConfigOptionsProvider(
                ImmutableDictionary<object, AnalyzerConfigOptions>.Empty,
                TestAnalyzerConfigOptions.Empty);

        internal TestAnalyzeConfigOptionsProvider(
            ImmutableDictionary<object, AnalyzerConfigOptions> treeDict,
            AnalyzerConfigOptions globalOptions)
        {
            _treeDict = treeDict;
            GlobalOptions = globalOptions;
        }

        public override AnalyzerConfigOptions GlobalOptions { get; }

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            => _treeDict.TryGetValue(tree, out var options) ? options : TestAnalyzerConfigOptions.Empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => _treeDict.TryGetValue(textFile, out var options) ? options : TestAnalyzerConfigOptions.Empty;

        internal TestAnalyzeConfigOptionsProvider WithAdditionalTreeOptions(ImmutableDictionary<object, AnalyzerConfigOptions> treeDict)
            => new TestAnalyzeConfigOptionsProvider(_treeDict.AddRange(treeDict), GlobalOptions);

        internal TestAnalyzeConfigOptionsProvider WithGlobalOptions(AnalyzerConfigOptions globalOptions)
            => new TestAnalyzeConfigOptionsProvider(_treeDict, globalOptions);
    }

    internal sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        public static TestAnalyzerConfigOptions Empty { get; } = new TestAnalyzerConfigOptions(ImmutableDictionary.Create<string, string>(KeyComparer));

        private readonly ImmutableDictionary<string, string> _backing;

        public TestAnalyzerConfigOptions(ImmutableDictionary<string, string> properties)
        {
            _backing = properties;
        }

        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => _backing.TryGetValue(key, out value);
    }
}
