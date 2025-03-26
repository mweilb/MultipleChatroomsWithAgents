 
using YamlDotNet.Serialization;

namespace  MultiAgents.Configurations
{
    // This class represents a reference to an agent in lists (for current/next steps).
    // It now uses the name NextAgent instead of AgentReference.
    public class YamlNextAgentConfig
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        // Optional context-transfer data.
        [YamlMember(Alias = "context-transfer")]
        public YamlTransferConfig? ContextTransfer { get; set; }

        
    }
}
