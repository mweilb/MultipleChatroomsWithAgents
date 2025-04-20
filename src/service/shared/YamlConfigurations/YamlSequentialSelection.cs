using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlSequentialSelection : YamlLineInfo
    {
        [YamlMember(Alias = "initial-agent")]
        public string? InitialAgent { get; set; }
    }
}