using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlExpression : YamlLineInfo
    {
        [YamlMember(Alias = "pattern")]
        public string? Pattern { get; set; }

        [YamlMember(Alias = "value")]
        public string? Value { get; set; }
    }
}