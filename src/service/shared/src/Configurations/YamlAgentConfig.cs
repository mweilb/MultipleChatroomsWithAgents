using YamlDotNet.Serialization;

namespace MultiAgents.Configurations
{
    // Global Agent definition.
    public class YamlAgentConfig
    {
        [YamlMember(Alias = "emoji")]
        public string Emoji { get; set; } = string.Empty;

        [YamlMember(Alias = "model")]
        public string Model { get; set; } = string.Empty;

        [YamlMember(Alias = "instructions")]
        public string Instructions { get; set; } = string.Empty;

        // Collection is optional.
        [YamlMember(Alias = "collection")]
        public YamlCollectionConfig? Collection { get; set; }

    }
}

 
