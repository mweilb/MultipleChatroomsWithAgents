using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlResultParser : YamlLineInfo
    {
        [YamlMember(Alias = "regex")]
        public List<YamlExpression>? Regex { get; set; }
    }
}