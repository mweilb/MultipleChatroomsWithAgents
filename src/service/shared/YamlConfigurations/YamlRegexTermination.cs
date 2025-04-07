using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YarmRegexTermination
    {
        
        [YamlMember(Alias = "expressions")]
        public List<string>? Patterns { get; set; }
    }
}
