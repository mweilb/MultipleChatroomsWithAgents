using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlResultParser
    {
        [YamlMember(Alias = "regex")]
        public List<YamlExpression>? Regex { get; set; }
    }
}