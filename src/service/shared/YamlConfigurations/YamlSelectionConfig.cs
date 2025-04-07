 
using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlSelectionConfig  
    {
        [YamlMember(Alias = "sequential-selection")]
        public YamlSequentialSelection? SequentialSelection { get; set; }

        [YamlMember(Alias = "round-robin-selection")]
        public YamlRoundRobinSelection? RoundRobinSelection { get; set; }

        [YamlMember(Alias = "prompt-select")]
        public YamlPromptSelect? PromptSelect { get; set; }
    }
}
