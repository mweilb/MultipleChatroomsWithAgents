 
using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    // The decision object used in various places.
    public class YamlModerationConfig
    {
        [YamlMember(Alias = "prompt")]
        public string Prompt { get; set; } = string.Empty;
    }
}
