using MultiAgents.AgentsChatRoom.WebSockets;
using MultiAgents.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multi_agents_shared.src.AgentsChatRoom.WebSockets
{

    public class WebSocketLibrarianProfile
    {
        /// <summary>
        /// Gets or sets the name of the agent.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the emoji representing the actor.
        /// </summary>
        public string Emoji { get; set; } = string.Empty;
    }

    public class WebSocketLibraryRoomProfile
    {
        /// <summary>
        /// Gets or sets the name of the agent.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the emoji representing the actor.
        /// </summary>
        public string Emoji { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of agents representing the chat room.
        /// </summary>
        public List<WebSocketLibrarianProfile> ActiveLibrarians { get; set; } = [];
        public List<WebSocketLibrarianProfile> NotActiveLibrarians { get; set; } = [];
    }

    public class WebSocketGetLibrarians : WebSocketBaseMessage
    {
        public List<WebSocketLibraryRoomProfile> Rooms { get; set; } = [];
    }
}
