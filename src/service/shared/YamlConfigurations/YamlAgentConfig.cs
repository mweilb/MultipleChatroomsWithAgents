using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    // Global Agent definition.
    public class YamlAgentConfig
    {
        [YamlMember(Alias = "emoji")]
        public string? Emoji { get; set; } = string.Empty;

        [YamlMember(Alias = "display-name")]
        public string? DisplayName { get; set; } = string.Empty;

        [YamlMember(Alias = "model")]
        public string? Model { get; set; } = string.Empty;

        [YamlMember(Alias = "instructions")]
        public string? Instructions { get; set; } = null;
       
        [YamlMember(Alias = "echo")]
        public string? Echo { get; set; } = null;

        // Collection is optional.
        [YamlMember(Alias = "collection")]
        public YamlCollectionConfig? Collection { get; set; }

    }
}

 
