 
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MultiAgents.AgentsChatRoom.Rooms;
using MultiAgents.WebSockets;
using YamlDotNet.Serialization;


namespace MultiAgents.Configurations
{
    public class YamlMultipleChatRooms : MultiAgentChatRooms
    {
        // YAML "emoji" key.
        [YamlMember(Alias = "emoji")]
        public override string Emoji { get; set; } = string.Empty;

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
       
        public List<string> Errors { get; internal set; } = [];

        public override Dictionary<string, IMultiAgentHandler>? GetRooms()
        {
            if (Rooms == null)
            {
                return null;
            }

            // Convert each YamlRoomConfig to IMultiAgentHandler
            return Rooms.ToDictionary(room => room.Key, room => (IMultiAgentHandler) room.Value);
        }


        public void Setup(Kernel kernel, int embeddedSize,ILoggerFactory loggerFactory)
        {
            if (Rooms != null)
            {
                foreach (var (roomName, room) in Rooms)
                {
                    room.Name = roomName;
                    room.GroupName = Name;

                    ApplyParentOverride(kernel, embeddedSize, room);
        
                    room.Setup(kernel, embeddedSize, loggerFactory);
                }
            }
        }

        internal async Task<bool> ResetAsync()
        {
            try
            {
                CurrentRoomName = StartRoom;
                if (Rooms != null)
                {
                    if (string.IsNullOrEmpty(CurrentRoomName) && (Rooms.Count > 0))
                    {
                        CurrentRoomName = Rooms.First().Value.Name;
                    }

                    foreach (var (roomName, room) in Rooms)
                    {
                        if (room != null)
                        {
                            await room.Reset();
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private void ApplyParentOverride(Kernel kernel, int embeddedSize, YamlRoomConfig room)
        {
            if (Agents != null)
            {
                foreach (var agent in room.Agents)
                {
                    if (Agents.TryGetValue(agent.Name, out var agentDefintion))
                    {
                        agent.ApplyParentOverride(agentDefintion);
                        agent.Setup(kernel, embeddedSize);
                    }
                }
            }
        }
    }
}
