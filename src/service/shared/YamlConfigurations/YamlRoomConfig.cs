 
using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    // Room definition used in "chatrooms".
    public class YamlRoomConfig  
    {
        public string GroupName { get; set; } = string.Empty;

        [YamlMember(Alias = "emoji")]
        public string Emoji { get; set; } = string.Empty;

        [YamlMember(Alias = "display-name")]
        public string? DisplayName { get; set; } = string.Empty;


        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        // Agents are defined as a list (to preserve order) with each reference including a name.
        [YamlMember(Alias = "agents")]
        public List<YamlInstanceOfAgentConfig> Agents { get; set; } = new List<YamlInstanceOfAgentConfig>();

        // Strategies for the room.
        [YamlMember(Alias = "strategies")]
        public YamlStrategyConfig? Strategies { get; set; }

        // Moderation rule.
        [YamlMember(Alias = "moderation")]
        public YamlModerationConfig? Moderation { get; set; }

        // Indicates if the room should yield on change ("yes", "true", etc.)
        [YamlMember(Alias = "yield-on-room-change")]
        public string? YieldOnRoomChange { get; set; }

        [YamlMember(Alias = "yield-user-canceled")]
        public string? YieldCanceledName { get; internal set; }


    }
}
