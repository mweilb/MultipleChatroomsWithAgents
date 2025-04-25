

using YamlDotNet.Serialization;


namespace YamlConfigurations
{
    public class YamlMultipleChatRooms : YamlRoomConfig
    {
         

        // YAML "start room" maps to our CurrentRoom property.
        [YamlMember(Alias = "start-room")]
        public YamlStringWithLocation? StartRoom { get; set; } = null!;

        // YAML "start room" maps to our CurrentRoom property.
        [YamlMember(Alias = "auto-start")]
        public string AutoStart { get; set; } = string.Empty;

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
                    var agentDefintion = Agents.FirstOrDefault(a => a.Name == agent.Name);
                    if (agentDefintion != null)
                    {
                        agent.ApplyParentOverride(agentDefintion);
                    }
                }
            }
        }
    }
}
