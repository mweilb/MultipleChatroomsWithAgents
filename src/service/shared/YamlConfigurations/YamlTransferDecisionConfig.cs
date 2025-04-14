using YamlConfigurations.Presets;
using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    // The decision object used in various places.
    public class YamlTransferConfig  
    {
        [YamlMember(Alias = "instructions")]
        public string Prompt { get; set; } = string.Empty;

        // Indicates if the room should yield on change ("yes", "true", etc.)
        [YamlMember(Alias = "need-user-approval")]
        public string? YieldOnRoomChange { get; set; }

        // Indicates if the room should yield on change ("yes", "true", etc.)
        [YamlMember(Alias = "cancellation-agent-name")]
        public string? YieldCanceledName { get; set; }

    }
}
