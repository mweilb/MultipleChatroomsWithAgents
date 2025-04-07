﻿ 
using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    // Room definition used in "chatrooms".
    public class YamlRoomConfig  
    {
        public string GroupName { get; set; } = string.Empty;

        [YamlMember(Alias = "emoji")]
        public string Emoji { get; set; } = string.Empty;

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

        
    }
}
