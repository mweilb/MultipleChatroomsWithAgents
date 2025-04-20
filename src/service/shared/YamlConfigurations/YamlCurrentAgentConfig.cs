using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    // This class represents a reference to an agent in lists (for current/next steps).
    // It now uses the name NextAgent instead of AgentReference.
    public class YamlCurrentAgentConfig : YamlLineInfo
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

    }

}
