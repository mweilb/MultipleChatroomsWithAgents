 
using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    // The decision object used in various places.
    public class YamlModerationConfig : YamlLineInfo
    {
        [YamlMember(Alias = "prompt")]
        public string Prompt { get; set; } = string.Empty;
    }
}
