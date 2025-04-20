using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YarmRegexTermination : YamlLineInfo
    {
        
        [YamlMember(Alias = "expressions")]
        public List<string>? Patterns { get; set; }
    }
}
