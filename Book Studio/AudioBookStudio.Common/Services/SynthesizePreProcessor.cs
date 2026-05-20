using AudioBookStudio.Common.Abstracts;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace AudioBookStudio.Common.Services;

public class SynthesizePreProcessor : ISynthesizePreProcessor
{
    private readonly SynthesizePreProcessConfig _config;
    private readonly IEnumerable<(Regex regex, string replacement)> _compiledPatterns;

    public SynthesizePreProcessor(IOptions<SynthesizePreProcessConfig> config)
    {
        _config = config.Value;
        _compiledPatterns = _config.Polyphones.Select(p => (new Regex(Regex.Escape(p.Key), RegexOptions.Compiled), p.Value));
    }
    public Task<string> ProcessAsync(string content)
    {
        if (string.IsNullOrEmpty(content))
            return Task.FromResult(content);

        string result = content;

        foreach (var (regex, replacement) in _compiledPatterns)
        {
            result = regex.Replace(result, replacement);
        }

        return Task.FromResult(result);
    }
}
