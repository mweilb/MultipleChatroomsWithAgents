﻿﻿
 
using YamlDotNet.Serialization;


namespace YamlConfigurations
{
    public class YamlMultipleChatRooms 
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        // YAML "emoji" key.
        [YamlMember(Alias = "emoji")]
        public string Emoji { get; set; } = string.Empty;

        // YAML "start room" maps to our CurrentRoom property.
        [YamlMember(Alias = "start-room")]
        public string StartRoom { get; set; } = string.Empty;

        // YAML "start room" maps to our CurrentRoom property.
        [YamlMember(Alias = "auto-start")]
        public string AutoStart { get; set; } = string.Empty;

        // YAML "agents" node: Global agents defined once.
        [YamlMember(Alias = "agents")]
        public Dictionary<string, YamlAgentConfig>? Agents { get; set; } = new Dictionary<string, YamlAgentConfig>();

        // YAML "chatrooms" node.
        [YamlMember(Alias = "chatrooms")]
        public Dictionary<string, YamlRoomConfig>? Rooms { get; set; } = new Dictionary<string, YamlRoomConfig>();
        
        //track the original Yaml file
        public string Yaml { get; internal set; } = string.Empty;
       
        public List<YamlConfigurations.Validations.ValidationError> Errors { get; internal set; } = new();

      
        public void ApplyParentOverride()
        {
            if (Rooms != null)
            {
                foreach (var (roomName, room) in Rooms)
                {
                    room.Name = roomName;
                    room.GroupName = Name;

                    ApplyParentOverride(room);
                    // Setup strategies and moderation.
                    room.Strategies?.Setup(room);
           
                }
            }
        }

       

        private void ApplyParentOverride(YamlRoomConfig room)
        {
            if (Agents != null)
            {
                foreach (var agent in room.Agents)
                {
                    if (Agents.TryGetValue(agent.Name, out var agentDefintion))
                    {
                        agent.ApplyParentOverride(agentDefintion);
                    }
                }
            }
        }
    }
}
