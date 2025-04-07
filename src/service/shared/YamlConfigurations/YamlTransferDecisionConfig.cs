using YamlConfigurations.Presets;
using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    // The decision object used in various places.
    public class YamlTransferConfig  
    {
        [YamlMember(Alias = "prompt")]
        public string Prompt { get; set; } = string.Empty;
    }
}
