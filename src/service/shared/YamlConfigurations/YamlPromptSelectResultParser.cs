using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlPromptSelectResultParser
    {
        [YamlMember(Alias = "regex")]
        public List<YamlRegexReplacement>? Regex { get; set; }

        [YamlMember(Alias = "json")]
        public List<YamlJsonParser>? Json { get; set; }


    }
}