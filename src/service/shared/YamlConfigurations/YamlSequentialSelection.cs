using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlSequentialSelection
    {
        [YamlMember(Alias = "initial-agent")]
        public string? InitialAgent { get; set; }
    }
}