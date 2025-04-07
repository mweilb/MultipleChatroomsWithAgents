using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlRegexReplacement
    {
        [YamlMember(Alias = "pattern")]
        public string? Pattern { get; set; } = null;

        [YamlMember(Alias = "replacement")]
        public string? Replacement { get; set; } = null;
    }
}