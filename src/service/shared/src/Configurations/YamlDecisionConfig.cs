using Microsoft.SemanticKernel;
using YamlDotNet.Serialization;

namespace MultiAgents.Configurations
{
    // The decision object used in various places.
    public class YamlDecisionConfig
    {
        [YamlMember(Alias = "prompt")]
        public string Prompt { get; set; } = string.Empty;

        [YamlMember(Alias = "messages-filter")]
        public string MessagesFilter { get; set; } = string.Empty;

        //Todo: Make this into preset (eventually)
        [YamlMember(Alias = "messages-preset-filters")]
        public List<string> MessagePresetFilters { get; set; } = new List<string>();


        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        [YamlMember(Alias = "presets")]
        public List<string> Presets { get; set; } = new List<string>();



    }
}
